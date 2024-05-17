using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace VehicleComponents.Actuators
{
    public class VBS : LinkAttachment, IPercentageActuator
    {
        [Header("VBS")]
        [Header("VBS")]
        [Range(0, 100)] public float percentage = 50f;
        [Range(0, 100)] public float resetValue = 50f;

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
            percentage = Mathf.Clamp(newValue, 0, 100);
        }

        public float GetResetValue()
        {
            return resetValue;
        }

        public float GetCurrentValue()
        {
            return percentage;
        }

        public void FixedUpdate()
        {
            articulationBody.mass = 0.00001f + _initialMass * percentage / 100;
            articulationBody.SetDriveTarget(ArticulationDriveAxis.X, Mathf.Lerp(_maximumPos, _minimumPos, percentage / 100));
        }
    }
}