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
        public bool includeAtmosphericPressure;
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

            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName; //from sensor

            if (includeAtmosphericPressure) pressure = 101325.0f;
            else pressure = 0;

            // 1m water = 9806.65 Pa
            if (depth > maxDepth) {
                pressure += maxDepth * 9806.65f;
            }else{
                pressure += depth * 9806.65f;
            }

            ros_msg.fluid_pressure = pressure;

            return true;
        }
    }
}
