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
        GameObject[] robots;
        GameObject[] baseLinks;
        GameObject[] robotMarkers;
        Canvas canvas;
        CameraManager cameraManager;

        public GameObject RobotMarker;

        // https://discussions.unity.com/t/how-to-convert-from-world-space-to-canvas-space/117981/18
        private Vector2 WorldToCanvasPosition(Canvas canvas, Camera worldCamera, Vector3 worldPosition) 
        {
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 viewportPoint = worldCamera.WorldToViewportPoint(worldPosition);

            var rootCanvasTransform = (canvas.isRootCanvas ? canvas.transform : canvas.rootCanvas.transform) as RectTransform;
            var rootCanvasSize = rootCanvasTransform!.rect.size;
            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            var rootCoord = (viewportPoint - rootCanvasTransform.pivot) * rootCanvasSize;
            if (canvas.isRootCanvas)
                return rootCoord;

            var rootToWorldPos = rootCanvasTransform.TransformPoint(rootCoord);
            return canvas.transform.InverseTransformPoint(rootToWorldPos);
        }

        void Start()
        {
            robots = GameObject.FindGameObjectsWithTag("robot");
            robotMarkers = new GameObject[robots.Length];
            baseLinks = new GameObject[robots.Length];
            for(int i=0; i<robots.Length; i++)
            {
                var robot = robots[i];

                // TODO Possibly instantiate a prefab with a script in it that allows
                // easy setting of texts, values etc. 
                var textGO = Instantiate(RobotMarker);
                textGO.transform.SetParent(transform);
                textGO.name = $"Text_{robot.transform.root.name}";
                var text = textGO.GetComponent<TMP_Text>();
                text.SetText(robot.transform.root.name);

                robotMarkers[i] = textGO;
                baseLinks[i] = Utils.FindDeepChildWithName(robot, "base_link");
            }
            canvas = GetComponent<Canvas>();
            cameraManager = FindObjectsByType<CameraManager>(FindObjectsSortMode.None)[0];
        }

        void Update()
        {
            if(canvas == null || cameraManager == null) return;
            for(int i=0; i<robots.Length; i++)
            {
                Vector2 robotPosInCanvas = WorldToCanvasPosition(canvas, cameraManager.currentCam, baseLinks[i].transform.position);
                robotMarkers[i].GetComponent<RectTransform>().anchoredPosition = robotPosInCanvas;
            }
        }

    }
}


