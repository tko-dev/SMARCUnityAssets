using UnityEngine;
using RosMessageTypes.Sam;
using Unity.Robotics.Core; // Clock

using SensorSamAct = VehicleComponents.Sensors.SAMActuators;

namespace VehicleComponents.ROS.Publishers.SAM
{
    [RequireComponent(typeof(SensorSamAct))]
    public class VBSFeedback: SensorPublisher<PercentStampedMsg, SensorSamAct>
    {

        public override void UpdateMessage()
        {
            ROSMsg.value = (float)sensor.vbs;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}