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
        int textureWidth = 640;
        int textureHeight = 480;
        RenderTexture rendTex;
        Camera cam;
        byte[] colorPixels;
        Texture2D debugTex;

        public bool viewCam=true;
        public int viewX=100;
        public int viewY=30;
        public int viewHeight = 500;
        public int viewWidth = 500;

        void Start()
        {
            cam = GetComponent<Camera>();
            rendTex = new RenderTexture(textureWidth, textureHeight, 16, RenderTextureFormat.ARGB32);
            cam.targetTexture = rendTex;
            colorPixels = new byte[textureHeight * textureWidth * 3];
            debugTex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);


            ros_msg.encoding = "rbg8";
            ros_msg.height = (byte)textureHeight;
            ros_msg.width = (byte)textureWidth;
            ros_msg.is_bigendian = 0;
            // not sure where 1920 came from, smells like
            // old stuff to me.
            ros_msg.step = 1920;

        }

        public override bool UpdateSensor(double deltaTime)
        {
            cam.Render();
            return true;
        }

        void OnGUI()
        {
            if(viewCam)
            {
                GUI.DrawTexture(
                    position:new Rect(viewX, viewY, width:viewWidth, height:viewHeight),
                    image:rendTex,
                    scaleMode:ScaleMode.ScaleToFit,
                    alphaBlend:false
                );
            }
        }
    }
}
