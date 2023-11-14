using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    [RequireComponent(typeof(CameraImage))]

    public class CameraInfo : Sensor<CameraInfoMsg>
    {
        CameraImage camImg;
        
        [Header("Camera Info")]
        [Header("Camera distortion model params for plumb_bob")]
        [Header("D")]
        public float k1=1;
        public float k2=1,t1=1,t2=1,k3=1;

        [Header("K")]
        public float fx=1;
        public float fy=1,cx=1,cy = 1;

        [Header("P")]
        public float fxp=1;
        public float fyp=1, cxp=1, cyp=1, Tx=1, Ty=1;

        void Start()
        {
            camImg = GetComponent<CameraImage>();
            ros_msg.distortion_model = "plumb_bob";
            ros_msg.D = new double[5];
            ros_msg.K = new double[9];
            ros_msg.P = new double[12];
        }


        public override bool UpdateSensor(double deltaTime)
        {
            ros_msg.height = (uint) camImg.textureHeight;
            ros_msg.width = (uint) camImg.textureWidth;
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName;

            float[] D = {k1,k2,t1,t2,k3};
            float[] K = {
                fx, 0,  cx,
                0,  fy, cy,
                0,  0,  1
            };
            float[] P = {
                fxp, 0,   cxp, Tx,
                0,   fyp, cyp, Ty,
                0,   0,   1,   0
            };
            for(int i=0; i<5; i++)  ros_msg.D[i] = D[i];
            for(int i=0; i<9; i++)  ros_msg.K[i] = K[i];
            for(int i=0; i<12; i++) ros_msg.P[i] = P[i];
            return true;
        }

    }
}