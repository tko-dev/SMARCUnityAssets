using System;
using System.Collections.Generic;
using UnityEngine;

namespace Force
{
    public class SAMUnityForceModel : MonoBehaviour, IForceModel, ISAMControl
    {
        public float VBSFixedPoint = 0.921f;
        public float VBSMaxDeviation = 0.009f;
        public float ThrusterAngleRange = 0.1f;
        public float rpm_multiplier = 7200;

        private Rigidbody rigidBody;

        public double lcg { get; set; }
        public double vbs { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get ; set; }
        public double rpm2 { get; set; }

        public Vector3 ThrusterPosition = new(0, 0, -0.73f);
        private List<ForcePoint> points;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            points = new List<ForcePoint>(GetComponentsInChildren<ForcePoint>());
            if (rigidBody == null) rigidBody = transform.parent.GetComponent<Rigidbody>();
        }

        public void SetRpm(double rpm1, double rpm2)
        {
            this.rpm1 = rpm1;
            this.rpm2 = rpm2;
        }

        public void SetRudderAngle(float dr)
        {
            d_rudder = 0 + Mathf.Lerp(-ThrusterAngleRange, ThrusterAngleRange, (dr + 1) / 2);
        }

        public void SetElevatorAngle(float de)
        {
            d_aileron = 0 + Mathf.Lerp(-ThrusterAngleRange, ThrusterAngleRange, (de + 1) / 2);
        }

        public void SetBatteryPack(double lcg)
        {
            this.lcg = lcg;
        }

        public void SetWaterPump(double vbs)
        {
            this.vbs = VBSFixedPoint + Mathf.Lerp(-VBSMaxDeviation, VBSMaxDeviation, (float) (vbs + 1) / 2);
            points.ForEach(point => point.displacementAmount = (float) this.vbs);
        }

        private void FixedUpdate()
        {
            var r1 = rpm1 * rpm_multiplier / 1000;
            var r2 = rpm2 * rpm_multiplier / 1000;

            var rotorPositionGlobalFrame = transform.position + transform.TransformDirection(ThrusterPosition);

            rigidBody.AddForceAtPosition(ThrustVectorForPropller(r1), rotorPositionGlobalFrame, ForceMode.Force);
            rigidBody.AddForceAtPosition(ThrustVectorForPropller(r2), rotorPositionGlobalFrame, ForceMode.Force);
            rigidBody.AddTorque(Vector3.forward * -(float) r1 / 2, ForceMode.Force);
            rigidBody.AddTorque(Vector3.forward * (float) r2 / 2, ForceMode.Force);
        }

        public Vector3 ThrustVectorForPropller(double r)
        {
            var localDirection = Quaternion.Euler(d_aileron * Mathf.Rad2Deg, -d_rudder * Mathf.Rad2Deg, 0) *
                                 transform.InverseTransformDirection(transform.forward);
            var localScaled = localDirection * (float) r / 2; //Divide by 2 since two forces
            var globalDirection = transform.TransformDirection(localScaled);
            return globalDirection;
        }

        public Vector3 GetTorqueDamping()
        {
            return Vector3.zero; // Uses rigidbody angular drag
        }

        public Vector3 GetForceDamping()
        {
            return Vector3.zero; // Uses rigidbody drag
        }
    }
}