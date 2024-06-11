using System.Collections.Generic;
using Force;
using UnityEngine; 

using Hinge = VehicleComponents.Actuators.Hinge;
using Propeller = VehicleComponents.Actuators.Propeller;
using VBS = VehicleComponents.Actuators.VBS;
using Prismatic = VehicleComponents.Actuators.Prismatic;

namespace GameUI
{
    [RequireComponent(typeof(ISAMControl))]
    public class SAMKeyboardControl : KeyboardController
    {
        private ISAMControl _samControl;

        public GameObject yawHingeGo;
        public GameObject pitchHingeGo;
        public GameObject frontPropGo;
        public GameObject backPropGo;
        public GameObject vbsGo;
        public GameObject lcgGo;

        Hinge yaw, pitch;
        Propeller frontProp, backProp;
        VBS vbs;
        Prismatic lcg;




        public float rollRpms = 0.1f;
        public float moveRpms = 800f;

        [Header("Mouse control")] [Tooltip("Use these when you dont want to press down for 10 minutes")]
        public bool useBothRpms = false;

        public float bothRpms = 0f;

        private void Awake()
        {
            _samControl = GetComponentInParent<ISAMControl>();
            yaw = yawHingeGo.GetComponent<Hinge>();
            pitch = pitchHingeGo.GetComponent<Hinge>();
            frontProp = frontPropGo.GetComponent<Propeller>();
            backProp = backPropGo.GetComponent<Propeller>();
            vbs = vbsGo.GetComponent<VBS>();
            lcg = lcgGo.GetComponent<Prismatic>();
        }

        private void FixedUpdate()
        {

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