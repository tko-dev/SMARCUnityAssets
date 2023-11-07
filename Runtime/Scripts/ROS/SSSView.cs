using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace{
    public class SSSView : MonoBehaviour
    {
        [SerializeField][Min(4)] int textureWidth = 64;
        [SerializeField][Min(4)] int textureHeight = 64;
        Texture2D texture;

        SideScanSonar sss;

        public int screenx=0;
        public int screeny=30;
        
        // the raw data to copy into texture
        byte[] line;
        byte[] reversePort;
        byte[] image;

        void CreateNewTexture ()
        {
            // R8 is a 1byte format, all-red, good for SSS data
            // Alternatively Alpha8 is the same, for alpha channel instead
            texture = new Texture2D(
                width:textureWidth,
                height:textureHeight,
                textureFormat:TextureFormat.R8, 
                mipCount:3,
                linear:false
            );
            texture.Apply();
        }

        void Start()
        {
            sss = GetComponent<SideScanSonar>();
            textureWidth = sss.numBucketsPerSide * 2;
            textureHeight = 500;
            CreateNewTexture();
            line = new byte[textureWidth];
            image = new byte[textureWidth*textureHeight];
            reversePort = new byte[sss.numBucketsPerSide];
        }

        void OnGUI()
        {
            GUI.DrawTexture(
                position:new Rect(screenx, screeny, width:textureWidth/2, height:textureHeight/2),
                image:texture,
                scaleMode:ScaleMode.ScaleToFit,
                alphaBlend:false
            );
        }

        void Update()
        {
            // First, create the current line
            // We want port(left) to be reversed so the nadir is in the middle
            Buffer.BlockCopy(sss.portBuckets, 0, reversePort, 0, sss.numBucketsPerSide);
            Array.Reverse(reversePort, 0, reversePort.Length);

            // Finally put the two sides together into one line
            Buffer.BlockCopy(reversePort, 0, line, 0, reversePort.Length);
            Buffer.BlockCopy(sss.strbBuckets, 0, line, sss.numBucketsPerSide, sss.numBucketsPerSide);

            // Scroll the current image by 1 pixel down
            // Since image is a flattened array, that means
            // shifting by width and copying one width-worth fewer pixels
            // to leave room at the top
            // src, srcOff, dst, dstOff, count
            Buffer.BlockCopy(image, 0, image, textureWidth, textureWidth*(textureHeight-1));

            // Copy the new line at the top
            Buffer.BlockCopy(line, 0, image, 0, textureWidth);

            // Straight up set the texture data to our bytes. 
            texture.SetPixelData(image, 0);
            texture.Apply();
        }
    }
}