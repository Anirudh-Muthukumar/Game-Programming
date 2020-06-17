using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of the house at the exit of the maze
// it simply has a trigger when player arrived in the house
public class House : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    // public AudioSource main_cam_audio_src;

    // Use this for initialization
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

    private void OnTriggerEnter(Collider other)
    {
        // level.source.PlayOneShot(level.success_clip);
        if (other.gameObject.name == "PLAYER")
        {   
            // Play audio clip
            level.source.PlayOneShot(level.success_clip);
            level.player_entered_house = true;
        }

    }
}