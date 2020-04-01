using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplyForce : MonoBehaviour {

	public float thrust = 2.0f;
    public Rigidbody fjBody;
	public Rigidbody sjBody;
	public Rigidbody hjBody;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		ApplyForceOnFixedJoint();
		ApplyForceOnSpringJoint();
		ApplyForceOnHingeJoint();
		GoToMainMenu();
	}

	void ApplyForceOnFixedJoint()
	{
		// Fixed Joint
		if(Input.GetKeyDown(KeyCode.A)){
			Debug.Log("Small Force on Fixed Joint");
			fjBody.AddForce(0, 0, thrust, ForceMode.Impulse);
		}

		if(Input.GetKeyDown(KeyCode.J)){
			Debug.Log("Large Force on Fixed Joint");
			fjBody.AddForce(0, 0, thrust + 10, ForceMode.Impulse);
		}
	}

	void ApplyForceOnSpringJoint() 
	{
		// Spring Joint
		if(Input.GetKeyDown(KeyCode.S)){
			Debug.Log("Small Force on Spring Joint");
			sjBody.AddForce(0, 0, thrust, ForceMode.Impulse);
		}

		if(Input.GetKeyDown(KeyCode.K)){
			Debug.Log("Large Force on Spring Joint");
			sjBody.AddForce(0, 0, thrust + 10, ForceMode.Impulse);
		}
	}

	void ApplyForceOnHingeJoint() 
	{
		// Hinge Joint
		if(Input.GetKeyDown(KeyCode.D)){
			Debug.Log("Small Force on Hinge Joint");
			hjBody.AddForce(0, 0, thrust, ForceMode.Impulse);
		}

		// More Force
		if(Input.GetKeyDown(KeyCode.L)){
			Debug.Log("Large Force on Hinge Joint");
			hjBody.AddForce(0, 0, thrust + 10, ForceMode.Impulse);
		}
	}

	void GoToMainMenu()
	{
		// If user presses Esc 
		if(Input.GetKeyDown(KeyCode.Escape)) {
			SceneManager.LoadScene("Scenes/MainMenu");
		}
	}
}
