// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using VehicleComponents.Actuators;
// namespace Drone.Scripts
// {
//     public class DroneKeyboardController : MonoBehaviour {
//         [Tooltip("Set to true to give up control to ROS commands")]
//         public bool letROSTakeTheWheel = true;
//         public float speed = 5.0f;
//         public float rotationSpeed = 100.0f;

//         public GameObject frontleftpropref;
//         public GameObject frontrightpropref;
//         public GameObject rearleftpropref;
//         public GameObject rearrightpropref;


//         private DronePropeller frontleftprop;
//         private DronePropeller frontrightprop;
//         private DronePropeller rearleftprop;
//         private DronePropeller rearrightprop;

//         DronePropellerCommand p1cmd, p2cmd, p3cmd p4cmd;


//         //private DronePropeller[] propellers;

//         void Start() {
//             // Find and initialize all DronePropeller components in children
//             propeller1 = frontleftpropref.GetComponents<DronePropeller>();
//             propeller2 = frontrightpropref.GetComponents<DronePropeller>();
//             propeller3 = rearleftpropref.GetComponents<DronePropeller>();
//             propeller4 = rearrightpropref.GetComponents<DronePropeller>();
//             p1cmd = frontleftpropref.GetComponents<DronePropellerCommand>();
//             p2cmd = frontrightpropref.GetComponents<DronePropellerCommand>();
//             p3cmd = rearleftpropref.GetComponents<DronePropellerCommand>();
//             p4cmd = rearrightpropref.GetComponents<DronePropellerCommand>();
//             if (propellers == null || propellers.Length == 0) {
//                 Debug.LogError("No DronePropeller components found!");
//             }
//         }

//         void Update() {
//             // Reset propeller RPMs
//             ResetPropellers();

//             // Handling linear movement
//             if (Input.GetKey(KeyCode.I)) {
//                 Debug.Log("moving forward");
//                 AdjustRPMs(Vector3.forward * speed);
//             }
//             if (Input.GetKey(KeyCode.K)) {
//                 AdjustRPMs(Vector3.back * speed);
//             }
//             if (Input.GetKey(KeyCode.J)) {
//                 AdjustRPMs(Vector3.left * speed);
//             }
//             if (Input.GetKey(KeyCode.L)) {
//                 AdjustRPMs(Vector3.right * speed);
//             }
//             if (Input.GetKey(KeyCode.O)) {
//                 AdjustRPMs(Vector3.up * speed);
//             }
//             if (Input.GetKey(KeyCode.P)) {
//                 AdjustRPMs(Vector3.down * speed);
//             }

//             // Handling rotation
//             if (Input.GetKey(KeyCode.Y)) {
//                 AdjustRPMs(Vector3.up * rotationSpeed);
//             }
//             if (Input.GetKey(KeyCode.U)) {
//                 AdjustRPMs(Vector3.down * rotationSpeed);
//             }
//         }

//         void AdjustRPMs(Vector3 direction) {
//             foreach (var propeller in propellers) {
//                 propeller.SetRpm(direction.magnitude); // Adjust RPM based on direction magnitude
//             }
//         }

//         void ResetPropellers() {
//             foreach (var propeller in propellers) {
//                 propeller.InitializeRPMToStayAfloat(); // Reset RPM to zero for all propellers
//             }
//         }
//     }
// }
