using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Pendulum : MonoBehaviour {
    // parameters of ths simulation (are specified through the Unity's interface)
    public float gravity_acceleration = 9.8f;
    public float mass = 1.0f;
    public float friction_coeficient = 0.0f;
    public float initial_angular_velocity = 0.0f;
    public float time_step_h = 0.05f;
    public string ode_method = "improved-euler";



    // parameters that will be populated automatically from the geometry/components of the scene
    private float rod_length = 0.0f;
    private float c = 0.0f;
    private float omega = 0.0f;
    private GameObject pendulum = null;

    // the state vector stores two entries:
    // state_vector[0] angle of pendulum (\theta) in radians
    // state_vector[1] angular velocity of pendulum
    private Vector2 state_vector;
    private List<float> kineticEnergies;
    private List<float> potentialEnergies;
    private List<float> totalEnergies;

    private int energyCount = 0;
    private int energyIterations = 0;
    private float currentTime = 0.0f;
    private string fileName = "";

    // Use this for initialization
    void Start ()
    {
        Time.fixedDeltaTime = time_step_h;      // set the simulation step - FixedUpdate is called every 'time_step_h' seconds 
        state_vector = new Vector2(0.0f, 0.0f); // initialization of the state vector
        pendulum = GameObject.Find("Pendulum");
        if (pendulum == null)
        {
            Debug.LogError("Sphere not found! Did you delete it from the starter scene?");
        }
        GameObject rod = GameObject.Find("Quad");
        if (rod == null)
        {
            Debug.LogError("Rod not found! Did you delete it from the starter scene?");
        }
        rod_length = rod.transform.localScale.y; // finds rod length (based on quad's scale)
        
        state_vector[0] = pendulum.transform.eulerAngles.z * Mathf.Deg2Rad; // initial angle is set from the starter scene
        state_vector[1] = initial_angular_velocity; 

        c = friction_coeficient / mass;        // following the ODE specification
        omega = gravity_acceleration / rod_length;

        // Initializing total number of iterations for plotting
        energyIterations = (int) (20/time_step_h);
        
        // FileName 
        string home_directory = Directory.GetCurrentDirectory();
        string plot_directory = home_directory + "//EnergyData/";

        System.IO.Directory.CreateDirectory(home_directory + "/EnergyData/");

        fileName =  plot_directory + time_step_h + "_v_" + initial_angular_velocity + "_p_" + friction_coeficient + "_" + ode_method + ".csv";
    }

    // Method to append data to given text file 
    void addEnergy(float index, float time, float kineticEnergy, float potentialEnergy, float totalEnergy, string filepath)
    {   
        // Overwrite if the file exists
        if (index==0) 
        {
            try 
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, false))
                {
                    file.WriteLine("Time" + "," + "Kinetic Energy" + "," + "Potential Energy" + "," + "Total Energy");
                }
            }
            catch (Exception e) {
                Debug.Log(e);
            }
        }

        try 
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(time + "," + kineticEnergy + "," + potentialEnergy + "," + totalEnergy);
            }
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }

    // Update is called once per Time.fixedDeltaTime  sec
    void FixedUpdate ()
    {
        // complete this function (measure kinetic, potential, total energy)
        float y = state_vector[0];
        float v = state_vector[1];

        float kinetic_energy = (float)(0.5f * mass * (float)Math.Pow(rod_length * v, 2.0f));    // change here
        float potential_energy =  (float)(mass * gravity_acceleration * rod_length * (1.0f - (float)Math.Cos(state_vector[0])));  // change here
        Debug.Log(kinetic_energy + potential_energy);

        OdeStep();
        pendulum.transform.eulerAngles = new Vector3(0.0f, 0.0f,  state_vector[0] * Mathf.Rad2Deg);

        // Add new values to the file
        // Debug.Log(Directory.GetCurrentDirectory());

        // 
 
        if (energyCount <= energyIterations){
            addEnergy(energyCount, currentTime, kinetic_energy, potential_energy, kinetic_energy + potential_energy, fileName);
            currentTime =  (float)Math.Round((double)(currentTime + time_step_h), 4);
            // Debug.Log(currenTime)
            energyCount += 1;
        }

        Debug.Log("Time : " + currentTime + " seconds");

        // kineticEnergies.Add(kinetic_energy);
        // potentialEnergies.Add(potential_energy);
        // totalEnergies.Add(totalEnergies);

        // addEnergy()
        
    }

    // complete this function!
    // it should return the right side of the two ODEs (in other words, the derivative of the pendulum's angle and angular velocity)
    // and you should use it in OdeStep!
    Vector2 PendulumDynamics(Vector2 current_state_vector)
    {   
        float y = current_state_vector[0];
        float v = current_state_vector[1];

        // g(v) = c * v
        float angularVelocity = c * v;

        // float omegaSquare = (float)Math.Pow(omega, 2.0f);

        // h(y) = w^2 sin(y) 
        float angle = omega * (float)Math.Sin(y);

        return new Vector2(v, - angularVelocity - angle); // change here
    }

    void OdeStep()
    {
        // delete the next line, and complete this function 
        // state_vector[0] += 0.1f; // update the state_vector (both entries) properly depending on the specified ode_method 

        if (ode_method == "euler")
        {
            Vector2 current_state = state_vector;
            state_vector = current_state + time_step_h * PendulumDynamics(current_state);
        }
        else if (ode_method == "improved-euler")
        {
			// // change here
            Vector2 current_state = state_vector;
            state_vector = current_state + time_step_h/2 * (PendulumDynamics(current_state) + PendulumDynamics(current_state + time_step_h * PendulumDynamics(current_state)));
        }
        else if (ode_method == "rk")
        {
            // Debug.Log(ode_method);
			// change here
            Vector2 current_state = state_vector;
            Vector2 k1 = time_step_h * PendulumDynamics(current_state);
            Vector2 k2 = time_step_h * PendulumDynamics(current_state + k1);
            Vector2 k3 = time_step_h * PendulumDynamics(current_state + k1/4 + k2/4);
            
            state_vector = current_state + (k1 + k2 + 4*k3)/6;
        }
        else if (ode_method == "semi-implicit")
        {
            // Debug.Log(ode_method);
			// change here
            Vector2 current_state = state_vector;
            Vector2 derivative = PendulumDynamics(current_state);
            float dv = derivative[1];

            float v_1 = current_state[1] + time_step_h * dv;

            float dy = v_1;

            float y_1 = current_state[0] + time_step_h * dy;

            state_vector = new Vector2(y_1, v_1);
         }
        else
        {
            Debug.LogError("ODE method should be one of the: euler, improved-euler, rk, semi-implicit");
        }
    }
}
