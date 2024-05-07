using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NormalDistribution = DefaultNamespace.NormalDistribution;
using System;

namespace VehicleComponents.Sensors
{
    public class SideScanSonar: Sensor
    {
        Sonar sonarPort;
        Sonar sonarStrb;

        [Header("SideScanSonar")]
        public int numBucketsPerSide = 1000;
        public byte[] portBuckets;
        public byte[] strbBuckets;
        [Header("Beam Profile")]
        public float beamOrientationAngleDeg = 45.0f;
        public float fullBeamAngleDeg = 90.0f;
        public float fwhmBeamAngleDeg = 60.0f;
        public bool gaussianProfile = false;
        public int maxRange = 100;
        public int totalBeamCount = 256;
        [Header("Noise")]
        public float multGain = 1;
        public bool useAdditiveNoise = true;
        public float addNoiseStd = 1;
        public float addNoiseMean = 0;

        NormalDistribution additiveNormal;

        void Start()
        {
            sonarPort = CreateSonar("sonarPort", -1);
            sonarStrb = CreateSonar("sonarStrb",  1);

            // Each bucket has a 1 byte intensity value 0-255
            portBuckets = new byte[numBucketsPerSide];
            strbBuckets = new byte[numBucketsPerSide];

            additiveNormal = new NormalDistribution(addNoiseMean, addNoiseStd);
        }

        Sonar CreateSonar(string name, int sign)
        {
            GameObject sonar_go = new GameObject(name);
            
            Sonar sonar = sonar_go.AddComponent(typeof(Sonar)) as Sonar;

            sonar_go.transform.SetParent(this.transform, false);

            var ab = sonar_go.GetComponent<ArticulationBody>();
            ab.enabled = false;
            
            sonar.frequency = frequency;
            sonar.beam_breadth_deg = fullBeamAngleDeg;
            sonar.beam_fwhm_deg = fwhmBeamAngleDeg;
            sonar.transform.localRotation = Quaternion.Euler(0, 0, sign*beamOrientationAngleDeg);
            sonar.max_distance = maxRange;
            sonar.beam_count = totalBeamCount/2;
            sonar.InitHits();
            if (gaussianProfile) sonar.InitBeamProfileGaussian();
            else sonar.InitBeamProfileSimple();
            return sonar;
        }


        void FillBucket(Sonar s, byte[] bucket)
        {
            // 0-out, since maybe not the same buckets will be written to.
            Array.Clear(bucket, 0, bucket.Length);

            // First we gotta know what distance ranges each bucket needs to
            // have, we can ask the sonar object for its max distance;
            float max_distance = s.max_distance;
            float min_distance = 0;
            float bucketSize = (max_distance-min_distance)/numBucketsPerSide;
            for(int i=0; i<s.beam_count; i++)
            {
                SonarHit sh = s.sonarHits[i];

                double addNoise = 0;
                if(useAdditiveNoise) addNoise = additiveNormal.Sample();

                float dis = (float)(sh.hit.distance + addNoise);
                if(dis<0) dis=0;
                int bucketIndex = Mathf.FloorToInt((dis - min_distance)/bucketSize);

                if(bucketIndex >= bucket.Length || bucketIndex < 0) continue;
                // Maybe there's a function for accumulation of intensities
                // but this'll do for now
                // intensities are stored as floats in [0,1], but we want bytes in 0-255 range
                bucket[bucketIndex] += (byte)(sh.intensity * 255 * multGain);
                if(bucket[bucketIndex] > 255) bucket[bucketIndex] = 255;
            }
        }

        public override bool UpdateSensor(double deltaTime)
        {
            FillBucket(sonarPort, portBuckets);
            FillBucket(sonarStrb, strbBuckets); 
            return true;
        }


    }
}