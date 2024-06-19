using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using RosMessageTypes.Smarc; // ThrusterRPMMsg
using DronePropeller = VehicleComponents.Actuators.DronePropeller;

namespace VehicleComponents.ROS.Subscribers
{
    [RequireComponent(typeof(DronePropeller))]
    public class DronePropellerCommand : ActuatorSubscriber<ThrusterRPMMsg>
    {
        DronePropeller propeller;

        void Start()
        {
            propeller = GetComponent<DronePropeller>();
            if (propeller == null)
            {
                Debug.Log("No drone propeller found!");
                return;
            }
        }

        public override void UpdateVehicle(bool reset)
        {
            if (propeller == null) return;
            if (reset)
            {
                propeller.SetRpm(0);
                return;
            }
            propeller.SetRpm(ROSMsg.rpm);
        }
    }
}


