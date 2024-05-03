using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using DefaultNamespace.Water; // WaterQueryModel

namespace VehicleComponents.Sensors
{
    public class DepthPressure: Sensor
    {
        [Header("Depth-Pressure")]
        public float maxDepth;
        public bool includeAtmosphericPressure;
        public float pressure;
        private WaterQueryModel _waterModel;

        void Start()
        {
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        }


        public override void UpdateSensor(double deltaTime)
        {
            var waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
            float depth = waterSurfaceLevel - transform.position.y;
            if (includeAtmosphericPressure) pressure = 101325.0f;
            else pressure = 0;

            // 1m water = 9806.65 Pa
            if (depth > maxDepth) {
                pressure += maxDepth * 9806.65f;
            }else{
                pressure += depth * 9806.65f;
            }
        }
    }
}