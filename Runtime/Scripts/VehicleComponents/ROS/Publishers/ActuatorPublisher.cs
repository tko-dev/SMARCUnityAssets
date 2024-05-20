using UnityEngine;

using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;


namespace VehicleComponents.ROS.Publishers
{
    public class ActuatorPublisher<RosMsgType> : MonoBehaviour
        where RosMsgType: ROSMessage, new()
    {
        ROSConnection ros;
        float frequency = 10f;
        float period => 1.0f/frequency;
        double lastTime;

        // Subclasses should be able to access the
        // ROSMsg to be able to update it.
        protected RosMsgType ROSMsg;

        [Header("ROS Publisher")]
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;


        void Awake()
        {
            // We namespace the topics with the root name
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";

            ROSMsg = new RosMsgType();

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<RosMsgType>(topic);
            lastTime = Clock.NowTimeInSeconds;
        }

        public virtual void UpdateMessage()
        {
            Debug.Log($"The ActuatorPublisher with topic {topic} did not override the UpdateMessage method!");
        }

        void FixedUpdate()
        {
            var deltaTime = Clock.NowTimeInSeconds - lastTime;
            if(deltaTime < period) return;
            
            UpdateMessage();
            ros.Publish(topic, ROSMsg);
            lastTime = Clock.NowTimeInSeconds;
        }
    }

}