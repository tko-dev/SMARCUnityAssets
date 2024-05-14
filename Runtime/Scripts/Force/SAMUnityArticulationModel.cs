using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Force
{
    public class SAMUnityArticulationModel : MonoBehaviour, IForceModel, ISAMControl
    {
        [FormerlySerializedAs("rigidBody")] public ArticulationBody baseLink;
        public ArticulationBody propller1;
        public ArticulationBody propller2;
        public ArticulationBody rudder;
        public ArticulationBody aileron;

        public double lcg { get; set; }
        public double vbs { get; set; }

        public SAMParameters parameters { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get; set; }
        public double rpm2 { get; set; }

        public Vector3 ThrusterPosition = new(0, 0, -0.73f);
        private List<ForcePoint> points;

        public SAMUnityArticulationModel()
        {
            parameters = new SAMParameters();
        }

        private void Awake()
        {
            //    rigidBody = GetComponent<Rigidbody>();
            Debug.Log(baseLink.inertiaTensor);
            Debug.Log(propller1.inertiaTensor);
            Debug.Log(propller2.inertiaTensor);
            Debug.Log(rudder.inertiaTensor);
            Debug.Log(aileron.inertiaTensor);
          
            points = new List<ForcePoint>(GetComponentsInChildren<ForcePoint>());
            //   if (rigidBody == null) rigidBody = transform.parent.GetComponent<Rigidbody>();
        }

        public void SetRpm1(double rpm)
        {
            this.rpm1 = Mathf.Clamp((float)rpm, -parameters.RPMMax, parameters.RPMMax);
        }

        public void SetRpm2(double rpm)
        {
            this.rpm2 = Mathf.Clamp((float)rpm, -parameters.RPMMax, parameters.RPMMax);
        }


        public void SetRpm(double rpm1, double rpm2)
        {
            SetRpm1(rpm1);
            SetRpm2(rpm2);
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
            // var r1 = rpm1 / 1000 * parameters.RPMToForceMultiplier;
            // var r2 = rpm2 / 1000 * parameters.RPMToForceMultiplier;
      
            // propller1.SetDriveTargetVelocity(ArticulationDriveAxis.X, -(float)rpm1);
            // propller2.SetDriveTargetVelocity(ArticulationDriveAxis.X, (float)rpm2);

            rudder.SetDriveTarget(ArticulationDriveAxis.X, d_rudder * Mathf.Rad2Deg);
            aileron.SetDriveTarget(ArticulationDriveAxis.X, d_aileron * Mathf.Rad2Deg);

          //  var rotorPositionGlobalFrame = transform.position + transform.TransformDirection(ThrusterPosition);

            // aileron.AddForceAtPosition((float)r1 * aileron.transform.forward, aileron.transform.position, ForceMode.Force);
            // aileron.AddForceAtPosition((float)r2 * aileron.transform.forward, aileron.transform.position, ForceMode.Force);
        }

        // public Vector3 ThrustVectorForPropller(double r)
        // {
        //     var localDirection = Quaternion.Euler(d_aileron * Mathf.Rad2Deg, -d_rudder * Mathf.Rad2Deg, 0) *
        //                          transform.InverseTransformDirection(transform.forward);
        //     var localScaled = localDirection * (float)r; //Divide by 2 since two forces
        //     var globalDirection = transform.TransformDirection(localScaled);
        //     return globalDirection;
        // }

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