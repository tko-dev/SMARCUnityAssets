using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rope
{
    public class RopeContainer : MonoBehaviour
    {
        [Header("Prefabs of the rope parts")]
        public GameObject RopeLinkPrefab;
        public GameObject BuoyPrefab;

        [Header("Connected Bodies")]
        [Tooltip("The base hull of the object this rope connects to. Used for setting the mass of the rope to make physics behave.")]
        public GameObject BaseLink;
        [Tooltip("What should the first link in the rope connect to?")]
        public ArticulationBody ConnectedAB;
        public Rigidbody ConnectedRB;


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

        void OnValidate()
        {
            numSegments = (int)(RopeLength / (SegmentLength-RopeDiameter));
            if(numSegments > 30) Debug.LogWarning($"There will be {numSegments} rope segments generated on game Start, might be too many?");
        }

        GameObject InstantiateLink(GameObject prevLink, int num, bool buoy=false)
        {
            var link = Instantiate(RopeLinkPrefab);
            link.transform.SetParent(transform);
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
                if(ConnectedAB != null)
                    linkJoint.connectedArticulationBody = ConnectedAB;
                else
                    linkJoint.connectedBody = ConnectedRB;
                link.transform.localPosition = Vector3.zero;
                link.transform.rotation = transform.rotation;
            }

            float mass = 1f;
            if(BaseLink.TryGetComponent(out ArticulationBody BaseAB))
                mass = BaseAB.mass;
            if(BaseLink.TryGetComponent(out Rigidbody BaseRB))
                mass = BaseRB.mass;

            var rl = link.GetComponent<RopeLink>();
            rl.SetRopeParams(RopeDiameter, RopeCollisionDiameter, SegmentLength, mass, buoy);

            return link;
        }


        public void SpawnRope()
        {
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
            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}