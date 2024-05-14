using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

namespace VehicleComponents.Actuators
{
    public class Propeller: LinkAttachment
    {
        [Header("Propeller")]
        public bool reverse = false;
        public double rpm;
        public float RPMMax = 1000;
        public float RPMToForceMultiplier = 5;
        
        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
        }

        void FixedUpdate()
        {
            var r = rpm / 1000 * RPMToForceMultiplier;
      
            int direction = reverse? -1 : 1;
            parentArticulationBody.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*(float)rpm);
            parentArticulationBody.AddForceAtPosition((float)r * parentArticulationBody.transform.forward,
                                                   parentArticulationBody.transform.position,
                                                   ForceMode.Force);
        }
    }
}