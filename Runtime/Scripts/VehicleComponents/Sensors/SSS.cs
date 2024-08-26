// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using NormalDistribution = DefaultNamespace.NormalDistribution;
// using System;

// namespace VehicleComponents.Sensors
// {
//     public class SideScanSonar: Sensor
//     {
//         Sonar sonarPort;
//         Sonar sonarStrb;
//         private ushort magic_number = 20860;
//         private Color rayColor;

//         [Header("SideScanSonar")]
//         public int numBucketsPerSide = 1000;
//         public byte[] portBuckets;
//         public byte[] strbBuckets;
//         public byte[] portBucketsAngleHigh;
//         public byte[] portBucketsAngleLow;
//         public byte[] strbBucketsAngleHigh;
//         public byte[] strbBucketsAngleLow;
//         public bool isISSS = false;
//         public bool drawRays = false;

//         [Header("Beam Profile")]
//         public float beamOrientationAngleDeg = 55.0f;
//         public float fullBeamAngleDeg = 90.0f;
//         public float fwhmBeamAngleDeg = 60.0f;
//         public bool gaussianProfile = false;
//         public int maxRange = 100;
//         public int totalBeamCount = 256;
//         [Header("Noise")]
//         public float multGain = 4;
//         public bool useAdditiveNoise = true;
//         public float addNoiseStd = 1;
//         public float addNoiseMean = 0;

//         NormalDistribution additiveNormal;

//         void Start()
//         {
//             sonarPort = CreateSonar("sonarPort", -1);
//             sonarStrb = CreateSonar("sonarStrb",  1);
//             rayColor = Color.white; //Random.ColorHSV();

//             // Each bucket has a 1 byte intensity value 0-255
//             portBuckets = new byte[numBucketsPerSide];
//             strbBuckets = new byte[numBucketsPerSide];

//              // followed by 2 bytes angle value 0-65535 [-pi,0]
//             // angle is in radians, but we store it as a 16bit unsigned int
//             // so we can have a resolution of pi/65535, the magic number is 20860
//             // angle (unsigned int) / 20860 = angle+pi (radians)
//             portBucketsAngleHigh = new byte[numBucketsPerSide];
//             portBucketsAngleLow = new byte[numBucketsPerSide];
//             strbBucketsAngleHigh = new byte[numBucketsPerSide];
//             strbBucketsAngleLow = new byte[numBucketsPerSide];

//             additiveNormal = new NormalDistribution(addNoiseMean, addNoiseStd);
//         }

//         Sonar CreateSonar(string name, int sign)
//         {
//             GameObject sonar_go = new GameObject(name);
            
//             Sonar sonar = sonar_go.AddComponent(typeof(Sonar)) as Sonar;

//             sonar_go.transform.SetParent(this.transform, false);
           
//             sonar.frequency = frequency;
//             sonar.beam_breadth_deg = fullBeamAngleDeg;
//             sonar.beam_fwhm_deg = fwhmBeamAngleDeg;
//             sonar.transform.localRotation = Quaternion.Euler(0, 0, sign*beamOrientationAngleDeg);
//             sonar.max_distance = maxRange;
//             sonar.beam_count = totalBeamCount/2;
//             sonar.InitHits();
//             if (gaussianProfile) sonar.InitBeamProfileGaussian();
//             else sonar.InitBeamProfileSimple();
//             return sonar;
//         }


//         void FillBucket(Sonar s, byte[] bucket, byte[] bucket_angle_high, byte[] bucket_angle_low, bool is_strb=false)
//         {
//             // 0-out, since maybe not the same buckets will be written to.
//             Array.Clear(bucket, 0, bucket.Length);
//             Array.Clear(bucket_angle_high, 0, bucket_angle_high.Length);
//             Array.Clear(bucket_angle_low, 0, bucket_angle_low.Length);

//             int[] cnt = new int[bucket.Length];
//             float[] bucket_sum = new float[bucket.Length];
//             float[] bucket_angle_high_sum = new float[bucket.Length];
//             float[] bucket_angle_low_sum = new float[bucket.Length];

//             // First we gotta know what distance ranges each bucket needs to
//             // have, we can ask the sonar object for its max distance;
//             float max_distance = s.MaxRange;
//             float min_distance = 0;
//             float bucketSize = (max_distance-min_distance)/numBucketsPerSide;
//             var angleStepDeg = s.BeamBreadthDeg / (s.NumRaysPerBeam - 1.0f);

//             for(int i=0; i<s.TotalRayCount; i++)
//             {
//                 SonarHit sh = s.sonarHits[i];
//                 if (drawRays && sh.hit.point != Vector3.zero) Debug.DrawLine(s.transform.position, sh.hit.point, rayColor);
//                 double addNoise = 0;
//                 if(useAdditiveNoise) addNoise = additiveNormal.Sample();

//                 float dis = (float)(sh.hit.distance + addNoise);
//                 if(dis<0) dis=0;
//                 int bucketIndex = Mathf.FloorToInt((dis - min_distance)/bucketSize);
//                 if(bucketIndex >= bucket.Length || bucketIndex < 0) continue;


//                 // Maybe there's a function for accumulation of intensities
//                 // but this'll do for now
//                 // intensities are stored as floats in [0,1], but we want bytes in 0-255 range
//                 bucket_sum[bucketIndex] += (sh.intensity * 255 * multGain);

//                 if (isISSS){
//                 Vector3 localPoint = s.transform.InverseTransformPoint(sh.hit.point);
//                 var beamOffsetDeg = beamOrientationAngleDeg - s.beam_breadth_deg ;
//                 var beamAngleDeg = (-s.beam_breadth_deg / 2 + i * angleStepDeg) * (2.0f*System.Convert.ToSingle(is_strb)-1.0f) + beamOffsetDeg;
//                 // still not sure why this angle is not the same as beamAngleDeg...
//                 // double angle = Mathf.Asin(localPoint.y/dis);// this is in radians [localPoint.y is localPointFLU.z]
//                 ushort angle_uint16 = (ushort) ( (Mathf.PI+beamAngleDeg*Mathf.Deg2Rad) * magic_number);
//                 byte angle_low = (byte) (angle_uint16 & 0xff);
//                 byte angle_high = (byte) ((angle_uint16 >> 8) & 0xff);

//                 bucket_angle_high_sum[bucketIndex] += angle_high;
//                 bucket_angle_low_sum[bucketIndex] += angle_low;
//                 }
//                 cnt[bucketIndex]++;
//             }
//             for(int bucketIndex=0; bucketIndex<bucket.Length; bucketIndex++){
//                 if (cnt[bucketIndex] == 0) continue;
//                 bucket[bucketIndex] = (byte) (bucket_sum[bucketIndex]/cnt[bucketIndex]);
//                 if (isISSS){
//                 bucket_angle_high[bucketIndex] = (byte) (bucket_angle_high_sum[bucketIndex]/cnt[bucketIndex]);
//                 bucket_angle_low[bucketIndex] = (byte) (bucket_angle_low_sum[bucketIndex]/cnt[bucketIndex]);
//                 }

//             }



//         }

//         public override bool UpdateSensor(double deltaTime)
//         {   
//             FillBucket(sonarPort, portBuckets, portBucketsAngleHigh, portBucketsAngleLow, false);
//             FillBucket(sonarStrb, strbBuckets, strbBucketsAngleHigh, strbBucketsAngleLow, true);
//             return true;
//         }


//     }
// }