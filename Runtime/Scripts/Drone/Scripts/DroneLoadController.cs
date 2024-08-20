using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using DefaultNamespace.LookUpTable;
using VehicleComponents.Actuators;
using Rope;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class DroneLoadController: MonoBehaviour {
    public GameObject base_link;
    public GameObject rope_link;
    public float computation_frequency = 50f;
    public bool follow_sphere = false;
	private Propeller[] propellers;
    private ArticulationBody base_link_ab;
	private float[] propellers_rpms;
    private Matrix<double> R_sb_d_prev;
    private Matrix<double> R_sb_c_prev;
    private Vector<double> W_b_d_prev;
    private Vector<double> W_b_c_prev;
    private Vector<double> q_c_prev;
    private Vector<double> q_c_dot_prev;
    private int times1 = 0;
    private int times2 = 0;
    // private GameObject sphere;
    private Transform buoy;
    private ArticulationBody rope_link_ab;

    // Quadrotor parameters
    double mQ;
    double d;
    Matrix<double> J;
    float c_tau_f;

    // Load parameters
    double mL;
    double l;

    // Simulation parameters
    double g;
    Vector<double> e3;
    double dt;
    float t;

    // Gains
    double kx;
    double kv;
    double kR;
    double kW;
    double kq;
    double kw;

	// Use this for initialization
	void Start() {
		propellers = new Propeller[4];
		propellers[0] = GameObject.Find("propeller_FL").GetComponent<Propeller>();
		propellers[1] = GameObject.Find("propeller_FR").GetComponent<Propeller>();
        propellers[2] = GameObject.Find("propeller_BR").GetComponent<Propeller>();
        propellers[3] = GameObject.Find("propeller_BL").GetComponent<Propeller>();

        base_link_ab = base_link.GetComponent<ArticulationBody>();

        R_sb_d_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        R_sb_c_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        W_b_d_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        W_b_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_dot_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });

		propellers_rpms = new float[] { 0, 0, 0, 0 };

        // sphere = GameObject.Find("Sphere");
        var rope = GameObject.Find("Rope");
        buoy = rope.transform.GetChild(rope.transform.childCount-1);
        // TODO: For now the position of the AUV is taken at the base of the rope
        rope_link_ab = rope_link.GetComponent<ArticulationBody>();
        
        // Quadrotor parameters
        mQ = base_link_ab.mass + 0.026;
        d = 0.315;
        J = DenseMatrix.OfArray(new double[,] { { base_link_ab.inertiaTensor.x, 0, 0 }, { 0, base_link_ab.inertiaTensor.z, 0 }, { 0, 0, base_link_ab.inertiaTensor.y } });
        c_tau_f = 8.004e-4f;

        // Load parameters
        mL = 12.012 + 2.7 + 0.3;
        l = 1 + 0.32;

        // Simulation parameters
        g = 9.81;
        e3 = DenseVector.OfArray(new double[] { 0, 0, 1 });
        dt = 1f/computation_frequency;

        // Gains
        kx = 16*mQ;
        kv = 5.6*mQ;
        kR = 8.81;
        kW = 2.54;
        kq = 1;
        kw = 1;

        // InvokeRepeating("ComputeRPMs", 0f, dt);
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		ComputeRPMs();
        ApplyRPMs();
	}

	void ComputeRPMs() {
        t = Time.time;

        double f;
        Vector<double> M;

        if (true) {          
            // Quadrotor states
            Vector<double> xQ_s = base_link.transform.position.To<NED>().ToDense();
            Vector<double> vQ_s = base_link_ab.velocity.To<NED>().ToDense();
            Matrix<double> R_wa = DenseMatrix.OfArray(new double[,] { { base_link.transform.right.x, base_link.transform.forward.x, base_link.transform.up.x },
                                                                    { base_link.transform.right.z, base_link.transform.forward.z, base_link.transform.up.z },
                                                                    { base_link.transform.right.y, base_link.transform.forward.y, base_link.transform.up.y } });
            Vector<double> W_b = FRD.ConvertAngularVelocityFromRUF(base_link.transform.InverseTransformDirection(base_link_ab.angularVelocity)).ToDense();

            // Load states
            Vector<double> xL_s = rope_link.transform.position.To<NED>().ToDense();
            Vector<double> vL_s = rope_link_ab.velocity.To<NED>().ToDense();
            Vector<double> q = (xL_s - xQ_s)/(xL_s - xQ_s).Norm(2);
            Vector<double> q_dot = unit_vector_derivative(xL_s - xQ_s, vL_s - vQ_s);

            // Transformations
            Matrix<double> R_ws = DenseMatrix.OfArray(new double[,] { { 0, 1, 0 },
                                                                    { 1, 0, 0 },
                                                                    { 0, 0, -1 } });
            Matrix<double> R_ab = R_ws;
            Matrix<double> R_sw = R_ws.Transpose();
            Matrix<double> R_sb = R_sw*R_wa*R_ab;
            Matrix<double> R_sa = R_sw*R_wa;
            Matrix<double> R_bs = R_sb.Transpose();
            Matrix<double> R_bw = R_bs*R_sw;

            // Desired states
            Vector<double> xL_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, Math.Pow(0.5*t-5, 2) });
            Vector<double> vL_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0.5*t-5 });
            Vector<double> aL_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0.5 });
            // if (follow_sphere) {
            //     xL_s_d = sphere.transform.position.To<NED>().ToDense();
            //     vL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });//sphere.GetComponent<ArticulationBody>().velocity.To<NED>().ToDense();
            //     aL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });//sphere.GetComponent<ArticulationBody>().acceleration.To<NED>().ToDense();
            // } else {
            //     xL_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, Math.Pow(0.5*t-5, 2) });
            //     vL_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0.5*t-5 });
            //     aL_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0.5 });
            // }
            Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, -Math.Sqrt(2)/2, 0 });

            // Control
            Vector<double> ex = xL_s - xL_s_d;
            Vector<double> ev = vL_s - vL_s_d;

            Vector<double> A = -kx*ex - kv*ev + (mQ+mL)*(aL_s_d + g*e3) + mQ*l*(q_dot*q_dot)*q;
            Vector<double> q_c = -A/A.Norm(2);
            Vector<double> q_c_dot = (q_c - q_c_prev)/dt;
            Vector<double> q_c_ddot = (q_c_dot - q_c_dot_prev)/dt;
            // Debug.DrawRay(ToUnity(xQ_s), ToUnity(-(mQ+mL)*(aL_s_d + g*e3)), Color.red);

            Vector<double> eq = _Hat(q)*_Hat(q)*q_c;
            Vector<double> eq_dot = q_dot - _Cross(_Cross(q_c, q_c_dot), q);
            
            Vector<double> F_n = (A*q)*q;
            Vector<double> F_pd = -kq*eq - kw*eq_dot;
            Vector<double> F_ff = mQ*l*(q*_Cross(q_c, q_c_dot))*_Cross(q, q_dot) + mQ*l*_Cross(_Cross(q_c, q_c_ddot), q);
            Vector<double> F_total = F_n - F_pd - F_ff;
            // Debug.DrawRay(ToUnity(xQ_s), ToUnity(F_n), Color.green);
            // Debug.DrawRay(ToUnity(xQ_s), ToUnity(-F_pd), Color.blue);
            // Debug.DrawRay(ToUnity(xQ_s), ToUnity(-F_ff), Color.yellow);
            Vector<double> b3c = F_total/F_total.Norm(2);
            Vector<double> b1c = -_Cross(b3c, _Cross(b3c, b1d))/_Cross(b3c, b1d).Norm(2);
            Vector<double> b2c = _Cross(b3c, b1c);
            Matrix<double> R_sb_c = DenseMatrix.OfArray(new double[,] { { b1c[0], b2c[0], b3c[0] },
                                                                        { b1c[1], b2c[1], b3c[1] },
                                                                        { b1c[2], b2c[2], b3c[2] } });
            Debug.DrawRay(ToUnity(xQ_s), ToUnity(b1c), Color.red);
            Debug.DrawRay(ToUnity(xQ_s), ToUnity(b2c), Color.green);
            Debug.DrawRay(ToUnity(xQ_s), ToUnity(b3c), Color.blue);
        
            Matrix<double> R_sb_c_dot = (R_sb_c - R_sb_c_prev)/dt;
            Vector<double> W_b_c = _Vee(R_sb_c.Transpose()*R_sb_c_dot);
            Vector<double> W_b_c_dot = (W_b_c - W_b_c_prev)/dt;

            Vector<double> eR = 0.5*_Vee(R_sb_c.Transpose()*R_sb - R_sb.Transpose()*R_sb_c);
            Vector<double> eW = W_b - R_sb.Transpose()*R_sb_c*W_b_c;

            f = F_total*(R_sb*e3);
            M = -kR*eR - kW*eW + _Cross(W_b, J*W_b) - J*(_Hat(W_b)*R_sb.Transpose()*R_sb_c*W_b_c - R_sb.Transpose()*R_sb_c*W_b_c_dot);
            // M = DenseVector.OfArray(new double[] { 0, 0, 0 });

            // Save previous values
            R_sb_c_prev = R_sb_c;
            W_b_c_prev = W_b_c;
            q_c_prev = q_c;
            q_c_dot_prev = q_c_dot;

            if (times1 < 20) {
                times1++;
                f = 0;
                M = DenseVector.OfArray(new double[] { 0, 0, 0 });
            }

        } else {
            // Quadrotor states
            Vector<double> x_s = base_link.transform.position.To<NED>().ToDense();
            Vector<double> v_s = base_link_ab.velocity.To<NED>().ToDense();
            Matrix<double> R_wa = DenseMatrix.OfArray(new double[,] { { base_link.transform.right.x, base_link.transform.forward.x, base_link.transform.up.x },
                                                                    { base_link.transform.right.z, base_link.transform.forward.z, base_link.transform.up.z },
                                                                    { base_link.transform.right.y, base_link.transform.forward.y, base_link.transform.up.y } });
            Vector<double> W_b = FRD.ConvertAngularVelocityFromRUF(base_link.transform.InverseTransformDirection(base_link_ab.angularVelocity)).ToDense();

            // Transformations
            Matrix<double> R_ws = DenseMatrix.OfArray(new double[,] { { 0, 1, 0 },
                                                                    { 1, 0, 0 },
                                                                    { 0, 0, -1 } });
            Matrix<double> R_ab = R_ws;
            Matrix<double> R_sw = R_ws.Transpose();
            Matrix<double> R_sb = R_sw*R_wa*R_ab;
            Matrix<double> R_sa = R_sw*R_wa;
            Matrix<double> R_bs = R_sb.Transpose();
            Matrix<double> R_bw = R_bs*R_sw;

            // Desired states
            Vector<double> buoy_w = R_ws*buoy.position.To<NED>().ToDense();
            Vector<double> x_s_d = R_sw*DenseVector.OfArray(new double[] { buoy_w[0], buoy_w[1], Math.Pow(0.5*t-5, 2) + 0.32 + buoy_w[2] });
            Vector<double> v_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0.5*t-5 });
            Vector<double> a_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0.5 });
            Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, -Math.Sqrt(2)/2, 0 });

            // Control
            Vector<double> ex = x_s - x_s_d;
            Vector<double> ev = v_s - v_s_d;

            Vector<double> pid = -kx*ex - kv*ev - mQ*g*e3 + mQ*a_s_d;
            Vector<double> b3d = -pid/pid.Norm(2);
            Vector<double> b2d = _Cross(b3d, b1d)/_Cross(b3d, b1d).Norm(2);
            Vector<double> b1d_temp = _Cross(b2d, b3d);
            Matrix<double> R_sb_d = DenseMatrix.OfArray(new double[,] { { b1d_temp[0], b2d[0], b3d[0] },
                                                                        { b1d_temp[1], b2d[1], b3d[1] },
                                                                        { b1d_temp[2], b2d[2], b3d[2] } });
            
            Vector<double> W_b_d = _Vee(_Logm3(R_sb_d_prev.Transpose()*R_sb_d)/dt);
            Vector<double> W_b_d_dot = (W_b_d - W_b_d_prev)/dt;

            Vector<double> eR = 0.5*_Vee(R_sb_d.Transpose()*R_sb - R_sb.Transpose()*R_sb_d);
            Vector<double> eW = W_b - R_sb.Transpose()*R_sb_d*W_b_d;

            f = -pid*(R_sb*e3);
            M = -kR*eR - kW*eW + _Cross(W_b, J*W_b) - J*(_Hat(W_b)*R_sb.Transpose()*R_sb_d*W_b_d - R_sb.Transpose()*R_sb_d*W_b_d_dot);

            R_sb_d_prev = R_sb_d;
            W_b_d_prev = W_b_d;

            if (times2 < 2) {
                times2++;
                f = 0;
                M = DenseVector.OfArray(new double[] { 0, 0, 0 });
            }
        }

        // Convert to propeller forces
        Matrix<double> T = DenseMatrix.OfArray(new double[,] { { 1, 1, 1, 1 }, { 0, -d, 0, d }, { d, 0, -d, 0 }, { -c_tau_f, c_tau_f, -c_tau_f, c_tau_f } });
        Vector<double> F = T.Inverse() * DenseVector.OfArray(new double[] { f, M[0], M[1], M[2] });

        // Debug.Log(F);

        // Set propeller rpms
        propellers_rpms[0] = (float)F[0]/0.005f;
        propellers_rpms[1] = (float)F[1]/0.005f;
        propellers_rpms[2] = (float)F[2]/0.005f;
        propellers_rpms[3] = (float)F[3]/0.005f;
	}

	void ApplyRPMs() {
		for (int i = 0; i < 4; i++) {
            propellers[i].SetRpm(propellers_rpms[i]);
        }
	}

    static Vector<double> _Cross(Vector<double> a, Vector<double> b) {
        // Calculate each component of the cross product
        double c1 = a[1] * b[2] - a[2] * b[1];
        double c2 = a[2] * b[0] - a[0] * b[2];
        double c3 = a[0] * b[1] - a[1] * b[0];

        // Create a new vector for the result
        return DenseVector.OfArray(new double[] { c1, c2, c3 });
    }

    static Matrix<double> _Hat(Vector<double> v) {
        return DenseMatrix.OfArray(new double[,] { { 0, -v[2], v[1] },
                                                   { v[2], 0, -v[0] },
                                                   { -v[1], v[0], 0 } });
    }
    
    static Vector<double> _Vee(Matrix<double> S) {
        return DenseVector.OfArray(new double[] { S[2, 1], S[0, 2], S[1, 0] });
    }

    static Matrix<double> _Logm3(Matrix<double> R) {
		double acosinput = (R[0, 0] + R[1, 1] + R[2, 2] - 1) / 2.0;
		Matrix<double> m_ret = DenseMatrix.OfArray(new double[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } });
		if (acosinput >= 1)
			return m_ret;
		else if (acosinput <= -1) {
			Vector<double> omg;
			if (!(Math.Abs(1 + R[2, 2]) < 1e-6f))
				omg = (1.0 / Math.Sqrt(2 * (1 + R[2, 2])))*DenseVector.OfArray(new double[] { R[0, 2], R[1, 2], 1 + R[2, 2] });
			else if (!(Math.Abs(1 + R[1, 1]) < 1e-6f))
				omg = (1.0 / Math.Sqrt(2 * (1 + R[1, 1])))*DenseVector.OfArray(new double[] { R[0, 1], 1 + R[1, 1], R[2, 1] });
			else
				omg = (1.0 / Math.Sqrt(2 * (1 + R[0, 0])))*DenseVector.OfArray(new double[] { 1 + R[0, 0], R[1, 0], R[2, 0] });
			m_ret = _Hat(Math.PI * omg);
			return m_ret;
		}
		else {
			double theta = Math.Acos(acosinput);
			m_ret = theta / 2.0 / Math.Sin(theta)*(R - R.Transpose());
			return m_ret;
		}
	}

    static Vector3 ToUnity(Vector<double> v) {
        return new Vector3((float)v[1], -1f*(float)v[2], (float)v[0]);
    }

    static Vector<double> unit_vector_derivative(Vector<double> r, Vector<double> r_dot) {
        return r_dot/r.Norm(2) - Math.Pow(r.Norm(2), -3)*(r*r_dot)*r;
    }
}