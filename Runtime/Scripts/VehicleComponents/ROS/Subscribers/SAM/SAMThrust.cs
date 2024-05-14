using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using RosMessageTypes.Smarc; // ThrusterRPM

// Sam's force model
using Force;

namespace VehicleComponents.ROS.Subscribers
{
    public class SAMThrust : ActuatorSubscriber<ThrusterRPMMsg>
    {
        ISAMControl model;
        [Header("Thrust control")]
        [Tooltip("Which thruster is this controlling? (1 or 2 usually)")]
        [Range(1,2)]
        public int thrusterNumber = 1;
        
        void Start()
        {
            var motionModel_go = Utils.FindDeepChildWithTag(transform.root.gameObject, "motion_model");
            if(motionModel_go == null)
            {
                Debug.Log("SAMThrust could not find a motion_model tagged object under root!");
                return;
            }
            model = motionModel_go.GetComponent<ISAMControl>();
        }

        void SetRpm(double rpm)
        {
            switch(thrusterNumber)
            {
                case 1: 
                    model.SetRpm1(rpm); 
                    break;
                case 2: 
                    model.SetRpm2(rpm);
                    break;
                default:
                    Debug.Log($"Dumb thurster num? {thrusterNumber}");
                    break;
            }
        }


        public override void UpdateVehicle(bool reset)
        {
            if(reset)
            {
                SetRpm(0);
                return;
            }
            SetRpm(ROSMsg.rpm);
        }
    }
}

