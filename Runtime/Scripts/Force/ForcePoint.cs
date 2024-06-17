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
        
        public float depthBeforeSubmerged = 0.03f;
        public bool addGravity = false;

        public GameObject volumeObject;
        public Mesh volumeMesh;
        public bool automaticCenterOfGravity = false;
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
                _body.AddForceAtPosition(_body.mass * Physics.gravity / _pointCount, forcePointPosition, ForceMode.Force);
            }


            float waterSurfaceLevel = _waterModel.GetWaterLevelAt(forcePointPosition);
            if (forcePointPosition.y < waterSurfaceLevel)
            {
                //Underwater
                //Apply buoyancy
                float displacementMultiplier = Mathf.Clamp01((waterSurfaceLevel - forcePointPosition.y) / depthBeforeSubmerged);

                _body.AddForceAtPosition(
                    volume * density * new Vector3(0, Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0),
                    forcePointPosition,
                    ForceMode.Force);
            }
        }
    }
}