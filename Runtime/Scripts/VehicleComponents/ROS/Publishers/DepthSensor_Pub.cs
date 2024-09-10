using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

using SensorDepth = VehicleComponents.Sensors.DepthSensor;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorDepth))]
    class DepthSensor_Pub: ROSPublisher<FluidPressureMsg, SensorDepth>
    { 
        protected override void InitializePublication()
        {
            ROSMsg.header.frame_id = sensor.linkName;
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.fluid_pressure = sensor.depth;
        }
    }
}