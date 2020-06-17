using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of drug
// animates color, and triggers a variable if the player reaches the drug 
public class Drug : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;

    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
    }

    void Update()
    {
        Color greenness = new Color
        {
            g = Mathf.Max(1.0f, 0.1f + Mathf.Abs(Mathf.Sin(Time.time)))
        };
        GetComponent<MeshRenderer>().material.color = greenness;
    }

    private void OnCollisionEnter(Collision collision)
    {
        level.source.PlayOneShot(level.drug_clip);
        if (collision.gameObject.name == "PLAYER")
        {
            level.drug_landed_on_player_recently = true;
            Destroy(gameObject);
        }
    }
}