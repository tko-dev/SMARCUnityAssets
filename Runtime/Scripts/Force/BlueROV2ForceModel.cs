using System;
using DefaultNamespace.LookUpTable;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using VehicleComponents.ROS.Subscribers;
// using RosMsgType = RosMessageTypes.SaabMsgs.MPCmsgMsg;

namespace DefaultNamespace
{
    public class BlueROV2ForceModel : MonoBehaviour // Actuator_Sub<RosMsgType>
    {
        
        // protected override void UpdateVehicle(bool reset)
        // {
        //     // Implement what happens when a new message is received
        //     print(ROSMsg);
        // }
        
        public ArticulationBody mainBody;
        public ArticulationBody propeller_front_left_top;
        public ArticulationBody propeller_front_right_top;
        public ArticulationBody propeller_back_left_top;
        public ArticulationBody propeller_back_right_top;
        public ArticulationBody propeller_front_left_bottom;
        public ArticulationBody propeller_front_right_bottom;
        public ArticulationBody propeller_back_left_bottom;
        public ArticulationBody propeller_back_right_bottom;

        // public double rpm_front_left_top = 0.0f;
        // public double rpm_front_right_top = 0.0f;
        // public double rpm_back_left_top = 0.0f;
        // public double rpm_back_right_top = 0.0f;
        // public double rpm_front_left_bottom = 0.0f;
        // public double rpm_front_right_bottom = 0.0f;
        // public double rpm_back_left_bottom = 0.0f;
        // public double rpm_back_right_bottom = 0.0f;
        
        
        //Variables
        private Vector<double> vel_vec_prev = Vector<double>.Build.DenseOfArray(new double[] { 0, 0, 0 ,0, 0, 0 });
        private Camera myCamera;
        
        //Constants
        public double vbs = 0.0f; //some weird thing
        private double m = 0; //mass kg
        private double W = 0; //weight N
        private double B = 0; // bouyancy N
        double g = 9.82; // gravity m/s²
        double rho = 1000; // water density [kg/m^3]
        double nabla = 0.0134; // volume of BlueRoV [m^3], given experimental by OSBS
        
        //Bouyancy point coordinates relative to report coordinate system
        double  x_b = 0; double y_b = 0; double z_b = -0.01;
        
        //Added from OSBS
        //Rotational damping (Ns/m)
        double Xuu = 141; // #1.0
        double Yvv = 217; // #100.0
        double Zww = 190; // #100.0
        double Kpp = 1.19; // #10.0
        double Mqq = 0.47; // #100.0
        double Nrr = 1.5; // #150.0
        
        //Translational damping (Ns/m)
        double Xu = 13.7;
        double Yv = 0;
        double Zw = 33;
        double Kp = 0;
        double Mq = 0.8;
        double Nr = 0;
        
        // Added mass coefficients 
        double X_udot = 6.36; // [kg]
        double Y_vdot = 7.12; // [kg]
        double Z_wdot = 18.68; // [kg]
        double K_pdot = 0.189; // [kg*m^2]
        double M_qdot = 0.135; // [kg*m^2]
        double N_rdot = 0.222; // [kg*m^2]
        
        //Inertia 
        double I_x = 0.26; // [kg*m^2], from OSBS's CAD
        double I_y = 0.23; // [kg*m^2], from OSBS's CAD
        double I_z = 0.37; // [kg*m^2], from OSBS's CAD

        public void Start()
        {
            myCamera =  Camera.main;
            var camera_offset = new Vector3(0f, 0.5f, -2f);
            // mass 11
            m = mainBody.mass;
            W = m * g; // weight In OSBS they use g = 9.82
            B = rho*g*nabla; // The buoyancy in [N] given by OSBS
        }
        
        public void FixedUpdate()
        {
            // var world_pos = mainBody.transform.position; // Needs to be verified
            var world_rot = mainBody.transform.rotation.eulerAngles; 
            // print(mainBody.angularVelocity);
            
            //Convert state vector from global to local reference point
            var inverseTransformDirection = mainBody.transform.InverseTransformDirection(mainBody.velocity); // Local frame vel
            var transformAngularVelocity = mainBody.transform.InverseTransformDirection(mainBody.angularVelocity); // Local frame angular vel
            // print(transformAngularVelocity);
            
            // Convert to OSBS coordinate system
            var phiThetaTau = -FRD.ConvertAngularVelocityFromRUF(world_rot).ToDense();
            float phi = (float) (Mathf.Deg2Rad * phiThetaTau[0]); 
            float theta = (float) (Mathf.Deg2Rad* phiThetaTau[1]);
            
            var uvw = inverseTransformDirection.To<NED>().ToDense(); // Might need to revisit. Rel. velocity in point m block.
            float u = (float) uvw[0];
            float v = (float) uvw[1];
            float w = (float) uvw[2];
            // print(uvw[0]+","+uvw[1]+","+uvw[2]);
            
            var pqr = -FRD.ConvertAngularVelocityFromRUF(transformAngularVelocity).ToDense(); // FRD is same as NED for ANGLES ONLY
            float p = (float) pqr[0];
            float q = (float) pqr[1];
            float r = (float) pqr[2];
            // print(pqr[0]+","+pqr[1]+","+pqr[2]);    
        
            //init state vector
            Vector<double> vel_vec = Vector<double>.Build.DenseOfArray(new double[] { u, v, w, p, q, r }); 
            
           // Vector3 I_vec = new Vector3((float) I_x, (float) I_y, (float) I_z);
           // Matrix<double> I_c = DenseMatrix.OfDiagonalArray(new double[] {I_x, I_y, I_z}); // TODO: check how we actually create diagonal matrices 
            
            // TODO: scrap?
           Matrix<double> M_RB = DenseMatrix.OfDiagonalArray(new double[] {m, m, m, I_x, I_y, I_z});
           Matrix<double> M_A = DenseMatrix.OfDiagonalArray(new double[] {X_udot, Y_vdot, Z_wdot, K_pdot, M_qdot, N_rdot});
           Matrix<double> M = M_RB + M_A;
            
           
            // Matrix<double> someMatrix = DenseMatrix.OfColumnMajor(4, 4,  new double[] { 11, 12, 13, 14, 21, 22, 23, 24, 31, 32, 33, 34, 41, 42, 43, 44 });
            // For Coriollis and centripetal forces
            Matrix<double> C_RB = DenseMatrix.OfArray(new double[,]
            {
                {0,     0,      0,      0,      m*w,    -m*v    },
                {0,     0,      0,      -m*w,   0,       m*u    },
                {0,     0,      0,      m*v,    -m*u,    0      },
                {0,     m*w,    -m*v,   0,      -I_z*r, -I_y*q  },
                {-m*w,  0,      m*u,    I_z*r,  0,       I_x*p  },
                {m*v,   -m*u,   0,      I_y*q,  -I_x*p,  0      },
            });
            Matrix<double> C_A = DenseMatrix.OfArray(new double[,] //wack
            {
                {0,         0,          0,          0,          -Z_wdot*w,  Y_vdot*v    },
                {0,         0,          0,          Z_wdot*w,   0,          -X_udot*u   },
                {0,         0,          0,          -Y_vdot*v,  X_udot*u,   0           },
                {0,         -Z_wdot*w,  Y_vdot*v,   0,          -N_rdot*r,  M_qdot*q    },
                {Z_wdot*w,  0,          -X_udot*u,  N_rdot*r,   0,          -K_pdot*p   },
                {-Y_vdot*v, X_udot*u,   0,          -M_qdot*q,  K_pdot*p,   0           }
            });
            Matrix<double> C = C_RB + C_A;
            
            // g_vec
            Vector<double> g_vec = Vector<double>.Build.DenseOfArray(new double[] 
                {
                (W-B)*Mathf.Sin(theta),
                -(W-B)*Mathf.Cos(theta)*Mathf.Sin(phi),
                -(W-B)*Mathf.Cos(theta)*Mathf.Cos(phi),
                y_b*B*Mathf.Cos(theta)*Mathf.Cos(phi)-z_b*B*Mathf.Cos(theta)*Mathf.Sin(phi),
                -z_b*B*Mathf.Sin(theta)-x_b*B*Mathf.Cos(theta)*Mathf.Cos(phi),
                x_b*B*Mathf.Cos(theta)*Mathf.Sin(phi)+y_b*B*Mathf.Sin(theta)
                }
            );
            // mainBody.inertiaTensor = I_vec;  
            //mainBody.mass = (float) m;

            //Usually the control and damping forces need to be computed separately 
            Matrix<double> D = DenseMatrix.OfDiagonalArray(new double[]
            {
                Xu,
                Yv,
                Zw,
                Kp,
                Mq,
                Nr
            });
            
            Matrix<double> Dn = DenseMatrix.OfDiagonalArray(new double[] 
            {
                Xuu*Mathf.Abs(u),
                Yvv*Mathf.Abs(v), 
                Zww*Mathf.Abs(w),
                Kpp*Mathf.Abs(p),
                Mqq*Mathf.Abs(q), 
                Nrr*Mathf.Abs(r)
            });
            Matrix<double> D_of_vel = D + Dn;

            //var mainBodyVelocity = mainBody.velocity;
            //var mainBodyAngularVelocity = mainBody.angularVelocity;
            //var mainBodyMass = mainBody.mass;

            //added mass stuff
            var vel_vec_dot = (vel_vec-vel_vec_prev)/Time.fixedDeltaTime; // ta acceleration från input forces, M matrix
            vel_vec_prev = vel_vec;
            
            // TODO: resulting forces -> lateral forces + coriolis
            var tau_sum_coriolis =  C * vel_vec;
            var v_c = 0; // Assume no ocean current. If desired to integrete it, info about it can be found in OSBS
            var vr = vel_vec - v_c;
            var tau_sum_damping = D_of_vel*vr; 

            // Resulting force and torque vectors
            // 3 first elements of tau_sum is force control
            // 3 last elements of tau_sum is torque control
            var coriolisForce  = tau_sum_coriolis.SubVector(0, 3).ToVector3();
            var coriolisTorque = tau_sum_coriolis.SubVector(3, 3).ToVector3();
            var RestoringForce  = g_vec.SubVector(0, 3).ToVector3();
            var RestoringTorque = g_vec.SubVector(3, 3).ToVector3();
            var force_damping = tau_sum_damping.SubVector(0, 3).ToVector3(); //Vector3.zero; //These will be replaced with your model output
            var torque_damping = tau_sum_damping.SubVector(3, 3).ToVector3();
            
            // print(torque_damping[0] + "," + torque_damping[1] + "," + torque_damping[2]);
            // print(vr[0] + "," + vr[1] + "," + vr[2] + "," + vr[3] + "," + vr[4] + "," + vr[5]);
            // print(tau_sum_dampining[0] + "," + tau_sum_dampining[1] + "," + tau_sum_dampining[2] + "," + tau_sum_dampining[3] + "," + tau_sum_dampining[4] + "," + tau_sum_dampining[5]);

            // Vector3 vel_test = Vector3.one;
            // print(vel_test[0] + "," + vel_test[1] + "," + vel_test[2]);
            // vel_test = -FRD.ConvertAngularVelocityFromRUF(vel_test);
            // print(vel_test[0] + "," + vel_test[1] + "," + vel_test[2]);
            // print("B:" + B +  "W:" + W);
            // print(RestoringForce[0] + "," + RestoringForce[1] + "," + RestoringForce[2]);
            // Back to RUF (Unity) coordinates)
            force_damping = NED.ConvertToRUF(force_damping);
            torque_damping = -FRD.ConvertAngularVelocityToRUF(torque_damping);
            coriolisForce = NED.ConvertToRUF(coriolisForce);
            coriolisTorque = -FRD.ConvertAngularVelocityToRUF(coriolisTorque);
            RestoringForce = NED.ConvertToRUF(RestoringForce);
            RestoringTorque = -FRD.ConvertAngularVelocityToRUF(RestoringTorque);
            
            // print("NED" +RestoringForce[0] + "," + RestoringForce[1] + "," + RestoringForce[2]);
            
            // print(torque_damping[0] + "," + torque_damping[1] + "," + torque_damping[2]);
            
            // VVV UNCOMMENT FOR FOLLOWING CAMERA VVV
            // myCamera.transform.position = camera_offset + world_pos;
           
            Vector3 inputForce = Vector3.zero;
            Vector3 inputTorque = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                inputForce[2] = 85;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputForce[0] = -85;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputForce[2] = -85;
            }
            if (Input.GetKey(KeyCode.D))
            {
                inputForce[0] = 85;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                inputForce[1] = 120;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                inputForce[1] = -120;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                inputTorque[1] = -14;
            }
            if (Input.GetKey(KeyCode.E))
            {
                inputTorque[1] = 14;
            }
            if (Input.GetKey(KeyCode.X))
            {
                inputTorque[0] = 14;
            }
        
            // ADDED MASS
            
            var input_forces = inputForce.To<NED>().ToDense(); // Might need to revisit. Rel. velocity in point m block.
            
            var input_torques = -FRD.ConvertAngularVelocityFromRUF(inputTorque).ToDense(); // FRD is same as NED for ANGLES ONLY         
            var reactive_force_sum = (-g_vec - tau_sum_damping - tau_sum_coriolis);
            
            Vector<double> input_forces_sum  = Vector<double>.Build.DenseOfArray(new double[] {input_forces[0], input_forces[1], input_forces[2], input_torques[0], input_torques[1], input_torques[2] });
            var total_force_sum = reactive_force_sum + input_forces_sum;
            
            Matrix<double> M_inv = DenseMatrix.OfDiagonalArray(new double[]
            {
                0.0504,
                0.0485,
                0.0311,
                2.2272,
                2.7397,
                1.6892
            });
            
            // print(vel_vec_dot);
            vel_vec_dot = M_inv*total_force_sum;
            var added_inertia = M_A * vel_vec_dot;
            // print(added_inertia[0] + "," + added_inertia[1] + "," + added_inertia[2] + "," + added_inertia[3] + "," + added_inertia[4] + "," + added_inertia[5]);

            // print(vel_vec_dot);
            //
            var addedForce = added_inertia.SubVector(0, 3).ToVector3();
            var addedTorque = added_inertia.SubVector(3, 3).ToVector3();
            addedForce = NED.ConvertToRUF(addedForce);
            addedTorque = -FRD.ConvertAngularVelocityToRUF(addedTorque);
            
                
           // print("x" + vel_vec[0]);
           //  print("y" + vel_vec[1]);
           //  print("z" + vel_vec[2]);
            // print(force_damping);
            // print(torque_damping);
            
            // ADD forces to rigid body 
            mainBody.AddRelativeForce(-force_damping); //invert
            // mainBody.AddRelativeForce(-coriolisForce); //invert
            mainBody.AddRelativeForce(-RestoringForce); //invert
            // mainBody.AddRelativeForce(-addedForce);
            mainBody.AddRelativeForce(inputForce);
            mainBody.AddRelativeTorque(-torque_damping);
            // mainBody.AddRelativeTorque(-coriolisTorque);
            mainBody.AddRelativeTorque(-RestoringTorque);
            // mainBody.AddRelativeTorque(-addedTorque);
            mainBody.AddRelativeTorque(inputTorque);
            // added mass torque and force


            // Set RPMs for Visuals
            propeller_front_left_top.SetDriveTargetVelocity(ArticulationDriveAxis.X, 0);
            propeller_front_right_top.SetDriveTargetVelocity(ArticulationDriveAxis.X, 0);
            propeller_back_left_top.SetDriveTargetVelocity(ArticulationDriveAxis.X, 0);
            propeller_back_right_top.SetDriveTargetVelocity(ArticulationDriveAxis.X, 0);

            propeller_front_left_bottom.SetDriveTargetVelocity(ArticulationDriveAxis.Z, 0);
            propeller_front_right_bottom.SetDriveTargetVelocity(ArticulationDriveAxis.Z, 0);
            propeller_back_left_bottom.SetDriveTargetVelocity(ArticulationDriveAxis.Z, 0);
            propeller_back_right_bottom.SetDriveTargetVelocity(ArticulationDriveAxis.Z, 0);
        }
    }
}
