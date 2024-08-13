﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using DefaultNamespace.LookUpTable;
using VehicleComponents.Actuators;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class DroneTrackingController: MonoBehaviour {
    public GameObject base_link;
    public float computation_frequency = 100f;
	private Propeller[] propellers;
    private ArticulationBody base_link_ab;
	private float[] propellers_rpms;
    private Matrix<double> R_wa_prev;
    private Matrix<double> R_sb_d_prev;
    private Vector<double> W_b_d_prev;
    private int times = 0;
    private GameObject ufo;

	// Use this for initialization
	void Start() {
		propellers = new Propeller[4];
		propellers[0] = GameObject.Find("propeller_FL").GetComponent<Propeller>();
		propellers[1] = GameObject.Find("propeller_FR").GetComponent<Propeller>();
        propellers[2] = GameObject.Find("propeller_BR").GetComponent<Propeller>();
        propellers[3] = GameObject.Find("propeller_BL").GetComponent<Propeller>();

        base_link_ab = base_link.GetComponent<ArticulationBody>();

        R_wa_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        R_sb_d_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        W_b_d_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });

		propellers_rpms = new float[] { 0, 0, 0, 0 };

        ufo = GameObject.Find("UFO");

        InvokeRepeating("ComputeRPMs", 0f, 1f/computation_frequency);
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		// ComputeRPMs();
        ApplyRPMs();
	}

	void ComputeRPMs() {
        // Parameters
        double m = base_link_ab.mass;
        double d = 0.315;
        Matrix<double> J = DenseMatrix.OfArray(new double[,] { { base_link_ab.inertiaTensor.x, 0, 0 }, { 0, base_link_ab.inertiaTensor.z, 0 }, { 0, 0, base_link_ab.inertiaTensor.y } });
        //Matrix<double> J = DenseMatrix.OfArray(new double[,] { {0.082, 0, 0}, {0, 0.082, 0}, {0, 0, 0.1377} });
        float c_tau_f = 8.004e-4f;
        double g = 9.81;
        Vector<double> e3 = DenseVector.OfArray(new double[] { 0, 0, 1 });
        double dt = 1f/computation_frequency;

        // Gains
        double kx = 16*m;
        double kv = 5.6*m;
        double kR = 8.81;
        double kW = 2.54;
        
        // States
        // Vector<double> x_w = DenseVector.OfArray(new double[] { base_link.transform.position.x , base_link.transform.position.z, base_link.transform.position.y });
        // Vector<double> v_w = DenseVector.OfArray(new double[] { base_link_ab.velocity.x, base_link_ab.velocity.z, base_link_ab.velocity.y });
        // Matrix<double> R_wa = DenseMatrix.OfArray(new double[,] { { base_link.transform.right.x, base_link.transform.forward.x, base_link.transform.up.x },
        //                                                           { base_link.transform.right.z, base_link.transform.forward.z, base_link.transform.up.z },
        //                                                           { base_link.transform.right.y, base_link.transform.forward.y, base_link.transform.up.y } });
        // // Vector<double> W_w = -1f*DenseVector.OfArray(new double[] { base_link_ab.angularVelocity.x, base_link_ab.angularVelocity.z, base_link_ab.angularVelocity.y });
        // Vector<double> W_a = _Vee(_Logm3(R_wa_prev.Transpose()*R_wa)/dt);
        // // Vector3 W_unity = base_link.transform.InverseTransformDirection(base_link_ab.angularVelocity);
        // // Vector<double> W_a = DenseVector.OfArray(new double[] { -W_unity.x, -W_unity.z, -W_unity.y });
        // // Debug.Log($"W: {W_unity}");

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

        // Vector<double> x_s = R_sw*x_w;
        // Vector<double> v_s = R_sw*v_w;
        // Vector<double> W_b = R_ab.Transpose()*W_a;
        
        // Desired states
        float t = Time.time;
        Vector<double> x_s_d = R_sw*DenseVector.OfArray(new double[] { ufo.transform.position.x, ufo.transform.position.z, ufo.transform.position.y });//{ 0, t-15, Mathf.Pow(t-5, 2) });
        Vector<double> v_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0 });//{ 0, 1, 2*(t-5) });
        Vector<double> a_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 0 });//{ 0, 0, 2 });
        Vector<double> b1d = DenseVector.OfArray(new double[] { 1, 0, 0 });

        // Control
        Vector<double> ex = x_s - x_s_d;
        Vector<double> ev = v_s - v_s_d;

        Vector<double> pid = -kx*ex - kv*ev - m*g*e3 + m*a_s_d;
        Vector<double> b3d = -pid/pid.Norm(2);
        Vector<double> b2d = _Cross(b3d, b1d)/_Cross(b3d, b1d).Norm(2);
        Vector<double> b1d_temp = _Cross(b2d, b3d);
        Matrix<double> R_sb_d = DenseMatrix.OfArray(new double[,] { { b1d_temp[0], b2d[0], b3d[0] },
                                                                    { b1d_temp[1], b2d[1], b3d[1] },
                                                                    { b1d_temp[2], b2d[2], b3d[2] } });
        
        Vector<double> W_b_d = _Vee(_Logm3(R_sb_d_prev.Transpose()*R_sb_d)/dt);
        Vector<double> W_b_dot_d = (W_b_d - W_b_d_prev)/dt;

        Vector<double> eR = 0.5*_Vee(R_sb_d.Transpose()*R_sb - R_sb.Transpose()*R_sb_d);
        Vector<double> eW = W_b - R_sb.Transpose()*R_sb_d*W_b_d;

        double f = -pid*(R_sb*e3);
        Vector<double> M = -kR*eR - kW*eW + _Cross(W_b, J*W_b) - J*(_Hat(W_b)*R_sb.Transpose()*R_sb_d*W_b_d - R_sb.Transpose()*R_sb_d*W_b_dot_d);

        R_wa_prev = R_wa;
        R_sb_d_prev = R_sb_d;
        W_b_d_prev = W_b_d;

        // Convert to propeller forces
        Matrix<double> T = DenseMatrix.OfArray(new double[,] { { 1, 1, 1, 1 }, { 0, -d, 0, d }, { d, 0, -d, 0 }, { -c_tau_f, c_tau_f, -c_tau_f, c_tau_f } });
        Vector<double> F = T.Inverse() * DenseVector.OfArray(new double[] { f, M[0], M[1], M[2] });

        if (times < 2) {
            times++;
            F = DenseVector.OfArray(new double[] { 0, 0, 0, 0 });
        }

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
}