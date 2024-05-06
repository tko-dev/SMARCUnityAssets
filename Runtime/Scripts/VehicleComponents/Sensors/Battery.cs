using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

namespace VehicleComponents.Sensors
{
    public class Battery: Sensor
    {
        [Header("Battery")]
        public float dischargePercentPerMinute = 1;
        public float currentPercent = 95f;
        public float maxVoltage = 12.5f;
        public float currentVoltage = 12.5f;

        public override bool UpdateSensor(double deltaTime)
        {
            currentPercent -= (float) ((deltaTime/60) * dischargePercentPerMinute);
            if(currentPercent < 0f) currentPercent = 0f;
            currentVoltage = maxVoltage * currentPercent / 100f;
            return true;
        }
    
    }

}