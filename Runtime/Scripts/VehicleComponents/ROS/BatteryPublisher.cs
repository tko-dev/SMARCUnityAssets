using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

using Battery = VehicleComponents.Sensors.Battery;

namespace VehicleComponents.ROS
{
    [RequireComponent(typeof(Battery))]
    class BatteryPublisher: SensorPublisher<BatteryStateMsg, Battery>
    {
        public override void UpdateMessage()
        {
            ROSMsg.voltage = sensor.currentVoltage;
            ROSMsg.percentage = sensor.currentPercent;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}