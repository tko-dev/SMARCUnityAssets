using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace DefaultNamespace.LookUpTable
{
    public class DampingForce : MonoBehaviour
    {
        private Rigidbody rb;

        public void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void FixedUpdate()
        {
            var Qr = transform.localRotation.To<NED>();
        }

        public void CalcRelVelAndFlowVel()
        {
            var uvw_rel_nb = rb.velocity.To<NED>().ToDense(); // Might need to revisit. Rel. velocity in point m block.
            // var pqr_nb = rb.angularVelocity.To<NED>().ToDense();
            var aoa_alpha_angleOfAttack = AngleOfAttack(uvw_rel_nb);
            
            
        }

        public Vector<double> AngleOfAttack(Vector<double> vr) // Relative Velocity, Center of Mass
        {
            var Vinf = vr.Norm(2);
            var a_angleOfAttack_alpha = Math.Atan2(vr[2], vr[0]);
            var b_beta = Math.Asin(vr[1] / Vinf);
            var ae_effectiveAoA = Math.Acos(vr[0] / Vinf); // 
            var ta_transversalAoA = Math.Atan2(vr[2], vr[1]); //Sideways AoA
            return Vinf > 0.000001
                ? Vector.Build.DenseOfArray(new[] { a_angleOfAttack_alpha, b_beta, ae_effectiveAoA, ta_transversalAoA, Vinf })
                : Vector.Build.DenseOfArray(new[] { 0, 0, 0, 0.0, 0.0 });
        }
    }
}