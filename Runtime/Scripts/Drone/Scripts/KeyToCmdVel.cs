using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyToCmdVel : MonoBehaviour {

	private DroneController controller;
	public CameraStabilizer droneCamStabilizer;
	public Light droneSpotlight;
	public Light worldLight;

	//private List<string> scenes;
	//private int sceneIdx;

	// Use this for initialization
	void Start () {
		print ("KeyListener started");
		//sceneIdx = 0;
		//scenes = new List<string> (new string[] { "start", "mine" });
	}
	
	// Update is called once per frame
	void Update () {


		if (Input.GetKeyDown (KeyCode.S)) {
			Debug.Log ("Toggling camera stabilizer");
			droneCamStabilizer.StabilizeCamera = !droneCamStabilizer.StabilizeCamera;
		} else if (Input.GetKeyDown (KeyCode.T)) {
			Debug.Log ("Toggling drone spotlight");
			droneSpotlight.enabled = !droneSpotlight.enabled;
		} else if (Input.GetKeyDown (KeyCode.L)) {
			Debug.Log ("Toggling world light");
			worldLight.enabled = !worldLight.enabled;
		} else if (Input.GetKeyDown (KeyCode.R)) {
			Debug.Log ("Reloading the environment");
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		} else if (Input.GetKeyDown (KeyCode.N)) {
			/*
			Debug.Log ("tipt");
			Debug.Log (SceneManager.sceneCountInBuildSettings);
			Debug.Log(SceneManager.GetActiveScene ().buildIndex);
			*/
			int sceneIdx = SceneManager.GetActiveScene ().buildIndex + 1;
			if (sceneIdx >= SceneManager.sceneCountInBuildSettings)
				sceneIdx = 0;
			Debug.Log ("Loading scene  " + sceneIdx);
			SceneManager.LoadScene (sceneIdx);
			/*
			sceneIdx = sceneIdx + 1;
			if (sceneIdx >= scenes.Count)
				sceneIdx = 0;

			Debug.Log ("Switching to the next environment " + sceneIdx);
			SceneManager.LoadScene (scenes[sceneIdx]);
			*/
			Debug.Log ("Top");
		}
	}
	/*
	void OnGUI() {
		Event e = Event.current;
		if (e.isKey)
			Debug.Log("Detected key code: " + e.keyCode);

	}*/
}
