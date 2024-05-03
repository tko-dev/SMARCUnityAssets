using System;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.SlamExample;
using UnityEngine;

namespace DefaultNamespace
{
    public class ROSTransformTreePublisher : Sensor<TFMessageMsg>
    {
        [SerializeField]
        List<string> m_GlobalFrameIds = new List<string> { "map", "odom" };
        public string prefix;

        TransformTreeNode m_TransformRoot;
        void Start()
        {
            m_TransformRoot = new TransformTreeNode(robot, prefix);
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

        void PublishMessage()
        {
            var tfMessageList = new List<TransformStampedMsg>();

            if (m_GlobalFrameIds.Count > 0)
            {
                var tfRootToGlobal = new TransformStampedMsg(
                    new HeaderMsg(new TimeStamp(Clock.time), m_GlobalFrameIds.Last()),
                    m_TransformRoot.PrefixedName(),
                    m_TransformRoot.Transform.To<FLU>());
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

            PopulateTFList(tfMessageList, m_TransformRoot);

            ros_msg = new TFMessageMsg(tfMessageList.ToArray());
        }

        public override bool UpdateSensor(double deltaTime)
        {
            PublishMessage();
            return true;
        }
    }
}