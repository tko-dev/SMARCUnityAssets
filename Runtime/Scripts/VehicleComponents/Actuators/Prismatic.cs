using UnityEngine;

namespace VehicleComponents.Actuators
{
    public class Prismatic : LinkAttachment, IPercentageActuator
    {
        [Header("Position")]
        [Range(0, 100)] public float percentage = 50f;
        [Range(0, 100)] public float resetValue = 50f;
     
        private float _maximumPos;
        private float _minimumPos;
        
        public void Start()
        {
            var xDrive = parentArticulationBody.xDrive;
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
            return articulationBody.jointPosition[0];  //TODO: Fix feedback. Percentage.
        }
        
        public void FixedUpdate()
        {
            articulationBody.SetDriveTarget(ArticulationDriveAxis.X, Mathf.Lerp(_minimumPos, _maximumPos, percentage / 100));
        }

        
    }
}