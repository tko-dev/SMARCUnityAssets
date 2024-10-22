using Importer;
using UnityEditor;
using UnityEngine;

using Rope;

namespace Editor.Scripts
{
    [CustomEditor(typeof(RopeSystemBase), true)]
    public class RopeSystemBaseEditor : UnityEditor.Editor
    {
        RopeSystemBase ropesys;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ropesys = (RopeSystemBase) target;

            if(GUILayout.Button("Setup"))
            {
                ropesys.Setup();
            }
        }
    }
}