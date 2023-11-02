using System; //Bit converter
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; // Clock
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace DefaultNamespace
{
    public class MBES : Sensor<PointCloud2Msg>
    {

        Sonar sonar;

        void Start()
        {
            sonar = gameObject.GetComponent<Sonar>();
            ros_msg.height = 1; // just one long list of points
            ros_msg.width = (uint)sonar.beam_count;
            ros_msg.is_bigendian = false;
            ros_msg.is_dense = true;
            // 3x 4bytes (float32 x,y,z) + 1x 1byte (uint8 intensity) = 13bytes
            // Could calc this from the fields field i guess.. but meh.
            ros_msg.point_step = 13; 
            ros_msg.row_step = ros_msg.width * ros_msg.point_step;
            ros_msg.data = new byte[ros_msg.point_step * ros_msg.width];
            

            ros_msg.fields = new PointFieldMsg[4];

            ros_msg.fields[0] = new PointFieldMsg();;
            ros_msg.fields[0].name = "x";
            ros_msg.fields[0].offset = 0;
            ros_msg.fields[0].datatype = PointFieldMsg.FLOAT32;
            ros_msg.fields[0].count = 1;

            ros_msg.fields[1] = new PointFieldMsg();;
            ros_msg.fields[1].name = "y";
            ros_msg.fields[1].offset = 4;
            ros_msg.fields[1].datatype = PointFieldMsg.FLOAT32;
            ros_msg.fields[1].count = 1;

            ros_msg.fields[2] = new PointFieldMsg();;
            ros_msg.fields[2].name = "z";
            ros_msg.fields[2].offset = 8;
            ros_msg.fields[2].datatype = PointFieldMsg.FLOAT32;
            ros_msg.fields[2].count = 1;

            ros_msg.fields[3] = new PointFieldMsg();;
            ros_msg.fields[3].name = "intensity";
            ros_msg.fields[3].offset = 12;
            ros_msg.fields[3].datatype = PointFieldMsg.UINT8;
            ros_msg.fields[3].count = 1;

        }


        byte[] GetSonarHitAsROSBytes(RaycastHit sonarHit)
        {
            // so first, we gotta convert the unity points to ros points
            // then x,y,z need to be byte-ified
            // then a fourth "intensity" needs to be created and byte-ified
            var point = sonarHit.point.To<FLU>();

            var xb = BitConverter.GetBytes(point.x);
            var yb = BitConverter.GetBytes(point.y);
            var zb = BitConverter.GetBytes(point.z);

            var intensity = sonar.GetSonarHitIntensity(sonarHit);
            byte[] ib = {(byte)(intensity*255)};

            int totalBytes = xb.Length + yb.Length + zb.Length+ ib.Length;
            byte[] ret = new byte[totalBytes];
            // src, offset, dest, offset, count
            // Imma hard-code the offsets and counts, to act as a weird
            // error catching mechanism
            Buffer.BlockCopy(xb, 0, ret, 0, 4);
            Buffer.BlockCopy(yb, 0, ret, 4, 4);
            Buffer.BlockCopy(zb, 0, ret, 8, 4);
            Buffer.BlockCopy(ib, 0, ret, 12,1);

            return ret;
        }

        public override bool UpdateSensor(double deltaTime)
        {
            for(int i=0; i<sonar.hits.Length; i++)
            {
                byte[] pointByte = GetSonarHitAsROSBytes(sonar.hits[i]);
                Buffer.BlockCopy(pointByte, 0, ros_msg.data, i*pointByte.Length, pointByte.Length);
            }

            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = "odom";
            return true;
        }
    }
}