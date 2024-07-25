using Importer;
using UnityEditor;
using UnityEngine;

using Rope;

namespace Editor.Scripts
{
    [CustomEditor(typeof(RopeLink))]
    public class RopeGeneratorEditor : UnityEditor.Editor
    {
        RopeLink rope;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            rope = (RopeLink) target;

            if(GUILayout.Button("(Re)Generate Rope"))
            {
                rope.DestroyRope();
                rope.SpawnRope();
            }
        }
    }
}