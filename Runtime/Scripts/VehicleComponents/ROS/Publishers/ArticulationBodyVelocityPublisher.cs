using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.Core;

public class ArticulationBodyVelocityPublisher : MonoBehaviour
{
    private ROSConnection ros;
    private ArticulationBody articulationBody;
    public string topicName = "/articulation_body/velocity";
    public float publishFrequency = 10.0f; // 10 Hz

    private float publishPeriod;
    private double lastPublishTime;

    void Start()
    {
        // Initialize ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName);

        // Get the ArticulationBody component
        articulationBody = GetComponent<ArticulationBody>();
        if (articulationBody == null)
        {
            Debug.LogError("ArticulationBody component not found on this GameObject.");
            return;
        }

        // Calculate the publish period
        publishPeriod = 1.0f / publishFrequency;
    }

    void FixedUpdate()
    {
        // Get the current time
        double currentTime = Clock.time;

        // Check if it's time to publish
        if (currentTime - lastPublishTime >= publishPeriod)
        {
            PublishVelocity();
            lastPublishTime = currentTime;
        }
    }

    void PublishVelocity()
    {
        if (articulationBody != null)
        {
            // Get linear and angular velocities
            Vector3 linearVelocity = articulationBody.velocity;
            Vector3 angularVelocity = articulationBody.angularVelocity;

            // Create a Twist message
            TwistMsg velocityMsg = new TwistMsg
            {
                linear = new Vector3Msg(linearVelocity.x, linearVelocity.y, linearVelocity.z),
                angular = new Vector3Msg(angularVelocity.x, angularVelocity.y, angularVelocity.z)
            };

            // Publish the message
            ros.Publish(topicName, velocityMsg);

            Debug.Log($"Published Velocity: Linear - {linearVelocity}, Angular - {angularVelocity}");
        }
    }
}

