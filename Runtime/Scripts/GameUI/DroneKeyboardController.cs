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
        [Tooltip("Y key uses this for all RPMs. Useful when you really wanna lift something.")]
        public float LiftingRPMDifferenceRatio = 30f;
        public float LiftingRPMRampUpSpeed = 100f;
        float currentLiftingRPM;

        bool mouseDown = false;


        void Awake()
        {
            frontleftProp = frontleftPropGo.GetComponent<Propeller>();
            frontrightProp = frontrightPropGo.GetComponent<Propeller>();
            backrightProp = backrightPropGo.GetComponent<Propeller>();
            backleftProp = backleftPropGo.GetComponent<Propeller>();

            currentLiftingRPM = (float)backleftProp.DefaultHoverRPM;
        }

        void Update()
        {
            if(Input.GetMouseButtonDown(1)) mouseDown = true;
            if(Input.GetMouseButtonUp(1)) mouseDown = false;
            if(mouseDown) return;

            float half = RPMDifferenceRatio/2f;
            float less = RPMDifferenceRatio/8f;

            if (Input.GetKeyDown("i"))
            {
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM * (1+half));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM * (1+half));
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM * (1-half));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM * (1-half));
            }

            if (Input.GetKeyDown("k"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM * (1+half));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM * (1+half));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM * (1-half));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM * (1-half));
            }

            if (Input.GetKeyDown("j"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM *(1+half));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1+half));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM *(1-half));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1-half));
            }

            if (Input.GetKeyDown("l"))
            {
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM *(1+half));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1+half));
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM *(1-half));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1-half));
            }

            if (Input.GetKeyDown("u"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM*(1+less));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1+less));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM*(1+less));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1+less));
            }

            if (Input.GetKeyDown("y"))
            {
                if(currentLiftingRPM < frontrightProp.DefaultHoverRPM*(1+LiftingRPMDifferenceRatio/4))
                {
                    currentLiftingRPM += LiftingRPMRampUpSpeed;
                }
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM + currentLiftingRPM);
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM + currentLiftingRPM);
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM + currentLiftingRPM);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM + currentLiftingRPM);
            }
            

            if (Input.GetKeyDown("n"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM*(1-less));
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*(1-less));
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM*(1-less));
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*(1-less));
            }
            
            if (Input.GetKeyUp("i") || Input.GetKeyUp("j") || Input.GetKeyUp("k") || Input.GetKeyUp("l") || Input.GetKeyUp("u") || Input.GetKeyUp("n")
            || Input.GetKeyUp("y"))
            {
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM);
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM);
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM); 

                currentLiftingRPM = (float)backrightProp.DefaultHoverRPM;
            }

        }
    }
}