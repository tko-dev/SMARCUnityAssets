using DefaultNamespace.Water;
using UnityEngine;
using RosMessageTypes.Sensor;

namespace DefaultNamespace
{
    public class GPS : Sensor<NavSatFixMsg>
    {
        [Header("GPS")]
        public double easting;
        public double northing;
        public double lat;
        public double lon;

        [Tooltip("Assign a Terrain object for the GPS to reference its location.")]
        public GameObject terrain;
        private TerrainOnGlobe tog;
        private WaterQueryModel _waterModel;



        void Start()
        {
            tog = terrain.GetComponent<TerrainOnGlobe>();
            if (tog == null) Debug.Log("Terrain script is missing~!");

            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
        }

        public override bool UpdateSensor(double deltaTime)
        {
            if (terrain == null) return false;

            float waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
            // It is! We can get a fix
            if (transform.position.y > waterSurfaceLevel)
            {
                (easting, northing, lat, lon) = tog.GetUTMLatLonOfObject(gameObject);
                ros_msg.status.status = NavSatStatusMsg.STATUS_FIX;
                ros_msg.latitude = lat;
                ros_msg.longitude = lon;
            }
            else
            {
                ros_msg.status.status = NavSatStatusMsg.STATUS_NO_FIX;
            }
            return true;
        }
    }
}
