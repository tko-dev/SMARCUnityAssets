using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

namespace VehicleComponents.Sensors
{
    public class Leak: Sensor
    {
        [Header("Leak")]
        [Tooltip("Manually set this to trigger a leak.")]
        public bool leaked = false;
        public int count = 0;

        public override void UpdateSensor(double deltaTime)
        {
            if(leaked) count++;
        }
    
    }
}