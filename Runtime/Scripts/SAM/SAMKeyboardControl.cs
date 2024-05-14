using System.Collections.Generic;
using Force;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(ISAMControl))]
    public class SAMKeyboardControl : MonoBehaviour
    {
        private ISAMControl _samControl;

        public float rollRpms = 0.1f;
        public float moveRpms = 800f;

        [Header("Mouse control")] [Tooltip("Use these when you dont want to press down for 10 minutes")]
        public bool useBothRpms = false;

        public float bothRpms = 0f;


        private void Awake()
        {
            _samControl = GetComponentInParent<ISAMControl>();
        }

        private void Update()
        {
            if (useBothRpms)
            {
                _samControl.SetRpm(bothRpms, bothRpms);
            }

            if (Input.GetKeyDown("down"))
            {
                _samControl.SetRpm(-moveRpms, -moveRpms);
            }

            if (Input.GetKeyDown("q"))
            {
                _samControl.SetRpm(-rollRpms, rollRpms);
            }

            if (Input.GetKeyDown("e"))
            {
                _samControl.SetRpm(rollRpms, -rollRpms);
            }

            if (Input.GetKeyDown("up"))
            {
                _samControl.SetRpm(moveRpms, moveRpms);
            }

            if (Input.GetKeyUp("up") || Input.GetKeyUp("down") || Input.GetKeyUp("q") || Input.GetKeyUp("e"))
            {
                _samControl.SetRpm(0, 0);
            }

            if (Input.GetKeyDown("a"))
            {
                _samControl.SetRudderAngle(-1);
            }

            if (Input.GetKeyDown("d"))
            {
                _samControl.SetRudderAngle(1);
            }

            if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
            {
                _samControl.SetRudderAngle(0);
            }

            if (Input.GetKeyDown("w"))
            {
                _samControl.SetElevatorAngle(-1);
            }

            if (Input.GetKeyDown("s"))
            {
                _samControl.SetElevatorAngle(1);
            }

            if (Input.GetKeyUp("w") || Input.GetKeyUp("s"))
            {
                _samControl.SetElevatorAngle(0);
            }

            if (Input.GetKeyDown("f"))
            {
                _samControl.SetWaterPump(0.5f);
            }

            if (Input.GetKeyDown("c"))
            {
                _samControl.SetWaterPump(0);
            }

            if (Input.GetKeyDown("space"))
            {
                _samControl.SetWaterPump(1);
            }
        }
    }
}