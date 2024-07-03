using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core; //Clock

namespace Acoustics
{
    public class Transceiver : MonoBehaviour, ISoundVelocityUser
    {   
        public double SoundVelocity = 1500;
        public float MaxRange = 100;
        public float sphereRadius = 10;

        Transceiver[] allTransceivers;

        public bool work=false;


        public void SetSoundVelocity(double vel)
        {
            // should be set by the water volume as needed, similar
            // to water currents and forcepoints
            SoundVelocity = vel;
        }    

        void Start()
        {
            allTransceivers = GameObject.FindObjectsByType<Transceiver>(FindObjectsSortMode.None);
        }
        
        void Send()
        {
            double now = Clock.NowTimeInSeconds;
            foreach(Transceiver tc in allTransceivers)
            {
                var id = tc.GetInstanceID();
                if(id == this.GetInstanceID()) continue; // skip self

                Vector3 selfPos = transform.position;
                Vector3 otherPos = tc.transform.position;

                if((otherPos-selfPos).magnitude > MaxRange) continue; // skip too far

                RaycastHit hit;
                if(Physics.SphereCast(selfPos, sphereRadius, otherPos-selfPos, out hit, MaxRange))
                {
                    Debug.DrawLine(selfPos, hit.point, Color.white, 1);
                    var dist = hit.distance-sphereRadius;
                    var c = Color.green;
                    if(dist < 0) c = Color.red;
                    Debug.DrawRay(selfPos, dist*((otherPos-selfPos).normalized), c, 1);
                }            
            }
        }

        void Receive(string data)
        {
            Debug.Log($"I am {this.GetInstanceID()}, got data:'{data}'");
        }

        void FixedUpdate()
        {
            if(work)
            {
                Send();
            }
        }

    }
}