using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI
{
    public class DropdownRobotSelector : MonoBehaviour
    {
        TMP_Dropdown dropdown;
        Toggle toggle_rosControl;
        
        GameObject selectedRobotRoot;
        
        void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            toggle_rosControl = GetComponentInChildren<Toggle>();
            
            // Get all the #robot tagged objects in the scene
            // then we'll use their root name in the list
            var robots = GameObject.FindGameObjectsWithTag("robot");
            foreach(var robot in robots)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(){text=robot.transform.root.name});
            }

            selectedRobotRoot = robots[0].transform.root.gameObject;
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            UpdateToggles();
        }

        void UpdateToggles()
        {
            IKeyboardController kbc = selectedRobotRoot.GetComponentInChildren<IKeyboardController>();
            if(kbc != null)
            {
                toggle_rosControl.isOn = kbc.GetLetROSTakeTheWheel();
            }
        }

        public void OnValueChanged(int ddIndex)
        {
            var selection = dropdown.options[ddIndex];
            selectedRobotRoot = GameObject.Find(selection.text);
            UpdateToggles();
        }

        public void OnToggleROSControl(bool t)
        {
            IKeyboardController kbc = selectedRobotRoot.GetComponentInChildren<IKeyboardController>();
            if(kbc != null)
            {
                kbc.LetROSTakeTheWheel(toggle_rosControl.isOn);
            }
        }

    
}
}

