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
        
        [Tooltip("Max radius we will consider before we say the channel is completely unobstructed.")]
        public float MaxGap = 10f;
        
        [Tooltip("Radius search steps. If too small, you will get a warning about number of steps being too high!")]
        public int GapStep = 2;
        public int CastCount => (int)((MaxGap-MinGap)/GapStep);

        [Tooltip("Should there be secondary messages received depending on the channel shape?")]
        public bool BroadcastWithEcho = true;


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
        
        void Broadcast(DataPacket data)
        {
            foreach(Transceiver tc in allTransceivers)
            {
                var id = tc.GetInstanceID();
                if(id == this.GetInstanceID()) continue; // skip self

                Vector3 selfPos = transform.position;
                Vector3 otherPos = tc.transform.position;

                var dist = (otherPos-selfPos).magnitude;
                if(dist > MaxRange) continue; // skip too far

                // no need to cast further than point to point distance
                var targetRange = Mathf.Min(dist, MaxRange);
                float sphereRadius = MinGap;
                float foundGap = -1;
                //TODO maybe do a binary search for the max gap? probably overkill :D
                for(; sphereRadius < MaxGap; sphereRadius += GapStep)
                {
                    // spherecast uses the center of the sphere, we want the tip
                    // otherwise things BEHIND the target point are picked up too.
                    // so we reduce max range by radius and start radius away towards the target
                    RaycastHit hit;
                    var direction = (otherPos-selfPos).normalized;
                    var startPos = selfPos + sphereRadius*direction;
                    if(Physics.SphereCast(startPos, sphereRadius, direction, out hit, targetRange-sphereRadius))
                    {
                        // There is no corridor of given radius between objects.
                        // no need to search wider
                        // Debug.DrawLine(startPos, hit.point, Color.red, 1);
                        break;
                    }
                    foundGap = sphereRadius;
                }
                if(foundGap > -1)
                {
                    // There is _some_ gap between the transceivers to send a signal :D
                    // find a color for the channel width/distance ratio because pretty

                    float gapPercent =  foundGap / (MaxGap - MinGap); // bigger, better
                    float rangePercent = targetRange / MaxRange; // smaller, better
                    float gapRangeRatio = gapPercent / rangePercent; // bigger, greener
                    Color c = new Color(0.1f, gapRangeRatio, 0.1f, 1f);
                    Debug.DrawLine(transform.position, tc.transform.position, c, 1);
                    float delay = targetRange / SoundVelocity;
                    // we want to delay the arrival of the data, so need a thread
                    // that just sleeps for the duration and then calls the receive function
                    StartCoroutine(TransmitWithDelay(delay, tc, data));
                }
            }
        }

        IEnumerator TransmitWithDelay(float delay, Transceiver receiver, DataPacket data)
        {
            yield return new WaitForSeconds(delay);
            receiver.Receive(data);
            if(BroadcastWithEcho)
            {
                yield return new WaitForSeconds(delay);
                receiver.Receive(data);
            }
        }

        void Receive(DataPacket data)
        {
            Debug.Log($"I am {this.GetInstanceID()}, got data:'{data.Data}' at t={data.TimeSent}, delay={Clock.NowTimeInSeconds - data.TimeSent}");
        }

        void FixedUpdate()
        {
            if(work)
            {
                Broadcast(new DataPacket($"Ping from {this.GetInstanceID()}", Clock.NowTimeInSeconds));
                work = false;
            }
        }

    }
}