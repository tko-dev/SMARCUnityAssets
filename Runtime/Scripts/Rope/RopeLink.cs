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
        public GameObject BuoyPrefab;

        ArticulationBody body;
        CapsuleCollider capsule;
        SphereCollider frontFP_sphereCollider, backFP_sphereCollider;
        Transform frontFP_tf, backFP_tf, frontVis_tf, backVis_tf, middleVis_tf;
        ForcePoint frontFP, backFP;

        [Tooltip("Diameter of the rope in meters")]
        public float RopeDiameter = 0.01f;
        [Tooltip("Diameter of the collision objects for the rope. The bigger the more stable the physics are.")]
        public float RopeCollisionDiameter = 0.1f;
        [Tooltip("How long each segment of the rope will be. Smaller = more realistic but harder to simulate.")]
        [Range(0.1f, 1f)]
        public float SegmentLength = 0.1f;

        [Tooltip("How long the entire rope should be. Rounded to SegmentLength. Ignored if this is not the root of the rope.")]
        public float RopeLength = 1f;
        public int numSegments;

        GameObject child;


        void OnValidate()
        {
            numSegments = (int)(RopeLength / (SegmentLength-RopeDiameter));
            if(numSegments > 30) Debug.LogWarning($"There will be {numSegments} rope segments generated on game Start, might be too many?");

            // scale and locate all the little bits and bobs that make up
            // this rope segment depending on the parameters above.
            // Because settings these by hand is a pain.
            body = GetComponent<ArticulationBody>();
            capsule = GetComponent<CapsuleCollider>();

            capsule.radius = RopeCollisionDiameter/2;
            capsule.height = SegmentLength+RopeCollisionDiameter; // we want the collision to overlap with the child's

            var frontSpherePos = new Vector3(0,0, SegmentLength/2 - RopeDiameter/4);
            var backSpherePos = new Vector3(0,0, -(SegmentLength/2 - RopeDiameter/4));

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
            middleVis_tf.localScale = new Vector3(RopeDiameter, (SegmentLength/2)-(RopeDiameter/4), RopeDiameter);

            frontFP_sphereCollider = frontFP_tf.GetComponent<SphereCollider>();
            backFP_sphereCollider = backFP_tf.GetComponent<SphereCollider>();

            frontFP_sphereCollider.radius = RopeDiameter/2;
            backFP_sphereCollider.radius = RopeDiameter/2;

            frontFP = frontFP_tf.GetComponent<ForcePoint>();
            backFP = backFP_tf.GetComponent<ForcePoint>();

            frontFP.depthBeforeSubmerged = RopeDiameter/5;
            backFP.depthBeforeSubmerged = RopeDiameter/5;
        }

        void Awake()
        {
            // Ignore other ropes!
            // Instead of using layers, this is a bit more portable
            var ropeTagged = GameObject.FindGameObjectsWithTag("rope");
            foreach(GameObject rope in ropeTagged)
            {
                Physics.IgnoreCollision(rope.GetComponent<Collider>(), capsule);
            }
        }

        public void SpawnRope()
        {
            SpawnChild(numSegments-1);
        }

        public void DestroyRope()
        {
            RopeLink rl;
            for (var i=transform.childCount-1; i>=0; i--)
            {
                if (transform.GetChild(i).TryGetComponent<RopeLink>(out rl))
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        void InstantiateChild(GameObject childPrefab)
        {
            child = Instantiate(childPrefab);
            child.transform.SetParent(this.transform);
            child.transform.localPosition = new Vector3(0, 0, SegmentLength-RopeDiameter/2);
            child.transform.rotation = this.transform.rotation;
        }

        void SpawnChild(int remainingSegments)
        {
            if(remainingSegments < 1)
            {
                // This is the last segment of the rope, spawn a buoy if given
                if(BuoyPrefab == null) return;
                InstantiateChild(BuoyPrefab);
            }
            else
            {   
                // Still got more to spawn
                InstantiateChild(RopeLinkPrefab);

                child.name = $"RopeLink_{remainingSegments}";

                var rl = child.GetComponent<RopeLink>();
                rl.SpawnChild(remainingSegments-1);
                var ab = child.GetComponent<ArticulationBody>();
                ab.swingZLock = ArticulationDofLock.LockedMotion;
            }
        }
        
    }

}
