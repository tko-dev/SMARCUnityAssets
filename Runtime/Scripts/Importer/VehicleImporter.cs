using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Robotics.UrdfImporter;
using UnityEngine;

namespace Importer
{
    public class VehicleImporter : MonoBehaviour
    {
        public GameObject objectToSave;

        public void SaveJson(String fileName)
        {
            if (objectToSave != null && fileName.Length > 0)
            {
                var vehicleModel = VehicleModel.WriteModel("./sam_auv.urdf", objectToSave);

                WriteJsonToFile(JsonUtility.ToJson(vehicleModel, prettyPrint: true), fileName);
            }
        }

        public void LoadJson(String fileName)
        {
            var saveData = JsonUtility.FromJson<VehicleModel>(File.ReadAllText(fileName));
            if (saveData.urdfFilePath.Length > 0)
            {
                StartCoroutine(ReadRobotFromUrdf(saveData, fileName));
            }
        }

        private IEnumerator ReadRobotFromUrdf(VehicleModel saveData, String jsonFile)
        {
            var odomObject = new GameObject();
            odomObject.name = "odom";
            odomObject.transform.parent = transform;

            ImportSettings settings = new ImportSettings
            {
                chosenAxis = ImportSettings.axisType.yAxis,
                convexMethod = ImportSettings.convexDecomposer.vHACD
            };

            var directoryName = Path.GetDirectoryName(jsonFile);
            var strings = saveData.urdfFilePath.Split("/");
            var join = Path.Join(directoryName, strings[^1]);
            //TODO: Need a way to construct the path correctly
            IEnumerator<GameObject> createRobot = UrdfRobotExtensions.Create("Packages/com.smarc.assets/Runtime/URDF/sam_auv/sam_auv.urdf", settings);
            yield return createRobot;
            var loadedRobot = createRobot.Current;

            loadedRobot.transform.parent = odomObject.transform;

            saveData.articulationModels.ForEach(model => model.LoadOntoObject(loadedRobot));
            saveData.forcePoints.ForEach(model => model.LoadOntoObject(loadedRobot));
            saveData.colliders.ForEach(model => model.LoadOntoObject(loadedRobot));

            odomObject.transform.localPosition = Vector3.zero;
            loadedRobot.transform.localPosition = Vector3.zero;
            loadedRobot.transform.localRotation = Quaternion.Euler(Vector3.zero);
            odomObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }


        public static string ReadFileTextContent(string filename)
        {
            string textContent = "";

            if (filename != null && filename.Length > 0)
            {
                string path = Application.dataPath + "/Resources/Text/" + filename + ".json";
                try
                {
                    textContent = File.ReadAllText(path);
                }
                catch (Exception)
                {
                }

                if (textContent.Length == 0)
                {
                    path = Application.streamingAssetsPath + "/Text/" + filename + ".json";
                    textContent = File.ReadAllText(path);
                }
            }


            if (textContent.Length == 0)
            {
                throw new FileNotFoundException("No file found trying to load text from file (" + filename +
                                                ")... - please check the configuration");
            }

            return textContent;
        }

        public static void WriteJsonToFile(string jsonString, string filename)
        {
            if (filename != null && filename.Length > 0)
            {
                Debug.Log("Writing Asset to Path:" + filename);
                File.WriteAllText(filename, jsonString);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
        }
    }
}