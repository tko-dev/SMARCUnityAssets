using UnityEngine;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;
using Unity.Robotics.Core;


namespace VehicleComponents.ROS.Core
{
    [RequireComponent(typeof(IROSPublishable))]
    public abstract class ROSPublisher<RosMsgType, PublishableType> : ROSBehaviour
        where RosMsgType: ROSMessage, new()
        where PublishableType: IROSPublishable
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;

        // Subclasses should be able to access these
        // to get data from the sensor and put it in
        // ROSMsg as needed.
        protected PublishableType sensor;
        protected RosMsgType ROSMsg;

        
        [Tooltip("If true, we will publish regardless, even if the underlying sensor says no data.")]
        public bool ignoreSensorState = false;

        protected override void StartROS()
        {
            sensor = GetComponent<PublishableType>();
            ROSMsg = new RosMsgType();
            rosCon.RegisterPublisher<RosMsgType>(topic);
        }

        /// <summary>
        /// Override this method to update the ROS message with the sensor data.
        /// This method is called in Update, so that the message can be published at a fixed frequency.
        /// </summary>
        protected abstract void UpdateMessage();

        /// <summary>
        /// Publish the message to ROS.
        /// We do this in Update, so that things can be disabled and enabled at runtime.
        /// </summary>
        void Update()
        {
            if (Clock.time - lastUpdate < period) return;
            lastUpdate = Clock.time;
            if(!(sensor.HasNewData() || ignoreSensorState)) return;
            UpdateMessage();
            rosCon.Publish(topic, ROSMsg);
        }

    }

}