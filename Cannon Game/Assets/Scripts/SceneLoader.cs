using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

	public Text nickName;

	public void StartGame()
	{	
		if (nickName.text.ToString()!=""){
			// Load the CanonGame Scene
			GameManager.nickName = nickName.text.ToString();
			SceneManager.LoadScene(1);
		}
		else {
			Debug.Log("Enter nick name!!!!");
		}
	}
}
