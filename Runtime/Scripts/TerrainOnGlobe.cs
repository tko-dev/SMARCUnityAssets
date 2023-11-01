using UnityEngine;
using CoordinateSharp;

public class TerrainOnGlobe : MonoBehaviour
{
    // Asko should be around lat/lon: 58.823220, 17.635160 which should be UTM east:652146.44 north: 6523362.01 zone: 33v
    // The asko terrain is at 33V 
    public string band;
    public int zone;
    public double easting;
    public double northing;
    public double lon = 17.596178; // asko bottom left defaults
    public double lat = 58.811481;
    public bool originIsLatLon = true;
    public bool drawLineToReferencePoint = true;

    void Start()
    {
        if(originIsLatLon)
        {
            var latlon = new Coordinate(lat, lon);
            easting = latlon.UTM.Easting;
            northing = latlon.UTM.Northing;
            band = latlon.UTM.LatZone;  //str
            zone = latlon.UTM.LongZone; //int
        }
        else
        {
            (lat, lon) = GetLatLonFromUTM(easting, northing);
        }
    }

    public (double, double) GetLatLonFromUTM(double easting, double northing)
    {
            var utm = new UniversalTransverseMercator(band, zone, easting, northing);
            var latlon = UniversalTransverseMercator.ConvertUTMtoLatLong(utm);
            return (latlon.Latitude.ToDouble(), latlon.Longitude.ToDouble());
    }

    public (double easting, double northing, double lat, double lon) GetUTMLatLonOfObject(GameObject o)
    {
        var posDiff = o.transform.position - gameObject.transform.position;
        if(drawLineToReferencePoint) Debug.DrawLine(o.transform.position, gameObject.transform.position);
        var xDiff = posDiff.x;
        var zDiff = posDiff.z;
        // +z = north
        // +x = east
        var obj_easting = easting + xDiff;
        var obj_northing = northing + zDiff;
        (var obj_lat, var obj_lon) = GetLatLonFromUTM(obj_easting, obj_northing);
        return (obj_easting, obj_northing, obj_lat, obj_lon);
    }
}
