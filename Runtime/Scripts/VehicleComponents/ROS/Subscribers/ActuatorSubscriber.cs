using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;

using Utils = DefaultNamespace.Utils;

namespace VehicleComponents.ROS.Subscribers
{
    public abstract class ActuatorSubscriber : MonoBehaviour
    {
        // Exists solely to be able to acquire all of these in a list, even though
        // they might have different ros msg types. So we can mass enable-disable them.
    }

    public class ActuatorSubscriber<RosMsgType> : ActuatorSubscriber
    where RosMsgType: ROSMessage, new()
    {
        [Header("ROS Subscriber")]
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;
        [Tooltip("If the subscription doesn't have data at least this frequently, actuator will be reset. Set to < 0 to disable.")]
        public float expectedFrequency = 2f;

        public bool resetting = true;
        public double receivedFrequency;


        ROSConnection ros;
        protected RosMsgType ROSMsg;
        double lastTime;


        void Start()
        {
            if(topic == null)
            {
                Debug.Log("ActuatorSubscriber has null topic!");
                return;
            }
            // We namespace the topics with the root name
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";

            ROSMsg = new RosMsgType();
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<RosMsgType>(topic, UpdateMessage);
            
            lastTime = Clock.NowTimeInSeconds;
        }

        void UpdateMessage(RosMsgType msg)
        {
            ROSMsg = msg;

            double deltaTime = Clock.NowTimeInSeconds - lastTime;
            receivedFrequency = 1.0/deltaTime;
            if(receivedFrequency < 0) receivedFrequency=0.0;

            lastTime = Clock.NowTimeInSeconds;
        }

        protected virtual void UpdateVehicle(bool reset)
        {
            Debug.Log($"The ActuatorSubscriber with topic {topic} did not override the UpdateVehicle method!");
        }
        

        void FixedUpdate()
        {
            if(topic == null) return;
            
            if(expectedFrequency > 0)
            {
                double deltaTime = Clock.NowTimeInSeconds - lastTime;
                receivedFrequency = 1.0/deltaTime;
                if(receivedFrequency < 0) receivedFrequency=0.0;
                resetting = receivedFrequency < expectedFrequency;
            }
            else
            {
                resetting = false;
            }
            
            UpdateVehicle(resetting);
            
        }
    }
}

