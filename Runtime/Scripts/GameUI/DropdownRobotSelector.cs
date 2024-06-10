using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropdownRobotSelector : MonoBehaviour
    {
        TMP_Dropdown dropdown;
        // Start is called before the first frame update
        void Start()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            
            // Get all the #robot tagged objects in the scene
            // then we'll use their root name in the list
            var robots = GameObject.FindGameObjectsWithTag("robot");
            foreach(var robot in robots)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(){text=robot.transform.root.name});
            }
        }

        public void OnValueChanged(int ddIndex)
        {
            var selection = dropdown.options[ddIndex];
            GameObject selectedGO = GameObject.Find(selection.text);

        }

    
}
}

