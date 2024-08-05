using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class TestPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/test_topic";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName);
        InvokeRepeating("PublishTestMessage", 1.0f, 1.0f);
    }

    void PublishTestMessage()
    {
        StringMsg testMsg = new StringMsg
        {
            data = "Hello, ROS2!"
        };
        ros.Publish(topicName, testMsg);
    }
}
