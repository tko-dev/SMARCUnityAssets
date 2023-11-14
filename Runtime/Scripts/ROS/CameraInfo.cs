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

        void Start()
        {
            camImg = GetComponent<CameraImage>();
        }


        public override bool UpdateSensor(double deltaTime)
        {
            ros_msg.height = (uint) camImg.textureHeight;
            ros_msg.width = (uint) camImg.textureWidth;
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName;
            return true;
        }

    }
}