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

        ArticulationBody thruster;

        void Start()
        {
            thruster = transform.parent.GetComponent<ArticulationBody>();
            if(thruster == null)
            {
                Debug.Log("Propeller could not find an Arti. Body in the parent.");
                return;
            }
        }
        
        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
        }

        void FixedUpdate()
        {
            var r = rpm / 1000 * RPMToForceMultiplier;
      
            int direction = reverse? -1 : 1;
            thruster.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*(float)rpm);
            thruster.AddForceAtPosition((float)r * thruster.transform.forward,
                                                   thruster.transform.position,
                                                   ForceMode.Force);
        }
    }
}