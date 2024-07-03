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
        
        void Submit()
        {
            double now = Clock.NowTimeInSeconds;
            foreach(Transceiver tc in allTransceivers)
            {
                var id = tc.GetInstanceID();
                if(id == this.GetInstanceID()) continue;
                
                Debug.Log($"Ping other tc:{id}");
                tc.Receive($"Ping from {this.GetInstanceID()}");
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
                Submit();
            }
        }

    }
}