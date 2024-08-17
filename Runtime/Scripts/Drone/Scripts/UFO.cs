using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : MonoBehaviour {

    void Start() {
        
    }

    void FixedUpdate() {
        float t = Time.time;
        transform.position = new Vector3(2*Mathf.Cos(t), 5, 2*Mathf.Sin(2*t) + 1000);
    }
}
