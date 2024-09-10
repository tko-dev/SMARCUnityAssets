using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // For file operations
using Utils = DefaultNamespace.Utils;
using DefaultNamespace.Water;

namespace VehicleComponents.Sensors
{
    public class DepthSensor : Sensor
    {
        [Header("Depth-Sensor")]
        public float depth;
        public Vector3 dronePosition;
        private WaterQueryModel _waterModel;
        private string filePath;
        private bool headerWritten = false;

        void Start()
        {
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
            depth = 0f
        }

        public override bool UpdateSensor(double deltaTime)
        {
            var waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
            depth = waterSurfaceLevel - transform.position.y;
        } 
    }
}
