using UnityEngine;


namespace GeoRef
{
    public class GeoReference: MonoBehaviour
    {
        public double Lat, Lon;

        GlobalReferencePoint globalRef;

        void OnValidate()
        {
            var refpoints = FindObjectsByType<GlobalReferencePoint>(FindObjectsSortMode.None);
            if(refpoints.Length != 1)
            {
                Debug.LogWarning($"Found {refpoints.Length} GlobalReferencePoint in the scene, there should only be one!");
            }
        }

        public void Place()
        {
            globalRef = FindFirstObjectByType<GlobalReferencePoint>();
            var (x,z) = globalRef.GetUnityXZFromLatLon(Lat, Lon);
            transform.position = new Vector3(x, transform.position.y, z);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            float size = 400;
            var top = new Vector3(transform.position.x, size, transform.position.z);
            var bottom = new Vector3(transform.position.x, -size, transform.position.z);
            Gizmos.DrawCube(top, Vector3.one * 10);
            Gizmos.DrawCube(bottom, Vector3.one * 10);
            Gizmos.DrawLine(top, bottom);
        }


    }
}
