using UnityEditor;
using UnityEngine;

using GeoRef;

namespace Editor.Scripts
{
    [CustomEditor(typeof(GeoReferenceTransformer))]
    public class GeoReferenceTransformerEditor : UnityEditor.Editor
    {
        GeoReferenceTransformer container;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            container = (GeoReferenceTransformer) target;

            if(GUILayout.Button("Scale from Distance"))
            {
                container.ScaleFromDistance();
            }

            if(GUILayout.Button("Place World Points"))
            {
                container.PlaceWorldPoints();
            }

            if(GUILayout.Button("Transform from Two Points"))
            {
                container.TransformFromTwoPoints();
            }

        }
    }
}