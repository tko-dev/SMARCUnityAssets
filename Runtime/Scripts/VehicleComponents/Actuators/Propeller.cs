// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Utils = DefaultNamespace.Utils;

// namespace VehicleComponents.Actuators
// {
//     public class Propeller: LinkAttachment
//     {
//         [Header("Propeller")]
//         public bool reverse = false;
//         public double rpm;
//         public float RPMMax = 1000;
//         public float RPMToForceMultiplier = 5;
        
//         public void SetRpm(double rpm)
//         {
//             this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
//         }

//         void FixedUpdate()
//         {
//             var r = rpm / 1000 * RPMToForceMultiplier;
      
//             int direction = reverse? -1 : 1;
//             parentArticulationBody.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*(float)rpm);
//             parentArticulationBody.AddForceAtPosition((float)r * parentArticulationBody.transform.forward,
//                                                    parentArticulationBody.transform.position,
//                                                    ForceMode.Force);
//         }
        
//         //TODO: Ensure RPM feedback in???
//     }
// }

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
        public float RPMMax = 100000;
        private float RPMToForceMultiplier = 5f;
        public float NumPropellers = 4f;

        [Header("Drone Propeller")]
        [Tooltip("Tick it for Drone and off for SAM/ROV")]
        public bool hoverdefault = true;
        [Tooltip("should there be a torque")]
        public bool torque_req = true;
        [Tooltip("direction of torque")]
        public bool torque_up = true;
        public double DefaultHoveringRPM ;

        [SerializeField] private ArticulationBody baseLinkArticulationBody;
        private float c_tau_f = 8.004e-2f;
        
        
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
            if(hoverdefault) InitializeRPMToStayAfloat();
        }

        void FixedUpdate()
        {
            var r = (float)rpm * RPMToForceMultiplier;
            if(hoverdefault) Debug.Log("the value of 4xr is: " + r*4 );

            // Visualize the applied force
            
            int direction = reverse? -1 : 1;
            parentArticulationBody.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*(float)rpm);
            
            parentArticulationBody.AddForceAtPosition((float)r * parentArticulationBody.transform.forward,
                                                   parentArticulationBody.transform.position,
                                                   ForceMode.Force);
            //manual torqueaddition
            if  (torque_req)   
            {
                int torque_sign = torque_up ? 1 : -1;
                float torque = torque_sign * c_tau_f * (float)r;
                Vector3 torqueVector = torque * transform.forward;
                parentArticulationBody.AddTorque(torqueVector, ForceMode.Force);
            }
        }

        private void InitializeRPMToStayAfloat()
        {
            // Calculate the required force to counteract gravity
            float requiredForce = (baseLinkArticulationBody.mass) * Physics.gravity.magnitude;
            Debug.Log("Required force to stay afloat: " + requiredForce);

            // Calculate the required RPM for each propeller
            float requiredForcePerProp = requiredForce/NumPropellers;
            float requiredRPM = requiredForcePerProp/RPMToForceMultiplier;
            this.DefaultHoveringRPM = requiredRPM;

            // Set the initial RPM to each propeller
            SetRpm(requiredRPM);
        }
        
        //TODO: Ensure RPM feedback in???
    }
}