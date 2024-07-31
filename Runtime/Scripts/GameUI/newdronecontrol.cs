using System.Collections.Generic;
using Force;
using UnityEngine;
using Propeller = VehicleComponents.Actuators.Propeller;

namespace GameUI
{
    public class NewDroneKeyboardControl : KeyboardController
    {

        public GameObject frontleftPropGo;
        public GameObject frontrightPropGo;
        public GameObject backrightPropGo;
        public GameObject backleftPropGo;

        Propeller frontrightProp, frontleftProp, backrightProp, backleftProp;

        bool mouseDown = false;

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
                backrightProp.SetRpm(backrightProp.defaulthoverrpm * 1.05);
                backleftProp.SetRpm(backleftProp.defaulthoverrpm * 1.05);
            }

            if (Input.GetKeyDown("k"))
            {
                frontrightProp.SetRpm(frontrightProp.defaulthoverrpm * 1.05);
                frontleftProp.SetRpm(frontleftProp.defaulthoverrpm * 1.05);
            }

            if (Input.GetKeyDown("j"))
            {
                frontrightProp.SetRpm(frontrightProp.defaulthoverrpm *1.05);
                backrightProp.SetRpm(backrightProp.defaulthoverrpm*1.05);
            }

            if (Input.GetKeyDown("l"))
            {
                frontleftProp.SetRpm(frontleftProp.defaulthoverrpm *1.05);
                backleftProp.SetRpm(backleftProp.defaulthoverrpm*1.05);
            }

            if (Input.GetKeyDown("o"))
            {
                frontrightProp.SetRpm(frontrightProp.defaulthoverrpm*1.05);
                backrightProp.SetRpm(backrightProp.defaulthoverrpm*1.05);
                frontleftProp.SetRpm(frontleftProp.defaulthoverrpm*1.05);
                backleftProp.SetRpm(backleftProp.defaulthoverrpm*1.05);
            }

            if (Input.GetKeyDown("p"))
            {
                frontrightProp.SetRpm(frontrightProp.defaulthoverrpm/1.05);
                backrightProp.SetRpm(backrightProp.defaulthoverrpm/1.05);
                frontleftProp.SetRpm(frontleftProp.defaulthoverrpm/1.05);
                backleftProp.SetRpm(backleftProp.defaulthoverrpm/1.05);
            }
            
            if (Input.GetKeyUp("i") || Input.GetKeyUp("j") || Input.GetKeyUp("k") || Input.GetKeyUp("l") || Input.GetKeyUp("o") || Input.GetKeyUp("p"))
            {
                backrightProp.SetRpm(backrightProp.defaulthoverrpm);
                backleftProp.SetRpm(backleftProp.defaulthoverrpm);
                frontrightProp.SetRpm(frontrightProp.defaulthoverrpm);
                frontleftProp.SetRpm(frontleftProp.defaulthoverrpm); 
            }

        }
    }
}