using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils = DefaultNamespace.Utils;

namespace Rope
{
    public class RopeGenerator : MonoBehaviour
    {
        [Header("Prefab of the rope parts")]
        public GameObject RopeLinkPrefab;

        [Header("Connected Body")]
        [Tooltip("What should the first link in the rope connect to?")]
        public string ConnectedLinkName;
    

        [Header("Rope parameters")]
        [Tooltip("Diameter of the rope in meters")]
        public float RopeDiameter = 0.01f;
        [Tooltip("Diameter of the collision objects for the rope. The bigger the more stable the physics are.")]
        public float RopeCollisionDiameter = 0.1f;
        [Tooltip("How long each segment of the rope will be. Smaller = more realistic but harder to simulate.")]
        [Range(0.01f, 1f)]
        public float SegmentLength = 0.1f;
        [Tooltip("How long the entire rope should be. Rounded to SegmentLength. Ignored if this is not the root of the rope.")]
        public float RopeLength = 1f;
        public int numSegments;

        GameObject ropeContainer, ropeLink, baseLink;
        string containerName = "Rope";

        void OnValidate()
        {
            numSegments = (int)(RopeLength / (SegmentLength-RopeDiameter));
            if(numSegments > 50) Debug.LogWarning($"There will be {numSegments} rope segments generated on game Start, might be too many?");
        }

        GameObject InstantiateLink(GameObject prevLink, int num, bool buoy=false)
        {
            var link = Instantiate(RopeLinkPrefab);
            link.transform.SetParent(ropeContainer.transform);
            link.name = $"{link.name}_{num}";
            if(buoy) link.name = $"{link.name}_buoy";

            var linkJoint = link.GetComponent<Joint>();
            if(prevLink != null)
            {
                var linkZ = prevLink.transform.localPosition.z + (SegmentLength-RopeDiameter/2);
                link.transform.localPosition = new Vector3(0, 0, linkZ);
                link.transform.rotation = prevLink.transform.rotation;
                linkJoint.connectedBody = prevLink.GetComponent<Rigidbody>();
            }
            else
            {
                // First link in the chain, not connected to another link
                // see what the parent has... and joint to it.
                if(ropeLink.TryGetComponent<ArticulationBody>(out ArticulationBody ab))
                    linkJoint.connectedArticulationBody = ab;
                if(ropeLink.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    linkJoint.connectedBody = rb;

                link.transform.localPosition = new Vector3(0, 0, SegmentLength/2);
                
            }

            float mass = 1f;
            if(baseLink.TryGetComponent(out ArticulationBody BaseAB))
                mass = BaseAB.mass;
            if(baseLink.TryGetComponent(out Rigidbody BaseRB))
                mass = BaseRB.mass;

            var rl = link.GetComponent<RopeLink>();
            rl.SetRopeParams(RopeDiameter, RopeCollisionDiameter, SegmentLength, mass, buoy);

            return link;
        }


        public void SpawnRope()
        {
            ropeLink = Utils.FindDeepChildWithName(transform.root.gameObject, ConnectedLinkName);
            baseLink = Utils.FindDeepChildWithName(transform.root.gameObject, "base_link");

            if(ropeContainer == null)
                ropeContainer = new GameObject(containerName);
                ropeContainer.transform.SetParent(transform.root);
                ropeContainer.transform.localPosition = ropeLink.transform.position;
                ropeContainer.transform.rotation = ropeLink.transform.rotation;

            var links = new GameObject[numSegments];

            links[0] = InstantiateLink(null, 0);

            for(int i=1; i < numSegments; i++)
            {
                var buoy = i+1 == numSegments;
                links[i] = InstantiateLink(links[i-1], i, buoy);
            }
        }

        public void DestroyRope()
        {
            while(true)
            {
                ropeContainer = Utils.FindDeepChildWithName(transform.root.gameObject, containerName);
                if(ropeContainer == null) break;
                DestroyImmediate(ropeContainer);
            }
        }
    }
}