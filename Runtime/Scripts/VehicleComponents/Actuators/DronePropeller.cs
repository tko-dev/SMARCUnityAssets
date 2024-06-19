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
        public float RPMMax = 1000;
        public float RPMToForceMultiplier = 5;

        private Rigidbody parentRigidBody;

        void Start()
        {
            parentRigidBody = GetComponentInParent<Rigidbody>();
            if (parentRigidBody == null)
            {
                Debug.LogError("Propeller's parent Rigidbody not found!");
            }
        }

        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
        }

        void FixedUpdate()
        {
            if (parentRigidBody == null) return;

            float force = (float)(rpm / 1000 * RPMToForceMultiplier);
            int direction = reverse ? -1 : 1;
            Vector3 forceVector = direction * force * transform.up;

            parentRigidBody.AddForceAtPosition(forceVector, transform.position, ForceMode.Force);
        }
    }
}
