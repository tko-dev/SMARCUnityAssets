using UnityEngine;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;
using System;

public class TwistSubscriber : MonoBehaviour
{
    [Tooltip("ROS topic name to subscribe to")]
    public string topicName = "/quadrotor/cmd_vel";
    
    [Tooltip("Articulation body of the quadrotor")]
    public ArticulationBody quadrotorBody;
    
    [Tooltip("Name of the GPS link child transform")]
    public string gpsLinkName = "GPSLink";
    
    private Transform gpsLink;
    private ROSConnection ros;

    private Vector3 latestForce;
    private Vector3 latestTorque;
    private bool receivedMessage = false;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TwistMsg>(topicName, ApplyTwistMessage);

        // Find the GPS link transform
        gpsLink = quadrotorBody.transform.Find(gpsLinkName);
        if (gpsLink == null)
        {
            Debug.LogError($"GPS link '{gpsLinkName}' not found in quadrotor children.");
        }
    }

    void ApplyTwistMessage(TwistMsg twist)
    {
        if (quadrotorBody == null || gpsLink == null) return;

        // Extract force and torque components from the Twist message
        Vector3 force = new Vector3((float)twist.linear.x, (float)twist.linear.y, (float)twist.linear.z);
        Vector3 torque = new Vector3((float)twist.angular.x, (float)twist.angular.y, (float)twist.angular.z);

        // Convert force and torque to local space
        Vector3 localForce = gpsLink.TransformDirection(force);
        Vector3 localTorque = gpsLink.TransformDirection(torque);

        // Apply force and torque at the quadrotor's position
        quadrotorBody.AddForce(localForce, ForceMode.Force);
        quadrotorBody.AddTorque(localTorque, ForceMode.Force);

        // Store the latest force and torque for visualization
        latestForce = localForce;
        latestTorque = localTorque;
        receivedMessage = true;
    }

    void OnDrawGizmos()
    {
        if (!receivedMessage || gpsLink == null) return;

        Gizmos.color = Color.green;
        // Draw force vector
        Gizmos.DrawLine(quadrotorBody.transform.position, quadrotorBody.transform.position + latestForce);
        DrawArrow(quadrotorBody.transform.position, quadrotorBody.transform.position + latestForce, Color.green);

        Gizmos.color = Color.red;
        // Draw torque vector
        Gizmos.DrawLine(quadrotorBody.transform.position, quadrotorBody.transform.position + latestTorque);
        DrawArrow(quadrotorBody.transform.position, quadrotorBody.transform.position + latestTorque, Color.red);
    }

    void DrawArrow(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
        Vector3 direction = (end - start).normalized;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawLine(end, end + right * 0.2f);
        Gizmos.DrawLine(end, end + left * 0.2f);
    }
}
