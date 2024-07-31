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

        public float rollRpms = 0.1f;
        public float moveRpms = 50f;

        void Awake()
        {
            frontleftProp = frontleftPropGo.GetComponent<Propeller>();
            frontrightProp = frontrightPropGo.GetComponent<Propeller>();
            backrightProp = backrightPropGo.GetComponent<Propeller>();
            backleftProp = backleftPropGo.GetComponent<Propeller>();
        }

        void Update()
        {

            if (Input.GetKeyDown("i"))
            {
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM * 1.05);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM * 1.05);
            }

            if (Input.GetKeyDown("k"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM * 1.05);
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM * 1.05);
            }

            if (Input.GetKeyDown("j"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM *1.05);
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*1.05);
            }

            if (Input.GetKeyDown("l"))
            {
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM *1.05);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*1.05);
            }

            if (Input.GetKeyDown("o"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM*1.05);
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM*1.05);
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM*1.05);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM*1.05);
            }

            if (Input.GetKeyDown("p"))
            {
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM/1.05);
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM/1.05);
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM/1.05);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM/1.05);
            }
            
            if (Input.GetKeyUp("i") || Input.GetKeyUp("j") || Input.GetKeyUp("k") || Input.GetKeyUp("l") || Input.GetKeyUp("o") || Input.GetKeyUp("p"))
            {
                backrightProp.SetRpm(backrightProp.DefaultHoverRPM);
                backleftProp.SetRpm(backleftProp.DefaultHoverRPM);
                frontrightProp.SetRpm(frontrightProp.DefaultHoverRPM);
                frontleftProp.SetRpm(frontleftProp.DefaultHoverRPM); 
            }

        }
    }
}