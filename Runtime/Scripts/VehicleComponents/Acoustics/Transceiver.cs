using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core; //Clock

namespace Acoustics
{

    public class DataPacket
    {
        public string Data;
        public double TimeSent;
        public DataPacket(string data, double timeSent)
        {
            Data = data;
            TimeSent = timeSent;
        }
    }


    public class Transceiver : MonoBehaviour, ISoundVelocityUser
    {   
        [Tooltip("Speed of sound underwater, defaults to 1500m/s.")]
        public float SoundVelocity = 1500f;
        
        [Tooltip("Maximum range of this transceiver for broadcasting.")]
        public float MaxRange = 100;
        
        [Tooltip("Min radius of the channel. Think of a tube between transceivers free of obstacles. How big should it be to transmit?")]
        public float MinGap = 0.2f;
        
        // [Tooltip("Max radius we will consider before we say the channel is completely unobstructed.")]
        // public float MaxGap = 10f;
        
        // [Tooltip("Radius search steps. If too small, you will get a warning about number of steps being too high!")]
        // public int GapStep = 2;
        // public int CastCount => (int)((MaxGap-MinGap)/GapStep);

        [Tooltip("Should there be secondary messages received depending on the channel shape?")]
        public bool EnableEchoing = true;

        // [Tooltip("How likely an echo will happen if the unobstructed channel length and radius are equal?")]
        // [Range(0f,1f)]
        // public float EchoProbIfSquareChannel = 0.1f;

        [Tooltip("If an echo happens, how much distance can that echo travel in total compared to max range?")]
        [Range(0f,1f)]
        public float RangeLossRatioOnEcho = 0.5f;

        public bool DrawSignalLines = true;


        Transceiver[] allTransceivers;

        public bool work=false;

        void OnValidate()
        {
            if(CastCount > 10)
            {
                Debug.LogWarning($"You have set a transceiver to check {CastCount}>10 different radii every ping. This might be very slow!");
            }
            if(MinGap > MaxGap)
            {
                Debug.LogWarning($"MinGap must be >= MaxGap. Setting equal.");
                MinGap = MaxGap;
            }
            if(GapStep <= 0)
            {
                Debug.LogWarning($"GapStep must be > 0. Setting to 1.");
                GapStep = 1;
            }
        }


        public void SetSoundVelocity(float vel)
        {
            // should be set by the water volume as needed, similar
            // to water currents and forcepoints
            SoundVelocity = vel;
        }    

        void Start()
        {
            allTransceivers = GameObject.FindObjectsByType<Transceiver>(FindObjectsSortMode.None);
        }

        float FindGapToOtherTx(Transceiver tx)
        {
            Vector3 selfPos = transform.position;
            Vector3 otherPos = tx.transform.position;
            Vector3 posDiffVec = otherPos-selfPos;

            // no need to cast further than point to point distance
            var dist = posDiffVec.magnitude;
            float sphereRadius = MinGap;
            float foundGap = -1;
            // maybe do a binary search for the max gap? probably overkill :D
            for(; sphereRadius < MaxGap; sphereRadius += GapStep)
            {
                // spherecast uses the center of the sphere, we want the tip
                // otherwise things BEHIND the target point are picked up too.
                // so we reduce max range by radius and start radius away towards the target
                var direction = posDiffVec.normalized;
                var startPos = selfPos + sphereRadius*direction;
                RaycastHit hit;
                if(Physics.SphereCast(startPos, sphereRadius, direction, out hit, dist-sphereRadius))
                {
                    // There is no open corridor of given radius between objects.
                    // no need to search wider
                    // Debug.DrawLine(startPos, hit.point, Color.red, 1);
                    break;
                }
                foundGap = sphereRadius;
            }
            return foundGap;
        }
        
        void Broadcast(DataPacket data)
        {
            foreach(Transceiver tx in allTransceivers)
            {
                var id = tx.GetInstanceID();
                if(id == this.GetInstanceID()) continue; // skip self

                var dist = Vector3.Distance(transform.position, tx.transform.position);
                if(dist > MaxRange) continue; // skip too far


                float freeRadius = FindGapToOtherTx(tx);
                if(freeRadius > -1)
                {
                    // There is _some_ gap between the transceivers to send a signal :D
                    // we want to delay the arrival of the data, so need a thread
                    // that just sleeps for the duration and then calls the receive function
                    StartCoroutine(TransmitWithDelay(freeRadius, dist, tx, data));

                    if(DrawSignalLines)
                    {
                        // find a color for the channel width/distance ratio because pretty
                        float green =  freeRadius / (MaxGap - MinGap); // bigger, better
                        float red = dist / MaxRange; // smaller, better
                        Color c = new Color(red, green, 0.1f, 1f);
                        Debug.DrawLine(transform.position, tx.transform.position, c, 1);
                    }
                }
            }
        }

        IEnumerator TransmitWithDelay(float channelRadius, float channelLength, Transceiver receiver, DataPacket data)
        {
            // first message arrives directly, and first
            float delay = channelLength / SoundVelocity;
            yield return new WaitForSeconds(delay);
            receiver.Receive(data);

            // if an echo is to happen...
            if(EnableEchoing)
            {
                // Things considered for echo:
                // - Do one large sphereCastAll, look at all the normals and decide if any would reflect towards target
                // - Do many CastAlls at different radii
                    // ^ anything with a sphereCast will basically never find a hit such that the normal of the surface
                    // is perpendicualar to the A-to-B vector precisely because it is a sphere moving along that vector,
                    // meaning there is exactly one point on that sphere that could hit such a surface.
                    // Thus sphereCast can never find echo-producing surfaces if it is cast towards the target.
                //
                // - Shotgun a million rays/spheres
                    // ^ No need to actually bounce the ray, just finding the normal of the hit is enough to calculate
                    // if it will bounce towards the target.
                    // Possibly can be done similar to MBES/SSS in terms of parallelism.
                    // These pings are way less frequently done than the sensors, but require way more rays due to being 2D rather than 1D.
                    // However, given that it will still be based on luck (the determinstically cast rays must hit few specific points)
                    // and that 99% of the time it will be disabled (because most modems already have echo-denial) I will not do this and
                    // simply approximate the chances of it happening given the unobstructed channel shape.
                //
                // - Cast ray to surface/bottom
                    // ^ HDRP surface cant have collider to hit.
                    // Standard surface is a plane. Use the same plane in all: AcousticReflectorPlane?
                    // Cheap to calculate the point of inflection and then do a cast to it and from it
                    // to see if its occluded.
                    // Bottom is HARD. Cant assume flat, annoying to simplify...
                //
                // - Combination
                // -- Sphere cast in the column towards target to check occlusion only.
                // -- Two Casts to/from reflection point on planar surface.
                // -- Shotgun rays to bottom and spheres from reflection points towards target
                // ^ Doable, accurate enough, accounts for MOST of the reflections. gg.

                // First of all, is an echo even feasible given length and radius?
                // As in, the distance traveled by the (perfect) echo must still be less than MaxRange
                // An ideal echo happens right in the middle of the channel, so we can do trig to find its
                // distance traveled.
                float echoRange = 2 * Mathf.Sqrt(Mathf.Pow(channelLength/2, 2) + Mathf.Pow(channelRadius, 2));
                if(echoRange <= MaxRange*RangeLossRatioOnEcho)
                {
                    // okay, the echo _could_ possibly make it to the target
                    // how likely is it to happen?
                    float narrowness = (channelLength/MaxRange) / (channelRadius/MaxGap);
                    // if its a narrow passage (ratio > 1) more likely than otherwise
                    // we use a parameter for the probability if it was perfectly square
                    // and modify that
                    float echoProb = EchoProbIfSquareChannel * narrowness;
                    Debug.Log($"l:{channelLength}, r:{channelRadius}, prob:{echoProb}, echoRange:{echoRange}");
                    Debug.DrawRay(transform.position, Vector3.up * 100* echoProb, new Color(echoProb*10, echoProb*10, echoProb*10, 1), 6000);

                    // a delay has already happened in execution for the primary signal
                    // so we need to delay _the extra bit_ for the echo
                    float echoDelay = (echoRange / SoundVelocity) - delay;
                    if(echoDelay > 0) // impossible, but just in case...
                    {
                        yield return new WaitForSeconds(echoDelay);
                        receiver.Receive(data);
                    }
                }

            }
        }

        void Receive(DataPacket data)
        {
            // Debug.Log($"I am {this.GetInstanceID()}, got data:'{data.Data}' at t={data.TimeSent}, delay={Clock.NowTimeInSeconds - data.TimeSent}");
        }

        void FixedUpdate()
        {
            if(work)
            {
                Broadcast(new DataPacket($"Ping from {this.GetInstanceID()}", Clock.NowTimeInSeconds));
                // work = false;
            }
        }

    }
}