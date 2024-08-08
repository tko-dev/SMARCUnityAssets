using System.Collections.Generic;
using Force;
using UnityEngine;
using Propeller = VehicleComponents.Actuators.Propeller;

namespace GameUI
{
    public class DroneKeyboardController : KeyboardController
    {

        public GameObject frontleftPropGo;
        public GameObject frontrightPropGo;
        public GameObject backrightPropGo;
        public GameObject backleftPropGo;

        Propeller frontrightProp, frontleftProp, backrightProp, backleftProp;

        [Tooltip("Difference in RPM between sides of the drone when moving around")]
        public float RPMDifferenceRatio = 0.25f;

        bool mouseDown = false;


        void Awake()
        {
            frontleftProp = frontleftPropGo.GetComponent<Propeller>();
            frontrightProp = frontrightPropGo.GetComponent<Propeller>();
            backrightProp = backrightPropGo.GetComponent<Propeller>();
            backleftProp = backleftPropGo.GetComponent<Propeller>();
        }

        void FixedUpdate()
        {
            if(Input.GetMouseButtonDown(1)) mouseDown = true;
            if(Input.GetMouseButtonUp(1)) mouseDown = false;
            if(mouseDown) return;

            if (Input.GetKeyDown("i"))
            {
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM * (1+RPMDifferenceRatio));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM * (1+RPMDifferenceRatio));
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM * (1-RPMDifferenceRatio));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM * (1-RPMDifferenceRatio));
            }

            if (Input.GetKeyDown("k"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM * (1+RPMDifferenceRatio));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM * (1+RPMDifferenceRatio));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM * (1-RPMDifferenceRatio));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM * (1-RPMDifferenceRatio));
            }

            if (Input.GetKeyDown("j"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM *(1+RPMDifferenceRatio));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1+RPMDifferenceRatio));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM *(1-RPMDifferenceRatio));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1-RPMDifferenceRatio));
            }

            if (Input.GetKeyDown("l"))
            {
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM *(1+RPMDifferenceRatio));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1+RPMDifferenceRatio));
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM *(1-RPMDifferenceRatio));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1-RPMDifferenceRatio));
            }

            if (Input.GetKeyDown("u"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM*(1+RPMDifferenceRatio));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1+RPMDifferenceRatio));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM*(1+RPMDifferenceRatio));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1+RPMDifferenceRatio));
            }

            if (Input.GetKeyDown("n"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM*(1-RPMDifferenceRatio));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1-RPMDifferenceRatio));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM*(1-RPMDifferenceRatio));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1-RPMDifferenceRatio));
            }
            
            if (Input.GetKeyUp("i") || Input.GetKeyUp("j") || Input.GetKeyUp("k") || Input.GetKeyUp("l") || Input.GetKeyUp("u") || Input.GetKeyUp("n"))
            {
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM);
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM);
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM); 
            }

        }
    }
}