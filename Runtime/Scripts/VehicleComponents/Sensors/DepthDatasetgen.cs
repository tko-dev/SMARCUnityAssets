// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO; // For file operations
// using Utils = DefaultNamespace.Utils;
// using DefaultNamespace.Water;

// namespace VehicleComponents.Sensors
// {
//     public class DepthSensor : Sensor
//     {
//         [Header("Depth-Pressure")]
//         public float depth;
//         public bool storeDepth = false;
//         public Vector3 dronePosition;

//         public WaterQueryModel _waterModel;
//         private string filePath;
//         private bool headerWritten = false;
//         private int iterationCount = -1;  // Counter for iterations
//         public ArticulationBody droneBody;
//         public float numDatapoints = 1000f;
//         public float dataStep = 100f;
//         public GameObject gameobject;
//         public int gridsize = 100;
//         public Vector3 firstPosition;

//         void Start()
//         {
//             _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
//             filePath = Application.dataPath + "/depth_data.csv";

//             // Find the 'base_link' articulation body
//             Transform current = transform;

//             while (current != null)
//             {
//                 ArticulationBody articulationBody = current.GetComponent<ArticulationBody>();
//                 if (articulationBody != null && current.name == "base_link")
//                 {
//                     droneBody = articulationBody;
//                     break;
//                 }
//                 current = current.parent; // Move to the parent in the hierarchy
//             }

//             if (droneBody == null)
//             {
//                 Debug.LogError("base_link ArticulationBody not found in the hierarchy!");
//             }

//             firstPosition = droneBody.transform.position;
//         }

//         public override bool UpdateSensor(double deltaTime)
//         {
//             if (storeDepth)
//             {
//                 iterationCount++;
//                 Debug.Log("Iteration: " + iterationCount);

//                 if (iterationCount % dataStep == 0 && iterationCount < numDatapoints)
//                 {
//                     System.Random RNG = new System.Random();

//                     // Get a random direction between 1 and 4
//                     int coord1 = RNG.Next(1, gridsize+1);
//                     int coord2 = RNG.Next(1, gridsize+1);

//                     // Calculate new position based on the random position on grid
//                     // Vector3 newPosition = droneBody.transform.position;

//                     Vector3 newPosition = firstPosition + new Vector3(0.5f*(float)coord1, 0, 0.5f*(float)coord2);

//                     // Use TeleportRoot to move the ArticulationBody
//                     droneBody.TeleportRoot(newPosition, droneBody.transform.rotation);
//                     dronePosition = newPosition;
                    
//                 }

//                 else if (iterationCount >= numDatapoints)
//                 {
//                     storeDepth = false;  // Stop collecting data after reaching the desired number of datapoints
//                 }
//                 // Update dronePosition to the new position
                
//                 float currentTime = Time.time;
//                 // Query the water surface level and calculate depth
//                 var waterSurfaceLevel = _waterModel.GetWaterLevelAt(dronePosition);
//                 Debug.Log(_waterModel.GetWaterLevelAt(dronePosition));
//                 depth = dronePosition.y - waterSurfaceLevel;

//                 // Store x, z, time, and depth in CSV
//                 AppendDataToCSV(dronePosition.x, dronePosition.z, currentTime, depth);

//                 return true; // Successfully stored data
//             }

//             return false; // Depth data was not stored
//         }

//         private void AppendDataToCSV(float x, float z, float time, float depth)
//         {
//             if (!headerWritten)
//             {
//                 File.AppendAllText(filePath, "X,Z,Time,Depth\n");
//                 headerWritten = true;
//             }

//             string newLine = $"{x},{z},{time},{depth}\n";
//             File.AppendAllText(filePath, newLine);
//         }
//     }
// }
