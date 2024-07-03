using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils = DefaultNamespace.Utils;


namespace GameUI
{
    public class RobotOverlayManager : MonoBehaviour
    {
        Canvas canvas;
        CameraManager cameraManager;
        [Tooltip("Assign a prefab to be spawned on screen")]
        public GameObject OverlayPrefab;

        

        void Awake()
        {
            var robots = GameObject.FindGameObjectsWithTag("robot");
            canvas = GetComponent<Canvas>();
            cameraManager = FindObjectsByType<CameraManager>(FindObjectsSortMode.None)[0];
            for(int i=0; i<robots.Length; i++)
            {
                var robot = robots[i];

                var overlayGO = Instantiate(OverlayPrefab);
                var overlay = overlayGO.GetComponent<RobotOverlay>();
                overlay.Initialize(robot, canvas, cameraManager);
                overlayGO.transform.SetParent(transform);
            }
            
        }

    }
}


