using System.Collections.Generic;
using Force;
using UnityEngine;
using Propeller = VehicleComponents.Actuators.Propeller;

namespace GameUI
{
    public class NewDroneKeyboardControl : KeyboardController
    {

        public GameObject frontrightPropGo;
        public GameObject backrightPropGo;
        public GameObject frontleftPropGo;
        public GameObject backleftPropGo;

        Propeller frontrightProp, frontleftProp, backrightProp, backleftProp;

        bool mouseDown = false;


        public float rollRpms = 0.1f;
        public float moveRpms = 50f;

        // [Header("Mouse control")] [Tooltip("Use these when you dont want to press down for 10 minutes")]
        // public bool useBothRpms = false;

        //public float bothRpms = 0f;

        void Awake()
        {
            
            frontrightProp = frontrightPropGo.GetComponent<Propeller>();
            backrightProp = backrightPropGo.GetComponent<Propeller>();
            frontleftProp = frontleftPropGo.GetComponent<Propeller>();
            backleftProp = backleftPropGo.GetComponent<Propeller>();
        }

        void Update()
        {

            // if (useBothRpms)
            // {
            //     frontProp.SetRpm(bothRpms);
            //     backProp.SetRpm(bothRpms);
            // }

            // Ignore inputs while the right mouse
            // button is held down. Since this is used for camera controls.
            // There is no "while button down" check, so we DIY.
            // if(Input.GetMouseButtonDown(1)) mouseDown = true;
            // if(Input.GetMouseButtonUp(1)) mouseDown = false;
            // if(mouseDown) return;

            if (Input.GetKeyDown("i"))
            {
                backrightProp.SetRpm(backrightProp.rpm * 1.05);
                backleftProp.SetRpm(backleftProp.rpm * 1.05);
            }

            if (Input.GetKeyDown("k"))
            {
                backrightProp.SetRpm(backrightProp.rpm-moveRpms);
                backleftProp.SetRpm(backleftProp.rpm-moveRpms);
            }

            if (Input.GetKeyDown("j"))
            {
                frontrightProp.SetRpm(frontrightProp.rpm-rollRpms);
                backrightProp.SetRpm(backrightProp.rpm-rollRpms);
            }

            if (Input.GetKeyDown("l"))
            {
                frontrightProp.SetRpm(frontrightProp.rpm+rollRpms);
                backrightProp.SetRpm(backrightProp.rpm+rollRpms);
            }

            if (Input.GetKeyDown("o"))
            {
                frontrightProp.SetRpm(+moveRpms);
                backrightProp.SetRpm(+moveRpms);
                frontleftProp.SetRpm(+moveRpms);
                backleftProp.SetRpm(+moveRpms);
            }

            if (Input.GetKeyDown("p"))
            {
                frontrightProp.SetRpm(-moveRpms);
                backrightProp.SetRpm(-moveRpms);
                frontleftProp.SetRpm(-moveRpms);
                backleftProp.SetRpm(-moveRpms);
            }

            // if (Input.GetKeyUp("up") || Input.GetKeyUp("down") || Input.GetKeyUp("q") || Input.GetKeyUp("e"))
            // {
            //     frontProp.SetRpm(0);
            //     backProp.SetRpm(0);
            // }
        }
    }
}