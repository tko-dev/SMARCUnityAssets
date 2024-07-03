using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils = DefaultNamespace.Utils;


namespace GameUI
{
    public class RobotOverlay : MonoBehaviour
    {
        public Vector2 NameOffset = new Vector2();
        GameObject robot;
        GameObject baseLink;
        Canvas canvas;
        CameraManager cameraManager;

        TMP_Text TextRobotName;
        RectTransform rectTf;



        string robotName;

        public void Initialize(GameObject robot, Canvas canvas, CameraManager cameraManager)
        {
            this.robot = robot;
            this.canvas = canvas;
            this.cameraManager = cameraManager;
        }

        void Start()
        {
            robotName = robot.transform.root.name;
            baseLink = Utils.FindDeepChildWithName(robot, "base_link");

            gameObject.name = $"Overlay_{robotName}";
            
            rectTf = GetComponent<RectTransform>();


            var panelGO = Utils.FindDeepChildWithName(gameObject, "OverlayPanel");
            var textRobotNameGO = Utils.FindDeepChildWithName(panelGO, "TextName");
            TextRobotName = textRobotNameGO.GetComponent<TMP_Text>();
            TextRobotName.SetText(robotName);

            var imageLineGO = Utils.FindDeepChildWithName(gameObject, "LineToRobot");
            

        }

        void Update()
        {
            if(canvas == null || cameraManager == null) return;
            Vector2 robotPosInCanvas = Utils.WorldToCanvasPosition(canvas,
                                                                   cameraManager.currentCam,
                                                                   baseLink.transform.position);
            rectTf.anchoredPosition = robotPosInCanvas + NameOffset;
        }
    }
}