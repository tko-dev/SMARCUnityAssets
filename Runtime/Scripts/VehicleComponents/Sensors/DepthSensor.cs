using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // For file operations
using Utils = DefaultNamespace.Utils;
using DefaultNamespace.Water;

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
        public ArticulationBody droneBody;
        public float numDatapoints = 10000f;
        public float dataStep = 100f;
        public GameObject gameobject;


        void Start()
        {
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
            filePath = Application.dataPath + "/depth_data.csv";
            
            droneBody = gameObject.GetComponent<ArticulationBody>();     
        }

        public override bool UpdateSensor(double deltaTime)
        {

            if (storeDepth)
            {
                System.Random RNG = new System.Random();
                float currentTime = Time.time;
                float i = currentTime/(float)deltaTime;
                if( i < numDatapoints)
                {
                    if (i % dataStep == 0)
                    {
                        // Get a random direction between 1 and 4
                        int direction = RNG.Next(1, 5);

                        // Move the object based on the random direction
                        switch (direction)
                        {
                            case 1:
                                transform.position += new Vector3(0.5f, 0, 0);  // Increase x by 0.5
                                break;
                            case 2:
                                transform.position += new Vector3(-0.5f, 0, 0);  // Decrease x by 0.5
                                break;
                            case 3:
                                transform.position += new Vector3(0, 0, 0.5f);  // Increase z by 0.5
                                break;
                            case 4:
                                transform.position += new Vector3(0, 0, -0.5f);  // Decrease z by 0.5
                                break;
                            default:
                                Debug.LogWarning("Unexpected direction value: " + direction);
                                break;
                        }
                    }
                }
                else{storeDepth = false;}
            
                
                var waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
                float depth = transform.position.y - waterSurfaceLevel;

                // Store x, y, z, time, and depth in CSV
                AppendDataToCSV(transform.position.x, transform.position.y, transform.position.z, currentTime, depth);
                
                return true; // Successfully stored data
            }

            return false; // Depth data was not stored
        }

        private void AppendDataToCSV(float x, float y, float z, float time, float depth)
        {
            if (!headerWritten)
            {
                File.AppendAllText(filePath, "X,Y,Z,Time,Depth\n");
                headerWritten = true;
            }

            string newLine = $"{x},{y},{z},{time},{depth}\n";
            File.AppendAllText(filePath, newLine);
        }
    }
}