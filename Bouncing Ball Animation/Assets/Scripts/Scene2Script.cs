using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene2Script : MonoBehaviour {
	public Camera Camera1;
	public Camera Camera2;
	public int currentCamera;
	public float distanceFromCamera = 1f;
	public GameObject ball;

	// Use this for initialization
	void Start () {
		Camera2.enabled = false;
		Camera1.enabled = true;
		currentCamera = 1;
	}
	
	// Update is called once per frame
	void Update () {
		
	
		// Return to Main Menu
		returnToMainMenu();

		// Change Camera
		changeCamera();
	}


	void returnToMainMenu()
	{
		// Check if Esc is pressed
		if (Input.GetKey(KeyCode.Escape)) {
			SceneManager.LoadScene("Scenes/MainMenu");
		}
	}

	void changeCamera()
	{
		// Check if Spacebar is pressed
		if (Input.GetKey(KeyCode.Space)) {

			if (Camera1.enabled) 
			{
				Camera1.enabled = false;
				Camera2.enabled = true;
				currentCamera = 2;
			}
			else
			{
				Camera2.enabled = false;
				Camera1.enabled = true;
				currentCamera = 1;
			}

		}
	}
}
