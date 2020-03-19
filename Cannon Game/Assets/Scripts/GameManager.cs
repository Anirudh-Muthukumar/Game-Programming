using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

// change this class as you may see fit 
// you need to incorporate a menu in your game, a timer, and a hall of fame
// you may break the functionality into multiple scripts/classes if you want

public class GameManager : MonoBehaviour {
    public float max_num_particles;     // set to 50 in the Unity scene
    public Text score_text;             // text UI element showing the score
    public Text updateTime;            // text UI element showing remaining time
    public Text updateName;             // text UI element showing user name

    private GameObject cannon_ball_template;    // will be set to the cannon ball prefab instance existing in the scene
    private GameObject cannon;                  // will be set to the cannon object
    private Material bad_ball_material;         // will be assigned with the red material for bad balls
	private int number_of_particles;            // number of currently active balls
    private int number_of_good_particles;       // number of currently good active balls
    private int number_of_bad_particles;        // number of currently bad active balls
    private int score;                          // keeps track of score
    private int ball_id;                        // a unique ID for each spawned ball (might be useful for debugging)

    public static string nickName;              // Nick Name of user
    private float timeRemaining = 60.0f;             // Remaining time
    private string database_directory = "";     // Path for database directory
    private string dbFile = "Game Database.txt";                 // File name for database

    // Use this for initialization
    void Start () {
        UnityEngine.Random.InitState( (int)System.DateTime.Now.Ticks );   // initialize the random seed generation based on current time

        // find the cannon ball prefab
        cannon_ball_template = GameObject.Find("cannon_ball_template");
        cannon = GameObject.Find("cannon");
        if (cannon_ball_template == null)
            Debug.LogError("cannnon_ball_template was not found in the scene!!! Did you delete it from the starter scene?");
        if (cannon == null)
            Debug.LogError("cannnon was not found in the scene!!! Did you delete it from the starter scene?");

        bad_ball_material = Resources.Load<Material>("Materials/BadBallMaterial");         // load material for bad balls
        number_of_particles = 0;        // initialize rest of variables
        number_of_good_particles = 0;
        number_of_bad_particles = 0;
        score = 0; 
        ball_id = 1;

        database_directory = Directory.GetCurrentDirectory() + "/Assets/";
        updateName.text = nickName;    // Update the nick name of the player
        StartCoroutine("Spawn");        // google "StartCoroutine" (Unity function). This function will executed in parallel with the game loop
	}


    // this function spawns good or bad balls
    // google "IEnumerator" (C# / .NET feature)
    // IEnumerator allows us to stop the function at a specific point, return something (or nothing) through 'yield return',
    // then gets back after that point
    private IEnumerator Spawn()
    {
        while (true)                   // Spawn runs continuously
        {
            // max wait time to spawn a ball
            // the more the score is, the less the wait time tends to be for the new ball to be spawned (making the game a bit harder)
            float wait_time = Mathf.Max(1.5f - (float)score / 15.0f, 0.0f) + UnityEngine.Random.Range(0.0f, 0.5f);

            // see https://docs.unity3d.com/ScriptReference/WaitForSeconds.html
            yield return new WaitForSeconds(wait_time); // wait here until wait_time, then keep executing

            // if the current number of active balls is above the score (or the maximum number of balls you can have)
            // don't generate any new balls. The higher the score, the more balls will be generated (making the game a bit harder)
            if (number_of_particles >= Mathf.Min(score + 1, max_num_particles))
                continue;

            Vector3 pos = cannon_ball_template.transform.position; // find the position of the main cannon ball prefab instance in world space (changes because the cannon moves)
            Quaternion rot = cannon.transform.transform.rotation;  // find the cannon orientation (will be used to initialize the ball velocity, see ParticleMotionScript.cs)
            GameObject new_object = Instantiate(cannon_ball_template, pos, rot);  // create the ball!
            new_object.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);      // scale it own a bit (the prefab was too big)

            // flip a coin: generate good or ball ball?
            // also generate bad balls, only if there is at least one good ball in the scene 
            if ( (number_of_good_particles > 0) && (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f) )
            {
                // bad ball condition
                new_object.GetComponent<Renderer>().material = bad_ball_material; //make it red
                number_of_bad_particles++;
                new_object.name = "cannon_ball_template_bad_clone" + ball_id; // give it a proper name
            }
            else
            {
                // good ball condition
                number_of_good_particles++;
                new_object.name = "cannon_ball_template_good_clone" + ball_id;
            }

            number_of_particles++;
            ball_id++;                       

            new_object.AddComponent<ParticleMotion>(); // important, attach the script to this instance!
        }
    }

    // Update is called once per frame
    void Update ()
    {   
        // Update the remaining time for every second
        timeRemaining -= Time.deltaTime;
        updateTime.text = Math.Round(timeRemaining, 0).ToString() + " seconds";

        if (timeRemaining<=0.0f || Input.GetKey(KeyCode.Escape)){

            // Save the player's score to database file in Assets directory

            string filepath = database_directory + dbFile;

            try {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
                {
                    file.WriteLine(nickName + ", " + score.ToString());
                    file.Close();
                }
            }
            catch (Exception e) {
                Debug.Log(e);
            }
            // Switch to Hall of Fame scene
            SceneManager.LoadScene(2);
        }

        if (Input.GetMouseButtonDown(0)) // mouse click detected
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // through a ray from the mouse position towards the scene
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))  // check for ray-object intersections
            {
                // ray-object intersection detected
                string name = hit.transform.gameObject.name;  // check the name of the object (we don't want to delete the cannnon or the main ball instance)
                if (name.Contains("clone"))  // if it is a generated ball instance
                {
                    Destroy(hit.transform.gameObject); // destroy it!
                    number_of_particles--;

                    if (name.Contains("bad"))  // and if it is a bad ball, update accordigly
                    {
                        number_of_bad_particles--;
                        score--;
                        if (score < 0)
                            score = 0;
                    }
                    else                      // and if it is a good ball, update accordigly
                    {
                        score++;
                        number_of_good_particles--;
                    }                    
                                        
                    score_text.text = "Score: " + score;  // update score text
                }
            }
            else
            {
            }
        }
    }
}