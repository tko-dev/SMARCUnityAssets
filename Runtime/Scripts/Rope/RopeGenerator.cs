using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

namespace Rope
{
    public class RopeGenerator : MonoBehaviour
    {
        [Header("Prefab of the rope parts")]
        public GameObject RopeLinkPrefab;

        [Header("Connected Body")]
        [Tooltip("What should the first link in the rope connect to?")]
        public string VehicleConnectionName;
    

        [Header("Rope parameters")]
        [Tooltip("Diameter of the rope in meters")]
        public float RopeDiameter = 0.01f;
        [Tooltip("How long the entire rope should be. Rounded to SegmentLength. Ignored if this is not the root of the rope.")]
        public float RopeLength = 1f;
        [Tooltip("How heavy is this rope?")]
        public float GramsPerMeter = 0.5f;
        [Tooltip("How heavy is the buoy at the end. Set to 0 for no buoy.")]
        public float BuoyGrams = 0f;
        [Tooltip("Linear drag of the buoy.")]
        public float BuoyLinearDrag = 1f;

        [Header("Physics stuff")]
        [Tooltip("Diameter of the collision objects for the rope. The bigger the more stable the physics are.")]
        public float RopeCollisionDiameter = 0.1f;
        [Tooltip("How long each segment of the rope will be. Smaller = more realistic but harder to simulate.")]
        [Range(0.01f, 1f)]
        public float SegmentLength = 0.1f;
        [Tooltip("Rope will be replaced by two sticks when its end-to-end distance is this close to RopeLength")]
        [Range(0f, 0.05f)]
        public float RopeTightnessTolerance = 0.02f;

        [Header("Rendering")]
        public Color RopeColor = Color.yellow;
        LineRenderer ropeLineRenderer;

        [Header("Debug")]
        public bool DrawGizmos = false;

        [HideInInspector] public float SegmentMass => GramsPerMeter * 0.001f * SegmentLength;
        [HideInInspector] public int NumSegments => (int)(RopeLength / (SegmentLength+RopeDiameter));
        //All the rope links we generate will go in here
        [HideInInspector] public GameObject RopeContainer;


        GameObject vehicleBaseLinkConnection, baseLink;
        readonly string containerName = "Rope";
        readonly string baseLinkName = "base_link";
        readonly string hookConnectionPointName = "ConnectionPoint";

        void OnValidate()
        {
            if(NumSegments > 50) Debug.LogWarning($"There will be {NumSegments} rope segments generated on game Start, might be too many?");
        }

        GameObject InstantiateLink(Transform prevLink, int num, bool buoy)
        {
            var link = Instantiate(RopeLinkPrefab);
            link.transform.SetParent(RopeContainer.transform);
            link.name = $"{link.name}_{num}";
            if(buoy) link.name = $"{link.name}_buoy";

            var rl = link.GetComponent<RopeLink>();
            rl.SetRopeParams(this, buoy);

            if(prevLink != null) rl.SetupConnectionToPrevLink(prevLink);
            else rl.SetupConnectionToVehicle(vehicleBaseLinkConnection, baseLink);
            
            return link;
        }


        public void SpawnRope()
        {
            vehicleBaseLinkConnection = Utils.FindDeepChildWithName(transform.root.gameObject, VehicleConnectionName);
            baseLink = Utils.FindDeepChildWithName(transform.root.gameObject, baseLinkName);

            if(RopeContainer == null)
            {
                RopeContainer = new GameObject(containerName);
                RopeContainer.transform.SetParent(transform.root);
                RopeContainer.transform.position = vehicleBaseLinkConnection.transform.position;
                RopeContainer.transform.rotation = vehicleBaseLinkConnection.transform.rotation;
            }

            ropeLineRenderer = RopeContainer.AddComponent<LineRenderer>();
            ropeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            ropeLineRenderer.startColor = RopeColor;
            ropeLineRenderer.endColor = RopeColor;
            ropeLineRenderer.startWidth = RopeDiameter;
            ropeLineRenderer.endWidth = RopeDiameter;
            ropeLineRenderer.positionCount = NumSegments+1;
            ropeLineRenderer.useWorldSpace = true;
            ropeLineRenderer.receiveShadows = false;

            InstantiateLink(null, 0, false);

            for(int i=1; i < NumSegments; i++)
            {
                var buoy = (i+1 == NumSegments) && (BuoyGrams > 0);
                InstantiateLink(RopeContainer.transform.GetChild(i-1), i, buoy);
            }

            foreach(Transform child in RopeContainer.transform)
            {
                var rl = child.GetComponent<RopeLink>();
                rl.AssignFirstAndLastSegments();
            }

            UpdateLineRenderer();
        }

        public void DestroyRope()
        {
            RopeContainer = Utils.FindDeepChildWithName(transform.root.gameObject, containerName);
            if(RopeContainer == null) return;

            if (Application.isPlaying) Destroy(RopeContainer);
            else DestroyImmediate(RopeContainer);
        }

        public void ReplaceRopeWithStick(GameObject connectedHookGO)
        {
            // the rope breaking means its tight and carrying something.
            // so we replace the entire rope with a STICK
            // to make the physics more stable!
            // Reverse loop because we're gonna remove things from the collection
            var container = RopeContainer.transform;
            for(int i=container.childCount-1; i>=0; i--)
                Destroy(container.GetChild(i).gameObject);

            // Now that all the rope is gone, create a new RopeLink object
            // but, at this point, we shall have TWO segments that is as long as the rope
            // why two? because one wouldnt allow the rope to bend _at all_, making
            // any kind of "pick up and lower" maneuver's "lower" part very hard.
            // two segments is still quite stable compared to 10s...
            SegmentLength = RopeLength/2;

            var stickBase = InstantiateLink(null, 1000, buoy:false);
            // InstantiateLink calls RopeLink::SetupConnectionToVehicle
            // where the rope link is created "going straigh out" from the baselink
            // but in this case we need the rope to be "looking at" the hook it is connected
            var hookConnectionPoint = connectedHookGO.transform.Find(hookConnectionPointName);
            stickBase.transform.LookAt(hookConnectionPoint.transform.position);
            var stickBaseRL = stickBase.GetComponent<RopeLink>();

            var stickTip = InstantiateLink(stickBase.transform, 1001, buoy:false);
            var stickTipRL = stickTip.GetComponent<RopeLink>();

            // this stick is already connected to the base_link
            // but now it also needs to connect to the hook's connection point
            // we took the hook object from the ropelink that called this method.
            // See RopeLink::OnCollisionEnter then RopeLink::FixedUpdate
            stickTipRL.ConnectToHook(connectedHookGO);

            // This is not needed! Code left as lesson.
            // and finally, we set the mass scales of the rope pieces
            // so that they pull the drone/auv proportional to auv/drone's mass.
            // var hookBaseLink = Utils.FindDeepChildWithName(connectedHookGO.transform.root.gameObject, baseLinkName);
            // var hookBaseLinkAB = hookBaseLink.GetComponent<ArticulationBody>();
            // var baseLinkAB = baseLink.GetComponent<ArticulationBody>();
            // var pullerOverPulledMassRatio = hookBaseLinkAB.mass / baseLinkAB.mass;
            // divide by 2 because one is pulling the other is getting pulled more by this much
            // mass ratio. Gotta share the force, not accumulate it.
            // stickBaseRL.SetMassScaleForPulling(pullerOverPulledMassRatio/2);
            // stickTipRL.SetMassScaleForPulling(pullerOverPulledMassRatio/2);

            // Disable collision between the stick and the hook so that we can attach the rope to he base links CoM.
            var colliders = connectedHookGO.transform.root.gameObject.GetComponentsInChildren<Collider>();
            foreach(var c in colliders)
            {
                Physics.IgnoreCollision(c, stickBase.GetComponent<Collider>());
                Physics.IgnoreCollision(c, stickTip.GetComponent<Collider>());
            }
        }

        void Awake()
        {
            if(RopeContainer == null) RopeContainer = Utils.FindDeepChildWithName(transform.root.gameObject, containerName);
            if(vehicleBaseLinkConnection == null) vehicleBaseLinkConnection = Utils.FindDeepChildWithName(transform.root.gameObject, VehicleConnectionName);
            if(baseLink == null) baseLink = Utils.FindDeepChildWithName(transform.root.gameObject, baseLinkName);
            if(RopeContainer != null && ropeLineRenderer == null) ropeLineRenderer = RopeContainer.GetComponent<LineRenderer>();
        }


        void UpdateLineRenderer()
        {
            for (int i = 0; i < NumSegments; i++)
            {
                var child = RopeContainer.transform.GetChild(i);
                ropeLineRenderer.SetPosition(i, child.position);
            }

            var lastChild = RopeContainer.transform.GetChild(NumSegments-1);;
            ropeLineRenderer.SetPosition(NumSegments, lastChild.position+lastChild.forward*SegmentLength);
        }
        void Update()
        {
            if(RopeContainer != null) UpdateLineRenderer();
        }

    }
}