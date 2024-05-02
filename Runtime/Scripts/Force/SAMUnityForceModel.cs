using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Force
{
    public class SAMUnityForceModel : MonoBehaviour, IForceModel, ISAMControl
    {
        private Rigidbody rigidBody;

        public double lcg { get; set; }
        public double vbs { get; set; }

        public SAMParameters parameters { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get; set; }
        public double rpm2 { get; set; }

        public Vector3 ThrusterPosition = new(0, 0, -0.73f);
        private List<ForcePoint> points;

        public SAMUnityForceModel()
        {
            parameters = new SAMParameters();
        }

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            points = new List<ForcePoint>(GetComponentsInChildren<ForcePoint>());
            if (rigidBody == null) rigidBody = transform.parent.GetComponent<Rigidbody>();
        }

        public void SetRpm(double rpm1, double rpm2)
        {
            this.rpm1 = Mathf.Clamp((float)rpm1, -parameters.RPMMax, parameters.RPMMax);
            this.rpm2 = Mathf.Clamp((float)rpm2, -parameters.RPMMax, parameters.RPMMax);
        }

        public void SetRudderAngle(float dr)
        {
            d_rudder = Mathf.Clamp(dr, -parameters.ThrusterAngleMax, parameters.ThrusterAngleMax); 
        }

        public void SetElevatorAngle(float de)
        {
            d_aileron = Mathf.Clamp(de, -parameters.ThrusterAngleMax, parameters.ThrusterAngleMax); 
        }

        public void SetBatteryPack(double lcg)
        {
            this.lcg = lcg; //TODO 
        }

        public void SetWaterPump(float vbs)
        {
            vbs = Mathf.Clamp01(vbs); //Percentages, given as decimal
            this.vbs = parameters.VBSFixedPoint + Mathf.Lerp(-parameters.VBSMaxDeviation, parameters.VBSMaxDeviation, vbs);
            points.ForEach(point => point.displacementAmount = (float)this.vbs);
        }

        private void FixedUpdate()
        {
            var r1 = rpm1 / 1000 * parameters.RPMToForceMultiplier;
            var r2 = rpm2 / 1000 * parameters.RPMToForceMultiplier;

            var rotorPositionGlobalFrame = transform.position + transform.TransformDirection(ThrusterPosition);

            rigidBody.AddForceAtPosition(ThrustVectorForPropller(r1), rotorPositionGlobalFrame, ForceMode.Force);
            rigidBody.AddForceAtPosition(ThrustVectorForPropller(r2), rotorPositionGlobalFrame, ForceMode.Force);
            rigidBody.AddTorque(Vector3.forward * -(float)r1, ForceMode.Force);
            rigidBody.AddTorque(Vector3.forward * (float)r2, ForceMode.Force);
        }

        public Vector3 ThrustVectorForPropller(double r)
        {
            var localDirection = Quaternion.Euler(d_aileron * Mathf.Rad2Deg, -d_rudder * Mathf.Rad2Deg, 0) *
                                 transform.InverseTransformDirection(transform.forward);
            var localScaled = localDirection * (float)r; //Divide by 2 since two forces
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