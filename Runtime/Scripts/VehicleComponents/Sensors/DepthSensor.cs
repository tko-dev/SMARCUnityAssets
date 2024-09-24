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
        private WaterQueryModel _waterModel;
        private bool headerWritten = false;

        void Start()
        {
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
            depth = 0f;
        }

        public override bool UpdateSensor(double deltaTime)
        {
            float maxRaycastDistance = 30f;  // Adjust based on your needs
            RaycastHit hit;

            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = Vector3.down;

            // Perform raycast downwards from the current position
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRaycastDistance))
            {
                // If raycast hits something, use the hit point's y-coordinate
                Debug.Log("Raycast hit at y: " + hit.point.y);
                depth = -(hit.point.y - transform.position.y);
            }
            else
            {
                // If no hit, fall back to water level calculation
                float waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
                // Debug.Log("y: " + transform.position.y);
                depth = -(waterSurfaceLevel - transform.position.y);
            }

            return true;
        } 
    }
}
