using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConstructorAnimator : MonoBehaviour {

	Animator animator;
	CharacterController controller;

	float speed = 2.0f;
	float rotationSpeed = 80f;
	float gravity = 8f;
	float rotation = 0f;
	float jumpSpeed = 4.0f;

	bool isRunning = false;
	bool isWalking = false;

	private Vector3 moveDirection = Vector3.zero;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>(); 
		controller = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update () {
		Movement();
		GetInput();
		GoToMainMenu();
	}

	void Movement()
	{
		if (controller.isGrounded) 
		{
			// Idle - state 0
			if (Input.GetKeyUp(KeyCode.W)) 
			{
				// Debug.Log("Idle");
				animator.SetBool("running", false);
				animator.SetBool("walking", false);
				animator.SetInteger("state", 0);  // update state
				moveDirection = Vector3.zero;
				moveDirection *= speed;
			}

			// Walk - state 1
			if (Input.GetKey(KeyCode.W)) 
			{
				animator.SetInteger("state", 1); 
				animator.SetBool("walking", true);
				moveDirection = new Vector3 (0, 0, 1); 
				moveDirection *= speed;
				moveDirection = transform.TransformDirection(moveDirection);
			}

			// Run - state 2
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W)) 
			{
				animator.SetBool("walking", false);
				animator.SetBool("running", true);
				animator.SetInteger("state", 2);    // update state
				moveDirection = new Vector3 (0, 0, 4); // 
				moveDirection *= speed;
				moveDirection = transform.TransformDirection(moveDirection);
			}

			// Jump - state 3
			
		}

		// at each instant gravity keeps pulling the player down
		moveDirection.y -= gravity * Time.deltaTime;

		// accounts for rotation along Y-axis 
		rotation += Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
		transform.eulerAngles = new Vector3(0, rotation, 0);

		// move the controller
		controller.Move(moveDirection * Time.deltaTime);
	}

	void GetInput() 
	{
		if(controller.isGrounded)
		{
			if(Input.GetKeyDown(KeyCode.J)) 
			{
				Jump();
			}
		}
	}

	void Jump()
	{
		StartCoroutine(JumpRoutine());
	}

	IEnumerator JumpRoutine() 
	{	
		int originalState = animator.GetInteger("state");
		moveDirection.y += jumpSpeed;
		// moveDirection *= speed;
		moveDirection = transform.TransformDirection(moveDirection);

		animator.SetInteger("state", 3);

		// at each instant gravity keeps pulling the player down
		// moveDirection.y -= gravity * Time.deltaTime;

		// accounts for rotation along Y-axis 
		rotation += Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
		transform.eulerAngles = new Vector3(0, rotation, 0);

		// move the controller
		controller.Move(moveDirection * Time.deltaTime);
		yield return new WaitForSeconds((float)0.2);
		animator.SetInteger("state", 0);
	}

	void GoToMainMenu()
	{
		// If user presses Esc 
		if(Input.GetKeyDown(KeyCode.Escape)) {
			SceneManager.LoadScene("Scenes/MainMenu");
		}
	}

}
