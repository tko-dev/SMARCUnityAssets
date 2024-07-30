using UnityEngine;
using RosMessageTypes.Sam;
using Unity.Robotics.Core; // Clock

using IPercentageActuator = VehicleComponents.Actuators.IPercentageActuator;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(IPercentageActuator))]
    public class PercentageFeedback: ROSPublisher<PercentStampedMsg, IPercentageActuator>
    {
        IPercentageActuator act;
        void Start()
        {
            act = GetComponent<IPercentageActuator>();
            if(act == null)
            {
                Debug.Log("No IPercentageActuator found!");
                return;
            }
        }

        public override void UpdateMessage()
        {
            if(act == null) return;
            ROSMsg.value = (float)act.GetCurrentValue();
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}