using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followCursor : MonoBehaviour {

	public float distanceFromCamera = 10f;
	public Camera camera1;
	public Camera camera2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		// Update ball position
		UpdateBallPosition();
		
	}

	// method for updating ball position according to cursor
	void UpdateBallPosition()
	{

		Vector3 currentPosition = Input.mousePosition; // fetch the current mouse position
		currentPosition.z = distanceFromCamera; // update the z distance
		Vector3 newPosition = Camera.current.ScreenToWorldPoint(currentPosition);

		transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		Debug.Log("Current Camera " + Camera.current.name);
		
		if (currentPosition.y <= 661 && currentPosition.y >= 80 && currentPosition.x>=41 && currentPosition.x <= 982 )
			transform.position =  Vector3.Lerp(transform.position, newPosition, 0.4f); // update the object position using transform
		else 
		{
			Vector3 scale = transform.localScale;
			float coefficient = 0.003f;
			float diff = 0;
			
			if (currentPosition.y <80)
				diff = 110 - currentPosition.y;
			else if (currentPosition.y > 661)
				diff = currentPosition.y - 661;
			else if (currentPosition.x > 982)
				diff = -(currentPosition.x - 982);
			else 
				diff = -(41 - currentPosition.x);

			if (Camera.current.name=="Camera1")
			{
				scale.x += coefficient * diff;
				scale.y -= coefficient * diff;
			}
			else
			{
				scale.x -= coefficient * diff;
				scale.z += coefficient * diff;
			}
			
			transform.localScale = Vector3.Lerp(transform.localScale, scale, 10f);
		}
	}
}
