using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace DefaultNamespace
{
    public class SAMUnityForceModel : MonoBehaviour, IForceModel, ISAMControl
    {
        private Rigidbody rigidBody;

        public float rpm_multiplier = 5;
        public double lcg { get; set; }
        public double vbs { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get; set; }
        public double rpm2 { get; set; }



        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        public void SetRpm(double rpm1, double rpm2)
        {
            this.rpm1 = rpm1;
            this.rpm2 = rpm2;
        }

        public void SetRudderAngle(float dr)
        {
            d_rudder = dr;
        }

        public void SetElevatorAngle(float de)
        {
            d_aileron = de;
        }

        public void SetBatteryPack(double lcg)
        {
            this.lcg = lcg;
        }

        public void SetWaterPump(double vbs)
        {
            this.vbs = vbs;
        }

        private void FixedUpdate()
        {
            var r1 = rpm1 / 1000;
            var r2 = rpm2 / 1000;

            var localDirection = Quaternion.Euler(d_aileron * Mathf.Rad2Deg , -d_rudder * Mathf.Rad2Deg, 0) * transform.InverseTransformDirection(transform.forward);
            var localScaled = localDirection * (float)r1 * rpm_multiplier;
            var globalDirection = transform.TransformDirection(localScaled);
            rigidBody.AddForceAtPosition(globalDirection, transform.position + transform.TransformDirection(new Vector3(0, 0, -0.73f)), ForceMode.Force);
        }

        public Vector3 GetTorqueDamping()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetForceDamping()
        {
            throw new System.NotImplementedException();
        }
    }
}