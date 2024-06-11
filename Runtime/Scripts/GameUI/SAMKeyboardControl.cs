using System.Collections.Generic;
using Force;
using UnityEngine; 

using Hinge = VehicleComponents.Actuators.Hinge;
using Propeller = VehicleComponents.Actuators.Propeller;
using VBS = VehicleComponents.Actuators.VBS;
using Prismatic = VehicleComponents.Actuators.Prismatic;

using HingeCommand = VehicleComponents.ROS.Subscribers.HingeCommand;
using PropellerCommand = VehicleComponents.ROS.Subscribers.PropellerCommand;
using PercentageCommand = VehicleComponents.ROS.Subscribers.PercentageCommand;


namespace GameUI
{
    [RequireComponent(typeof(ISAMControl))]
    public class SAMKeyboardControl : MonoBehaviour, IKeyboardController
    {
        private ISAMControl _samControl;

        [Tooltip("Set to true to give up control to ROS commands")]
        public bool letROSTakeTheWheel = true;

        public GameObject yawHingeGo;
        public GameObject pitchHingeGo;
        public GameObject frontPropGo;
        public GameObject backPropGo;
        public GameObject vbsGo;
        public GameObject lcgGo;

        Hinge yaw, pitch;
        HingeCommand yawCmd, pitchCmd;
        Propeller frontProp, backProp;
        PropellerCommand frontPropCmd, backPropCmd;
        VBS vbs;
        Prismatic lcg;
        PercentageCommand vbsCmd, lcgCmd;




        public float rollRpms = 0.1f;
        public float moveRpms = 800f;

        [Header("Mouse control")] [Tooltip("Use these when you dont want to press down for 10 minutes")]
        public bool useBothRpms = false;

        public float bothRpms = 0f;

        public void LetROSTakeTheWheel(bool yes)
        {
            letROSTakeTheWheel = yes;
        }

        public bool GetLetROSTakeTheWheel()
        {
            return letROSTakeTheWheel;
        }

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
            lcg = lcgGo.GetComponent<Prismatic>();
            lcgCmd = lcgGo.GetComponent<PercentageCommand>();
        }

        private void FixedUpdate()
        {
            yawCmd.enabled = letROSTakeTheWheel;
            pitchCmd.enabled = letROSTakeTheWheel;
            frontPropCmd.enabled = letROSTakeTheWheel;
            backPropCmd.enabled = letROSTakeTheWheel;
            vbsCmd.enabled = letROSTakeTheWheel;
            lcgCmd.enabled = letROSTakeTheWheel;

            if (useBothRpms)
            {
                frontProp.SetRpm(bothRpms);
                backProp.SetRpm(bothRpms);
            }

            if (Input.GetKeyDown("down"))
            {
                frontProp.SetRpm(-moveRpms);
                backProp.SetRpm(-moveRpms);
            }

            if (Input.GetKeyDown("q"))
            {
                frontProp.SetRpm(-rollRpms);
                backProp.SetRpm(rollRpms);
            }

            if (Input.GetKeyDown("e"))
            {
                frontProp.SetRpm(rollRpms);
                backProp.SetRpm(-rollRpms);
            }

            if (Input.GetKeyDown("up"))
            {
                frontProp.SetRpm(moveRpms);
                backProp.SetRpm(moveRpms);
            }

            if (Input.GetKeyUp("up") || Input.GetKeyUp("down") || Input.GetKeyUp("q") || Input.GetKeyUp("e"))
            {
                frontProp.SetRpm(0);
                backProp.SetRpm(0);
            }

            if (Input.GetKeyDown("a"))
            {
                yaw.SetAngle(-1);
            }

            if (Input.GetKeyDown("d"))
            {
                yaw.SetAngle(1);
            }

            if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
            {
                yaw.SetAngle(0);
            }

            if (Input.GetKeyDown("w"))
            {
                pitch.SetAngle(-1);
            }

            if (Input.GetKeyDown("s"))
            {
                pitch.SetAngle(1);
            }

            if (Input.GetKeyUp("w") || Input.GetKeyUp("s"))
            {
                pitch.SetAngle(0);
            }

            if (Input.GetKeyDown("r"))
            {
                vbs.SetPercentage(0f);
            }

            if (Input.GetKeyDown("f"))
            {
                vbs.SetPercentage(50f);
            }

            if (Input.GetKeyDown("c"))
            {
                vbs.SetPercentage(100f);
            }

            if (Input.GetKeyDown("t"))
            {
                lcg.SetPercentage(0f);
            }

            if (Input.GetKeyDown("g"))
            {
                lcg.SetPercentage(50f);
            }

            if (Input.GetKeyDown("v"))
            {
                lcg.SetPercentage(100f);
            }

        }
    }
}