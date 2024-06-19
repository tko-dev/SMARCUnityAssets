using UnityEngine;
using RosMessageTypes.Smarc;
using Unity.Robotics.Core; // Clock

using DronePropeller = VehicleComponents.Actuators.DronePropeller;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(DronePropeller))]
    public class DronePropellerFeedback : ActuatorPublisher<ThrusterFeedbackMsg>
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

        public override void UpdateMessage()
        {
            if (propeller == null) return;

            ROSMsg.rpm.rpm = (int)propeller.rpm;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}
