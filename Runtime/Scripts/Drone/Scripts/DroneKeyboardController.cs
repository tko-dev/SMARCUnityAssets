using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControlScripts;


namespace GameUI
{
    public class DroneKeyboardController : KeyboardController 
    {

        public DroneController droneController;

        public float speed = 5.0f;
        public float rotationSpeed = 5f;

        void Update() {
            Vector3 linearVelocity = Vector3.zero;
            Vector3 angularVelocity = Vector3.zero;

            // Handling linear movement
            if (Input.GetKey(KeyCode.I)) {
                linearVelocity += Vector3.forward * speed;
            }
            if (Input.GetKey(KeyCode.K)) {
                linearVelocity += Vector3.back * speed;
            }
            if (Input.GetKey(KeyCode.J)) {
                linearVelocity += Vector3.left * speed;
            }
            if (Input.GetKey(KeyCode.L)) {
                linearVelocity += Vector3.right * speed;
            }
            if (Input.GetKey(KeyCode.O)) {
                linearVelocity += Vector3.up * speed;
            }
            if (Input.GetKey(KeyCode.P)) {
                linearVelocity += Vector3.down * speed;
            }

            // Handling rotation
            if (Input.GetKey(KeyCode.Y)) {
                angularVelocity += Vector3.up * rotationSpeed;
            }
            if (Input.GetKey(KeyCode.U)) {
                angularVelocity += Vector3.down * rotationSpeed;
            }

            droneController.UpdateVelocities(linearVelocity, angularVelocity);
        }
    }
}

