using UnityEditor;
using UnityEngine;

using GeoRef;

namespace Editor.Scripts
{
    [CustomEditor(typeof(GeoReference))]
    public class GeoReferenceEditor : UnityEditor.Editor
    {
        GeoReference container;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            container = (GeoReference) target;

            if(GUILayout.Button("Place in World from Lat/Lon"))
            {
                container.Place();
            }
        }
    }
}