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

        public int numBucketsPerSide = 1000;

        public byte[] portBuckets;
        public byte[] strbBuckets;

        void Start()
        {
            sonarPort = transform.Find("SSS Port").GetComponent<Sonar>();
            sonarStrb = transform.Find("SSS Strb").GetComponent<Sonar>();
            ros_msg.header.frame_id = robot.name + linkSeparator + "base_link";
            // Each bucket has a 1 byte intensity value 0-255
            portBuckets = new byte[numBucketsPerSide];
            strbBuckets = new byte[numBucketsPerSide];
            ros_msg.port_channel = portBuckets;
            ros_msg.starboard_channel = strbBuckets;
            // Other fields seem unused in the rosbags, so thats what we do too.
        }

        byte[] GetBucket(Sonar s)
        {
            // First we gotta know what distance ranges each bucket needs to
            // have, we can ask the sonar object for its max distance;
            byte[] ret = new byte[numBucketsPerSide];
            float max_distance = s.max_distance;
            float min_distance = 0;
            float bucketSize = (max_distance-min_distance)/numBucketsPerSide;
            for(int i=0; i<s.beam_count; i++)
            {
                SonarHit sh = s.sonarHits[i];
                int bucketIndex = Mathf.FloorToInt((sh.hit.distance - min_distance)/bucketSize);
                // Maybe there's a function for accumulation of intensities
                // but this'll do for now
                // intensities are stored as floats in [0,1], but we want bytes in 0-255 range
                ret[bucketIndex] += (byte)(sh.intensity * 255);
                if(ret[bucketIndex] > 255) ret[bucketIndex] = 255;
            }
            return ret;
        }

        public override bool UpdateSensor(double deltaTime)
        {
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            portBuckets = GetBucket(sonarPort);
            strbBuckets = GetBucket(sonarStrb);
            ros_msg.port_channel = portBuckets;
            ros_msg.starboard_channel = strbBuckets;
            return true;
        }
    }
}
