// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Utils = DefaultNamespace.Utils;

// using RosMessageTypes.Smarc; // ThrusterRPMMsg
// using DronePropeller = VehicleComponents.Actuators.DronePropeller;

// namespace VehicleComponents.ROS.Subscribers
// {
//     [RequireComponent(typeof(DronePropeller))]
//     public class DronePropellerCommand : ActuatorSubscriber<ThrusterRPMMsg>
//     {
//         DronePropeller propeller;

//         void Start()
//         {
//             propeller = GetComponent<DronePropeller>();
//             if (propeller == null)
//             {
//                 Debug.Log("No drone propeller found!");
//                 return;
//             }
//         }

//         public override void UpdateVehicle(bool reset)
//         {
//             if (propeller == null) return;
//             reset = false;
//             if (reset)
//             {
//                 propeller.SetRpm(0);
//                 return;
//             }
//             if 
//             propeller.SetRpm(ROSMsg.rpm);
//         }
//     }
// }
using UnityEngine;
using RosMessageTypes.Smarc;
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
                Debug.LogError("No drone propeller component found on GameObject or parent!");
            }
        }

        public override void UpdateVehicle(bool reset)
        {
            if (propeller == null)
            {
                Debug.LogWarning("Drone propeller not initialized.");
                return;
            }
            // reset = false;
            // // If reset is true, set RPM to 0 and return immediately
            // if (reset)
            // {
            //     propeller.SetRpm(0);
            //     return;
            // }

            // Only set RPM if a valid ROS message is received
            if (ROSMsg.rpm!= 0)
            {
                //Debug.Log("Setting to : " + ROSMsg.rpm);
                propeller.SetRpm(ROSMsg.rpm);
            }
        }
    }
}



