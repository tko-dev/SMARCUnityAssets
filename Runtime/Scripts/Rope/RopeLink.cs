using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force; // ForcePoints

namespace Rope
{
    [RequireComponent(typeof(ArticulationBody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RopeLink : MonoBehaviour
    {
        public GameObject RopeLinkPrefab;
        ArticulationBody body;
        CapsuleCollider capsule;
        SphereCollider frontFP_sphereCollider, backFP_sphereCollider;
        Transform frontFP_tf, backFP_tf, frontVis_tf, backVis_tf, middleVis_tf;
        ForcePoint frontFP, backFP;

        [Tooltip("Diameter of the rope in meters")]
        public float RopeDiameter = 0.01f;
        [Tooltip("How long each segment of the rope will be. Smaller = more realistic but harder to simulate.")]
        [Range(0.01f, 1f)]
        public float SegmentLength = 0.1f;

        [Tooltip("How long the entire rope should be. Rounded to SegmentLength. Ignored if this is not the root of the rope.")]
        public float RopeLength = 1f;

        public int numSegments;

        public bool Root = false;


        void OnValidate()
        {
            numSegments = (int)(RopeLength / SegmentLength);
            if(numSegments > 20) Debug.LogWarning($"There will be {numSegments} rope segments generated on game Start, might be too many?");

            // scale and locate all the little bits and bobs that make up
            // this rope segment depending on the parameters above.
            // Because settings these by hand is a pain.
            body = GetComponent<ArticulationBody>();
            capsule = GetComponent<CapsuleCollider>();

            capsule.radius = RopeDiameter/2;
            capsule.height = SegmentLength;

            var frontSpherePos = new Vector3(0,0, SegmentLength/2 - RopeDiameter/2);
            var backSpherePos = new Vector3(0,0, -(SegmentLength/2 - RopeDiameter/2));

            body.anchorPosition = backSpherePos;

            frontFP_tf = transform.Find("ForcePoint_F");
            backFP_tf = transform.Find("ForcePoint_B");
            frontVis_tf = transform.Find("Visuals/Front");
            backVis_tf = transform.Find("Visuals/Back");
            middleVis_tf = transform.Find("Visuals/Middle");

            frontFP_tf.localPosition = frontSpherePos;
            frontVis_tf.localPosition = frontSpherePos;
            backFP_tf.localPosition = backSpherePos;
            backVis_tf.localPosition = backSpherePos;

            var visualScale = new Vector3(RopeDiameter, RopeDiameter, RopeDiameter);
            frontVis_tf.localScale = visualScale;
            backVis_tf.localScale = visualScale;
            middleVis_tf.localScale = new Vector3(RopeDiameter, (SegmentLength/2)-(RopeDiameter/2), RopeDiameter);

            frontFP_sphereCollider = frontFP_tf.GetComponent<SphereCollider>();
            backFP_sphereCollider = backFP_tf.GetComponent<SphereCollider>();

            frontFP_sphereCollider.radius = RopeDiameter/2;
            backFP_sphereCollider.radius = RopeDiameter/2;

            frontFP = frontFP_tf.GetComponent<ForcePoint>();
            backFP = backFP_tf.GetComponent<ForcePoint>();

            frontFP.depthBeforeSubmerged = RopeDiameter/5;
            backFP.depthBeforeSubmerged = RopeDiameter/5;
        }

        void Start()
        {

            if(!Root) return; // dont spawn stuff if youre not the root

            GameObject currentTip = gameObject;

            for(int i=0; i < numSegments-1; i++) // -1 because we already have a segment: the root
            {
                Debug.Log($"Tip is:{currentTip.name}");
                // create a new instance of the rope link
                GameObject newChild = Instantiate(
                    RopeLinkPrefab,
                    new Vector3(0, 0, SegmentLength),
                    Quaternion.identity,
                    currentTip.transform
                );
                newChild.name = $"Link_{i}";
                foreach (Transform child in newChild.transform)
                {
                    Debug.Log($"{newChild}'s child:{child.name}");
                }
                Debug.Log($"{newChild}'s parent:{newChild.transform.parent.name}");
                
                var rl = newChild.GetComponent<RopeLink>();
                rl.Root = false;
                currentTip = newChild;
                Debug.Log($"New tip is: {currentTip.name}");
            }


            // if(RemainingRopeLength <= SegmentLength) return;
            // if(childSegment != null) return;

            // // still got rope to extend, make a child
            // childSegment = Instantiate(RopeLinkPrefab);
            // childSegment.transform.SetParent(transform);
            // childSegment.transform.localPosition = new Vector3(0, 0, SegmentLength);
            // var childRopeLink = childSegment.GetComponent<RopeLink>();
            // childRopeLink.RemainingRopeLength = RemainingRopeLength - SegmentLength;
            // childRopeLink.SegmentLength = SegmentLength;
            // childRopeLink.RopeDiameter = RopeDiameter;
            // childSegment.name = $"RopeLink_{childRopeLink.RemainingRopeLength}";

            // // this is the root segment, spawn children
            // int numSegments = (int)(RemainingRopeLength/SegmentLength);
            // segments = new Transform[numSegments];

            // for(int i=0; i<numSegments-1; i++) // there is already one segment, the root.
            // {
            //     GameObject segment = Instantiate(RopeLinkPrefab);
            //     RopeLink rl = segment.GetComponent<RopeLink>();
            //     rl.Root = false;
            //     segment.name = $"RopeLink_{i+1}";
            //     segments[i] = segment.transform;
            // }

            // segments[0].SetParent(this.transform);
            // Debug.Log($"Setting parent of {segments[0].name} to {this.transform.name}");

            // for(int i=1; i<numSegments-1; i++)
            // {
            //     Debug.Log(i);
            //     var current = segments[i];
            //     var prev = segments[i-1];
            //     current.SetParent(prev);
            //     Debug.Log($"Setting parent of {current.name} to {prev.name}");
            //     current.localPosition = new Vector3(SegmentLength, 0, 0);
            // }
            
            
        }
        
    }

}
