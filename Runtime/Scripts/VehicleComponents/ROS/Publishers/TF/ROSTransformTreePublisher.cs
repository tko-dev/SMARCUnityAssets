using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using VehicleComponents.ROS.Core;

namespace VehicleComponents.ROS.Publishers
{
    public class ROSTransformTreePublisher : ROSBehaviour
    {
        [SerializeField]
        List<string> m_GlobalFrameIds = new List<string> { "map" };
        TransformTreeNode m_TransformRoot;
        string prefix;

        
        [Header("TF Tree")]
        [Tooltip("Suffix to add to all published TF links.")]
        public string Suffix = "_gt";
        [Tooltip("The name of the object that is under the robot that the TF tree will start at.")]
        public string TransformTreeRootName = "odom";
        GameObject TFTreeRootGO;

        public float Frequency = 10f;

        
        float period => 1.0f/Frequency;
        double lastUpdate;

        TFMessageMsg finalMsg;
        bool registered = false;


        void OnValidate()
        {
            if(period < Time.fixedDeltaTime)
            {
                Debug.LogWarning($"TF Publisher update frequency set to {Frequency}Hz but Unity updates physics at {1f/Time.fixedDeltaTime}Hz. Setting to Unity's fixedDeltaTime!");
                Frequency = 1f/Time.fixedDeltaTime;
            }
        }

        protected override void StartROS()
        {
            var robotGO = Utils.FindParentWithTag(gameObject, "robot", false);
            if(robotGO == null)
            {
                Debug.LogError($"No #robot tagged parent found for {gameObject.name}! Disabling.");
                enabled = false;
            }
            prefix = robotGO.name;

            TFTreeRootGO = Utils.FindDeepChildWithName(robotGO, TransformTreeRootName);
            if(TFTreeRootGO == null)
            {
                Debug.LogError($"No object with name {TransformTreeRootName} found under {robotGO.name}! Disabling.");
                enabled = false;
                return;
            }
            m_TransformRoot = new TransformTreeNode(TFTreeRootGO);

            if(!registered)
            {
                rosCon.RegisterPublisher<TFMessageMsg>(topic);
                registered = true;
            }
            
        }

        static void PopulateTFList(List<TransformStampedMsg> tfList, TransformTreeNode tfNode)
        {
            // TODO: Some of this could be done once and cached rather than doing from scratch every time
            // Only generate transform messages from the children, because This node will be parented to the global frame
            foreach (var childTf in tfNode.Children)
            {
                tfList.Add(TransformTreeNode.ToTransformStamped(childTf));

                if (!childTf.IsALeafNode)
                {
                    PopulateTFList(tfList, childTf);
                }
            }
        }

        void PopulateGlobalFrames(List<TransformStampedMsg> tfMessageList)
        {
            if (m_GlobalFrameIds.Count > 0)
            {
                var tfRootToGlobal = new TransformStampedMsg(
                    new HeaderMsg(new TimeStamp(Clock.time), m_GlobalFrameIds.Last()),
                    $"{prefix}/{m_TransformRoot.name}",
                    m_TransformRoot.Transform.To<ENU>());
                tfMessageList.Add(tfRootToGlobal);
            }
            else
            {
                Debug.LogWarning($"No {m_GlobalFrameIds} specified, transform tree will be entirely local coordinates.");
            }
            
            // In case there are multiple "global" transforms that are effectively the same coordinate frame, 
            // treat this as an ordered list, first entry is the "true" global
            for (var i = 1; i < m_GlobalFrameIds.Count; ++i)
            {
                var tfGlobalToGlobal = new TransformStampedMsg(
                    new HeaderMsg(new TimeStamp(Clock.time), m_GlobalFrameIds[i - 1]),
                    m_GlobalFrameIds[i],
                    // Initializes to identity transform
                    new TransformMsg());
                tfMessageList.Add(tfGlobalToGlobal);
            }
        }

        void PopulateMessage()
        {
            var tfMessageList = new List<TransformStampedMsg>();
            try
            {
                PopulateTFList(tfMessageList, m_TransformRoot);
            }catch(MissingReferenceException)
            {
                // If the object tree was modified after the TF Tree was built
                // such as deleting a child object, this will throw an exception
                // So we need to re-build the TF tree and skip the publish.
                Debug.Log($"[{transform.name}] TF Tree was modified, re-building.");
                m_TransformRoot = new TransformTreeNode(TFTreeRootGO);
                return;
            }
            foreach(TransformStampedMsg msg in tfMessageList)
            {
                msg.header.frame_id = $"{prefix}/{msg.header.frame_id}";
                msg.child_frame_id = $"{prefix}/{msg.child_frame_id}";
            }

            // populate the global frames last, dont wanna prefix those.
            PopulateGlobalFrames(tfMessageList);

            // and finally, suffix _everything_
            foreach(TransformStampedMsg msg in tfMessageList)
            {
                msg.header.frame_id = $"{msg.header.frame_id}{Suffix}";
                msg.child_frame_id = $"{msg.child_frame_id}{Suffix}";
            }

            finalMsg = new TFMessageMsg(tfMessageList.ToArray());
        }

        void Update()
        {
            if (Clock.time - lastUpdate < period) return;
            lastUpdate = Clock.time;
            PopulateMessage();
            rosCon.Publish(topic, finalMsg);
        }
    }
}
