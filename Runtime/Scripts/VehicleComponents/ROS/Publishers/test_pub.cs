using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;

public class TestPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/timer";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float64Msg>(topicName);
        InvokeRepeating("PublishTestMessage", 0.02f, 0.02f);
    }


    void PublishTestMessage()
    {
        Float64Msg testMsg = new Float64Msg
        {
            data = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
        };
        ros.Publish(topicName, testMsg);
    }
}
