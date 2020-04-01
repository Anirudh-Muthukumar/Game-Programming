using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

	public void Scene1Loader () {
		// Load the Main Scene
		SceneManager.LoadScene("Scenes/Scene1");
	}
}
