using System;
using DefaultNamespace;
using DefaultNamespace.Water;
using UnityEngine;

// This is a very simple example of how we could compute a buoyancy force at variable points along the body.
// Its not really accurate per se.
// [RequireComponent(typeof(Rigidbody))]
// [RequireComponent(typeof(IForceModel))]
public class ForcePoint : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private int _pointCount;

    private WaterQueryModel _waterModel;

    public float depthBeforeSubmerged = 1.5f;
    public float displacementAmount = 1f;

    public GameObject motionModel;
    public bool addGravity = false;
    public void Awake()
    {
        if (motionModel == null) Debug.Log("ForcePoints require a motionModel object with a rigidbody to function!");
        _rigidbody = motionModel.GetComponent<Rigidbody>();
        _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        _pointCount = transform.parent.gameObject.GetComponentsInChildren<ForcePoint>().Length;
        addGravity = !_rigidbody.useGravity;
    }

    private void FixedUpdate()
    {
        var forcePointPosition = transform.position;
        if (addGravity)
        {
            _rigidbody.AddForceAtPosition(_rigidbody.mass * Physics.gravity / _pointCount, forcePointPosition, ForceMode.Force);
        }


        float waterSurfaceLevel = _waterModel.GetWaterLevelAt(forcePointPosition);
        if (forcePointPosition.y < waterSurfaceLevel)
        {
            float displacementMultiplier = Mathf.Clamp01((waterSurfaceLevel - forcePointPosition.y) / depthBeforeSubmerged) * displacementAmount;

            _rigidbody.AddForceAtPosition(
                _rigidbody.mass * new Vector3(0, Math.Abs(Physics.gravity.y) * displacementMultiplier / _pointCount, 0),
                forcePointPosition,
                ForceMode.Force);
        }

    }
}
