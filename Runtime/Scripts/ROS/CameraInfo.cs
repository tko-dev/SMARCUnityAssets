using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    public class CameraInfo : Sensor<CameraInfoMsg>
    {
        SensorCamera sensorCam;

        void Start()
        {
            sensorCam = GetComponent<SensorCamera>();
        }


        public override bool UpdateSensor(double deltaTime)
        {
            ros_msg.height = (uint) sensorCam.textureHeight;
            ros_msg.width = (uint) sensorCam.textureWidth;
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName;
            return true;
        }

    }
}