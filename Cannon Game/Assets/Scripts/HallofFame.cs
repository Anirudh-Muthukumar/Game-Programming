using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class HallofFame : MonoBehaviour {

	private string database_directory = "";     // Path for database directory
    private string dbFile = "Game Database.txt";                 // File name for database

	public Text player1;
	public Text player2;
	public Text player3;
	public Text player4;
	public Text player5;

	public Text score1;
	public Text score2;
	public Text score3;
	public Text score4;
	public Text score5;

	// Use this for initialization
	void Start () {

		player1.text = "Empty";
		player2.text = "Empty";
		player3.text = "Empty";
		player4.text = "Empty";
		player5.text = "Empty";

		score1.text = "0";
		score2.text = "0";
		score3.text = "0";
		score4.text = "0";
		score5.text = "0";
		
		// Data structure to keep track of username and their score
		var database = new List<KeyValuePair<string, int>>();

		// Reading the contents of database file
		database_directory = Directory.GetCurrentDirectory() + "/Assets/";
		string filepath = database_directory + dbFile;
		string[] lines = System.IO.File.ReadAllLines(@filepath);

		// Debug.Log(lines.Length);

		foreach(string line in lines){

			string name = "";
			string scoreText = "";
			int i=0;

			// extracting name and score for each line of the database file
			while(i<line.Length && line[i]!=',') 
				name += line[i++];

			i++;

			while(i<line.Length)
				scoreText += line[i++] ;

			int score = 0;

			try{
				score = Int32.Parse(scoreText);
			}
			catch(Exception e)
			{
				continue;
			}
			
			// Add the key value pair to the list
			database.Add(new KeyValuePair<string,int>(name, score));
			Debug.Log("Name: " + name + "; Score: " + score);
		}

		// Total number of records
		Debug.Log(database.Count);

		// Sort the list based on score
		database.Sort((x, y) => y.Value.CompareTo(x.Value));

		if (database.Count>0){
			player1.text = database[0].Key;
			score1.text = database[0].Value.ToString();
		}
		if (database.Count>1){
			player2.text = database[1].Key;
			score2.text = database[1].Value.ToString();
		}
		if (database.Count>2){
			player3.text = database[2].Key;
			score3.text = database[2].Value.ToString();
		}

		if (database.Count>3){
			player4.text = database[3].Key;
			score4.text = database[3].Value.ToString();
		}

		if (database.Count>4){
			player5.text = database[4].Key;
			score5.text = database[4].Value.ToString();
		}

	}
	
	// method to return to Main Menu Scene
	public void returnToMainMenu()
	{
		SceneManager.LoadScene(0);
	}
	// method to return to Cannon game Scene
	public void playAgain()
	{
		SceneManager.LoadScene(1);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
