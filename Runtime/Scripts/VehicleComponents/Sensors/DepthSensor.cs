// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO; // For file operations
// using Utils = DefaultNamespace.Utils;
// using DefaultNamespace.Water;

<<<<<<< HEAD
namespace VehicleComponents.Sensors
{
    public class DepthSensor : Sensor
    {
        [Header("Depth-Pressure")]
        public float waveHeight;
        public bool storeDepth = false;
        public Vector3 dronePosition;
        
        private WaterQueryModel _waterModel;
        private string filePath;
        private bool headerWritten = false;
=======
// namespace VehicleComponents.Sensors
// {
//     public class DepthSensor : Sensor
//     {
//         [Header("Depth-Pressure")]
//         public float waveHeight;
//         public bool storeDepth = false;
//         public Vector3 dronePosition;
//         public Vector3 pressure;
//         private WaterQueryModel _waterModel;
//         private string filePath;
//         private bool headerWritten = false;
>>>>>>> origin/tracking_control

//         void Start()
//         {
//             _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
//             filePath = Application.dataPath + "/depth_data.csv";
//         }

//         public override bool UpdateSensor(double deltaTime)
//         {
//             if (storeDepth)
//             {
//                 float currentTime = Time.time;
//                 var waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
//                 float depth = transform.position.y - waterSurfaceLevel;

<<<<<<< HEAD
                // Store x, y, z, time, and depth in CSV
                AppendDataToCSV(transform.position.x, transform.position.y, transform.position.z, currentTime, depth);
                return true; // Successfully stored data
            }

            return false; // Depth data was not stored
        }
=======
//                 // Store x, y, z, time, and depth in CSV
//                 AppendDataToCSV(transform.position.x, transform.position.y, transform.position.z, currentTime, depth);
//             }
//         }
>>>>>>> origin/tracking_control

//         private void AppendDataToCSV(float x, float y, float z, float time, float depth)
//         {
//             if (!headerWritten)
//             {
//                 File.AppendAllText(filePath, "X,Y,Z,Time,Depth\n");
//                 headerWritten = true;
//             }

//             string newLine = $"{x},{y},{z},{time},{depth}\n";
//             File.AppendAllText(filePath, newLine);
//         }
//     }
// }