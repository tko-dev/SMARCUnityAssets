using UnityEngine;

using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector;

using Sensor = VehicleComponents.Sensors.Sensor;

namespace VehicleComponents.ROS
{
    [RequireComponent(typeof(Sensor))]
    public class SensorPublisher<T> : MonoBehaviour
        where T: Unity.Robotics.ROSTCPConnector.MessageGeneration.Message, new()
    {
        ROSConnection ros;
        float frequency = 10f;
        float period => 1.0f/frequency;
        double lastTime;
        Sensor sensor;

        // Subclasses should be able to modify this
        protected T ROSMsg;

        [Header("ROS Publisher")]
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;


        void Awake()
        {
            // We namespace the topics with the root name
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";

            sensor = GetComponent<Sensor>();
            frequency = sensor.frequency;
            ROSMsg = new T();

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<T>(topic);
            lastTime = Clock.NowTimeInSeconds;
        }

        public virtual void UpdateMessage()
        {
            Debug.Log($"The SensorPublisher with topic {topic} did not override the UpdateMessage method!");
        }

        void FixedUpdate()
        {
            var deltaTime = Clock.NowTimeInSeconds - lastTime;
            if(deltaTime < period) return;
            
            // If the underlying sensor does not have new data
            // do not publish anything.
            if(sensor.hasNewData)
            {
                UpdateMessage();
                ros.Publish(topic, ROSMsg);
                lastTime = Clock.NowTimeInSeconds;
            }
        }
    }

}