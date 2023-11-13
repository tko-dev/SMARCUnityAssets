using System; //Bit converter
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    public class SensorCamera : Sensor<ImageMsg>
    {
        public readonly int textureWidth = 640;
        public readonly int textureHeight = 480;
        RenderTexture rendTex;
        Texture2D tex;
        Camera cam;
        byte[] ros_img;


        public bool viewCam=true;
        public int viewX=100;
        public int viewY=30;
        public int viewHeight = 500;
        public int viewWidth = 500;

        void Start()
        {
            rendTex = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);

            cam = GetComponent<Camera>();
            cam.targetTexture = rendTex;

            ros_img = new byte[textureHeight * textureWidth * 3];
            tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);


            ros_msg.encoding = "rgb8";
            ros_msg.height = (uint) textureHeight;
            ros_msg.width = (uint) textureWidth;
            ros_msg.is_bigendian = 0;
            ros_msg.step = (uint)(3*textureWidth);

        }

        public override bool UpdateSensor(double deltaTime)
        {
            // If need be, use AsyncGPUReadback.RequestIntoNativeArray
            // for asynch render->texture movement

            // gotta read from the ARGB32 render into RGB24 (which is rgb8 in ros... THANK YOU.)
            RenderTexture.active = rendTex;
            tex.ReadPixels (new Rect (0, 0, textureWidth, textureHeight), 0, 0);
            tex.Apply ();
            RenderTexture.active = null;

            ros_img = tex.GetRawTextureData();
            ros_msg.data = ros_img;

            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName;

            return true;
        }

        void OnGUI()
        {
            if(viewCam)
            {
                GUI.DrawTexture(
                    position:new Rect(viewX, viewY, width:viewWidth, height:viewHeight),
                    image:tex,
                    scaleMode:ScaleMode.ScaleToFit,
                    alphaBlend:false
                );
            }
        }
    }
}
