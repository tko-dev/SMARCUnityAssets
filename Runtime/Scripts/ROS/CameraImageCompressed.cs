using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    [RequireComponent(typeof(CameraImage))]

    public class CameraImageCompressed : Sensor<CompressedImageMsg>
    {
        [Header("Compressed Image")]
        [Tooltip("Jpg compression quality. 1=lowest quality")]
        [Range(1,100)]
        public int quality = 75;
        CameraImage camImg;
        byte[] ros_img;

        void Start()
        {
            camImg = GetComponent<CameraImage>();
            ros_msg.format = "rgb8;jpeg compressed rgb8";
        }


        public override bool UpdateSensor(double deltaTime)
        {
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName;
            ros_img = ImageConversion.EncodeToJPG(camImg.image, quality);
            ros_msg.data = ros_img;
            Debug.Log($"Compressed image from {robotLinkName}");
            return true;
        }

    }
}