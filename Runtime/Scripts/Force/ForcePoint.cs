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
    public class ForcePoint : MonoBehaviour
    {
        public ArticulationBody _body;

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
        public float density = 997; // kg/m3

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

                Vector3 buoyancyForce = volume * density * new Vector3(0, Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0);
                _body.AddForceAtPosition(
                    buoyancyForce,
                    forcePointPosition,
                    ForceMode.Force);
                if(drawForces) Debug.DrawLine(forcePointPosition, forcePointPosition+buoyancyForce, Color.blue, 0.1f);
            }
        }
    }
}