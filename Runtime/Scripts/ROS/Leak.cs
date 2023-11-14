using UnityEngine;
using RosMessageTypes.Sam;

namespace DefaultNamespace
{
    public class Leak : Sensor<LeakMsg>
    {
        [Header("Leak")]
        [Tooltip("Manually set this to trigger a leak.")]
        public bool leaked = false;
        int count = 0;

        public override bool UpdateSensor(double deltaTime)
        {
            if(leaked)
            {
                ros_msg.value = true;
                ros_msg.leak_counter = count;
                count++;
            }
            else
            {
                ros_msg.value = false;
            }
            return true;
        }
    }
}