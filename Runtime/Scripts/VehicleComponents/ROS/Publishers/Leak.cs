using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using RosMessageTypes.Smarc;

using SensorLeak = VehicleComponents.Sensors.Leak;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorLeak))]
    class Leak: ROSPublisher<LeakMsg, SensorLeak>
    { 
        
        public override void UpdateMessage()
        {
            if(sensor.leaked)
            {
                ROSMsg.value = true;
                ROSMsg.leak_counter = sensor.count;
            }
            else ROSMsg.value = false;
        }
    }
}