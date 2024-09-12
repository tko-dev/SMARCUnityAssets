using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sonar = VehicleComponents.Sensors.Sonar;

namespace GameUI
{

    public class RayViewer : MonoBehaviour
    {
        Sonar sonar;

        [Header("Visuals")]
        [Tooltip("Draw rays in the scene view as lines?")]
        public bool DrawRays = false;
        [Tooltip("Just draw the hit points as 1m-long lines?")]
        public bool DrawHits = false;
        [Tooltip("Use rainbow colormap for the hit points?")]
        public bool UseRainbow = false;

        private Color rayColor = Color.white;
        private Color hitColor = Color.red;

        private float MaxZ = 0f;
        private float MinZ = Mathf.Infinity;



        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int) ((div % 1) * 255);
            int descending = 255 - ascending;

            static Color FromArgb (int alpha, int red, int green, int blue)
            {
                float fa = ((float)alpha) / 255.0f;
                float fr = ((float)red)   / 255.0f;
                float fg = ((float)green) / 255.0f;
                float fb = ((float)blue)  / 255.0f;
                return new Color(fr,fg,fb,fa);
            }

            switch ((int) div)
            {
                case 0:
                    return FromArgb(255, 255, ascending, 0);
                case 1:
                    return FromArgb(255, descending, 255, 0);
                case 2:
                    return FromArgb(255, 0, 255, ascending);
                case 3:
                    return FromArgb(255, 0, descending, 255);
                case 4:
                    return FromArgb(255, ascending, 0, 255);
                default: // case 5:
                    return FromArgb(255, 255, 0, descending);
            }
        }

        void Start()
        {
            sonar = GetComponent<Sonar>();
            
            
        }
        void FixedUpdate()
        {

            if (DrawRays)
            {
                for (int i=0; i<sonar.SonarHits.Length; i++)
                {
                    if (sonar.SonarHits[i].Hit.point != Vector3.zero)
                    {
                        Debug.DrawLine(sonar.transform.position, sonar.SonarHits[i].Hit.point, rayColor, 1f);
                    }
                }
            }
            if (DrawHits)
            {
                if (UseRainbow){
                // if using rainbow colormap, find the min and max Z values
                for (int i=0; i<sonar.SonarHits.Length; i++)
                    {
                        if((sonar.SonarHits[i].Hit.point.y > MaxZ)&& (sonar.SonarHits[i].Hit.point.y<0)) MaxZ = sonar.SonarHits[i].Hit.point.y;
                        if(sonar.SonarHits[i].Hit.point.y < MinZ) MinZ = sonar.SonarHits[i].Hit.point.y;
                    }
                }
                for (int i=0; i<sonar.SonarHits.Length; i++)
                {
                    if (UseRainbow)
                    {
                        float normalizedZ = Mathf.InverseLerp(MaxZ, MinZ, sonar.SonarHits[i].Hit.point.y);
                        hitColor= Rainbow(normalizedZ);
                    }
                    Debug.DrawRay(sonar.SonarHits[i].Hit.point, Vector3.up, hitColor, 1f);

                }
            }


        }

    }

}