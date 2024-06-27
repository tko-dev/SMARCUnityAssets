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

//         private Articulationbody parentArticulationBody;

//         void Start()
//         {
//             parentArticulationBody = GetComponentInParent<Articulationbody>();
//             if (parentArticulationBody == null)
//             {
//                 Debug.LogError("Propeller's parent Articulationbody not found!");
//             }
//         }

//         public void SetRpm(double rpm)
//         {
//             this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
//         }

//         void FixedUpdate()
//         {
//             if (parentArticulationBody == null) return;

//             float force = (float)(rpm / 1000 * RPMToForceMultiplier);
//             int direction = reverse ? -1 : 1;
//             Vector3 forceVector = direction * force * transform.up;

//             parentArticulationBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
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

//         private Articulationbody parentArticulationBody;

//         void Start()
//         {
//             parentArticulationBody = GetComponentInParent<Articulationbody>();
//             if (parentArticulationBody == null)
//             {
//                 Debug.LogError("Propeller's parent Articulationbody not found!");
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
//             if (parentArticulationBody == null) return;

//             float force = (float)(rpm / 1000 * RPMToForceMultiplier);
//             int direction = reverse ? -1 : 1;
//             Vector3 forceVector = direction * force * transform.forward;

//             parentArticulationBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
//             Debug.Log($"FixedUpdate called. Current rpm value: {rpm}, Force applied: {forceVector}");

//             // Visualize the applied force
//             Debug.DrawRay(transform.position, forceVector, Color.red, 0.1f, false);
//         }

//         private void InitializeRPMToStayAfloat()
//         {
//             // Calculate the required force to counteract gravity
//             float requiredForce = parentArticulationBody.mass * Physics.gravity.magnitude;
//             Debug.Log("req force: " + requiredForce );

//             // Calculate the required RPM for each propeller
//             float requiredRPM = (requiredForce / (RPMToForceMultiplier * 4)) * 1000;

//             // Set the initial RPM to each propeller
//             SetRpm(requiredRPM);
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
//         public float RPMMax = 100000;
//         public float RPMToForceMultiplier = 5;

//         private ArticulationBody parentArticulationBody;
//         private GameObject propellerModel; // Reference to the propeller model for visual rotation

//         void Start()
//         {
//             parentArticulationBody = GetComponentInParent<ArticulationBody>();
//             if (parentArticulationBody == null)
//             {
//                 Debug.LogError("Propeller's parent ArticulationBody not found!");
//                 return;
//             }

//             // Initialize RPM to keep the drone afloat
//             InitializeRPMToStayAfloat();

//             // Assuming the propeller model is a child of the DronePropeller object
//             propellerModel = transform.GetChild(0).gameObject; // Adjust as per your actual hierarchy
//         }

//         public void SetRpm(double rpm)
//         {
//             this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
//             Debug.Log($"SetRpm called. New rpm value: {this.rpm}");
//         }

//         void FixedUpdate()
//         {
//             if (parentArticulationBody == null) return;

//             float force = (float)(rpm / 1000 * RPMToForceMultiplier);
//             int direction = reverse ? -1 : 1;
//             Vector3 forceVector = direction * force * transform.forward;

//             parentArticulationBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
//             Debug.Log($"FixedUpdate called. Current rpm value: {rpm}, Force applied: {forceVector}");

//             // Visualize the applied force
//             Debug.DrawRay(transform.position, forceVector, Color.red, 0.1f, false);

//             // Rotate the propeller model based on RPM
//             RotatePropeller();
//         }

//         private void RotatePropeller()
//         {
//             if (propellerModel != null)
//             {
//                 float rotationSpeed = (float)rpm * 360.0f / 60.0f; // RPM to degrees per second
//                 propellerModel.transform.Rotate(100 * Vector3.forward * Time.deltaTime, Space.Self);
//             }
//         }

//         private void InitializeRPMToStayAfloat()
//         {
//             // Calculate the required force to counteract gravity
//             float requiredForce = parentArticulationBody.mass * Physics.gravity.magnitude;
//             Debug.Log("Required force to stay afloat: " + requiredForce);

//             // Calculate the required RPM for each propeller
//             float requiredRPM = (requiredForce / (RPMToForceMultiplier * 4)) * 1000;

//             // Set the initial RPM to each propeller
//             SetRpm(requiredRPM);
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
//         public float RPMMax = 100000;
//         public float RPMToForceMultiplier = 5;
//         public float c_tau_f = 8.004e-4f;
//         public bool torque_up = true;
//         private ArticulationBody parentArticulationBody;
//         private GameObject propellerModel; // Reference to the propeller model for visual rotation

//         void Start()
//         {
//             parentArticulationBody = GetParentArticulationBody();
//             if (parentArticulationBody == null)
//             {
//                 Debug.LogError("Propeller's parent ArticulationBody not found!");
//                 return;
//             }

//             // Initialize RPM to keep the drone afloat
//             InitializeRPMToStayAfloat();

//             // Assuming the propeller model is a child of the DronePropeller object
//             // propellerModel = transform.GetChild(0).gameObject; // Adjust as per your actual hierarchy
//             propellerModel = this.gameObject;
//             Debug.Log("gameobject is: " + propellerModel );
//         }

//         private ArticulationBody GetParentArticulationBody()
//         {
//             Transform current = transform;
//             while (current.parent != null)
//             {
//                 current = current.parent;
//                 ArticulationBody articulationBody = current.GetComponent<ArticulationBody>();
//                 if (articulationBody != null)
//                 {
//                     Debug.Log("parent articulation body" + articulationBody);
//                     return articulationBody;
//                 }
//             }
//             return null;
//         }

//         public void SetRpm(double rpm)
//         {
//             this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
//             Debug.Log($"SetRpm called. New rpm value: {this.rpm}");
//         }

//         void FixedUpdate()
//         {
//             if (parentArticulationBody == null) return;
//             if (propellerModel != null)
//             {
//                 propellerModel.transform.Rotate(propellerModel.transform.forward * 100 * Time.deltaTime, Space.Self);
//             }
//             float force = (float)(rpm / 1000 * RPMToForceMultiplier);
//             int direction = reverse ? -1 : 1;
//             Vector3 forceVector = direction * force * transform.forward;

//             parentArticulationBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
//             Debug.Log($"FixedUpdate called. Current rpm value: {rpm}, Force applied: {forceVector}");

//             // Visualize the applied force
//             Debug.DrawRay(transform.position, forceVector, Color.red, 0.1f, false);

//             // Apply torque to simulate the propeller's rotation
//             int torque_sign = torque_up ? 1 : -1;
//             float torque = torque_sign * c_tau_f * force;
//             Vector3 torqueVector = torque * transform.forward;
//             parentArticulationBody.AddTorque(torqueVector, ForceMode.Force);

//             // Rotate the propeller model based on RPM
//             //RotatePropeller();
//         }

//         private void RotatePropeller()
//         {
//             if (propellerModel != null)
//             {
//                 float rotationSpeed = (float)rpm * 360.0f / 60.0f; // RPM to degrees per second
//                 Debug.Log("trying to rotate at: " + Time.deltaTime);
//                 propellerModel.transform.Rotate(rotationSpeed * Time.deltaTime * transform.forward, Space.Self);
//             }
//         }

//         private void InitializeRPMToStayAfloat()
//         {
//             // Calculate the required force to counteract gravity
//             float requiredForce = parentArticulationBody.mass * Physics.gravity.magnitude;
//             //Debug.Log("Required force to stay afloat: " + requiredForce);

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
    public class DronePropeller : LinkAttachment
    {
        [Header("DronePropeller")]
        public bool reverse = false;
        public double rpm;
        public float RPMMax = 100000;
        public float RPMToForceMultiplier = 5;
        public float c_tau_f = 8.004e-4f;
        public bool torque_up = true;
        private GameObject propellerModel; // Reference to the propeller model for visual rotation

        void Start()
        {
            // Ensure the parent articulation body is set up by the base class
            if (parentArticulationBody == null)
            {
                Debug.LogError("Propeller's parent ArticulationBody not found!");
                return;
            }

            // Initialize RPM to keep the drone afloat
            InitializeRPMToStayAfloat();

            // Assuming the propeller model is a child of the DronePropeller object
            // propellerModel = transform.GetChild(0).gameObject; // Adjust as per your actual hierarchy
            propellerModel = this.gameObject;
            Debug.Log("gameobject is: " + propellerModel);
        }

        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
            Debug.Log($"SetRpm called. New rpm value: {this.rpm}");
        }

        void FixedUpdate()
        {
            if (parentArticulationBody == null) return;
            if (propellerModel != null)
            {
                propellerModel.transform.Rotate(propellerModel.transform.forward * 100 * Time.deltaTime, Space.Self);
            }
            float force = (float)(rpm / 1000 * RPMToForceMultiplier);
            int direction = reverse ? -1 : 1;
            Vector3 forceVector = direction * force * transform.forward;

            parentArticulationBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
            Debug.Log($"FixedUpdate called. Current rpm value: {rpm}, Force applied: {forceVector}");

            // Visualize the applied force
            Debug.DrawRay(transform.position, forceVector, Color.red, 0.1f, false);

            // Apply torque to simulate the propeller's rotation
            int torque_sign = torque_up ? 1 : -1;
            float torque = torque_sign * c_tau_f * force;
            Vector3 torqueVector = torque * transform.forward;
            parentArticulationBody.AddTorque(torqueVector, ForceMode.Force);

            // Rotate the propeller model based on RPM
            //RotatePropeller();
        }

        private void RotatePropeller()
        {
            if (propellerModel != null)
            {
                float rotationSpeed = (float)rpm * 360.0f / 60.0f; // RPM to degrees per second
                Debug.Log("trying to rotate at: " + Time.deltaTime);
                propellerModel.transform.Rotate(rotationSpeed * Time.deltaTime * transform.forward, Space.Self);
            }
        }

        private void InitializeRPMToStayAfloat()
        {
            // Calculate the required force to counteract gravity
            float requiredForce = parentArticulationBody.mass * Physics.gravity.magnitude;
            //Debug.Log("Required force to stay afloat: " + requiredForce);

            // Calculate the required RPM for each propeller
            float requiredRPM = (requiredForce / (RPMToForceMultiplier * 4)) * 1000;

            // Set the initial RPM to each propeller
            SetRpm(requiredRPM);
        }
    }
}
