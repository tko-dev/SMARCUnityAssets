// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;


// public class DatasetGenerator : MonoBehaviour
// {
//     public GameObject drone;  //drone baselink
//     public GameObject auv;    //auv baselink
//     public GameObject ocean = GameObject.Find("Ocean");
//     public Camera droneCamera;
//     public float hoverHeight = 10f;
//     public float rotationStep = 10f;  // Rotate in 30-degree increments
//     public float positionOffset = 0.5f;  // Offset for moving the drone
//     public float numberOffsets = 7f;
//     private Vector3 centerPosition = new Vector3();


//     private string datasetPath;

//     void Start()
//     {
//         // Set the dataset path where images will be stored
//         datasetPath = Application.dataPath + "/AUV_Dataset/";
//         if (!Directory.Exists(datasetPath))
//         {
//             Directory.CreateDirectory(datasetPath);
//         }        

//         StartCoroutine(CaptureDataset());
//     }

//     IEnumerator CaptureDataset()
//     {
//         int imageIndex = 0;

//         // Initial drone position above the AUV
//         centerPosition = auv.transform.position + new Vector3(0, hoverHeight, 0);
//         drone.transform.position = centerPosition;

//         // Capture images at different rotations and offsets
//         for (float yRotation = 0; yRotation < 360; yRotation += rotationStep)
//         {
//             for (float centerOffset = 0; centerOffset < numberOffsets; centerOffset++)
//             { 
//                 // CaptureImage(imageIndex++);
//                 //yield return null;

//                 // Introduce position offsets
//                 Vector3 offset = new Vector3(positionOffset, 0, 0);
//                 offset = offset*(centerOffset+1);
//                 drone.transform.position = centerPosition + offset;
//                 drone.transform.rotation = Quaternion.Euler(0, yRotation, 0);
//                 CaptureImage(imageIndex++, false);
//                 // Deactivate the ocean GameObject
//                 ocean.SetActive(false);
//                 CaptureImage(imageIndex, true);
//                 // Activate the ocean GameObject after getting annotation image
//                 ocean.SetActive(true);
//                 yield return null;

//             }
//         }
//     }

//     void CaptureImage(int index, bool annotationMode)
//     {
//         RenderTexture renderTexture = new RenderTexture(256, 256, 24);
//         droneCamera.targetTexture = renderTexture;
//         Texture2D image = new Texture2D(256, 256, TextureFormat.RGB24, false);
//         droneCamera.Render();

//         RenderTexture.active = renderTexture;
//         image.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
//         image.Apply();
//         droneCamera.targetTexture = null;
//         RenderTexture.active = null;
//         Destroy(renderTexture);

//         // Save the image
//         byte[] bytes = image.EncodeToPNG();
//         if(annotationMode) File.WriteAllBytes(datasetPath + "annotation_image_" + index.ToString("D4") + ".png", bytes);
//         else {File.WriteAllBytes(datasetPath + "image_" + index.ToString("D4") + ".png", bytes);}
//         Debug.Log("Captured image: " + index);
//     }
// }