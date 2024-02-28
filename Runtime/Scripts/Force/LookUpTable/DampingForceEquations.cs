using System;
using DefaultNamespace;
using DefaultNamespace.LookUpTable;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

namespace Force.LookUpTable
{
    public static class DampingForceEquations
    {
        //TODO: Not really constants
        public static double WaterDensity = 1023;
        public static double dVisc_DynamicViscosity = 0.0001002; // 1.002 e-3
        public static double C_SamLength = 1.2705;

        public static double Ar = 0.4754;
        public static double dens_Density = WaterDensity;

        public static double CKpp = 0.1;
        public static double CMqq = 40;
        public static double CNrr = 40;

        public static (Vector3 forces, Vector3 moments) CalculateDamping(Rigidbody rb)
        {
            var uvw_nm_nb = rb.velocity.To<NED>().ToDense(); // Might need to revisit. Rel. velocity in point m block.
            var pqr_nm = rb.angularVelocity.To<NED>().ToDense();

            var aoa_alpha_angleOfAttack = AngleOfAttack(uvw_nm_nb);
            var (forces, moments) = CalculateMomentsForces(uvw_nm_nb, pqr_nm, aoa_alpha_angleOfAttack);

            var forcesUnity = new Vector3<NED>(forces).toUnity;
            var momentsUnity = new Vector3<NED>(moments).toUnity;
            return (forcesUnity, momentsUnity);
        }


        public static (Vector3 forces, Vector3 moments) CalculateMomentsForces(Vector<double> uvw_nm, Vector<double> pqr_nm, Vector<double> aoa)
        {
            var mb = DenseMatrix.Build;
            var vb = DenseVector.Build;

            var vInf = aoa[4];
            var Re_velocityReynolds = CalculateReynolds(vInf, WaterDensity, C_SamLength, dVisc_DynamicViscosity);
            var CX = FetchCoefficients_CX(Re_velocityReynolds, aoa[2]);
            var CY = FetchCoefficients_CY(Re_velocityReynolds, aoa[1]);
            var CZ = FetchCoefficients_CZ(Re_velocityReynolds, aoa[0]);
            var XCp = FetchCoefficients_XCp(Re_velocityReynolds, aoa[2]);
            var aoaTransversal = aoa[3];

            var u = uvw_nm[0];
            var v = uvw_nm[1];
            var w = uvw_nm[2];

            var p = pqr_nm[0];
            var q = pqr_nm[1];
            var r = pqr_nm[2];

            var ce = Vector.Build.DenseOfArray(new[] { XCp, 0, 0 });

            var translationalDampingCoefficients = Vector.Build.DenseOfArray(new[]
            {
                0.5 * dens_Density * Ar * CX,
                0.5 * dens_Density * Ar * CY,
                0.5 * dens_Density * Ar * CZ
            });

            var rotationalDampingCoefficients = Vector.Build.DenseOfArray(new[]
            {
                -CKpp * p * Math.Abs(p),
                -CMqq * q * Math.Abs(q),
                -CNrr * r * Math.Abs(r)
            });

            var sth = mb.Diagonal(new[]
            {
                Math.Pow(u, 2), // Paper says Abs of |u| * u, etc
                Math.Pow(v, 2),
                Math.Pow(w, 2)
            });

            var forces = -sth.Multiply(translationalDampingCoefficients);
            var moments = Vector3.Cross(ce.ToVector3(), forces.ToVector3());
            moments += rotationalDampingCoefficients.ToVector3();

            return (forces.ToVector3(), moments);
        }

        private static double CalculateReynolds(double v_Inf, double density, double c, double visc)
        {
            return v_Inf * density * c / visc;
        }

        public static Vector<double> AngleOfAttack(Vector<double> vr) // Relative Velocity, Center of Mass
        {
            var Vinf = vr.Norm(2);
            var a_angleOfAttack_alpha = Math.Atan2(vr[2], vr[0]);
            var b_beta_sideslip = Math.Asin(vr[1] / Vinf);
            var ae_effectiveAoA = Math.Acos(vr[0] / Vinf); // 
            var ta_transversalAoA = Math.Atan2(vr[2], vr[1]); //Sideways AoA
            return Vinf > 0.000001
                ? Vector.Build.DenseOfArray(new[] { a_angleOfAttack_alpha, b_beta_sideslip, ae_effectiveAoA, ta_transversalAoA, Vinf })
                : Vector.Build.DenseOfArray(new[] { 0, 0, 0, 0.0, 0.0 });
        }

        public static double FetchCoefficients_CX(Double v_inf, Double ae_effectiveAngleOfAttack)
        {
            return 0.0;
        }

        public static double FetchCoefficients_CY(Double v_inf, Double b_beta)
        {
            return 0.0;
        }

        public static double FetchCoefficients_CZ(Double v_inf, Double a_alpha)
        {
            return 0.0;
        }

        public static double FetchCoefficients_XCp(Double v_inf, Double ae_effectiveAngleOfAttack)
        {
            return 0.0;
        }
    }
}