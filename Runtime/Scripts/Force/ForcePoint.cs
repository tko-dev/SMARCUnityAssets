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
        [FormerlySerializedAs("_rigidbody")] public ArticulationBody _body;
        private int _pointCount;

        private WaterQueryModel _waterModel;

        public float depthBeforeSubmerged = 1.5f;
        public float displacementAmount = 1f;

        public GameObject motionModel;
        public bool addGravity = false;

        public GameObject volumeObject;
        public Mesh volumeMesh;
        public bool automaticCenterOfGravity = false;
        public float volume;
        public float density = 997; // kg/m3

        // Keep a dict of current query objects
        // that we'll update as we enter and exit
        // their colliders. On update, we'll 
        // go through the dict and query the current
        // vector with the points position and apply it.
        Dictionary<int, IWaterCurrent> currents = new Dictionary<int, IWaterCurrent>();


        void OnTriggerEnter(Collider col)
        {
            if(col.gameObject.TryGetComponent<IWaterCurrent>(out IWaterCurrent current))
            {
                currents.Add(col.gameObject.GetInstanceID(), current);
            }
        }

        void OnTriggerExit(Collider col)
        {
            if(col.gameObject.TryGetComponent<IWaterCurrent>(out IWaterCurrent current))
            {
                currents.Remove(col.gameObject.GetInstanceID());
            }
        }


        public void Awake()
        {
            if (motionModel == null) Debug.Log("ForcePoints require a motionModel object with a rigidbody to function!");
            //_rigidbody = motionModel.GetComponent<Rigidbody>();
            //  if (_rigidbody == null) _rigidbody = motionModel.transform.parent.GetComponent<Rigidbody>();
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
                float displacementMultiplier = Mathf.Clamp01((waterSurfaceLevel - forcePointPosition.y) / depthBeforeSubmerged) * displacementAmount;

                _body.AddForceAtPosition(
                    volume * density * new Vector3(0, Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0),
                    forcePointPosition,
                    ForceMode.Force);

                // Also apply currents while underwater
                foreach(IWaterCurrent current in currents.Values)
                {
                    _body.AddForceAtPosition
                    (
                        current.GetCurrentAt(forcePointPosition),
                        forcePointPosition,
                        ForceMode.Force
                    );
                }

            }
        }
    }
}