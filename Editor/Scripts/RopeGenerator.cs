using Importer;
using UnityEditor;
using UnityEngine;

using Rope;

namespace Editor.Scripts
{
    [CustomEditor(typeof(RopeContainer))]
    public class RopeGeneratorEditor : UnityEditor.Editor
    {
        RopeContainer container;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            container = (RopeContainer) target;

            if(GUILayout.Button("(Re)Generate Rope"))
            {
                container.DestroyRope();
                container.SpawnRope();
            }
        }
    }
}