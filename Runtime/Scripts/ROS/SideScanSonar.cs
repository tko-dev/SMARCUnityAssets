using RosMessageTypes.Smarc;
using UnityEngine;
using Unity.Robotics.Core; // Clock
using System; //Bit converter

namespace DefaultNamespace
{
    public class SideScanSonar : Sensor<SidescanMsg>
    {
        Sonar sonarPort;
        Sonar sonarStrb;

        [Header("Sidescan")]
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
            sonarPort = transform.Find("SSS Port").GetComponent<Sonar>();
            sonarStrb = transform.Find("SSS Strb").GetComponent<Sonar>();

            SetSonars();

            ros_msg.header.frame_id = robot.name + linkSeparator + "base_link";
            // Each bucket has a 1 byte intensity value 0-255
            portBuckets = new byte[numBucketsPerSide];
            strbBuckets = new byte[numBucketsPerSide];
            ros_msg.port_channel = portBuckets;
            ros_msg.starboard_channel = strbBuckets;
            // Other fields seem unused in the rosbags, so thats what we do too.

            additiveNormal = new NormalDistribution(addNoiseMean, addNoiseStd);
        }

        void SetSonars()
        {
            sonarPort.beam_breadth_deg = fullBeamAngleDeg;
            sonarPort.beam_fwhm_deg = fwhmBeamAngleDeg;
            sonarPort.transform.localRotation = Quaternion.Euler(0, 0, -beamOrientationAngleDeg);
            sonarPort.max_distance = maxRange;
            sonarPort.beam_count = totalBeamCount/2;
            sonarPort.InitHits();
            if (gaussianProfile)
            {
                sonarPort.InitBeamProfileGaussian();
            }
            else
            {
                sonarPort.InitBeamProfileSimple();
            }

            sonarStrb.beam_breadth_deg = fullBeamAngleDeg;
            sonarStrb.beam_fwhm_deg = fwhmBeamAngleDeg;
            sonarStrb.transform.localRotation = Quaternion.Euler(0, 0, beamOrientationAngleDeg);
            sonarStrb.max_distance = maxRange;
            sonarStrb.beam_count = totalBeamCount/2;
            sonarStrb.InitHits();
            if (gaussianProfile)
            {
                sonarStrb.InitBeamProfileGaussian();
            }
            else
            {
                sonarStrb.InitBeamProfileSimple();
            }
            
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
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            FillBucket(sonarPort, portBuckets);
            FillBucket(sonarStrb, strbBuckets);
            ros_msg.port_channel = portBuckets;
            ros_msg.starboard_channel = strbBuckets;
            return true;
        }
    }
}
