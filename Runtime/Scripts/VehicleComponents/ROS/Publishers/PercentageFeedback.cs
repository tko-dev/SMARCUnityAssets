using UnityEngine;
using RosMessageTypes.Sam;
using Unity.Robotics.Core; // Clock

using IPercentageActuator = VehicleComponents.Actuators.IPercentageActuator;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(IPercentageActuator))]
    public class PercentageFeedback: ActuatorPublisher<PercentStampedMsg>
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