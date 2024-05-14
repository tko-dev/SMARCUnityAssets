using Force;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace DefaultNamespace
{
    public class SAMForceModelExample : MonoBehaviour, IForceModel, ISAMControl
    {
        private Rigidbody rigidBody;
        public double lcg { get; set; }
        public double vbs { get; set; }
        public SAMParameters parameters { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get; set; }
        public double rpm2 { get; set; }

        //Unity method that gets run on startup. Fetches the rigidBody on the vehicle (SAM) in the scene.
        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        public void SetRpm1(double rpm)
        {
            this.rpm1 = rpm;
        }

        public void SetRpm2(double rpm)
        {
            this.rpm2 = rpm;
        }

        //The below methods are for the controls that we input into SAM. I.e what your controller will input to control your vehicle (Will be different from BlueROV)
        public void SetRpm(double rpm1, double rpm2)
        {
            SetRpm1(rpm1);
            SetRpm2(rpm2);
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

        public void SetWaterPump(float vbs)
        {
            this.vbs = vbs;
        }

        //The "Tick" method the gets called every step of the physics simulation.
        private void FixedUpdate()
        {
            //Helper objects for doing matrix computations
            var mb = DenseMatrix.Build;
            var vb = DenseVector.Build;

            // Often you need to translate your coordinate systems. In this case the original controller was:
            // x=forw, y=right, z=down
            // Unity is
            // x=right y=up z=forward
            var x = transform.position.x;
            var y = transform.position.z;
            var z = -transform.position.y;

            var phi = transform.localEulerAngles.x;
            var theta = transform.localEulerAngles.z;
            var psi = -transform.localEulerAngles.y;

            
            //Getting the vehicle information. Here we assume ONE rigidbody, could be multiple ones if its needed (usually isnt).
            var velocity = transform.InverseTransformDirection(rigidBody.velocity);
            var u = velocity.x;
            var v = velocity.z;
            var w = -velocity.y;

            var angularVelocity = transform.InverseTransformDirection(rigidBody.angularVelocity);
            // originally pqr=zx-y
            var p = angularVelocity.x;
            var q = angularVelocity.z;
            var r = -angularVelocity.y;

            // Inertia tensor
            Matrix<double> I_o = DenseMatrix.OfArray(new double[,]
            {
                { rigidBody.inertiaTensor.x, 0, 0 },
                { 0, rigidBody.inertiaTensor.z, 0 },
                { 0, 0, -rigidBody.inertiaTensor.y } //Here we use the inertia configured in Unity. Could replace if you want.
            });

            var d_scale = 0.1f;
            var vbs_scale = 1;
            var lcg_scale = 1;

            var rpm1 = this.rpm1;
            var rpm2 = this.rpm2;
            var dElevator = d_aileron * d_scale;
            var dRudder = d_rudder * d_scale;
            var vbs = this.vbs * vbs_scale;
            var lcg = this.lcg * lcg_scale;

            // # Hydrodynamic coefficients. Damping
            // # Mostly pressure drag.
            var m = rigidBody.mass;
            var W = rigidBody.mass * 9.81;
            var B = W + vbs * 1.5;
            var Xuu = 5; // #1.0
            var Yvv = 20; // #100.0
            var Zww = 50; // #100.0
            var Kpp = 1; // #10.0
            var Mqq = 20; // #100.0
            var Nrr = 20; // #150.0
            
            
            //TODO: Magic model :). Based on current velocity, angle of attack etc.
            
            var force_control = Vector3.zero; //These will be replaced with your model output
            var torque_control = Vector3.zero;
            
            //TODO: Usually the control and damping forces need to be computed separately 
            
            var force_damping = Vector3.zero; //These will be replaced with your model output
            var torque_damping = Vector3.zero;

            rigidBody.AddForce(force_control, ForceMode.Force);
            rigidBody.AddTorque(torque_control, ForceMode.Force);
            
            rigidBody.AddForce(force_damping, ForceMode.Force);
            rigidBody.AddTorque(torque_damping, ForceMode.Force);
            
        }
        
        
        //Ignore the rest, just a technical thing.

        public Vector3 TorqueDamping { get; set; }
        public Vector3 ForceDamping { get; set; }

        public Vector3 GetTorqueDamping()
        {
            return TorqueDamping;
        }

        public Vector3 GetForceDamping()
        {
            return ForceDamping;
        }
    }
}