// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Utils = DefaultNamespace.Utils;

// namespace VehicleComponents.Actuators
// {
//     public class DronePropeller : MonoBehaviour
//     {
//         [Header("DronePropeller")]
//         public bool reverse = false;
//         public double rpm;
//         public float RPMMax = 1000;
//         public float RPMToForceMultiplier = 5;

//         private Rigidbody parentRigidBody;

//         void Start()
//         {
//             parentRigidBody = GetComponentInParent<Rigidbody>();
//             if (parentRigidBody == null)
//             {
//                 Debug.LogError("Propeller's parent Rigidbody not found!");
//             }
//         }

//         public void SetRpm(double rpm)
//         {
//             this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
//         }

//         void FixedUpdate()
//         {
//             if (parentRigidBody == null) return;

//             float force = (float)(rpm / 1000 * RPMToForceMultiplier);
//             int direction = reverse ? -1 : 1;
//             Vector3 forceVector = direction * force * transform.up;

//             parentRigidBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
//             Debug
//         }
//     }
// }

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Utils = DefaultNamespace.Utils;

// namespace VehicleComponents.Actuators
// {
//     public class DronePropeller : MonoBehaviour
//     {
//         [Header("DronePropeller")]
//         public bool reverse = false;
//         public double rpm;
//         public float RPMMax = 10000;
//         public float RPMToForceMultiplier = 5;

//         private Rigidbody parentRigidBody;

//         void Start()
//         {
//             parentRigidBody = GetComponentInParent<Rigidbody>();
//             if (parentRigidBody == null)
//             {
//                 Debug.LogError("Propeller's parent Rigidbody not found!");
//                 return;
//             }

//             // Initialize RPM to keep the drone afloat
//             InitializeRPMToStayAfloat();
//         }

//         public void SetRpm(double rpm)
//         {
//             this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
//             Debug.Log($"SetRpm called. New rpm value: {this.rpm}");
//         }

//         void FixedUpdate()
//         {
//             if (parentRigidBody == null) return;

//             float force = (float)(rpm / 1000 * RPMToForceMultiplier);
//             int direction = reverse ? -1 : 1;
//             Vector3 forceVector = direction * force * transform.forward;

//             parentRigidBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
//             Debug.Log($"FixedUpdate called. Current rpm value: {rpm}, Force applied: {forceVector}");

//             // Visualize the applied force
//             Debug.DrawRay(transform.position, forceVector, Color.red, 0.1f, false);
//         }

//         private void InitializeRPMToStayAfloat()
//         {
//             // Calculate the required force to counteract gravity
//             float requiredForce = parentRigidBody.mass * Physics.gravity.magnitude;
//             Debug.Log("req force: " + requiredForce );

//             // Calculate the required RPM for each propeller
//             float requiredRPM = (requiredForce / (RPMToForceMultiplier * 4)) * 1000;

//             // Set the initial RPM to each propeller
//             SetRpm(requiredRPM);
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

namespace VehicleComponents.Actuators
{
    public class DronePropeller : MonoBehaviour
    {
        [Header("DronePropeller")]
        public bool reverse = false;
        public double rpm;
        public float RPMMax = 10000;
        public float RPMToForceMultiplier = 5;

        private Rigidbody parentRigidBody;
        private GameObject propellerModel; // Reference to the propeller model for visual rotation

        void Start()
        {
            parentRigidBody = GetComponentInParent<Rigidbody>();
            if (parentRigidBody == null)
            {
                Debug.LogError("Propeller's parent Rigidbody not found!");
                return;
            }

            // Initialize RPM to keep the drone afloat
            InitializeRPMToStayAfloat();

            // Assuming the propeller model is a child of the DronePropeller object
            propellerModel = transform.GetChild(0).gameObject; // Adjust as per your actual hierarchy
        }

        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
            Debug.Log($"SetRpm called. New rpm value: {this.rpm}");
        }

        void FixedUpdate()
        {
            if (parentRigidBody == null) return;

            float force = (float)(rpm / 1000 * RPMToForceMultiplier);
            int direction = reverse ? -1 : 1;
            Vector3 forceVector = direction * force * transform.forward;

            parentRigidBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
            Debug.Log($"FixedUpdate called. Current rpm value: {rpm}, Force applied: {forceVector}");

            // Visualize the applied force
            Debug.DrawRay(transform.position, forceVector, Color.red, 0.1f, false);

            // Rotate the propeller model based on RPM
            RotatePropeller();
        }

        private void RotatePropeller()
        {
            if (propellerModel != null)
            {
                float rotationSpeed = (float)rpm * 360.0f / 60.0f; // RPM to degrees per second
                propellerModel.transform.Rotate(100 * Vector3.forward * Time.deltaTime, Space.Self);
            }
        }

        private void InitializeRPMToStayAfloat()
        {
            // Calculate the required force to counteract gravity
            float requiredForce = parentRigidBody.mass * Physics.gravity.magnitude;
            Debug.Log("Required force to stay afloat: " + requiredForce);

            // Calculate the required RPM for each propeller
            float requiredRPM = (requiredForce / (RPMToForceMultiplier * 4)) * 1000;

            // Set the initial RPM to each propeller
            SetRpm(requiredRPM);
        }
    }
}

