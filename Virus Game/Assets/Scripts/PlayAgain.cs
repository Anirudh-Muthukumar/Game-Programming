using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour {

	public void NewGame () {
		// Load the Level Scene
		SceneManager.LoadScene("Scenes/level");
	}
	
}
