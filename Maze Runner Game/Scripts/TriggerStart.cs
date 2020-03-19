using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStart : MonoBehaviour {

	// link to trail start point light
	public Light light;
	Color light_color;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter()
	{	
		// light_color = light.color;
		light.color = Color.yellow; // indicates right trial
		
	}

	void OnTriggerExit()
	{
		// light.color = light_color;
	}
}
