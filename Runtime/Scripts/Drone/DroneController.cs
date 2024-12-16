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



public enum DroneControllerState
{
    TrackingControl = 0,
    LoadControl = 1,
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
    public DroneControllerState controllerState;

    ////////////////// SYSTEM SPECIFIC //////////////////
    // Quadrotor parameters (from unity)
    double massQuadrotor;
    Matrix<double> inertiaJ;
    const int NUM_PROPS = 4;

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
        double[] propellersRPMs =  { 0, 0, 0, 0 };
        for (int i = 0; i < propellers.Length; i++)
            propellersRPMs[i] = F[i] / propellers[i].RPMToForceMultiplier;
        return propellersRPMs;
    }
}
