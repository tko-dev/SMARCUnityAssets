using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using Utils = DefaultNamespace.Utils;


namespace VehicleComponents.ROS.Subscribers
{

    public class TFtoUnity: MonoBehaviour
    {
        string topic = "/tf";
        ROSConnection ros;
        TFMessageMsg tfMsg;
        GameObject tfViz_go;

        void Awake()
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<TFMessageMsg>(topic, UpdateMessage);
            tfViz_go = new GameObject("ROS_TF_Visualizer");
        }

        void UpdateMessage(TFMessageMsg tfMsg)
        {
            this.tfMsg = tfMsg;
        }

        void OnDrawGizmos()
        {
            if(tfMsg == null) return;

            foreach(TransformStampedMsg tfStamped in tfMsg.transforms)
            {
                string parent_id = tfStamped.header.frame_id;
                // utm frame is forever away and is uninteresting inside unity
                // so we skip anything to do with it
                if(parent_id.Contains("utm")) return;
                
                string child_id = tfStamped.child_frame_id;
                TransformMsg ros_tf = tfStamped.transform;
                // so, we have an objects name, and its child, and the transform
                // between these two.
                // first things first, does this object even exist in the scene under
                // the tfViz_go object?
                var parent_go = Utils.FindDeepChildWithName(tfViz_go, parent_id);
                if(parent_go == null)
                {
                    // doesnt exist, create it. parent it under our viz object.
                    parent_go = new GameObject(parent_id);
                    parent_go.transform.SetParent(tfViz_go.transform);

                    // the map_gt frame should correspond to unity's origin.
                    // needs to be done once, since it is static.
                    if(parent_id.Contains("map_gt"))
                    {
                        parent_go.transform.position = Vector3.zero;
                        // the map_gt frame is globally oriented, x=easting y=northing, z=up
                        // in unity, x=east, y=up, z=north
                        // so we need to rotate it in unity to match that.
                        // if we rotate it 90 around unity-X, we get the east/north right
                        // but then z points down, so we scale z  -1 so it points up 
                        // then everything will match once the child is also transformed to RUF
                        parent_go.transform.Rotate(Vector3.right, 90);
                        parent_go.transform.localScale = new Vector3(1,1,-1);
                    }
                }
                // so now the parent object exists.
                // but, it could be that we are reading the tf of a child
                // before we read its parent, so the child might already exist
                // under the tfvizgo already.
                var child_go = Utils.FindDeepChildWithName(tfViz_go, child_id);
                if(child_go == null)
                {
                    child_go = new GameObject(child_id);
                }
                if(child_go.transform.parent == null || child_go.transform.parent.name != parent_go.transform.name)
                {
                    // most of the time the parent-child relations dont change
                    // between message updates, so only set parent if its needed
                    child_go.transform.SetParent(parent_go.transform);
                }
                // either way, update local transform from parent->child
                // ROS is FLU, unity is RUF
                var unity_trans = new Vector3((float)ros_tf.translation.x,
                                              (float)ros_tf.translation.y,
                                              (float)ros_tf.translation.z).To<RUF>();
                child_go.transform.localPosition = (Vector3)unity_trans;
                var unity_rot = new Quaternion((float)ros_tf.rotation.x,
                                               (float)ros_tf.rotation.y,
                                               (float)ros_tf.rotation.z,
                                               (float)ros_tf.rotation.w).To<RUF>();
                child_go.transform.localRotation = (Quaternion)unity_rot;

                // now, we draw a line from parent position to child position
                // in global coords
                Gizmos.color = new Color(1f, 0f, 1f, 1f);
                Gizmos.DrawLine(parent_go.transform.position, child_go.transform.position);
            }
        }
    }
}