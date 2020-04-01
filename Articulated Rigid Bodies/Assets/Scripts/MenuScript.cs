using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

	public void MainSceneLoader () {
		// Load the Main Scene
		SceneManager.LoadScene("Scenes/Main");
	}
	
}
