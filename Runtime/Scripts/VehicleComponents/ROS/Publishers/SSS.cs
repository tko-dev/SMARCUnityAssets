using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using RosMessageTypes.Smarc;

using SensorSSS = VehicleComponents.Sensors.SideScanSonar;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorSSS))]
    class SSS: ROSPublisher<SidescanMsg, SensorSSS>
    { 
        protected override void InitializePublication()
        {
            ROSMsg.header.frame_id = sensor.linkName;
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.port_channel = sensor.portBuckets;
            ROSMsg.starboard_channel = sensor.strbBuckets;
        }
    }
}