using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Water;
using UnityEngine;
using UnityEngine.Serialization;

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
        public ArticulationBody ConnectedArticulationBody;
        public Rigidbody ConnectedRigidbody;

        MixedBody _body;

        public bool drawForces = false;
        
        public float depthBeforeSubmerged = 0.03f;
        bool addGravity = false;

        [Tooltip("GameObject that we will calculate the volume of. Set volume below to 0 to use.")]
        public GameObject volumeObject;
        [Tooltip("If the gameObject above has many meshes, set the one to use for volume calculations here.")]
        public Mesh volumeMesh;


        public bool automaticCenterOfGravity = false;

        [Tooltip("If not zero, will be used for buoyancy calculations. If zero, the volumeObject/Mesh above will be used to calculate.")]
        public float volume;
        public float WaterDensity = 997; // kg/m3

        private int _pointCount;
        private WaterQueryModel _waterModel;

        public void ApplyCurrent(Vector3 current)
        {
            float waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
            if (transform.position.y < waterSurfaceLevel)
            {
                _body.AddForceAtPosition
                        (
                            current / _pointCount,
                            transform.position,
                            ForceMode.Force
                        );
            }
        }


        public void Awake()
        {
            _body = new MixedBody();
            
            if(ConnectedArticulationBody == null && ConnectedRigidbody == null)
                Debug.LogWarning($"{gameObject.name} requires at least one of ConnectedArticulationBody or ConnectedRigidBody to be set!");
            
            if(ConnectedArticulationBody != null) _body.ab = ConnectedArticulationBody;
            if(ConnectedRigidbody!= null) _body.rb = ConnectedRigidbody;
            
            
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
            var forcePoints = transform.parent.gameObject.GetComponentsInChildren<ForcePoint>();
            if (automaticCenterOfGravity)
            {
                _body.automaticCenterOfMass = false;
                var centerOfMass = forcePoints.Select(point => point.transform.localPosition).Aggregate(new Vector3(0, 0, 0), (s, v) => s + v);
                _body.centerOfMass = centerOfMass / forcePoints.Length;
            }

            _pointCount = forcePoints.Length;
            addGravity = !_body.useGravity;
            if (volumeMesh == null && volumeObject != null) volumeMesh = volumeObject.GetComponent<MeshFilter>().mesh;
            if (volume == 0 && volumeMesh != null) volume = MeshVolume.CalculateVolumeOfMesh(volumeMesh, volumeObject.transform.lossyScale);
        }

        // Volume * Density * Gravity
        private void FixedUpdate()
        {
            var forcePointPosition = transform.position;
            if (addGravity)
            {
                Vector3 gravityForce = _body.mass * Physics.gravity / _pointCount;
                _body.AddForceAtPosition(gravityForce, forcePointPosition, ForceMode.Force);
                if(drawForces) Debug.DrawLine(forcePointPosition, forcePointPosition+gravityForce, Color.red, 0.1f);
            }


            float waterSurfaceLevel = _waterModel.GetWaterLevelAt(forcePointPosition);
            if (forcePointPosition.y < waterSurfaceLevel)
            {
                //Underwater
                //Apply buoyancy
                float displacementMultiplier = Mathf.Clamp01((waterSurfaceLevel - forcePointPosition.y) / depthBeforeSubmerged);

                float verticalBuoyancyForce = (volume * WaterDensity * Math.Abs(Physics.gravity.y)) * displacementMultiplier / _pointCount;
                // TODO reduce this force to only allow a max force that'd bring this object to the surface in one fixed update.
                
                Vector3 buoyancyForce =  new Vector3(0, verticalBuoyancyForce, 0);
                _body.AddForceAtPosition(
                    buoyancyForce,
                    forcePointPosition,
                    ForceMode.Force);
                if(drawForces) Debug.DrawLine(forcePointPosition, forcePointPosition+buoyancyForce, Color.blue, 0.1f);
            }
        }
    }
}