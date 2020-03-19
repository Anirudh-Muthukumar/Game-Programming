using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

// change this class as you may see fit
public class ParticleMotion : MonoBehaviour
{
    float[] particle_state;
    Vector3 lcenter, rcenter;
    private float ballMass = 0.2f;

    // Particle (ball instance initialization)
    void Start()
    {        
        particle_state = new float[6]; // you may keep the velocity/position in the same array (it can also be separated into two Vector3's)
        lcenter = new Vector3(-5.0f, 1.0f, 0.0f);  // left center of gravity 
        rcenter = new Vector3(5.0f, 1.0f, 0.0f);  // right center of gravity 

        // initialize particle state (position from the initial instance's position, and velocity according to the cannon orientation)
        for (int d = 0; d < 3; d++)
            particle_state[d] = transform.position[d];  

        float angle_radians = transform.rotation.eulerAngles.x * (Mathf.PI / 180.0f);
        particle_state[3] = 2.0f * Mathf.Cos(-angle_radians);
        particle_state[4] = 2.0f * Mathf.Sin(-angle_radians);
        particle_state[5] = 0.0f;
    }

    void FixedUpdate()
    {
        // your function to implement!
        UpdateState(lcenter, rcenter, Time.deltaTime, ref particle_state);
        
        // update position for particle
        transform.position = new Vector3(particle_state[0], particle_state[1], particle_state[2]);
    }



    // you need to change this function for numerical integration to calculate the particle state
    // to support
    // (a) two attraction forces
    // (b) a simple linear drag function (only for good balls!)
    // (c) update velocity from forces
    // (d) update position from velocity
    void UpdateState(Vector3 lcenter, Vector3 rcenter, float dt, ref float[] particle_state)
    {
        float[] currentState = particle_state;

        Vector3 currentPosition = new Vector3(currentState[0], currentState[1], currentState[2]);
        Vector3 currentVelocity = new Vector3(currentState[3], currentState[4], currentState[5]);
        
        // Get the derivatives of position and velocity
        Vector3 dv = CanonDynamics(currentState);

        // Update the position using Euler ODE
        particle_state[0] = currentPosition[0] + dt * particle_state[3];
        particle_state[1] = currentPosition[1] + dt * particle_state[4];
        particle_state[2] = currentPosition[2] + dt * particle_state[5];

        particle_state[3] = currentVelocity[0] + dt * dv[0];
        particle_state[4] = currentVelocity[1] + dt * dv[1];
        particle_state[5] = currentVelocity[2] + dt * dv[2];

    }

    Vector3 CanonDynamics(float[] currentState)
    {
        Vector3 currentPosition = new Vector3(currentState[0], currentState[1], currentState[2]);
        Vector3 currentVelocity = new Vector3(currentState[3], currentState[4], currentState[5]);
        
        // Two attraction forces
        Vector3 F1a = (lcenter - currentPosition)/(lcenter - currentPosition).magnitude;
        Vector3 F1b = (rcenter - currentPosition)/(rcenter - currentPosition).magnitude;

        Vector3 Forces = F1a + F1b;

        // Drag force for only good balls
        if (gameObject.name.Contains("cannon_ball_template_good_clone"))
            Forces -= 0.01f * currentVelocity;

        Forces = Forces/ballMass;

        return Forces;
    }
}
