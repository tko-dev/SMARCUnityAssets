using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace VehicleComponents.Actuators
{
    public class VBS : LinkAttachment
    {
        [Header("VBS")]
        [Range(0, 1)] public float percentage = 0.5f;

        private float _initialMass;
        private float _maximumPos;
        private float _minimumPos;

        public void Start()
        {
            var xDrive = parentArticulationBody.xDrive;

            _initialMass = parentArticulationBody.mass;
            _minimumPos = xDrive.upperLimit;
            _maximumPos = xDrive.lowerLimit;
        }

        public void SetPercentage(float newValue)
        {
            percentage = Mathf.Clamp01(newValue);
        }

        public void FixedUpdate()
        {
            articulationBody.mass = 0.00001f + _initialMass * percentage;
            articulationBody.SetDriveTarget(ArticulationDriveAxis.X, Mathf.Lerp(_maximumPos, _minimumPos, percentage));
        }
    }
}