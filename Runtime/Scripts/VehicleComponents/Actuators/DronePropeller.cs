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
        private float c_tau_f = 8.004e-4f;
        public bool torque_up = true;
        private GameObject propellerModel; // Reference to the propeller model for visual rotation
        [SerializeField] private ArticulationBody baseLinkArticulationBody; // Reference to the base_link ArticulationBody

        void Start()
        {
            // Ensure the parent articulation body is set up by the base class
            if (parentArticulationBody == null)
            {
                Debug.LogError("Propeller's parent ArticulationBody not found!");
                return;
            }

            // Find the base_link ArticulationBody if it's not set
            if (baseLinkArticulationBody == null)
            {
                baseLinkArticulationBody = FindBaseLinkArticulationBody();
                if (baseLinkArticulationBody == null)
                {
                    Debug.LogError("base_link ArticulationBody not found!");
                    return;
                }
            }

            // Initialize RPM to keep the drone afloat
            InitializeRPMToStayAfloat();

            // Assuming the propeller model is a child of the DronePropeller object
            propellerModel = this.gameObject;
            Debug.Log("gameobject is: " + parentArticulationBody);
        }

        private ArticulationBody FindBaseLinkArticulationBody()
        {
            Transform current = transform;
            while (current.parent != null)
            {
                current = current.parent;
                ArticulationBody articulationBody = current.GetComponent<ArticulationBody>();
                if (articulationBody != null && articulationBody.name == "base_link")
                {
                    Debug.Log("base_link articulation body found: " + articulationBody);
                    return articulationBody;
                }
            }
            return null;
        }

        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
            Debug.Log($"SetRpm called. New rpm value: {this.rpm}");
        }

        void FixedUpdate()
        {
            if (parentArticulationBody == null) return;
            // if (propellerModel != null)
            // {
            //     propellerModel.transform.Rotate(Vector3.forward* 100 * Time.deltaTime, Space.Self);
            // }
            float force = (float)(rpm / 1000 * RPMToForceMultiplier);
            int direction = reverse ? -1 : 1;
            Vector3 forceVector = direction * force * transform.forward;

            parentArticulationBody.AddForceAtPosition(forceVector, parentArticulationBody.transform.position, ForceMode.Force);
            Debug.Log($"FixedUpdate called. Current rpm value: {rpm}, Force applied: {forceVector}");

            // Visualize the applied force
            Debug.DrawRay(parentArticulationBody.transform.position, ((float)(rpm-2129)/10)*transform.forward, Color.red);

            // Apply torque to simulate the propeller's rotation
            int torque_sign = torque_up ? 1 : -1;
            float torque = torque_sign * c_tau_f * force;
            Vector3 torqueVector = torque * transform.forward;
            parentArticulationBody.AddTorque(torqueVector, ForceMode.Force);

            // Rotate the propeller model based on RPM
            RotatePropeller(torque_sign);
        }

        private void RotatePropeller(int direction)
        {
            if (propellerModel != null)
            {
                float rotationSpeed = (float)rpm * 360.0f / 60.0f; // RPM to degrees per second
                Debug.Log("trying to rotate at: " + rotationSpeed * 1 * Time.deltaTime);
                // Visualize the axis of rotation
                //Debug.DrawRay(transform.position, transform.forward, Color.green, 0.1f, false);
                propellerModel.transform.Rotate(direction * rotationSpeed * 20/2000 * Time.deltaTime * Vector3.forward, Space.Self);
            }
        }

        private void InitializeRPMToStayAfloat()
        {
            // Calculate the required force to counteract gravity
            float requiredForce = baseLinkArticulationBody.mass * Physics.gravity.magnitude;
            Debug.Log("Required force to stay afloat: " + requiredForce);

            // Calculate the required RPM for each propeller
            float requiredRPM = (requiredForce / (RPMToForceMultiplier * 4)) * 1000;

            // Set the initial RPM to each propeller
            SetRpm(0);
        }
    }
}
