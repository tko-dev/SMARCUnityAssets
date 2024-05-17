namespace VehicleComponents.Actuators
{
    public interface IPercentageActuator
    {
        public void SetPercentage(float newValue);
        public float GetResetValue();
        public float GetCurrentValue();
    }
}