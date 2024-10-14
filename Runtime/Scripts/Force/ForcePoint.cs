using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Water;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor.EditorTools;

// This is a very simple example of how we could compute a buoyancy force at variable points along the body.
// Its not really accurate per se.
// [RequireComponent(typeof(Rigidbody))]
// [RequireComponent(typeof(IForceModel))]
namespace Force
{

    // Because Arti bodies and Rigid bodies dont share
    // an ancestor, even though they share like 99% of the
    // methods and semantics...
    public class MixedBody
    {
        public ArticulationBody ab;
        public Rigidbody rb;

        public bool automaticCenterOfMass
        {
            get {return ab ? ab.automaticCenterOfMass : rb.automaticCenterOfMass; }
            set {
                if(ab != null) ab.automaticCenterOfMass = value;
                else rb.automaticCenterOfMass = value;
                }
        }

        public Vector3 centerOfMass
        {
            get {return ab ? ab.centerOfMass : rb.centerOfMass; }
            set {
                if(ab != null) ab.centerOfMass = value;
                else rb.centerOfMass = value;
            }
        }

        public bool useGravity
        {
            get {return ab ? ab.useGravity : rb.useGravity; }
            set {
                if(ab != null) ab.useGravity = value;
                else rb.useGravity = value;
            }
        }

        public float mass
        {
            get {return ab ? ab.mass : rb.mass; }
            set {
                if(ab != null) ab.mass = value;
                else rb.mass = value;
            }
        }

        public float drag
        {
            get {return ab ? ab.linearDamping : rb.drag; }
            set {
                if(ab != null) ab.linearDamping = value;
                else rb.drag = value;
            }
        }

        public float angularDrag
        {
            get {return ab ? ab.angularDamping : rb.angularDrag; }
            set {
                if(ab != null) ab.angularDamping = value;
                else rb.angularDrag = value;
            }
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
        {
            if(ab != null)
                ab.AddForceAtPosition(force, position, mode);
            else
                rb.AddForceAtPosition(force, position, mode);
        }
    }

    public class ForcePoint : MonoBehaviour
    {
        [Header("Connected Body")]
        public ArticulationBody ConnectedArticulationBody;
        public Rigidbody ConnectedRigidbody;


        [Header("Buoyancy")]
        [Tooltip("GameObject that we will calculate the volume of. Set volume below to 0 to use.")]
        public GameObject VolumeObject;
        [Tooltip("If the gameObject above has many meshes, set the one to use for volume calculations here.")]
        public Mesh VolumeMesh;
        [Tooltip("If not zero, will be used for buoyancy calculations. If zero, the volumeObject/Mesh above will be used to calculate.")]
        public float Volume;
        public float WaterDensity = 997; // kg/m3
        [Tooltip("How deep should the point be to apply the entire buoyancy force. Force is applied proportionally.")]
        public float DepthBeforeSubmerged = 0.03f;
        [Tooltip("Maximum force applied by buoyancy. Nice to keep things from going to space :)")]
        public float MaxBuoyancyForce = 1000f;

        [Header("Underwater/Air Drag")]
        [Tooltip("Linear Drag applied while underwater. Sets the connected body's drag/linearDamping value when underwater. Set to -1 to use the starting drag value of the body for this.")]
        public float UnderwaterDrag = -1f;

        [Tooltip("Angular Drag applied while underwater. Sets the connected body's drag/linearDamping value when underwater. Set to -1 to use the starting drag value of the body for this.")]
        public float UnderwaterAngularDrag = -1f;

        [Tooltip("Linear Drag applied while underwater. Sets the connected body's drag/linearDamping value when above water. Set to -1 to use the starting drag value of the body for this.")]
        public float AirDrag = -1f;

        [Tooltip("Angular Drag applied while underwater. Sets the connected body's drag/linearDamping value when above water. Set to -1 to use the starting drag value of the body for this.")]
        public float AirAngularDrag = -1f;
        public bool IsUnderwater = false;


        [Header("Gravity")]
        [Tooltip("Do we over-ride the gravity of the connected body?")]
        public bool AddGravity = false;
        [Tooltip("If true, calculates center of gravity from all the ForcePoints on the body and overrides the body's centerOfMass, otherwise the centerOfMass of the connected body is used.")]
        public bool AutomaticCenterOfGravity = false;
        [Tooltip("If not zero, will be used for gravity force. If zero, the connected body's mass will be used instead.")]
        public float Mass;


        [Header("Debug")]
        public bool DrawForces = false;
        public float GizmoSize = 0f;
        public float AppliedBuoyancyForce, AppliedGravityForce;
        public bool ApplyCustomForce = false;
        public Vector3 CustomForce = Vector3.zero;


        private MixedBody body;
        private WaterQueryModel waterModel;
        private ForcePoint[] allForcePoints;

        public void ApplyCurrent(Vector3 current)
        {
            if (GetDepth() > 0)
            {
                body.AddForceAtPosition
                        (
                            current / allForcePoints.Length,
                            transform.position,
                            ForceMode.Force
                        );
            }
        }


        public void Awake()
        {
            body = new MixedBody();
            
            if(ConnectedArticulationBody == null && ConnectedRigidbody == null)
                Debug.LogWarning($"{gameObject.name} requires at least one of ConnectedArticulationBody or ConnectedRigidBody to be set!");
            
            if(ConnectedArticulationBody != null) body.ab = ConnectedArticulationBody;
            if(ConnectedRigidbody!= null) body.rb = ConnectedRigidbody;

            if(AirDrag == -1) AirDrag = body.drag;
            if(AirAngularDrag == -1) AirAngularDrag = body.angularDrag;
            if(UnderwaterDrag == -1) UnderwaterDrag = body.drag;
            if(UnderwaterAngularDrag == -1) UnderwaterAngularDrag = body.angularDrag;

             // If the force point is doing the gravity, disable the body's own
            if(AddGravity)
            {
                body.useGravity = false;
                if(Mass == 0) Mass = body.mass;
            }

            waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];

            allForcePoints = transform.root.gameObject.GetComponentsInChildren<ForcePoint>();
            if (AutomaticCenterOfGravity)
            {
                body.automaticCenterOfMass = false;
                var centerOfMass = allForcePoints.Select(point => point.transform.localPosition).Aggregate(new Vector3(0, 0, 0), (s, v) => s + v);
                body.centerOfMass = centerOfMass / allForcePoints.Length;
            }
           
            if (VolumeMesh == null && VolumeObject != null) VolumeMesh = VolumeObject.GetComponent<MeshFilter>().mesh;
            if (Volume == 0 && VolumeMesh != null) Volume = MeshVolume.CalculateVolumeOfMesh(VolumeMesh, VolumeObject.transform.lossyScale);
        }

        float GetDepth()
        {
            if(waterModel == null) waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
            float waterSurfaceLevel = waterModel.GetWaterLevelAt(transform.position);
            float depth = waterSurfaceLevel - transform.position.y;
            return depth;
        }

        // Volume * Density * Gravity
        private void FixedUpdate()
        {
            var forcePointPosition = transform.position;
            if (AddGravity)
            {
                Vector3 gravityForce = Mass * Physics.gravity / allForcePoints.Length;
                body.AddForceAtPosition(gravityForce, forcePointPosition, ForceMode.Force);
                AppliedGravityForce = gravityForce.y;
                if(DrawForces) Debug.DrawLine(forcePointPosition, forcePointPosition+gravityForce, Color.red, 0.1f);
            }


            var depth = GetDepth();
            IsUnderwater = depth > 0f;
            if (depth > 0f)
            {
                //Underwater
                //Apply buoyancy
                float displacementMultiplier = Mathf.Clamp01(depth / DepthBeforeSubmerged);

                AppliedBuoyancyForce = Volume * WaterDensity * Math.Abs(Physics.gravity.y) * displacementMultiplier / allForcePoints.Length;
                AppliedBuoyancyForce = Mathf.Min(MaxBuoyancyForce, AppliedBuoyancyForce);
            
                var buoyancyForce =  new Vector3(0, AppliedBuoyancyForce, 0);
                body.AddForceAtPosition(
                    buoyancyForce,
                    forcePointPosition,
                    ForceMode.Force);
                if(DrawForces) Debug.DrawLine(forcePointPosition, forcePointPosition+buoyancyForce, Color.blue, 0.1f);
            }

            // change the drag of the body to underwater if any is submerged. This is a ad-hoc way to 
            // simulate the sticktion water usually applies to objects
            // also, some objects might need to be useful under AND over water (like ropes...)
            // and their drag really should reflect where they are moment to moment
            // yes, all of the points will do the same thing. but this makes it so we dont need
            // a central forcepoint controller or sth
            var anySubmerged = allForcePoints.Select(p => p.IsUnderwater).Aggregate(false, (s, v) => s || v);
            body.drag = anySubmerged? UnderwaterDrag:AirDrag;
            body.angularDrag = anySubmerged? UnderwaterAngularDrag:AirAngularDrag;

            // And lastly, whatever custom force was set.
            if(ApplyCustomForce)
            {
                body.AddForceAtPosition(
                    CustomForce,
                    forcePointPosition,
                    ForceMode.Force);
            }
        }

        void OnDrawGizmos()
        {
            if(GizmoSize <= 0) return;
            
            Gizmos.color = new Color(0.5f, 0.5f, 0f, 0.5f);
            var d = GetDepth();
            if (d > 0f)
                Gizmos.color = new Color(0f, 0.5f, 0.5f, 0.5f);
            if (d > DepthBeforeSubmerged)
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);

            Gizmos.DrawSphere(transform.position, GizmoSize);
        }
    }
}