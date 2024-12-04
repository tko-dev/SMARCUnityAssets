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
/// Tracking controller is implemented per "Geometric tracking control of a quadrotor UAV on SE(3)"
/// Source: https://ieeexplore.ieee.org/document/5717652
///
/// </summary>
public class DroneLoadController : MonoBehaviour
{
    [Header("Basics")]
    [Tooltip("Baselink of the drone")]
    public GameObject BaseLink;
    // [Tooltip("Load's connection point to the rope")]
    // public float ControlFrequency = 50f;
    // [Tooltip("The maximum distance error between the load and the target position, kind of controls the aggressiveness of the maneuvers.")]
    // public float DistanceErrorCap = 10f;
    // private Vector<double> startingPosition = null;
    public float MaxVelocityWithTrackingTarget = 1f;
    // public float MaxAccelerationWithTrackingTarget = 1f;
    public float DecelerationDistance = 1f;

    [Header("Drone Configuration")]
    [Tooltip("The euclidean distance from the center of gravity of the drone to rotor")]
    double rotorMomentArm = 0.315; // Distance from the center of the quadrotor to each propeller (assumes square prop configuration) (m)
    [Tooltip("The ratio specifying how much of the rotor force is translated into torque around the drones 3rd axis (normal to the rotor plane)")]
    double torqueCoefficient = 0.08; // Torque to force ratio of the propellers (also found in Propeller.cs, TODO: make this one variable) (m)

    [Header("Tracking")]
    [Tooltip("An object to follow")]
    public Transform TrackingTargetTF;


    [Header("Load")]
    [Tooltip("The rope object that this drone is expected to get connected, maybe. Will be used to check for attachment state and such.")]
    public Transform Rope; // TODO remove this requirement.
    [Tooltip("If true, instead of tracking the target object, drone will first track the buoy and when attached to it, make the LoadLinkTF track the target.")]
    public bool AttackTheBuoy = false;
    [Tooltip("The position of where the load is attached to the rope. rope_link on SAM")]
    public Transform LoadLinkTF; // The position of the AUV is taken at the base of the rope

    [Header("Props")]
    public Transform PropFR, PropFL, PropBR, PropBL;



    Propeller[] propellers;
    float[] propellersRPMs;
    ArticulationBody baseLinkAB;
    ArticulationBody loadLinkAB;
    Matrix<double> R_sb_d_prev;
    Matrix<double> R_sb_c_prev;
    Vector<double> Omega_b_d_prev;
    Vector<double> Omega_b_c_prev;
    Vector<double> q_c_prev;
    Vector<double> q_c_dot_prev;
    int times1 = 0;
    int times2 = 0;

    ////////////////// SYSTEM SPECIFIC //////////////////
    // Quadrotor parameters (from unity)
    double massQuadrotor;
    Matrix<double> inertiaJ;

    // General quadrotor parameters
    const int NUM_PROPS = 4;



    // Mapping from propeller forces to the equivalent wrench (similar version found in the paper)
    Matrix<double> propellorForceToGlobalMap = DenseMatrix.OfArray(new double[,]
        { { 1, 1, 1, 1 },
        { rotorMomentArm, 0, -rotorMomentArm, 0 },
        { 0, -rotorMomentArm, 0, rotorMomentArm },
        { torqueCoefficient, -torqueCoefficient, torqueCoefficient, -torqueCoefficient }
        }
    );
    Matrix<double> propllerForceToGlobalMapInverse = propellorForceToGlobalMap.Inverse();
    Matrix<double> Q;

    // Load parameters
    double mL;
    double l;

    // Gains
    double kx;
    double kv;
    double kR;
    double kW;
    double kq;
    double kw;
    /////////////////////////////////////////////////////

    // Simulation parameters
    double g;
    static readonly Vector<double> e3 = DenseVector.OfArray(new double[] { 0, 0, 1 });
    float dt;

    // Min snap trajectory
    int min_snap_flag;
    double catching_time;
    // TODO: This should not be tracked in a global state
    MinimumSnapTrajectory xTraj;
    MinimumSnapTrajectory yTraj;
    MinimumSnapTrajectory zTraj;

    // Logging
    string filePath = Application.dataPath + "/../../SMARCUnityAssets/Logs/log.csv";
    TextWriter tw;

    // Use this for initialization
    void Start()
    {
        propellers = new Propeller[4];
        propellers[0] = PropFL.GetComponent<Propeller>();
        propellers[1] = PropFR.GetComponent<Propeller>();
        propellers[2] = PropBR.GetComponent<Propeller>();
        propellers[3] = PropBL.GetComponent<Propeller>();

        baseLinkAB = BaseLink.GetComponent<ArticulationBody>();

        // Creating identity matrices (3 x 3) for previous frame transforms
        R_sb_d_prev = DenseMatrix.CreateDiagonal(3, 3, 1.0);
        R_sb_c_prev = DenseMatrix.CreateDiagonal(3, 3, 1.0);

        // Creating zero vector for previous frame angular velocities
        Omega_b_d_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        Omega_b_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_dot_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });

        propellersRPMs = new float[] { 0, 0, 0, 0 };

        if (LoadLinkTF != null) loadLinkAB = LoadLinkTF.GetComponent<ArticulationBody>();

        ////////////////// SYSTEM SPECIFIC //////////////////
        // Quadrotor parameters
        massQuadrotor = baseLinkAB.mass; // Quadrotor mass (kg)

        // Creating diagonal matrix of inertia
        double[] diagonal = { baseLinkAB.inertiaTensor.x, baseLinkAB.inertiaTensor.z, baseLinkAB.inertiaTensor.y };
        inertiaJ = DenseMatrix.CreateDiagonal(3, 3, index => diagonal[index]);

        Q = propellorForceToGlobalMap.Transpose() * propellorForceToGlobalMap;

        // Load parameters
        mL = 0; // Load mass (sum of all mass elements on sam) ~15 kg
        if (LoadLinkTF != null)
        {
            ArticulationBody[] sam_ab_list = LoadLinkTF.root.gameObject.GetComponentsInChildren<ArticulationBody>();
            foreach (ArticulationBody sam_ab in sam_ab_list)
            {
                mL += sam_ab.mass;
            }
        }
        // Rope length l is calculated dynamically
        /////////////////////////////////////////////////////

        // Simulation parameters
        g = Physics.gravity.magnitude;
        dt = Time.fixedDeltaTime;//1f/ControlFrequency;
        //
        min_snap_flag = 0;
        catching_time = 0;

        tw = new StreamWriter(filePath, false);
        tw.WriteLine("t,x_s1,x_s2,x_s3,x_s_d1,x_s_d2,x_s_d3");
        tw.Close();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ComputeRPMs();
        ApplyRPMs();
    }

    (double, Vector<double>) SuspendedLoadControl()
    {
        double f;
        Vector<double> M;

        ////////////////// SYSTEM SPECIFIC //////////////////
        // Gains
        kx = 16 * massQuadrotor;
        kv = 5.6 * massQuadrotor;
        kR = 8.81;
        kW = 0.5;
        kq = 2;
        kw = 0.5;
        /////////////////////////////////////////////////////

        // Quadrotor states
        Vector<double> xQ_s = BaseLink.transform.position.To<ENU>().ToDense();
        Vector<double> vQ_s = baseLinkAB.velocity.To<ENU>().ToDense();
        Matrix<double> R_sb = DenseMatrix.OfArray(new double[,] { { BaseLink.transform.right.x, BaseLink.transform.forward.x, BaseLink.transform.up.x },
                                                                { BaseLink.transform.right.z, BaseLink.transform.forward.z, BaseLink.transform.up.z },
                                                                { BaseLink.transform.right.y, BaseLink.transform.forward.y, BaseLink.transform.up.y } });
        Vector<double> W_b = -1f * (BaseLink.transform.InverseTransformDirection(baseLinkAB.angularVelocity)).To<ENU>().ToDense();

        // Load states
        Vector<double> xL_s = LoadLinkTF.position.To<ENU>().ToDense();
        Vector<double> vL_s = loadLinkAB.velocity.To<ENU>().ToDense(); // NOTE: Not realistic for actual load controller
        l = (xL_s - xQ_s).Norm(2); // TODO: Figure out the fixed rope length from the rope object, the controller should work even with stretching
        Vector<double> q = (xL_s - xQ_s) / l;
        Vector<double> q_dot = (vL_s - vQ_s) / l;

        // Desired states
        Vector<double> xL_s_d;
        Vector<double> vL_s_d;
        Vector<double> aL_s_d;
        (xL_s_d, vL_s_d, aL_s_d) = TrackingTargetTrajectory(TrackingTargetTF.position.To<ENU>().ToDense(), xL_s, vL_s);

        Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2) / 2, -Math.Sqrt(2) / 2, 0 });

        // Load position controller
        Vector<double> ex = xL_s - xL_s_d;
        Vector<double> ev = vL_s - vL_s_d;

        Vector<double> A = -kx * ex - kv * ev + (massQuadrotor + mL) * (aL_s_d + g * e3) + massQuadrotor * l * (q_dot * q_dot) * q;
        Vector<double> q_c = -A / A.Norm(2);
        Vector<double> q_c_dot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c - q_c_prev)/dt;
        Vector<double> q_c_ddot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c_dot - q_c_dot_prev)/dt;
        Vector<double> F_n = (A * q) * q;
        Debug.DrawRay(ToUnity(xQ_s), ToUnity(q_c), Color.magenta);

        // Load attitude controller
        Vector<double> eq = _HatMap(q) * _HatMap(q) * q_c;
        Vector<double> eq_dot = q_dot - _Cross(_Cross(q_c, q_c_dot), q);

        Vector<double> F_pd = -kq * eq - kw * eq_dot;
        Vector<double> F_ff = massQuadrotor * l * (q * _Cross(q_c, q_c_dot)) * _Cross(q, q_dot) + massQuadrotor * l * _Cross(_Cross(q_c, q_c_ddot), q);
        Vector<double> F_for_f = F_n - F_pd - F_ff;

        F_n = -(q_c * q) * q;
        Vector<double> F_for_M = F_n - F_pd - F_ff;

        // Quadrotor attitude controller
        Vector<double> b3c = F_for_M / F_for_M.Norm(2);
        Vector<double> b1c = -_Cross(b3c, _Cross(b3c, b1d)) / _Cross(b3c, b1d).Norm(2);
        Vector<double> b2c = _Cross(b3c, b1c);
        Matrix<double> R_sb_c = DenseMatrix.OfArray(new double[,] { { b1c[0], b2c[0], b3c[0] },
                                                                    { b1c[1], b2c[1], b3c[1] },
                                                                    { b1c[2], b2c[2], b3c[2] } });

        Vector<double> W_b_c = _VeeMap(_Logm3(R_sb_c_prev.Transpose() * R_sb_c) / dt);
        Vector<double> W_b_c_dot = (W_b_c - Omega_b_c_prev) / dt;

        Vector<double> eR = 0.5 * _VeeMap(R_sb_c.Transpose() * R_sb - R_sb.Transpose() * R_sb_c);
        Vector<double> eW = W_b - R_sb.Transpose() * R_sb_c * W_b_c;

        f = F_for_f * (R_sb * e3);
        M = -kR * eR - kW * eW + _Cross(W_b, inertiaJ * W_b) - inertiaJ * (_HatMap(W_b) * R_sb.Transpose() * R_sb_c * W_b_c - R_sb.Transpose() * R_sb_c * W_b_c_dot);

        // Save previous values
        R_sb_c_prev = R_sb_c;
        Omega_b_c_prev = W_b_c;
        q_c_prev = q_c;
        q_c_dot_prev = q_c_dot;

        if (times1 < 2 || M.Norm(2) > 100) // If previous values have not been initialized yet or moments are excessive
        {
            times1++;
            f = 0;
            M = DenseVector.OfArray(new double[] { 0, 0, 0 });
        }

        return (f, M);
    }

    (double, Vector<double>) TrackingControl()
    {
        double f;
        Vector<double> M;

        ////////////////// SYSTEM SPECIFIC //////////////////
        // Gains
        kx = 16 * massQuadrotor;
        kv = 5.6 * massQuadrotor;
        kR = 8.81;
        kW = 2.54;
        /////////////////////////////////////////////////////

        // Quadrotor states
        Vector<double> x_s = BaseLink.transform.position.To<ENU>().ToDense();
        Vector<double> v_s = baseLinkAB.velocity.To<ENU>().ToDense();
        Matrix<double> R_sb = DenseMatrix.OfArray(new double[,] { { BaseLink.transform.right.x, BaseLink.transform.forward.x, BaseLink.transform.up.x },
                                                                { BaseLink.transform.right.z, BaseLink.transform.forward.z, BaseLink.transform.up.z },
                                                                { BaseLink.transform.right.y, BaseLink.transform.forward.y, BaseLink.transform.up.y } });
        Vector<double> W_b = -1f * BaseLink.transform.InverseTransformDirection(baseLinkAB.angularVelocity).To<ENU>().ToDense();

        // Desired states
        Vector<double> x_s_d;
        Vector<double> v_s_d;
        Vector<double> a_s_d;
        (x_s_d, v_s_d, a_s_d) = TrackingTargetTrajectory(TrackingTargetTF.position.To<ENU>().ToDense(), x_s, v_s);

        // Minum snap trajectory parameters
        double T = 5.0; // Total time for trajectory



        // TODO: Need to fix global state on trajectory here
        if (AttackTheBuoy && Rope != null)
        {
            if (min_snap_flag == 0)
            {
                min_snap_flag = 1;
            }


            if (min_snap_flag == 1)
            {
                Vector3 startPos = new Vector3((float)x_s[0], (float)x_s[1], (float)x_s[2]);
                // Vector3 startPos = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 startVel = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 startAcc = new Vector3(0.0f, 0.0f, 0.0f);

                // For now we want the end position to be the middle of the rope,
                // but later it will be refined based on the state estimation and
                // optimal touchdown point
                Vector3<ENU> endPosENU = 0.5f * (LoadLinkTF.position + Rope.GetChild(Rope.childCount - 1).position).To<ENU>();
                Vector3 endPos = new Vector3(endPosENU.x, endPosENU.y, endPosENU.z);
                //Vector3 endPos = new Vector3(10.0f, 5.0f, 3.0f);
                Vector3 endVel = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 endAcc = new Vector3(0.0f, 0.0f, 0.0f);


                // Calculate minimum snap trajectory coefficients for each axis (x, y, z)
                xTraj = new MinimumSnapTrajectory(startPos.x,
                    startVel.x,
                    startAcc.x,
                    endPos.x,
                    endVel.x,
                    endAcc.x,
                    T);
                yTraj = new MinimumSnapTrajectory(startPos.y,
                    startVel.y,
                    startAcc.y,
                    endPos.y,
                    endVel.y,
                    endAcc.y,
                    T);
                zTraj = new MinimumSnapTrajectory(startPos.z,
                    startVel.z,
                    startAcc.z,
                    endPos.z,
                    endVel.z,
                    endAcc.z,
                    T);
                min_snap_flag = 2;
            }

            if (min_snap_flag == 2 && catching_time <= T)
            {
                catching_time = catching_time + 0.1;
                double posX = xTraj.EvaluatePolynomial(catching_time);
                double posY = yTraj.EvaluatePolynomial(catching_time);
                double posZ = zTraj.EvaluatePolynomial(catching_time);

                // Evaluate velocity (first derivative)
                double velX = xTraj.EvaluatePolynomialDerivative(catching_time);
                double velY = yTraj.EvaluatePolynomialDerivative(catching_time);
                double velZ = zTraj.EvaluatePolynomialDerivative(catching_time);

                // Evaluate acceleration (second derivative)
                double accX = xTraj.EvaluatePolynomialSecondDerivative(catching_time);
                double accY = yTraj.EvaluatePolynomialSecondDerivative(catching_time);
                double accZ = zTraj.EvaluatePolynomialSecondDerivative(catching_time);


                x_s_d = DenseVector.OfArray(new double[] { posX, posY, posZ });
                v_s_d = DenseVector.OfArray(new double[] { velX, velY, velZ });
                a_s_d = DenseVector.OfArray(new double[] { accX, accY, accZ });

            }


        }
        else
        {
            catching_time = 0; // reset time
        }


        // Control
        Vector<double> errorTrackingPosition = x_s - x_s_d;
        Vector<double> errorTrackingVelocity = v_s - v_s_d;

        Vector<double> PIDGain = _ComputePIDTerm(
            kx,
            kv,
            g,
            massQuadrotor,
            a_s_d,
            errorTrackingPosition,
            errorTrackingVelocity
        );

        Matrix<double> R_sb_d = _ComputeDesiredAttitudeVectors(
            PIDGain
        );

        Vector<double> W_b_d = _VeeMap(_Logm3(R_sb_d_prev.Transpose() * R_sb_d) / dt);
        Vector<double> W_b_d_dot = (W_b_d - Omega_b_d_prev) / dt;

        Vector<double> eR = 0.5 * _VeeMap(R_sb_d.Transpose() * R_sb - R_sb.Transpose() * R_sb_d);
        Vector<double> eW = W_b - R_sb.Transpose() * R_sb_d * W_b_d;

        f = PIDGain * (R_sb * e3);
        M = -kR * eR - kW * eW + _Cross(W_b, inertiaJ * W_b) - inertiaJ * (_HatMap(W_b) * R_sb.Transpose() * R_sb_d * W_b_d - R_sb.Transpose() * R_sb_d * W_b_d_dot);

        R_sb_d_prev = R_sb_d;
        Omega_b_d_prev = W_b_d;

        if (times2 < 2 || M.Norm(2) > 100) // If previous values have not been initialized yet or moments are excessive
        {
            times2++;
            f = 0;
            M = DenseVector.OfArray(new double[] { 0, 0, 0 });
        }
        // Debug.Log($"R_sb: {R_sb}, R_sb_d: {R_sb_d} R_sb_d_prev: {R_sb_d_prev}");
        return (f, M);
    }

    void ComputeRPMs()
    {
        double f;
        Vector<double> M;

        // If rope has been replaced (tension is high enough) use suspended load controller
        // If we have not hooked the rope yet, use normal tracking controller
        if (Rope != null && Rope.childCount == 2) (f, M) = SuspendedLoadControl();
        else (f, M) = TrackingControl();

        // Compute optimal propeller forces
        Vector<double> globalForces = _StackForceMomentVector(f, M);
        Vector<double> F_star = propllerForceToGlobalMapInverse * globalForces;

        // Build a matrix A and a vector b to solve for the variation on the optimal propeller forces
        Matrix<double> A = Matrix<double>.Build.Dense(NUM_PROPS, NUM_PROPS);
        Vector<double> b = Vector<double>.Build.Dense(NUM_PROPS);
        for (int i = 0; i < NUM_PROPS; i++)
        {
            for (int j = 0; j < NUM_PROPS; j++)
            {
                if (F_star[i] >= 0 && F_star[j] >= 0)
                {
                    A[i, j] = Q[i, j];
                }
                else if (i == j)
                {
                    A[i, j] = 1;
                }
                else
                {
                    A[i, j] = 0;
                }
                if (F_star[i] >= 0 && F_star[j] < 0)
                {
                    b[i] += Q[i, j] * F_star[j];
                }
            }
            if (F_star[i] < 0)
            {
                b[i] = -F_star[i];
            }
        }
        Vector<double> F = F_star + A.Solve(b);

        // If the gradient tangent to any of the boundaries makes the solution not satisfy the constraints,
        // project it back to the boundary
        for (int i = 0; i < NUM_PROPS; i++)
        {
            if (F[i] < 0)
            {
                F[i] = 0;
            }
        }

        // Set propeller rpms
        for (int i = 0; i < propellers.Length; i++)
            propellersRPMs[i] = (float)F[i] / propellers[i].RPMToForceMultiplier;
    }

    void ApplyRPMs()
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

    (Vector<double>, Vector<double>, Vector<double>) TrackingTargetTrajectory(Vector<double> x_TT, Vector<double> x_s, Vector<double> v_s)
    {
        Vector<double> x_s_d;
        Vector<double> v_s_d;
        Vector<double> a_s_d;

        Vector<double> unitVectorTowardsTarget = (x_TT - x_s) / (x_TT - x_s).Norm(2);
        double distanceToTarget = (x_TT - x_s).Norm(2);

        if (distanceToTarget > DecelerationDistance)
        {
            x_s_d = x_s + DecelerationDistance * unitVectorTowardsTarget;
            v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });//MaxVelocityWithTrackingTarget*unitVectorTowardsTarget;
            a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        }
        else
        {
            x_s_d = x_TT;
            v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });//MaxVelocityWithTrackingTarget*distanceToTarget/DecelerationDistance*unitVectorTowardsTarget;
            a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        }

        return (x_s_d, v_s_d, a_s_d);
    }

    /// <summary>
    /// Computes "PID"-like gain for tracking controller
    ///
    /// Uses global e3 vector FIXME: this probably can be done better
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
        Matrix<double> R_sb_d = DenseMatrix.OfArray(new double[,] { { b1d[0], b2d[0], b3d[0] },
                                                                    { b1d[1], b2d[1], b3d[1] },
                                                                    { b1d[2], b2d[2], b3d[2] } });
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

    /// <summary>
    /// Stacks global force and moment vector as: [f moment[0] moment[1] moment[2]].Transpose() where 0-2 refer to axis index
    ///
    /// <para>
    /// f, referes to global force along axis of propellors hence it is only a scaler
    /// </para>
    /// </summary>
    /// <param name="f"></param>
    /// <param name="moments"></param>
    /// <returns></returns>
    private static Vector<double> _StackForceMomentVector(double f, Vector<double> moments)
    {
        return DenseVector.OfArray(new double[] { f, moments[0], moments[1], moments[2] });
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

    static Vector3 ToUnity(Vector<double> v)
    {
        return new Vector3((float)v[0], (float)v[2], (float)v[1]);
    }
}
