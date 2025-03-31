using UnityEngine;

using DefaultNamespace; // ResetArticulationBody() extension

using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry; 
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Subscribers
{
    public class Teleporter_Sub : ROSBehaviour
    {
        [Header("Teleporter")]
        [Tooltip("The object to teleport around. Can handle Arti. Bodies too.")]
        public Transform Target;

        ArticulationBody[] ABparts;
        Rigidbody[] RBparts;

        int immovableStage = 2;


        [Header("Debug")]
        public bool UseDebugInput = false;
        public bool ResetDebugInput = false;
        public Vector3 ROSCoordInput;


        protected override void StartROS()
        {
            ABparts = Target.gameObject.GetComponentsInChildren<ArticulationBody>();
            RBparts = Target.gameObject.GetComponentsInChildren<Rigidbody>();
            ROSCoordInput = ENU.ConvertFromRUF(Target.position);

            rosCon.Subscribe<PoseMsg>(topic, UpdateMessage);
        }


        void UpdateMessage(PoseMsg pose)
        {
            if(Target.TryGetComponent(out ArticulationBody targetAb))
            {
                if(!targetAb.isRoot)
                {
                    Debug.LogWarning($"[{transform.name}] Assigned target object is an Arti. body, but it is not the root. Non-root articulation bodies can not be teleported! Disabling.");
                    enabled = false;
                    rosCon.Unsubscribe(topic);
                    return;
                }
            }

            // if its an articulation body, we need to use a specific method
            // otherwise just setting local position/rotation is enough.
            var unityPosi = ENU.ConvertToRUF(
                        new Vector3(
                            (float)pose.position.x,
                            (float)pose.position.y,
                            (float)pose.position.z));

            var unityOri = ENU.ConvertToRUF(
                        new Quaternion(
                            (float)pose.orientation.x,
                            (float)pose.orientation.y,
                            (float)pose.orientation.z,
                            (float)pose.orientation.w));

            if (Target.TryGetComponent(out ArticulationBody _))
            {
                targetAb.immovable = true;
                immovableStage = 0;
                targetAb.TeleportRoot(unityPosi, unityOri);
                targetAb.linearVelocity = Vector3.zero;
                targetAb.angularVelocity = Vector3.zero;
            }
            else
            {
                Target.SetPositionAndRotation(unityPosi, unityOri);
            }


            foreach (var ab in ABparts)
            {
                ab.linearVelocity = Vector3.zero;
                ab.angularVelocity = Vector3.zero;
                ab.ResetArticulationBody();
            }

            foreach(var rb in RBparts)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        void FixedUpdate()
        {
            if(UseDebugInput && immovableStage >= 2)
            {
                UpdateMessage(new PoseMsg
                {
                    position = new PointMsg
                    {
                        x = ROSCoordInput.x,
                        y = ROSCoordInput.y,
                        z = ROSCoordInput.z
                    },
                    orientation = new QuaternionMsg
                    {
                        x = 0,
                        y = 0,
                        z = 0,
                        w = 1
                    }
                });
                if(ResetDebugInput) UseDebugInput = false;
            }

            switch(immovableStage)
            {
                case 0:
                    immovableStage = 1;
                    break;
                case 1:
                    if(Target.TryGetComponent(out ArticulationBody targetAb))
                    {
                        if(!targetAb.isRoot) return;
                        targetAb.immovable = false;
                    }
                    immovableStage = 2;
                    break;
                default:
                    break;
            }
        }
    }
}
