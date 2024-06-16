using UnityEngine;
using RosMessageTypes.Nav;
using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using SensorIMU = VehicleComponents.Sensors.IMU;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorIMU))]
    class Odometry: SensorPublisher<OdometryMsg, SensorIMU>
    { 
        [Tooltip("If false, orientation is in ENU in ROS.")]
        public bool useNED = false;

        void Start()
        {
            ROSMsg.header.frame_id = "map_gt";
            ROSMsg.child_frame_id = sensor.linkName;
        }

        public override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);

            if(useNED) 
            {
                ROSMsg.pose.pose.orientation = sensor.orientation.To<NED>();
                ROSMsg.pose.pose.position = sensor.transform.position.To<NED>();
            }
            else
            {
                ROSMsg.pose.pose.orientation = sensor.orientation.To<ENU>();
                ROSMsg.pose.pose.position = sensor.transform.position.To<ENU>();
            } 

            ROSMsg.twist.twist.linear = sensor.localVelocity.To<FLU>();
            ROSMsg.twist.twist.angular = sensor.angularVelocity.To<FLU>();
        }
    }
}