using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

	public void Scene1Loader()
	{
		// SceneManager.UnloadScene("Scenes/MainMenu");
		SceneManager.LoadScene("Scenes/Scene1");
	}

	public void Scene2Loader()
	{
		// SceneManager.UnloadScene("Scenes/MainMenu");
		SceneManager.LoadScene("Scenes/Scene2");
	}
}
