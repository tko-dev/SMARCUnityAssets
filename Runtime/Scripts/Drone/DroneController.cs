using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using VehicleComponents.Actuators;
using Rope;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MinimumSnapTrajectory = Trajectory.MinimumSnapTrajectory;



/// <summary>
/// Enumeration of possible drone controller states
/// </summary>
public enum DroneControllerState
{
    TrackingControl = 0,
    LoadControl = 1,
    TrackingControlMinSnap = 2,
}

/// <summary>
/// Tracking controller is implemented per "Geometric tracking control of a quadrotor UAV on SE(3)"
/// Source: https://ieeexplore.ieee.org/document/5717652
///
/// </summary>
public class DroneController : MonoBehaviour
{
    [Header("Basics")]
    [Tooltip("Baselink of drone")]
    public GameObject BaseLink;
    public ArticulationBody baseLinkDroneAB;

    [Header("Tracking")]
    [Tooltip("An object to follow for drone tracking control")]
    public Transform TrackingTargetTF;

    [Header("Drone Configuration")]
    [Tooltip("The euclidean distance from the center of gravity of the drone to rotor (assumes square prop configuration)")]

    /// <value> Distance from the center of the quadrotor to each propeller (assumes square prop configuration) (m) </value>
    public double rotorMomentArm = 0.315;

    [Tooltip("The ratio specifying how much of the rotor force is translated into torque around the drones 3rd axis (normal to the rotor plane)")]

    /// <value> The amount of rotor torque that gets translated into the drones 3rd axis. </value>
    public double torqueCoefficient = 0.08;

    [Header("Propellors")]
    public Transform propFR;
    public Transform propFL, propBR, propBL;

    public Matrix<double> propellorForceToGlobalMap;
    public Matrix<double> propellerForceToGlobalMapInverse;

    Propeller[] propellers;

    [Header("Control Mode")]
    public DroneControllerState controllerState = DroneControllerState.TrackingControl;

    ////////////////// SYSTEM SPECIFIC //////////////////
    // Quadrotor parameters (from unity)
    double massQuadrotor;
    Matrix<double> inertiaJ;
    const int NUM_PROPS = 4;

    // Cached values to avoid recompute
    static readonly Vector<double> e3 = DenseVector.OfArray(new double[] { 0, 0, 1 });
    double g;

    // State Tracking
    Matrix<double> R_sb_d_prev;
    Matrix<double> R_sb_c_prev;

    // FIXME: Was in the middle of fixing all the errors in the tracking controller and missing variable / states
    // Just had fixed the fact that Omega is a vector not a matrix, I also believe I partially only refactored W to Omega in old code
    Vector<double> Omega_sb_d_prev;
    Vector<double> Omega_sb_c_prev;


    // Initialization function
    void Start()
    {
        propellers = new Propeller[4];
        propellers[0] = propFL.GetComponent<Propeller>();
        propellers[1] = propFR.GetComponent<Propeller>();
        propellers[2] = propBR.GetComponent<Propeller>();
        propellers[3] = propBL.GetComponent<Propeller>();

        propellorForceToGlobalMap = DenseMatrix.OfArray(new double[,]
            { { 1, 1, 1, 1 },
            { rotorMomentArm, 0, -rotorMomentArm, 0 },
            { 0, -rotorMomentArm, 0, rotorMomentArm },
            { torqueCoefficient, -torqueCoefficient, torqueCoefficient, -torqueCoefficient }
            }
        );
        propellerForceToGlobalMapInverse = propellorForceToGlobalMap.Inverse();

        baseLinkDroneAB = BaseLink.GetComponent<ArticulationBody>();

        // Creating diagonal matrix of inertia
        double[] diagonal = { baseLinkDroneAB.inertiaTensor.x, baseLinkDroneAB.inertiaTensor.z, baseLinkDroneAB.inertiaTensor.y };
        inertiaJ = DenseMatrix.CreateDiagonal(3, 3, index => diagonal[index]);
        g = Physics.gravity.magnitude;
        //
        // Creating identity matrices (3 x 3) for previous frame transforms
        R_sb_d_prev = DenseMatrix.CreateDiagonal(3, 3, 1.0);
        R_sb_c_prev = DenseMatrix.CreateDiagonal(3, 3, 1.0);
        

        // Creating zero vector for previous frame angular velocities
        Omega_sb_d_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        Omega_sb_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });

    }

    void FixedUpdate()
    {
        if (controllerState == DroneControllerState.TrackingControl)
        {
            // TODO: Implement me
        }
        else if (controllerState == DroneControllerState.LoadControl)
        {
            // TODO: Implement me
        }
        else if (controllerState == DroneControllerState.TrackingControlMinSnap)
        {
            // TODO: Implement me
        }
        else
        {
            Debug.Log("Controller state is outside possible states");
        }

        //TODO: Compute RPMs from Force and moment
        //TODO: Apply RPMs

    }

    (double, Vector<double>) ComputeTrackingControl()
    {
        double f;
        Vector<double> M;

        ////////////////// SYSTEM SPECIFIC //////////////////
        // Gains
        double kx = 16 * massQuadrotor;
        double kv = 5.6 * massQuadrotor;
        double kR = 8.81;
        double kW = 2.54;
        /////////////////////////////////////////////////////



        // Quadrotor states
        Vector<double> x_s = BaseLink.transform.position.To<ENU>().ToDense();
        Vector<double> v_s = baseLinkDroneAB.velocity.To<ENU>().ToDense();
        Matrix<double> R_sb = DenseMatrix.OfArray(new double[,] { { BaseLink.transform.right.x, BaseLink.transform.forward.x, BaseLink.transform.up.x },
                                                                { BaseLink.transform.right.z, BaseLink.transform.forward.z, BaseLink.transform.up.z },
                                                                { BaseLink.transform.right.y, BaseLink.transform.forward.y, BaseLink.transform.up.y } });
        Vector<double> W_b = -1f * BaseLink.transform.InverseTransformDirection(baseLinkDroneAB.angularVelocity).To<ENU>().ToDense();

        // Desired states
        Vector<double> x_s_d;
        Vector<double> v_s_d;
        Vector<double> a_s_d;

        x_s_d = TrackingTargetTF.position.To<NED>().ToDense();
        v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });

        
        // Control

        // FIXME: Hardcoded Error cap
        float DistanceErrorCap = 10f;
        Vector<double> errorTrackingPosition = (x_s - x_s_d) * Math.Min(DistanceErrorCap / (x_s - x_s_d).Norm(2), 1);
        Vector<double> errorTrackingVelocity = v_s - v_s_d;

        Vector<double> pid = _ComputePIDTerm(kx,
            kv,
            g,
            massQuadrotor,
            a_s_d,
            errorTrackingPosition,
            errorTrackingVelocity);

        Matrix<double> R_sb_d = _ComputeDesiredAttitudeVectors(pid);

        // TODO: Abstract away computation of forces and moments if possible
        Vector<double> W_b_d = _VeeMap(_Logm3(R_sb_d_prev.Transpose() * R_sb_d) / dt);
        Vector<double> W_b_d_dot = (W_b_d - Omega_b_d_prev) / dt;

        Vector<double> eR = 0.5 * _VeeMap(R_sb_d.Transpose() * R_sb - R_sb.Transpose() * R_sb_d);
        Vector<double> eW = W_b - R_sb.Transpose() * R_sb_d * W_b_d;

        f = PIDGain * (R_sb * e3);
        M = -kR * eR - kW * eW + _Cross(W_b, inertiaJ * W_b) - inertiaJ * (_HatMap(W_b) * R_sb.Transpose() * R_sb_d * W_b_d - R_sb.Transpose() * R_sb_d * W_b_d_dot);

        R_sb_d_prev = R_sb_d;
        Omega_b_d_prev = W_b_d;

    }
    /// <summary>
    /// Stacks force scaler and moment vecotrs into single Vector
    /// </summary>
    private static Vector<double> _StackForceMomentVector(double f, Vector<double> moments)
    {
        return DenseVector.OfArray(new double[] { f, moments[0], moments[1], moments[2] });
    }

    /// <summary>
    /// Computes RPMs needed for control regardless of controller state
    /// </summary>
    /// <param name="f"> Desired forces </param>
    /// <param name="M"> Desired moments </param>
    double[] ComputeRPMs(double f, Vector<double> M)
    {

        // Compute optimal propeller forces
        Vector<double> globalForces = _StackForceMomentVector(f, M);
        Vector<double> F_star = propellerForceToGlobalMapInverse * globalForces;
        Vector<double> F = F_star;

        for (int i = 0; i < NUM_PROPS; i++)
        {
            if (F[i] < 0)
            {
                F[i] = 0;
            }
        }

        // Set propeller rpms
        double[] propellersRPMs = { 0, 0, 0, 0 };
        for (int i = 0; i < propellers.Length; i++)
            propellersRPMs[i] = F[i] / propellers[i].RPMToForceMultiplier;
        return propellersRPMs;
    }

    /// <summary>
    /// Computes "PID"-like gain for tracking controller
    ///
    /// </summary>
    private static Vector<double> _ComputePIDTerm(double gainX,
                                                  double gainV,
                                                  double gravityMagnitude,
                                                  double mass,
                                                  Vector<double> desiredAcceleration,
                                                  Vector<double> errorPosition,
                                                  Vector<double> errorVelocity)
    {
        // This is upper term of equation (12) from geometric tracking paper
        Vector<double> pid = -gainX * errorPosition - gainV * errorVelocity + mass * gravityMagnitude * e3 + mass * desiredAcceleration;
        return pid;
    }

    /// <summary>
    /// Computes desired attitude vector for quadrotor from Geometric Tracking and Control.
    ///
    /// Computes the desired headings for body vectors 1,2,3 where 3 is normal to rotor plane.
    /// Equations that pertain to this section can be found in Tracking Errors Section
    /// </summary>
    private static Matrix<double> _ComputeDesiredAttitudeVectors(Vector<double> pid)
    {

        Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2) / 2, -Math.Sqrt(2) / 2, 0 });
        Vector<double> b3d = pid / pid.Norm(2);
        Vector<double> b2d = _Cross(b3d, b1d) / _Cross(b3d, b1d).Norm(2);
        b1d = _Cross(b2d, b3d);
        // NOTE: If needed can update for performance to just manually creating matrix like before can easily be reimplemented here
        Matrix<double> R_sb_d = CreateMatrix.DenseOfColumns(new Vector<double>[] { b1d, b2d, b3d });

        return R_sb_d;
    }


    /// <summary>
    /// Cross product operation for R^3 vectors
    /// </summary>
    /// TODO: Does cross product exist in unity math
    private static Vector<double> _Cross(Vector<double> a, Vector<double> b)
    {
        // Calculate each component of the cross product
        double c1 = a[1] * b[2] - a[2] * b[1];
        double c2 = a[2] * b[0] - a[0] * b[2];
        double c3 = a[0] * b[1] - a[1] * b[0];

        // Create a new vector for the result
        return DenseVector.OfArray(new double[] { c1, c2, c3 });
    }
    /// <summary>
    ///  Constructs skew symmetric matrix from vector. Also known as the hat map.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private static Matrix<double> _HatMap(Vector<double> v)
    {
        return DenseMatrix.OfArray(new double[,] { { 0, -v[2], v[1] },
                                                   { v[2], 0, -v[0] },
                                                   { -v[1], v[0], 0 } });
    }


    /// <summary>
    ///  Constructs vector from skew symmetric matrix. Also known as the vee map.
    /// </summary>
    /// <param name="S"></param>
    /// <returns></returns>
    private static Vector<double> _VeeMap(Matrix<double> S)
    {
        return DenseVector.OfArray(new double[] { S[2, 1], S[0, 2], S[1, 0] });
    }

    static Matrix<double> _Logm3(Matrix<double> R)
    {
        double acosinput = (R[0, 0] + R[1, 1] + R[2, 2] - 1) / 2.0;
        Matrix<double> m_ret = DenseMatrix.OfArray(new double[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } });
        if (acosinput >= 1)
            return m_ret;
        else if (acosinput <= -1)
        {
            Vector<double> omg;
            if (!(Math.Abs(1 + R[2, 2]) < 1e-6f))
                omg = (1.0 / Math.Sqrt(2 * (1 + R[2, 2]))) * DenseVector.OfArray(new double[] { R[0, 2], R[1, 2], 1 + R[2, 2] });
            else if (!(Math.Abs(1 + R[1, 1]) < 1e-6f))
                omg = (1.0 / Math.Sqrt(2 * (1 + R[1, 1]))) * DenseVector.OfArray(new double[] { R[0, 1], 1 + R[1, 1], R[2, 1] });
            else
                omg = (1.0 / Math.Sqrt(2 * (1 + R[0, 0]))) * DenseVector.OfArray(new double[] { 1 + R[0, 0], R[1, 0], R[2, 0] });
            m_ret = _HatMap(Math.PI * omg);
            return m_ret;
        }
        else
        {
            double theta = Math.Acos(acosinput);
            m_ret = theta / 2.0 / Math.Sin(theta) * (R - R.Transpose());
            return m_ret;
        }
    }
}
