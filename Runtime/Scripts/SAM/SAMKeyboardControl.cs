using System.Collections.Generic;
using Force;
using UnityEngine; 

using Hinge = VehicleComponents.Actuators.Hinge;
using Propeller = VehicleComponents.Actuators.Propeller;
using VBS = VehicleComponents.Actuators.VBS;
using HingeCommand = VehicleComponents.ROS.Subscribers.HingeCommand;
using PropellerCommand = VehicleComponents.ROS.Subscribers.PropellerCommand;
using PercentageCommand = VehicleComponents.ROS.Subscribers.PercentageCommand;

namespace DefaultNamespace
{
    [RequireComponent(typeof(ISAMControl))]
    public class SAMKeyboardControl : MonoBehaviour
    {
        private ISAMControl _samControl;

        [Tooltip("Set to true to give up control to ROS commands")]
        public bool letROSTakeTheWheel = true;

        public GameObject yawHingeGo;
        public GameObject pitchHingeGo;
        public GameObject frontPropGo;
        public GameObject backPropGo;
        public GameObject vbsGo;

        Hinge yaw, pitch;
        HingeCommand yawCmd, pitchCmd;
        Propeller frontProp, backProp;
        PropellerCommand frontPropCmd, backPropCmd;
        VBS vbs;
        PercentageCommand vbsCmd;




        public float rollRpms = 0.1f;
        public float moveRpms = 800f;

        [Header("Mouse control")] [Tooltip("Use these when you dont want to press down for 10 minutes")]
        public bool useBothRpms = false;

        public float bothRpms = 0f;


        private void Awake()
        {
            _samControl = GetComponentInParent<ISAMControl>();
            yaw = yawHingeGo.GetComponent<Hinge>();
            yawCmd = yawHingeGo.GetComponent<HingeCommand>();
            pitch = pitchHingeGo.GetComponent<Hinge>();
            pitchCmd = pitchHingeGo.GetComponent<HingeCommand>();
            frontProp = frontPropGo.GetComponent<Propeller>();
            frontPropCmd = frontPropGo.GetComponent<PropellerCommand>();
            backProp = backPropGo.GetComponent<Propeller>();
            backPropCmd = backPropGo.GetComponent<PropellerCommand>();
            vbs = vbsGo.GetComponent<VBS>();
            vbsCmd = vbsGo.GetComponent<PercentageCommand>();
        }

        private void FixedUpdate()
        {
            yawCmd.enabled = letROSTakeTheWheel;
            pitchCmd.enabled = letROSTakeTheWheel;
            frontPropCmd.enabled = letROSTakeTheWheel;
            backPropCmd.enabled = letROSTakeTheWheel;
            vbsCmd.enabled = letROSTakeTheWheel;

            if (useBothRpms)
            {
                // _samControl.SetRpm(bothRpms, bothRpms);
                frontProp.SetRpm(bothRpms);
                backProp.SetRpm(bothRpms);
            }

            if (Input.GetKeyDown("down"))
            {
                // _samControl.SetRpm(-moveRpms, -moveRpms);
                frontProp.SetRpm(-moveRpms);
                backProp.SetRpm(-moveRpms);
            }

            if (Input.GetKeyDown("q"))
            {
                // _samControl.SetRpm(-rollRpms, rollRpms);
                frontProp.SetRpm(-rollRpms);
                backProp.SetRpm(rollRpms);
            }

            if (Input.GetKeyDown("e"))
            {
                // _samControl.SetRpm(rollRpms, -rollRpms);
                frontProp.SetRpm(rollRpms);
                backProp.SetRpm(-rollRpms);
            }

            if (Input.GetKeyDown("up"))
            {
                // _samControl.SetRpm(moveRpms, moveRpms);
                frontProp.SetRpm(moveRpms);
                backProp.SetRpm(moveRpms);
            }

            if (Input.GetKeyUp("up") || Input.GetKeyUp("down") || Input.GetKeyUp("q") || Input.GetKeyUp("e"))
            {
                // _samControl.SetRpm(0, 0);
                frontProp.SetRpm(0);
                backProp.SetRpm(0);
            }

            if (Input.GetKeyDown("a"))
            {
                // _samControl.SetRudderAngle(-1);
                yaw.SetAngle(-1);
            }

            if (Input.GetKeyDown("d"))
            {
                // _samControl.SetRudderAngle(1);
                yaw.SetAngle(1);
            }

            if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
            {
                // _samControl.SetRudderAngle(0);
                yaw.SetAngle(0);
            }

            if (Input.GetKeyDown("w"))
            {
                // _samControl.SetElevatorAngle(-1);
                pitch.SetAngle(-1);
            }

            if (Input.GetKeyDown("s"))
            {
                // _samControl.SetElevatorAngle(1);
                pitch.SetAngle(1);
            }

            if (Input.GetKeyUp("w") || Input.GetKeyUp("s"))
            {
                // _samControl.SetElevatorAngle(0);
                pitch.SetAngle(0);
            }

            if (Input.GetKeyDown("f"))
            {
                vbs.SetPercentage(0.5f);
                // _samControl.SetWaterPump(0.5f);
            }

            if (Input.GetKeyDown("c"))
            {
                vbs.SetPercentage(0f);
                // _samControl.SetWaterPump(0);
            }

            if (Input.GetKeyDown("space"))
            {
                vbs.SetPercentage(1f);
                // _samControl.SetWaterPump(1);
            }
        }
    }
}