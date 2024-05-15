using UnityEngine;

namespace Force
{
    public class SAMUnityNormalizationLayer : ISAMControl
    {
        private readonly ISAMControl underlyingController;

        public SAMUnityNormalizationLayer(ISAMControl samControl)
        {
            underlyingController = samControl;
        }

        public void SetRpm1(double rpm)
        {
            underlyingController.SetRpm1(rpm * underlyingController.parameters.RPMMax);
        }

        public void SetRpm2(double rpm)
        {
            underlyingController.SetRpm2(rpm * underlyingController.parameters.RPMMax);
        }

        public void SetRpm(double rpm1, double rpm2)
        {
            underlyingController.SetRpm(
                rpm1 * underlyingController.parameters.RPMMax,
                rpm2 * underlyingController.parameters.RPMMax);
        }

        public void SetRudderAngle(float dr)
        {
            underlyingController.SetRudderAngle(dr * underlyingController.parameters.ThrusterAngleMax);
        }

        public void SetElevatorAngle(float de)
        {
            underlyingController.SetElevatorAngle(de * underlyingController.parameters.ThrusterAngleMax);
        }

        public void SetBatteryPack(double lcg)
        {
            underlyingController.SetBatteryPack(lcg);
        }

        public void SetWaterPump(float vbs)
        {
            underlyingController.SetWaterPump((vbs + 1) / 2);
        }

        public float d_rudder
        {
            get => underlyingController.d_rudder;
        }

        public float d_aileron
        {
            get => underlyingController.d_aileron;
        }

        public double rpm1
        {
            get => underlyingController.rpm1;
        }

        public double rpm2
        {
            get => underlyingController.rpm2;
        }

        public double lcg
        {
            get => underlyingController.lcg;
        }

        public double vbs
        {
            get => underlyingController.vbs;
        }

        public SAMParameters parameters
        {
            get => underlyingController.parameters;
            set => underlyingController.parameters = value;
        }
    }
}