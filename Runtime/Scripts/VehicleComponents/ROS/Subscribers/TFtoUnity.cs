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

        GameObject utm_go, map_gt_go, utm_zb_go;

        void Awake()
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<TFMessageMsg>(topic, UpdateMessage);
            tfViz_go = new GameObject("ROS_TF_Visualizer");
        }

        void UpdateMessage(TFMessageMsg tfMsg)
        {
            this.tfMsg = tfMsg;

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
                    Debug.Log($"Created {parent_id} under main viz object");
                }

                // it could be that we are reading the tf of a child
                var child_go = Utils.FindDeepChildWithName(tfViz_go, child_id);
                if(child_go == null) 
                {
                    child_go = new GameObject(child_id);
                    Debug.Log($"Created {child_id}");
                }
                if(child_go.transform.parent == null || child_go.transform.parent.name != parent_go.transform.name)
                {
                    // most of the time the parent-child relations dont change
                    // between message updates, so only set parent if its needed
                    child_go.transform.SetParent(parent_go.transform);
                    Debug.Log($"Parented {child_id} to {parent_id}");
                }

                // special case for utm->map_gt transform
                // because utm is not FLU/RUF, its NED
                // and unity is EUN
                // we also want map_gt to be at unity-origin
                // so instead of moving the child map_gt wrt to its parent utm
                // we move utm wrt to its child map_gt
                if(parent_id == "utm" && child_id.Contains("map"))
                {
                    // this needs to be done just once
                    if(this.utm_go == null || this.map_gt_go == null)
                    {

                    Debug.Log("utm->map");
                    parent_go.transform.localPosition = new Vector3((float)-ros_tf.translation.x, 
                                                                    0, //utm frame is always at height=0
                                                                    (float)-ros_tf.translation.y);
                    
                    child_go.transform.localPosition = new Vector3((float)ros_tf.translation.x, 
                                                                   0,
                                                                   (float)ros_tf.translation.y);
                    // utm is at global-MINUS and map_gt at relative-PLUS -> map_gt at global-zero

                    // then we need to rotate map_gt in unity to match EUN.
                    // if we rotate it 90 around unity-X, we get the east/north right
                    // but then z points down, so we scale z  -1 so it points up 
                    // then everything will match once the child is also transformed to RUF
                    child_go.transform.Rotate(Vector3.right, 90);
                    child_go.transform.localScale = new Vector3(1,1,-1);

                    this.utm_go = parent_go;
                    this.map_gt_go = child_go;
                    }
                }
                else 
                if(parent_id.Contains("utm_") && child_id == "utm" && utm_zb_go == null)   
                {
                    // this is a utm_ZONE_BAND->utm kind of tf.
                    // and it should be a relative-zero
                    // BUT, since utm is set wrt to map_gt, we need that first
                    // once we have utm->map_gt, we can transferm utm's global position
                    // to utm_XXX, and set utm's local position to 0 under utm_XXX.
                    if(map_gt_go != null && utm_go != null)
                    {
                        Debug.Log($"Set {parent_id}->utm");
                        parent_go.transform.localPosition = utm_go.transform.localPosition;
                        utm_go.transform.localPosition = Vector3.zero;
                        utm_zb_go = parent_go;
                    }
                }
                else
                {
                    // this is a non-global transform, as in, it is not referenced
                    // to earth itself, but another euclidian tf frame.
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
                }                
            }
        }


        void RecursiveGizmoLines(Transform parent_tf)
        {
            foreach(Transform child_tf in parent_tf)
            {
                Gizmos.color = new Color(1f, 0f, 1f, 1f);
                Gizmos.DrawLine(parent_tf.position, child_tf.position);

                RecursiveGizmoLines(child_tf);
            }
        }

        void OnDrawGizmos()
        {
            if(tfViz_go == null) return;
            RecursiveGizmoLines(tfViz_go.transform);
        }
    }
}