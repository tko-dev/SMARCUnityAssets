using Importer;
using UnityEditor;
using UnityEngine;

using Rope;

namespace Editor.Scripts
{
    [CustomEditor(typeof(Winch))]
    public class WinchEditor : UnityEditor.Editor
    {
        Winch winch;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            winch = (Winch) target;

            if(GUILayout.Button("Setup Rope"))
            {
                winch.SetupEnds();
            }
        }
    }
}