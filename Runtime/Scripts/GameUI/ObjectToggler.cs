using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace GameUI
{
    public class ObjectToggler : MonoBehaviour
    {
        public GameObject ToggledObject;

        public void OnToggle(bool t)
        {
            ToggledObject.SetActive(t);
        }
    }
}