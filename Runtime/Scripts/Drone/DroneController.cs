using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using DefaultNamespace.LookUpTable;
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
    ArticulationBody baseLinkDroneAB;

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
    float dt;

    // State Tracking
    Matrix<double> R_sb_d_prev;
    Matrix<double> R_sb_c_prev;

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
        massQuadrotor = baseLinkDroneAB.mass; // Quadrotor mass (kg)

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

        dt = Time.fixedDeltaTime;

    }

    void FixedUpdate()
    {
        double f = 0;
        Vector<double> M = DenseVector.OfArray(new double[] { 0, 0, 0 });

        if (controllerState == DroneControllerState.TrackingControl)
        {
            (f, M) = ComputeTrackingControl();
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

        float[] currPropellerRPMs = ComputeRPMs(f, M);
        ApplyRPMs(currPropellerRPMs);

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

        x_s_d = TrackingTargetTF.position.To<ENU>().ToDense();
        v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });


        // Control

        // FIXME: Hardcoded Error cap
        float DistanceErrorCap = 10f;
        Vector<double> errorTrackingPosition = (x_s - x_s_d) * Math.Min(DistanceErrorCap / (x_s - x_s_d).Norm(2), 1);
        Debug.Log($"Error in tracking position {errorTrackingPosition}");
        Vector<double> errorTrackingVelocity = v_s - v_s_d;
        Debug.Log($"Error in tracking velocity {errorTrackingVelocity}");

        Vector<double> pidGain = _ComputePIDTerm(
            kx,
            kv,
            g,
            massQuadrotor,
            a_s_d,
            errorTrackingPosition,
            errorTrackingVelocity);

        Debug.Log($"PID Gain {pidGain}");

        Matrix<double> R_sb_d = _ComputeDesiredAttitudeVectors(pidGain);
        Debug.Log($"Desired attitude Vectors: \n {R_sb_d}");

        // TODO: Abstract away computation of forces and moments if possible
        Vector<double> Omega_sb_d = _VeeMap(_Logm3(R_sb_d_prev.Transpose() * R_sb_d) / dt);
        Vector<double> Omega_sb_d_dot = (Omega_sb_d - Omega_sb_d_prev) / dt;

        Vector<double> eRotation = 0.5 * _VeeMap(R_sb_d.Transpose() * R_sb - R_sb.Transpose() * R_sb_d);
        Vector<double> eOmega = W_b - R_sb.Transpose() * R_sb_d * Omega_sb_d;

        f = pidGain * (R_sb * e3);
        M = -kR * eRotation - kW * eOmega + _Cross(W_b, inertiaJ * W_b) - inertiaJ * (_HatMap(W_b) * R_sb.Transpose() * R_sb_d * Omega_sb_d - R_sb.Transpose() * R_sb_d * Omega_sb_d_dot);

        // Updating trailing values needed at each computation
        R_sb_d_prev = R_sb_d;
        Omega_sb_d_prev = Omega_sb_d;

        // f = massQuadrotor * g;
        // M = DenseVector.OfArray(new double[] { 0, 0, 0 });
        return (f, M);
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
    float[] ComputeRPMs(double f, Vector<double> M)
    {

        // Compute optimal propeller forces
        Debug.Log($"Desired force [{f}], desired Moment [{M}]");
        Vector<double> globalForces = _StackForceMomentVector(f, M);
        Vector<double> F_star = propellerForceToGlobalMapInverse * globalForces;
        Vector<double> F = F_star;
        Debug.Log($"Optimal forces computed as [{F_star}]");

        for (int i = 0; i < NUM_PROPS; i++)
        {
            if (F[i] < 0)
            {
                F[i] = 0;
            }
        }

        // Set propeller rpms
        float[] currPropVals = { 0, 0, 0, 0 };
        for (int i = 0; i < propellers.Length; i++)
            currPropVals[i] = (float)F[i] / propellers[i].RPMToForceMultiplier;
        return currPropVals;
    }

    /// <summary>
    /// Applies RPMs to unity drone object 
    ///
    /// Ensures thats propellers does not have a negative RPM
    /// </summary>
    void ApplyRPMs(float[] propellersRPMs)
    {
        Debug.Log($"RPM: {propellersRPMs[0]:F2},{propellersRPMs[1]:F2},{propellersRPMs[2]:F2},{propellersRPMs[3]:F2}"); // desired position
        for (int i = 0; i < propellers.Length; i++)
        {
            // Now, all props should always have positive rpms, but just in case...
            if (propellersRPMs[i] < 0)
            {
                Debug.LogWarning("Propeller " + i + " has negative RPMs: " + propellersRPMs[i]);
                propellersRPMs[i] = 0;
            }
            propellers[i].SetRpm(propellersRPMs[i]);
        }
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
        Debug.Log($"X Gain {-gainX * errorPosition}");
        Debug.Log($"V Gain {-gainV * errorVelocity}");
        Debug.Log($"Grav Gain {mass * gravityMagnitude * e3}");
        Debug.Log($"Accel Gain {mass * desiredAcceleration}");
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

    // TODO: Find a source for this math function, and rationale. (Doesn't seem to exits in paper)
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
