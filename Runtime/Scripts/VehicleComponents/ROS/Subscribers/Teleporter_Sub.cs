using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry; 
using Unity.Robotics.ROSTCPConnector;

namespace VehicleComponents.ROS.Subscribers
{
    public class Teleporter_Sub : MonoBehaviour
    {
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;
        [Tooltip("The object to teleport around. Can handle Arti. Bodies too.")]
        public Transform Target;

        ROSConnection ros;

        void OnValidate()
        {
            if(Target.TryGetComponent<ArticulationBody>(out ArticulationBody targetAb))
                if(!targetAb.isRoot) Debug.LogWarning($"Assigned target object is an Arti. body, but it is not the root. Non-root articulation bodies can not be teleported!");
        }

        void Start()
        {
            if(topic == null) return;
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";

            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<PoseMsg>(topic, UpdateMessage);
        }


        void UpdateMessage(PoseMsg pose)
        {
            // if its an articulation body, we need to use a specific method
            // otherwise just setting local position/rotation is enough.
            var unityPosi = FLU.ConvertToRUF(
                        new Vector3(
                            (float)pose.position.x,
                            (float)pose.position.y,
                            (float)pose.position.z));

            var unityOri = FLU.ConvertToRUF(
                        new Quaternion(
                            (float)pose.orientation.x,
                            (float)pose.orientation.y,
                            (float)pose.orientation.z,
                            (float)pose.orientation.w));

            if(Target.TryGetComponent<ArticulationBody>(out ArticulationBody targetAb))
            {
                if(!targetAb.isRoot) return;
                targetAb.TeleportRoot(unityPosi, unityOri);
                targetAb.velocity = Vector3.zero;
                targetAb.angularVelocity = Vector3.zero;
                
            }
            else
            {
                Target.localPosition = unityPosi;
                Target.localRotation = unityOri;
                if(Target.TryGetComponent<Rigidbody>(out Rigidbody targetRB))
                {
                    targetRB.velocity = Vector3.zero;
                    targetRB.angularVelocity = Vector3.zero;
                }
            }


        }
    }
}
