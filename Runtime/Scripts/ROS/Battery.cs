using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

namespace DefaultNamespace
{
    public class Battery : Sensor<BatteryStateMsg>
    {
        [Header("Battery")]
        public float dischargePercentPerMinute = 1;
        public float currentPercent = 95f;

        public override bool UpdateSensor(double deltaTime)
        {
           currentPercent -= (float) ((deltaTime/60) * dischargePercentPerMinute);
           if(currentPercent < 0f) currentPercent = 0f;
           ros_msg.voltage = 12.5f;
           ros_msg.percentage = currentPercent;
           ros_msg.header.stamp = new TimeStamp(Clock.time);
           return true;
        }
    }
}
