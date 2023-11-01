using System;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace DefaultNamespace
{
    public class RosSubscriberExample : MonoBehaviour
    {
        [SerializeField]
        string m_topicName = "auv_input";
        private SAMForceModel _samForceModel;
        private List<ForcePoint> points;

        private void Awake()
        {
            _samForceModel = GetComponent<SAMForceModel>();
            points = new List<ForcePoint>(GetComponentsInChildren<ForcePoint>());
        }

        void Start()
        {
            ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>(m_topicName, InputVector);
        }

        void InputVector(TwistMsg twist)
        {

            if (Math.Abs(twist.linear.x - (-1f)) < 0.5f)
            {
                _samForceModel.SetRpm(-0.8f, -0.8f);
            }

            if (Math.Abs(twist.linear.x - 1) < 0.5f)
            {
                _samForceModel.SetRpm(0.8f, 0.8f);
            }

            if (twist.linear.x == 0)
            {
                _samForceModel.SetRpm(0, 0);
            }

            if (Math.Abs(twist.angular.y - (-1f)) < 0.5f)
            {
                _samForceModel.SetRudderAngle(-0.1f);
            }

            if (Math.Abs(twist.angular.y - 1) < 0.5f)
            {
                _samForceModel.SetRudderAngle(0.1f);
            }

            if (twist.angular.y == 0)
            {
                _samForceModel.SetRudderAngle(0);
            }

            if (Math.Abs(twist.angular.z - (-1f)) < 0.5f)
            {
                _samForceModel.SetElevatorAngle(-0.1f);
            }

            if (Math.Abs(twist.angular.z - 1) < 0.5f)
            {
                _samForceModel.SetElevatorAngle(0.1f);
            }

            if (twist.angular.z == 0)
            {
                _samForceModel.SetElevatorAngle(0);
            }

            if (Math.Abs(twist.linear.y - (-1f)) < 0.5f)
            {
                points.ForEach(point => point.displacementAmount = 1.0f );
            }

            if (Math.Abs(twist.linear.y - 1) < 0.5f)
            {
                points.ForEach(point => point.displacementAmount = 1.1f );
            }

        }
    }
}
