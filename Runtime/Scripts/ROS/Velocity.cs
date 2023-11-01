using DefaultNamespace.Water;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.Core; // Clock
using RosMessageTypes.Geometry;

namespace DefaultNamespace
{
    public class Velocity : Sensor<TwistStampedMsg>
    {
        private WaterQueryModel _waterModel;
        // This is different from the DVL in the sense that it reports the
        // velocity of the gps link so i assume it only works
        // when the gps antenna is out of the water


        void Start()
        {
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        }

        public override bool UpdateSensor(double deltaTime)
        {

            float waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);

            // It aint, no fix, no velocity.
            if (transform.position.y < waterSurfaceLevel) return false;

            // On surface, we good.
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName; //from sensor
            ros_msg.twist.linear = rb.transform.InverseTransformVector(rb.velocity).To<FLU>();
            ros_msg.twist.angular = rb.angularVelocity.To<FLU>();
            return true;

        }
    }
}
