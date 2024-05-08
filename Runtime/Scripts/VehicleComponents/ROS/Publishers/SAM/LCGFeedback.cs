using UnityEngine;
using RosMessageTypes.Sam;
using Unity.Robotics.Core; // Clock

using SensorSamAct = VehicleComponents.Sensors.SAMActuators;

namespace VehicleComponents.ROS.Publishers.SAM
{
    [RequireComponent(typeof(SensorSamAct))]
    public class LCGFeedback: SensorPublisher<PercentStampedMsg, SensorSamAct>
    {

        public override void UpdateMessage()
        {
            ROSMsg.value = (float)sensor.lcg;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}