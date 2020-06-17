using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of virus
// animates color and size of the virus
// and attacks the player if the player is near the virus (check the code)
public class Virus : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    private float radius_of_search_for_player;
    private float virus_speed;

	void Start ()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
        Bounds bounds = level.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 10.0f;
        virus_speed = level.virus_speed;
    }

    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION TO ANIMATE THE VIRUS ***
    // so that it moves towards the player when the player is within radius_of_search_for_player
    // a simple strategy is to update the position of the virus
    // so that it moves towards the direction (fps_player_obj.transform.position - transform.position)
    // with rate of change (virus_speed * Time.deltaTime)
    void Update()
    {
        if (level.player_health < 0.001f || level.player_entered_house)
            return;
        Color redness = new Color
        {
            r = Mathf.Max(1.0f, 0.25f + Mathf.Abs(Mathf.Sin(2.0f * Time.time)))
        };
        if ( transform.childCount > 0)
            transform.GetChild(0).GetComponent<MeshRenderer>().material.color = redness;
        else
            transform.GetComponent<MeshRenderer>().material.color = redness;
        transform.localScale = new Vector3(
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time))
                               );
        /*** implement the rest ! */
        
        if(fps_player_obj==null)
            fps_player_obj = level.fps_player_obj;

        if ((fps_player_obj.transform.position - transform.position).magnitude<=radius_of_search_for_player){
            // var distance = (fps_player_obj.transform.position - transform.position).magnitude;
            Vector3 direction = (fps_player_obj.transform.position - transform.position).normalized;
            transform.position =  transform.position + direction * virus_speed * Time.deltaTime / 10.0f;
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER")
        {
            // Play Audio Clip
            level.source.PlayOneShot(level.cough_clip);
            if (!level.virus_landed_on_player_recently)
                level.timestamp_virus_landed = Time.time;
            level.num_virus_hit_concurrently++;
            level.virus_landed_on_player_recently = true;
            Destroy(gameObject);
        }
    }
    
}
