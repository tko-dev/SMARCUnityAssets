using UnityEngine;
using RosMessageTypes.Smarc;
using Unity.Robotics.Core; // Clock

using SensorSamAct = VehicleComponents.Sensors.SAMActuators;

namespace VehicleComponents.ROS.Publishers.SAM
{
    [RequireComponent(typeof(SensorSamAct))]
    public class ThrusterFeedback: SensorPublisher<ThrusterFeedbackMsg, SensorSamAct>
    {
        [Header("Thruster FB")]
        [Tooltip("Set the number (1 or 2) of the thruster the feedback will come from")]
        [Range(1,2)]
        public int thrusterNum = 1;

        public override void UpdateMessage()
        {
            var rpm = thrusterNum==1 ? sensor.rpm1 : sensor.rpm2;
            ROSMsg.rpm.rpm = (int)rpm;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}