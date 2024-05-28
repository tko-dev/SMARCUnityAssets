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

                    // utm frame(s) needs special handling, since they are
                    // not at unity origin, and their location is dependent on
                    // the GPS Reference object in the scene.
                    // they need to positioned such that map_gt ends up at unity-origin.
                    // normally, we dont even NEED the utm frame in unity
                    // but ros-vehicles will be under utm and not map_gt.
                    // so to be able to compare things under map_gt and utm, 
                    // we need the utm frame to exist in unity too

                    
                }

                if(parent_id == "utm" && child_id == "map_gt")
                {
                    // the utm frame is globally oriented in ros, x=easting y=northing, z=up
                    // in unity, x=east, y=up, z=north
                    // and we want map_gt to end up at unity-origin
                    // so we move the unity-utm object to negative tf of its child map-gt
                    var utm_unity_pos = new Vector3((float)-ros_tf.translation.x, 
                                                    0, //utm frame is always at height=0
                                                    (float)-ros_tf.translation.y);
                    parent_go.transform.localPosition = utm_unity_pos;
                    
                    // finally, make the map_gt if it doesnt exist
                    // and put it at unity-origin
                    var mapgt_go = Utils.FindDeepChildWithName(tfViz_go, child_id);
                    if(mapgt_go == null)
                    {
                        mapgt_go = new GameObject(child_id);
                    }
                    mapgt_go.transform.SetParent(parent_go.transform);
                    var mapgt_unity_pos = new Vector3((float)ros_tf.translation.x, 
                                                      0,
                                                      (float)ros_tf.translation.y);
                    mapgt_go.transform.localPosition = mapgt_unity_pos;
                    // map_gt should now be in global-unity-origin and the child of "utm"
                    Gizmos.color = new Color(0f, 1f, 1f, 1f);
                    Gizmos.DrawLine(parent_go.transform.position, mapgt_go.transform.position);
                    continue;


                    // // then we need to rotate it in unity to match that.
                    // // if we rotate it 90 around unity-X, we get the east/north right
                    // // but then z points down, so we scale z  -1 so it points up 
                    // // then everything will match once the child is also transformed to RUF
                    // parent_go.transform.Rotate(Vector3.right, 90);
                    // parent_go.transform.localScale = new Vector3(1,1,-1);
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