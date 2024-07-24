using Importer;
using UnityEditor;
using UnityEngine;

using Rope;

namespace Editor.Scripts
{
    [CustomEditor(typeof(RopeLink))]
    public class RopeGeneratorEditor : UnityEditor.Editor
    {
        RopeLink ropeLink;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ropeLink = (RopeLink) target;

            if(GUILayout.Button("Generate Rope"))
            {
                ropeLink.DestroyRope();
                ropeLink.SpawnRope();
            }
        }
    }
}