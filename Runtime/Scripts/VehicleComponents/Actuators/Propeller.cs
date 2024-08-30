using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using VehicleComponents.ROS.Core;

namespace VehicleComponents.Actuators
{
    public class Propeller: LinkAttachment, IROSPublishable
    {
        [Header("Propeller")]
        public bool reverse = false;
        public double rpm;
        public float RPMMax = 100000;
        public float RPMToForceMultiplier = 0.005f;
        public float NumPropellers = 4f;

        [Header("Drone Propeller")]
        [Tooltip("Tick it for Drone and off for SAM/ROV")]
        public bool HoverDefault = false;
        [Tooltip("should there be a torque")]
        public bool ApplyTorque = false;
        [Tooltip("direction of torque")]
        public bool TorqueUp = false;
        public double DefaultHoverRPM;

        [SerializeField] private ArticulationBody baseLinkArticulationBody;
        private float c_tau_f = 8.004e-4f;
        
        
        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
            //if(hoverdefault) Debug.Log("setting rpm to: " + rpm);
        }
        
        void Start()
        {
            Transform current = transform;
            while (current.parent != null)
            {
                current = current.parent;
                ArticulationBody articulationBody = current.GetComponent<ArticulationBody>();
                if (articulationBody != null && articulationBody.name == "base_link")
                {
                   // Debug.Log("base_link articulation body found: " + articulationBody);
                    baseLinkArticulationBody = articulationBody;
                }
            }
            if(HoverDefault) InitializeRPMToStayAfloat();
        }

        void FixedUpdate()
        {
            var r = (float)rpm * RPMToForceMultiplier;
            // if(HoverDefault) Debug.Log("the value of 4xr is: " + r*4 );

            // Visualize the applied force
            
            int direction = reverse? -1 : 1;
            //parentArticulationBody.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*(float)rpm);
            
            parentArticulationBody.AddForceAtPosition((float)r * parentArticulationBody.transform.forward,
                                                   parentArticulationBody.transform.position,
                                                   ForceMode.Force);
            // //manual torqueaddition
            if(ApplyTorque)   
            {
                int torque_sign = TorqueUp ? 1 : -1;
                float torque = torque_sign * c_tau_f * (float)r;
                Vector3 torqueVector = torque * transform.forward;
                parentArticulationBody.AddTorque(torqueVector, ForceMode.Force);
            }
        }

        private void InitializeRPMToStayAfloat()
        {
            // Calculate the required force to counteract gravity
            float requiredForce = baseLinkArticulationBody.mass * Physics.gravity.magnitude;
            // Debug.Log("Required force to stay afloat: " + requiredForce);

            // Calculate the required RPM for each propeller
            float requiredForcePerProp = requiredForce/NumPropellers;
            float requiredRPM = requiredForcePerProp / RPMToForceMultiplier;
            DefaultHoverRPM = requiredRPM;

            // Set the initial RPM to each propeller
            SetRpm(requiredRPM);
        }

        public bool HasNewData()
        {
            return true;
        }
        
    }
}