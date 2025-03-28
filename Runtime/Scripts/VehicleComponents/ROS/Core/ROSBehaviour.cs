using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using DefaultNamespace;


namespace VehicleComponents.ROS.Core
{
    /// <summary>
    /// Base class for all ROS behaviours. This class handles the connection to ROS and the topic name.
    /// It also provides a method to initialize the ros-related objects.
    /// </summary>
    public abstract class ROSBehaviour : MonoBehaviour
    {
        [Header("ROS Behaviour")]
        protected ROSConnection rosCon;
        public string topic = "default_topic";

        void OnEnable()
        {
            if (topic == null || topic == "")
            {
                Debug.LogError($"ROS topic is not set for {gameObject.name}! Disabling.");
                enabled = false;
                return;
            }
        }

        void Start()
        {
            rosCon = ROSConnection.GetOrCreateInstance();
            // Aldready in root namespace, dont touch.
            if(topic[0] == '/') return;

            // We namespace the topic with the robot name
            string robot_name = Utils.FindParentWithTag(gameObject, "robot", false).name;
            if(robot_name == null)
            {
                Debug.LogWarning($"ROS topic is not namespaced with a robot name for {gameObject.name}! It will be under `/`");
                topic = $"/{topic}";
            }
            else topic = $"/{robot_name}/{topic}";

            StartROS();

            enabled = false;
        }



        /// <summary>
        /// Override this method to initialize the ROS-related objects.
        /// This method is called after the ROS connection is established and the topic name is set.
        /// If you don't need any initialization, you can ignore it.
        /// </summary>
        protected virtual void StartROS(){}

    }
}