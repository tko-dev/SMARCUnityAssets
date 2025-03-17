using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public class WorldspaceGhost : MonoBehaviour
    {
        public Transform ModelTF;
        public float FarAwayDistance = 50;
        float distSq;

        GUIState guiState;


        void Awake()
        {
            distSq = FarAwayDistance * FarAwayDistance;
            guiState = FindFirstObjectByType<GUIState>();
        }

        void LateUpdate()
        {
            if(guiState.CurrentCam == null) return;
            var camDiff = transform.position - guiState.CurrentCam.transform.position;
            bool closeEnough = camDiff.sqrMagnitude < distSq;
            ModelTF.gameObject.SetActive(closeEnough);
        }
    }
}