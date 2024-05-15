using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

// Sam's force model
using Force;

namespace VehicleComponents.Sensors
{
    public class SAMActuators: Sensor
    {
        [Header("SAM Actuators")]
        public double vbs = -1;
        public double lcg = -1;
        public double rpm1 = -1;
        public double rpm2 = -1;

        ISAMControl model;

        void Start()
        {
            var motionModel_go = Utils.FindDeepChildWithTag(transform.root.gameObject, "motion_model");
            if(motionModel_go == null)
            {
                Debug.Log("SAMActuators could not find a motion_model tagged object under root!");
                return;
            }
            model = motionModel_go.GetComponent<ISAMControl>();
        }

        public override bool UpdateSensor(double deltaTime)
        {
            if(model == null) return false;
            vbs = model.vbs;
            lcg = model.lcg;
            rpm1 = model.rpm1;
            rpm2 = model.rpm2;
            return true;
        }
    }
}