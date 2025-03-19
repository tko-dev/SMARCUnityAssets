using UnityEngine;


namespace GeoRef
{
    public class GeoReferenceTransformer: MonoBehaviour
    {
        [Header("Unity Space")]
        public Transform UnitySW;
        public Transform UnityNE;
        public Transform UnityWaterLevel;
        

        [Header("Scale Only")]
        [Tooltip("The distance between the two Unity points above in the real world.")]
        public float RequiredDistanceBetweenUnityPoints = 0;

        [Header("Earth Space")]
        public GeoReference EarthSW;
        public GeoReference EarthNE;

        [Header("Target")]
        [Tooltip("The object to transform. If this is a Terrain object, the terrainData size will be adjusted.")]
        public Transform TargetObject;
        


        void OnValidate()
        {
            var refpoints = FindObjectsByType<GlobalReferencePoint>(FindObjectsSortMode.None);
            if(refpoints.Length != 1)
            {
                Debug.LogWarning($"Found {refpoints.Length} GlobalReferencePoint in the scene, there should only be one!");
            }

            if(UnitySW == null || UnityNE == null || EarthSW == null || EarthNE == null || TargetObject == null)
            {
                Debug.LogWarning("One or more of the required fields are not set!");
            }

            if(Vector3.Distance(UnitySW.position, UnityNE.position) == 0)
            {
                Debug.LogWarning("Unity points are at the same position!");
            }
        }


        public void PlaceWorldPoints()
        {
            EarthSW.Place();
            EarthNE.Place();
        }

        void ResetTransform()
        {
            TargetObject.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            TargetObject.localScale = Vector3.one;
        }



        void Scale(float dist)
        {
            // Calculate the scale factor based on the distances between Unity and World points
            float unityDistance = Vector3.Distance(UnitySW.position, UnityNE.position);
            float scaleFactor = dist / unityDistance;

            // Apply the scale to the target object
            TargetObject.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }


        public void ScaleFromDistance()
        {
            if(RequiredDistanceBetweenUnityPoints == 0)
            {
                Debug.LogWarning("RequiredDistanceBetweenUnityPoints is 0!");
                return;
            }
            Scale(RequiredDistanceBetweenUnityPoints);
        }

        public void TransformFromTwoPoints()
        {
            ResetTransform();
            PlaceWorldPoints();

            // Apply the scale of the earth points to the target object
            float worldDistance = Vector3.Distance(EarthSW.transform.position, EarthNE.transform.position);
            Scale(worldDistance);

            // Calculate the rotation needed to align Unity points with World points
            Vector3 unityDirection = (UnityNE.position - UnitySW.position).normalized;
            Vector3 worldDirection = (EarthNE.transform.position - EarthSW.transform.position).normalized;
            Quaternion rotation = Quaternion.FromToRotation(unityDirection, worldDirection);

            // Apply the rotation to the target object
            TargetObject.rotation = rotation * TargetObject.rotation;
            

            if(TargetObject.gameObject.TryGetComponent(out Terrain terrain))
            {
                // terrain objects have a locked rotation and scale
                // as in, even if you change, it wont have an effect ON THE TERRAIN
                // but any children are still affected by the scale and rotation
                // so we need to apply the scale and rotation to the terrainData
                var terrainHeight = terrain.terrainData.size.y;
                var terrainWidth = UnityNE.position.x - UnitySW.position.x;
                var terrainLength = UnityNE.position.z - UnitySW.position.z;
                terrain.terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
            }


            // Move the target object to the correct position
            // we can do this now, because the unitysw is a child of the target object, so the scale and rotation
            // is already applied to the unity points!
            // but so are the world points, so we need to re-set them where they belong
            PlaceWorldPoints();
            Vector3 offset = EarthSW.transform.position - UnitySW.position;
            // and finally, put the water level where it needs to be
            offset.y = -UnityWaterLevel.position.y;
            TargetObject.position += offset;
            // and again after. lol.
            PlaceWorldPoints();
        }
    }
}