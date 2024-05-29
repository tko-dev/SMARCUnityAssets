using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
// All the ENU etc conversion come from this:
using Unity.Robotics.ROSTCPConnector.ROSGeometry; 

using Utils = DefaultNamespace.Utils;
using GPSReferencePoint = VehicleComponents.Sensors.GPSReferencePoint;


namespace VehicleComponents.ROS.Subscribers
{

    public class TFtoUnity: MonoBehaviour
    {
        public bool drawLines = true;
        public bool createArrows = true;
        public GameObject arrowsPrefab;
        string topic = "/tf";
        ROSConnection ros;
        TFMessageMsg tfMsg;
        GameObject utm_go, utm_zb_go;

        // easting,northing wrt unity-origin.
        public double[] unity_origin_in_utm;
        GPSReferencePoint gpsRef;


        void Awake()
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<TFMessageMsg>(topic, UpdateMessage);
            unity_origin_in_utm = new double[2];

            // UTM stuff needs speical handling due to their large numbers
            // and Unity's float representation in transforms.
            transform.position = Vector3.zero;
            var gpsRefs = FindObjectsByType<GPSReferencePoint>(FindObjectsSortMode.None);
            if(gpsRefs.Length < 1)
            {
                Debug.Log("No GPS Reference found in the scene. UTM-related functions will not work!");
                return;
            }
            gpsRef = gpsRefs[0];
            double easting, northing, lat, lon;
            (easting, northing, lat, lon) = gpsRef.GetUTMLatLonOfObject(gameObject);
            unity_origin_in_utm[0] = easting;
            unity_origin_in_utm[1] = northing;
        }

        Transform GetOrCreate(string id)
        {
            GameObject go = Utils.FindDeepChildWithName(gameObject, id);
            if(go == null)
            {
                go = new GameObject(id);
                go.transform.SetParent(transform);
                if(createArrows && arrowsPrefab != null)
                {
                    var arrows = Instantiate(arrowsPrefab, go.transform.position, go.transform.rotation);
                    arrows.transform.SetParent(go.transform);
                }
            }
            return go.transform;
        }

        void UpdateMessage(TFMessageMsg tfMsg)
        {
            this.tfMsg = tfMsg;
            foreach(TransformStampedMsg tfStamped in tfMsg.transforms)
            {
                string parent_id = tfStamped.header.frame_id;                
                string child_id = tfStamped.child_frame_id;
                TransformMsg ros_tf = tfStamped.transform;
                
                Transform parent_tf = GetOrCreate(parent_id);
                Transform child_tf = GetOrCreate(child_id);
                // ros_tf is always relative to parent, so we need the same
                // relative transform structure in unity.
                // However, unity and ros have different coordinate frames.
                // ROS = FLU
                // Uni = RUF
                // in addition, globally-referenced frames like utm
                // should be in ENU.
                // ros-utm = ENU
                // uni-utm = EUN
                // so we gotta map these proper.
                // ALSO ALSO, utm-related stuff tend to have large numerical coordinates
                // which break things when put in floats in unity transforms
                // thus, we must work in relation to unity's origin point.
                
                if(parent_id.Contains("utm"))
                {
                    if(gpsRef == null) continue;
                    
                    // ENU -> EUN, globally positioned such that
                    // unity's origin ends up at 0,0
                    // and if there is a hierarchy of utm frames they all end
                    // up in the same location.
                    parent_tf.position = ENU.ConvertToRUF(
                        new Vector3(
                            (float) -unity_origin_in_utm[0],
                            (float) -unity_origin_in_utm[1],
                            0));


                    // similarly, the child of any utm frame is also in easting/northing
                    // so it is positioned globally, but this time with the ros_tf included
                    // notice math done in doubles!
                    child_tf.position = ENU.ConvertToRUF(
                        new Vector3(
                            (float) (ros_tf.translation.x - unity_origin_in_utm[0]),
                            (float) (ros_tf.translation.y - unity_origin_in_utm[1]),
                            0));
                    // notice that we dont parent these together!
                    
                    // further, if the child of the utm is a map
                    // then we want it to also orient FLU->RUF
                    // from map down, everything should be
                    // in FLU and no more utm exceptions
                    if(child_id.Contains("map"))
                    {
                        child_tf.localRotation = ENU.ConvertToRUF(
                            new Quaternion(
                                (float)ros_tf.rotation.x,
                                (float)ros_tf.rotation.y,
                                (float)ros_tf.rotation.z,
                                (float)ros_tf.rotation.w));
                    }
                }
                else
                {
                    // at "map" level downwards, things in ROS are FLU
                    // and with relatively small coordinate numbers
                    // such that unity's floats should not kill them.
                    
                    // ASSUMPTION: all maps are children of utms in ros.
                    // which means this "map" frame object is placed where it
                    // should be in the unity world when the parent was "utm".

                    // then put the child frame under this
                    child_tf.SetParent(parent_tf);
                    // and position it
                    child_tf.localPosition = FLU.ConvertToRUF(
                        new Vector3(
                            (float)ros_tf.translation.x,
                            (float)ros_tf.translation.y,
                            (float)ros_tf.translation.z));

                    child_tf.localRotation = FLU.ConvertToRUF(
                        new Quaternion(
                            (float)ros_tf.rotation.x,
                            (float)ros_tf.rotation.y,
                            (float)ros_tf.rotation.z,
                            (float)ros_tf.rotation.w));
                }


            }
        }

        void XUpdateMessage(TFMessageMsg tfMsg)
        {
            this.tfMsg = tfMsg;

            foreach(TransformStampedMsg tfStamped in tfMsg.transforms)
            {
                string parent_id = tfStamped.header.frame_id;                
                string child_id = tfStamped.child_frame_id;
                TransformMsg ros_tf = tfStamped.transform;
                // so, we have an objects name, and its child, and the transform
                // between these two.
                // first things first, does this pair of objects even exist under this gameObject?
                var parent_go = Utils.FindDeepChildWithName(gameObject, parent_id);
                if(parent_go == null)
                {
                    // doesnt exist, create it. parent it under our viz object.
                    parent_go = new GameObject(parent_id);
                    parent_go.transform.SetParent(transform);
                    // Debug.Log($"Created {parent_id} under main viz object");
                    if(createArrows && arrowsPrefab != null)
                    {
                        var arrows = Instantiate(arrowsPrefab, parent_go.transform.position, parent_go.transform.rotation);
                        arrows.transform.SetParent(parent_go.transform);
                    }
                }

                // it could be that we are reading the tf of a child
                var child_go = Utils.FindDeepChildWithName(gameObject, child_id);
                if(child_go == null) 
                {
                    child_go = new GameObject(child_id);
                    // Debug.Log($"Created {child_id}");
                    if(createArrows && arrowsPrefab != null)
                    {
                        var arrows = Instantiate(arrowsPrefab, child_go.transform.position, child_go.transform.rotation);
                        arrows.transform.SetParent(child_go.transform);
                    }
                }
                if(child_go.transform.parent == null || child_go.transform.parent.name != parent_go.transform.name)
                {
                    if(parent_id.Contains("utm")) // utm stuff needs special handling.
                    {
                        child_go.transform.SetParent(transform);
                    }
                    else{
                        // most of the time the parent-child relations dont change
                        // between message updates, so only set parent if its needed
                        child_go.transform.SetParent(parent_go.transform);
                        // Debug.Log($"Parented {child_id} to {parent_id}");
                    }
                }

                // There are 3 special cases that involve utm frames:
                // 1) utm     -> map_gt 
                // 2) utm_XXX -> utm
                // 3) utm     -> XXX
                // For these cases, we gotta do the math outside of unity transforms using doubles.
                // We will place all frames that (eventually) go under utm
                // under gameObject instead to avoid floating point problems.

                // CASE 1 utm->map_gt
                // utm is not FLU/RUF, its NED
                // and unity is EUN
                // we also want map_gt to be at unity-origin
                // so instead of moving the child map_gt wrt to its parent utm
                // we move utm wrt to its child map_gt
                if(parent_id == "utm" && child_id == "map_gt")
                {
                    if(utm_go != null) continue;

                    // Debug.Log("utm -> map_gt");
                    // we could just use the transform, but utm frames
                    // are usually _very_ far. So we gotta connect things
                    // over utm through double-valued objects and not transforms
                    // what we really want is parent_frame wrt map_gt always.
                    // TODO: this could be done without the TF stuff.. using the GPSRef object in awake?
                    // unity_origin_in_utm[0] = -ros_tf.translation.x;
                    // unity_origin_in_utm[1] = -ros_tf.translation.y;

                    // we want map_gt to be independent of the utm object
                    // so that there is no 65km float and 6mm float within
                    // the same transform tree. floats are not good for such ranges.
                    child_go.transform.SetParent(transform);
                    parent_go.transform.SetParent(transform);
                    
                    // put the utm frame where it is, the object is useless
                    // but good for completeness
                    parent_go.transform.localPosition = new Vector3((float)unity_origin_in_utm[0], 
                                                                    0, //utm frame is always at height=0
                                                                    (float)unity_origin_in_utm[1]);
                    
                    // and the map_gt frame where it would be under utm, but under tfviz
                    child_go.transform.localPosition = new Vector3(
                        (float)(ros_tf.translation.x + unity_origin_in_utm[0]),
                        0,
                        (float)(ros_tf.translation.y + unity_origin_in_utm[1])
                    );
                    // utm is at global-MINUS and map at relative-PLUS 
                    // -> map_gt at global-zero

                    // then we need to rotate map_gt in unity to match EUN.
                    // if we rotate it 90 around unity-X, we get the east/north right
                    // but then z points down, so we scale z  -1 so it points up 
                    // then everything will match once the child is also transformed to RUF
                    child_go.transform.Rotate(Vector3.right, 90);
                    child_go.transform.localScale = new Vector3(1,1,-1);

                    // and finally, we remember where we put that utm object
                    // so if we later get a utm_zone_band frame, we can use that too
                    utm_go = parent_go;
                    continue;
                }

                // CASE 2 utm_XXX -> utm
                if(parent_id.Contains("utm_") && child_id == "utm")   
                {
                    if(utm_zb_go != null) continue;
                    if(utm_go == null) continue;
                    // this is a utm_ZONE_BAND->utm kind of tf.
                    // and it should be a relative-zero
                    // BUT, since utm is set wrt to map_gt, we need that first
                    // once we have utm->map_gt, we can transform utm's global position
                    // to utm_XXX, and set utm's local position to 0 under utm_XXX.
                    
                    // Debug.Log($"Set {parent_id}->utm");
                    parent_go.transform.SetParent(transform);
                    parent_go.transform.localPosition = new Vector3((float)unity_origin_in_utm[0], 
                                                                    0, //utm frame is always at height=0
                                                                    (float)unity_origin_in_utm[1]);
                    utm_go.transform.SetParent(parent_go.transform);
                    utm_go.transform.localPosition = Vector3.zero;
                    utm_zb_go = parent_go;
                    continue;
                }

                // CASE 3 utm -> XXX
                // This is similar to case 1
                // with the biggest difference being the lack of "set up"
                // since by this point, we assume a roundtrip of sim --(utm)--> ros --(utm)--> sim
                // has happened and we have set things up for utm-connected frames using case 1 above
                if(parent_id == "utm")
                {
                    if(utm_go == null) continue;
                    
                    child_go.transform.localPosition = new Vector3(
                        (float)(ros_tf.translation.x + unity_origin_in_utm[0]),
                        0,
                        (float)(ros_tf.translation.y + unity_origin_in_utm[1])
                    );
                    child_go.transform.Rotate(Vector3.right, 90);
                    child_go.transform.localScale = new Vector3(1,1,-1);
                    continue;
                }
                
                if(child_id == "map_gt") Debug.Log($"unchecked map_gt case: {parent_id} -> map_gt");
                // GENERAL CASE
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
            if(drawLines) RecursiveGizmoLines(transform);
        }
    }
}