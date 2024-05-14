using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using RosMessageTypes.Sam; // PercentStamped

// Sam's force model
using Force;

namespace VehicleComponents.ROS.Subscribers
{
    public class SAMLCG : ActuatorSubscriber<PercentStampedMsg>
    {
        ISAMControl model;

        void Start()
        {
            var motionModel_go = Utils.FindDeepChildWithTag(transform.root.gameObject, "motion_model");
            if(motionModel_go == null)
            {
                Debug.Log("SAMLCG could not find a motion_model tagged object under root!");
                return;
            }
            model = motionModel_go.GetComponent<ISAMControl>();
        }

        
        public override void UpdateVehicle(bool reset)
        {
            // LCG doesnt reset when theres no ros message
            if(reset) return;
            model.SetBatteryPack(ROSMsg.value);
        }
    }
}

