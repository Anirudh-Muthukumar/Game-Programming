using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour {

	private Level level;

	void Start()
	{
		GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
	}

	// Play Again Button loads a new game 
	public void TaskOnClick () {
		// Debug.Log("Try Again = " + level.try_again.ToString() + "Play Again = " + level.play_again.ToString());

		// if(level.play_again)
			SceneManager.LoadScene("Scenes/level");

		// if(level.try_again)
			// SceneManager.LoadScene("Scenes/level");
	}

}
