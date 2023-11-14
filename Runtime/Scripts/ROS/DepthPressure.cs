using DefaultNamespace.Water;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    public class DepthPressure : Sensor<FluidPressureMsg>
    {

        [Header("Depth-Pressure")]
        public float maxDepth;
        float pressure;
        private WaterQueryModel _waterModel;

        void Start()
        {
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        }

        public override bool UpdateSensor(double deltaTime)
        {
            var waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
            float depth = waterSurfaceLevel - transform.position.y;

            // Out of water, no data.
            if (depth < 0) return false;
            // Out of max depth, no data.
            if (depth > maxDepth) return false;

            // 1m water = 9806.65 Pa
            pressure = depth * 9806.65f;
            ros_msg.fluid_pressure = pressure;

            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName; //from sensor
            return true;
        }
    }
}
