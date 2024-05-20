using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using SensorIMU = VehicleComponents.Sensors.IMU;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorIMU))]
    class IMU: SensorPublisher<ImuMsg, SensorIMU>
    { 
        [Tooltip("If false, orientation is in ENU in ROS.")]
        public bool useNED = false;
        void Start()
        {
            ROSMsg.header.frame_id = sensor.linkName;
        }

        public override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            if(useNED) ROSMsg.orientation = sensor.orientation.To<NED>();
            else ROSMsg.orientation = sensor.orientation.To<ENU>();
            ROSMsg.angular_velocity = sensor.angularVelocity.To<FLU>();
            ROSMsg.linear_acceleration = sensor.linearAcceleration.To<FLU>();
        }
    }
}