using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using RosMessageTypes.Sam; // ThrusterAngles, PercentStamped

// Sam's force model
using Force;

namespace VehicleComponents.ROS.Subscribers
{
    public class SAMThrustVector : ActuatorSubscriber<ThrusterAnglesMsg>
    {
        ISAMControl model;
        
        void Start()
        {
            var motionModel_go = Utils.FindDeepChildWithTag(transform.root.gameObject, "motion_model");
            if(motionModel_go == null)
            {
                Debug.Log("SAMThrustVector could not find a motion_model tagged object under root!");
                return;
            }
            model = motionModel_go.GetComponent<ISAMControl>();
        }

        public override void UpdateVehicle(bool reset)
        {
            if(reset)
            {
                model.SetElevatorAngle(0);
                model.SetRudderAngle(0);
                return;
            }
            model.SetElevatorAngle(ROSMsg.thruster_horizontal_radians);
            model.SetRudderAngle(ROSMsg.thruster_vertical_radians);
        }
    }
}

