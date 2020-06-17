using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour {

	// Load the level.unity 
	public void LevelLoader () {
		// Load the Level Scene
		SceneManager.LoadScene("Scenes/level");
	}
}
