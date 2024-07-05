using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core; //Clock
using DefaultNamespace.Water; // WaterQueryModel

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
        
        [Tooltip("Min radius of the unoccupied channel. Think of a tube between transceivers free of obstacles. How big should it be to transmit?")]
        public float MinChannelRadius = 0.2f;


        [Tooltip("Should there be secondary messages received depending on the channel shape?")]
        public bool EnableEchoing = true;

        [Tooltip("If an echo happens, how much distance can that echo travel in total compared to max range?")]
        [Range(0f,1f)]
        public float RangeLossRatioOnEcho = 0.5f;

        public bool DrawSignalLines = true;
        public bool work=false;



        WaterQueryModel waterModel;
        Transceiver[] allTransceivers;
        

        public void SetSoundVelocity(float vel)
        {
            // should be set by the water volume as needed, similar
            // to water currents and forcepoints
            SoundVelocity = vel;
        }    

        void Start()
        {
            allTransceivers = GameObject.FindObjectsByType<Transceiver>(FindObjectsSortMode.None);
            waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        }

        bool FrontSphereCast(Vector3 from, Vector3 to, out RaycastHit hit, float maxDist = Mathf.Infinity)
        {
            // SphereCast puts the center of the sphere at the start position
            // but want it to start/end a channel-radius away to avoid hitting things
            // that are behind the source/target
            Vector3 posDiffVec = to-from;
            float dist = posDiffVec.magnitude;
            Vector3 direction = posDiffVec.normalized;
            Vector3 startPos = from + MinChannelRadius * direction;
            float castDist = Mathf.Min(maxDist, dist) - MinChannelRadius;
            return Physics.SphereCast(startPos, MinChannelRadius, direction, out hit, castDist);
        }


        void TransmitDirectPath(DataPacket data, Transceiver tx)
        {
            RaycastHit hit;
            if(FrontSphereCast(transform.position, tx.transform.position, out hit))
            {
                // There is no open corridor of given radius between objects.
                if(DrawSignalLines)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red, 1);
                }
                return;
            }
            else
            {
                // There is an unobstructed corridor
                float delay = Vector3.Distance(transform.position, tx.transform.position) / SoundVelocity;
                StartCoroutine(TransmitWithDelay(data, tx, delay));
                if(DrawSignalLines)
                {
                    Debug.DrawLine(transform.position, tx.transform.position, Color.green, 1);
                }
            }
        }

        void TransmitSurfaceEcho(DataPacket data, Transceiver tx)
        {
            // To create an echo from the water surface, we need to
            // find out _where_ on the surface the refleciton would occur.
            // To do this efficiently
            // - we assume the surface is flat
            // - reflect the transmitter around the flat plane
            // -- send a raycast from the reflection towards the target
            // -- ignore the hits on the water surface
            // -- if there are no other hits, the water has reflected the signal
            // - use the hit on the water surface to find the distance the signal travels
            // etc.

            // this reflection could be generalized to _any plane_ rather
            // than the xz-plane we use here with little effort if needed.
            float waterSurfaceLevel = waterModel.GetWaterLevelAt(transform.position);
            Vector3 selfPos = transform.position;
            Vector3 selfReflectionPos = new Vector3(
                selfPos.x,
                -selfPos.y + waterSurfaceLevel,
                selfPos.z
            );

            Vector3 targetPos = tx.transform.position;

            RaycastHit surfaceHit;
            if(!Physics.Raycast(selfReflectionPos, targetPos-selfReflectionPos, out surfaceHit, MaxRange))
            {
                // the water surface is too far to bounce
                return;
            }

            Collider waterCollider = waterModel.GetComponent<Collider>();
            
            // found the point on the surface where the echo will happen
            // now we check from source to surface

            RaycastHit sourceToSurfaceHit;
            float remainingRange = MaxRange;
            if(FrontSphereCast(selfPos, surfaceHit.point, out sourceToSurfaceHit, remainingRange))
            {
                // there is a hit, but is it just the water?
                
                if(sourceToSurfaceHit.colliderInstanceID != waterCollider.GetInstanceID())
                {
                    // its not the water. no echo
                    if(DrawSignalLines) Debug.DrawLine(selfPos, sourceToSurfaceHit.point, Color.red, 1);
                    return;
                }
            } 
            if(DrawSignalLines) Debug.DrawLine(selfPos, surfaceHit.point, Color.green, 1);

            // either its water, or there's nothing in the way,
            // source can reach surface, can the surface reach target?

            // reduce availble range due to bounce.
            remainingRange = (remainingRange-sourceToSurfaceHit.distance) * RangeLossRatioOnEcho;
            RaycastHit surfaceToTargetHit;
            if(FrontSphereCast(surfaceHit.point, tx.transform.position, out surfaceToTargetHit, remainingRange))
            {
                // there is a hit, check if water again
                if(surfaceToTargetHit.colliderInstanceID != waterCollider.GetInstanceID())
                {
                    // its not the water. no echo
                    if(DrawSignalLines) Debug.DrawLine(surfaceHit.point, surfaceToTargetHit.point, Color.red, 1);
                    return;
                }
            }
            if(DrawSignalLines) Debug.DrawLine(surfaceHit.point, targetPos, Color.green, 1);
            
            // We got the entire path cleared.
            // Time to transmit!

            float totalDistanceTraveled = Vector3.Distance(selfPos, surfaceHit.point) + Vector3.Distance(targetPos, surfaceHit.point);
            float delay = totalDistanceTraveled / SoundVelocity;
            StartCoroutine(TransmitWithDelay(data, tx, delay));

        }

        void TransmitBottomEcho(DataPacket data, Transceiver tx)
        {
            // bottom is problematic, because we cant in good conciense assume its flat like the surface
            // thus we use the shotgun approach:
            // - fire a bunch of rays towards the target and bottom
            // -- let them reflect once from only the bottom
            // -- hope one hits the target
            // -- if it does, transmit

            // any ray that is looking "up" towards
            // the water surface is a waste, since we handle
            // both the surface reflections and direct-path 
            // separately and more efficiently.
            // so we just need the xz vector out of this:
            Vector3 toTarget = tx.transform.position - transform.position;
            toTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            // now, we can create rays on the sphere-slice where the
            // surface is relevant
        }
        
        void Broadcast(DataPacket data)
        {
            // Doesnt work out of water :(
            float selfWaterSurfaceLevel = waterModel.GetWaterLevelAt(transform.position);
            float selfDepth = selfWaterSurfaceLevel - transform.position.y;
            if(selfDepth < 0) return;

            // TODO this could be parallelized
            foreach(Transceiver tx in allTransceivers)
            {
                var id = tx.GetInstanceID();
                if(id == this.GetInstanceID()) continue; // skip self

                var dist = Vector3.Distance(transform.position, tx.transform.position);
                if(dist > MaxRange) continue; // skip too far

                float txWaterSurfaceLevel = waterModel.GetWaterLevelAt(tx.transform.position);
                float txDepth = txWaterSurfaceLevel - tx.transform.position.y;
                if(txDepth < 0) continue; // skip not-in-water

                TransmitDirectPath(data, tx);

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
                    // -- Two Casts to/from reflection point on planar water surface.
                    // -- Shotgun rays to bottom and spheres from reflection points towards target
                    // ^ Doable, accurate enough, accounts for MOST of the reflections. gg.

                    TransmitSurfaceEcho(data, tx);
                    TransmitBottomEcho(data, tx);
                }
            }
        }

        IEnumerator TransmitWithDelay(DataPacket data, Transceiver tx, float delay)
        {
            yield return new WaitForSeconds(delay);
            tx.Receive(data);
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