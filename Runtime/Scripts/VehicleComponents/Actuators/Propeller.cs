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
        public float RPMToForceMultiplier = 5;
        public bool hoverdefault = true;
        [SerializeField] private ArticulationBody baseLinkArticulationBody;
        
        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
            if(hoverdefault) Debug.Log("setting rpm to: " + rpm);
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
                Debug.Log("base_link articulation body found: " + articulationBody);
                baseLinkArticulationBody = articulationBody;
            }
        }
        if(hoverdefault) InitializeRPMToStayAfloat();
        }

        void FixedUpdate()
        {
            var r = rpm / 1000 * RPMToForceMultiplier;
            if(hoverdefault) Debug.Log("the value of r is: " + r );
      
            int direction = reverse? -1 : 1;
            parentArticulationBody.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*(float)rpm);
            if(hoverdefault) Debug.Log("the torque on propeller is" + parentArticulationBody.GetAccumulatedTorque());
            parentArticulationBody.AddForceAtPosition((float)r * parentArticulationBody.transform.forward,
                                                   parentArticulationBody.transform.position,
                                                   ForceMode.Force);
        }

        private void InitializeRPMToStayAfloat()
        {
            // Calculate the required force to counteract gravity
            float requiredForce = (baseLinkArticulationBody.mass+4) * Physics.gravity.magnitude;
            Debug.Log("Required force to stay afloat: " + requiredForce);

            // Calculate the required RPM for each propeller
            float requiredRPM = (requiredForce / (RPMToForceMultiplier * 4)) * 1000;

            // Set the initial RPM to each propeller
            SetRpm(requiredRPM);
        }
        
        //TODO: Ensure RPM feedback in???
    }
}