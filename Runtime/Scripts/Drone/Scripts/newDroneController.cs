// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using VehicleComponents.Actuators; // Adjust based on your component
// using VehicleComponents.ROS.Subscribers;

// namespace Drone.Scripts
// {
//     public class DroneKeyboardControl : MonoBehaviour
//     {
//         private DroneController _droneController;

//         [Tooltip("Set to true to give up control to ROS commands")]
//         public bool letROSTakeTheWheel = true;

//         public GameObject propellerFLGo;
//         public GameObject propellerFRGo;
//         public GameObject propellerRLGo;
//         public GameObject propellerRRGo;

//         DronePropeller propellerFL, propellerFR, propellerRL, propellerRR;
//         PropellerCommand propellerFLCmd, propellerFRCmd, propellerRLCmd, propellerRRCmd;

//         public float rollRpms = 0.1f;
//         public float moveRpms = 800f;

//         [Header("Mouse control")] [Tooltip("Use these when you don't want to press down for 10 minutes")]
//         public bool useBothRpms = false;

//         public float bothRpms = 0f;

//         private void Awake()
//         {
//             _droneController = GetComponentInParent<DroneController>();

//             propellerFL = propellerFLGo.GetComponent<DronePropeller>();
//             propellerFLCmd = propellerFLGo.GetComponent<PropellerCommand>();
//             propellerFR = propellerFRGo.GetComponent<DronePropeller>();
//             propellerFRCmd = propellerFRGo.GetComponent<PropellerCommand>();
//             propellerRL = propellerRLGo.GetComponent<DronePropeller>();
//             propellerRLCmd = propellerRLGo.GetComponent<PropellerCommand>();
//             propellerRR = propellerRRGo.GetComponent<DronePropeller>();
//             propellerRRCmd = propellerRRGo.GetComponent<PropellerCommand>();
//         }

//         private void FixedUpdate()
//         {
//             propellerFLCmd.enabled = letROSTakeTheWheel;
//             propellerFRCmd.enabled = letROSTakeTheWheel;
//             propellerRLCmd.enabled = letROSTakeTheWheel;
//             propellerRRCmd.enabled = letROSTakeTheWheel;

//             // Handling linear movement with IJKL keys
//             Vector3 linearVelocity = Vector3.zero;
//             Vector3 angularVelocity = Vector3.zero;

//             if (Input.GetKey(KeyCode.I))
//             {
//                 linearVelocity += Vector3.forward * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.K))
//             {
//                 linearVelocity += Vector3.back * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.J))
//             {
//                 linearVelocity += Vector3.left * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.L))
//             {
//                 linearVelocity += Vector3.right * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.O))
//             {
//                 linearVelocity += Vector3.up * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.P))
//             {
//                 linearVelocity += Vector3.down * moveRpms;
//             }

//             // // Handling rotation with YU keys
//             // if (Input.GetKey(KeyCode.Y))
//             // {
//             //     angularVelocity += Vector3.up * rotationSpeed;
//             // }
//             // if (Input.GetKey(KeyCode.U))
//             // {
//             //     angularVelocity += Vector3.down * rotationSpeed;
//             // }

//             _droneController.UpdateVelocities(linearVelocity, angularVelocity);
//         }
//     }
// }

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using VehicleComponents.Actuators; // Adjust based on your component
// using VehicleComponents.ROS.Subscribers;

// namespace DefaultNamespace
// {
//     public class DroneKeyboardControl : MonoBehaviour
//     {
//         private DroneController _droneController;

//         [Tooltip("Set to true to give up control to ROS commands")]
//         public bool letROSTakeTheWheel = false;

//         public GameObject propellerFLGo;
//         public GameObject propellerFRGo;
//         public GameObject propellerRLGo;
//         public GameObject propellerRRGo;

//         Propeller propellerFL, propellerFR, propellerRL, propellerRR;
//         PropellerCommand propellerFLCmd, propellerFRCmd, propellerRLCmd, propellerRRCmd;

//         public float rollRpms = 0.1f;
//         public float moveRpms = 800f;
//         public float rotationSpeed = 100.0f; // Define rotation speed here

//         [Header("Mouse control")] [Tooltip("Use these when you don't want to press down for 10 minutes")]
//         public bool useBothRpms = false;

//         public float bothRpms = 0f;

//         private void Awake()
//         {
//             _droneController = GetComponentInParent<DroneController>();

//             propellerFL = propellerFLGo.GetComponent<Propeller>();
//             propellerFLCmd = propellerFLGo.GetComponent<PropellerCommand>();
//             propellerFR = propellerFRGo.GetComponent<Propeller>();
//             propellerFRCmd = propellerFRGo.GetComponent<PropellerCommand>();
//             propellerRL = propellerRLGo.GetComponent<Propeller>();
//             propellerRLCmd = propellerRLGo.GetComponent<PropellerCommand>();
//             propellerRR = propellerRRGo.GetComponent<Propeller>();
//             propellerRRCmd = propellerRRGo.GetComponent<PropellerCommand>();
//         }

//         private void FixedUpdate()
//         {
//             propellerFLCmd.enabled = letROSTakeTheWheel;
//             propellerFRCmd.enabled = letROSTakeTheWheel;
//             propellerRLCmd.enabled = letROSTakeTheWheel;
//             propellerRRCmd.enabled = letROSTakeTheWheel;

//             // Handling linear movement with IJKL keys
//             Vector3 linearVelocity = Vector3.zero;
//             Vector3 angularVelocity = Vector3.zero;

//             if (Input.GetKey(KeyCode.I))
//             {
//                 linearVelocity += Vector3.forward * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.K))
//             {
//                 linearVelocity += Vector3.back * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.J))
//             {
//                 linearVelocity += Vector3.left * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.L))
//             {
//                 linearVelocity += Vector3.right * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.O))
//             {
//                 linearVelocity += Vector3.up * moveRpms;
//             }
//             if (Input.GetKey(KeyCode.P))
//             {
//                 linearVelocity += Vector3.down * moveRpms;
//             }

//             // Handling rotation with YU keys
//             if (Input.GetKey(KeyCode.Y))
//             {
//                 angularVelocity += Vector3.up * rotationSpeed;
//             }
//             if (Input.GetKey(KeyCode.U))
//             {
//                 angularVelocity += Vector3.down * rotationSpeed;
//             }

//             _droneController.UpdateVelocities(linearVelocity, angularVelocity);
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleComponents.Actuators; // Adjust based on your component
using VehicleComponents.ROS.Subscribers;

namespace DefaultNamespace
{
    public class DroneKeyboardControl : MonoBehaviour
    {
        private DroneController _droneController;

        [Tooltip("Set to true to give up control to ROS commands")]
        public bool letROSTakeTheWheel = true;

        public GameObject propellerFLGo;
        public GameObject propellerFRGo;
        public GameObject propellerRLGo;
        public GameObject propellerRRGo;

        DronePropeller propellerFL, propellerFR, propellerRL, propellerRR;
        DronePropellerCommand propellerFLCmd, propellerFRCmd, propellerRLCmd, propellerRRCmd;

        public float rollRpms = 0.1f;
        public float moveRpms = 800f;
        public float rotationSpeed = 100.0f; // Define rotation speed here

        [Header("Mouse control")] [Tooltip("Use these when you don't want to press down for 10 minutes")]
        public bool useBothRpms = false;

        public float bothRpms = 0f;

        private void Awake()
        {
            _droneController = GetComponentInParent<DroneController>();

            propellerFL = propellerFLGo.GetComponent<DronePropeller>();
            propellerFLCmd = propellerFLGo.GetComponent<DronePropellerCommand>();
            propellerFR = propellerFRGo.GetComponent<DronePropeller>();
            propellerFRCmd = propellerFRGo.GetComponent<DronePropellerCommand>();
            propellerRL = propellerRLGo.GetComponent<DronePropeller>();
            propellerRLCmd = propellerRLGo.GetComponent<DronePropellerCommand>();
            propellerRR = propellerRRGo.GetComponent<DronePropeller>();
            propellerRRCmd = propellerRRGo.GetComponent<DronePropellerCommand>();
        }

        private void FixedUpdate()
        {
            propellerFLCmd.enabled = letROSTakeTheWheel;
            propellerFRCmd.enabled = letROSTakeTheWheel;
            propellerRLCmd.enabled = letROSTakeTheWheel;
            propellerRRCmd.enabled = letROSTakeTheWheel;

            if (letROSTakeTheWheel)
            {
                // If ROS is controlling, do not process keyboard inputs
                return;
            }

            // Handling linear movement with IJKL keys
            Vector3 linearVelocity = Vector3.zero;
            Vector3 angularVelocity = Vector3.zero;

            if (Input.GetKey(KeyCode.I))
            {
                Debug.Log("Moving forward");
                linearVelocity += Vector3.forward * moveRpms;
            }
            if (Input.GetKey(KeyCode.K))
            {
                linearVelocity += Vector3.back * moveRpms;
            }
            if (Input.GetKey(KeyCode.J))
            {
                linearVelocity += Vector3.left * moveRpms;
            }
            if (Input.GetKey(KeyCode.L))
            {
                linearVelocity += Vector3.right * moveRpms;
            }
            if (Input.GetKey(KeyCode.O))
            {
                linearVelocity += Vector3.up * moveRpms;
            }
            if (Input.GetKey(KeyCode.P))
            {
                linearVelocity += Vector3.down * moveRpms;
            }

            // Handling rotation with YU keys
            if (Input.GetKey(KeyCode.Y))
            {
                angularVelocity += Vector3.up * rotationSpeed;
            }
            if (Input.GetKey(KeyCode.U))
            {
                angularVelocity += Vector3.down * rotationSpeed;
            }

            _droneController.UpdateVelocities(linearVelocity, angularVelocity);
        }
    }
}

