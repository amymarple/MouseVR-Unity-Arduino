using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Looming Stimulus Control Script
 * 
 * Simulates an approaching predator (overhead threat) using physics-based
 * player tracking and interception algorithms. The stimulus spawns at a
 * distance and approaches the player with optimal heading calculation.
 * 
 * Scientific Context:
 * - Models innate defensive responses to looming threats
 * - Based on classic looming stimulus paradigms (Schiff et al., 1962)
 * - Used to study predator avoidance in rodents (De Franceschi et al., 2016)
 * 
 * Key Features:
 * - Randomized spawn position within elevation/azimuth ranges
 * - Physics-based interception (accounts for player velocity)
 * - Triggers air puff punishment on collision
 * - Automatically deactivates after contact
 */

public class LoomingStimulusControlScript : MonoBehaviour 
{
    public GameObject player;
    public float startDistance = 40f;
    public float speed = 1f;
    public Vector2 ThRange = new Vector2(25.0f, 30.0f);  // Elevation angle range (degrees)
    public Vector2 PhRange = new Vector2(-35.0f, 35.0f); // Azimuth angle range (degrees)

    float toRad = (2.0f * Mathf.PI)/360.0f;

    public void activate()
    {
        // Calculate random spawn position in spherical coordinates
        float Th = Random.Range(ThRange.x, ThRange.y) * toRad;
        float Ph = Random.Range(PhRange.x, PhRange.y) * toRad;
        
        // Convert to Cartesian coordinates
        Vector3 v = new Vector3(
            Mathf.Sin(Ph) * Mathf.Cos(Th), 
            Mathf.Cos(Ph) * Mathf.Cos(Th), 
            Mathf.Sin(Th)
        );
        
        gameObject.transform.position = player.transform.position + v * startDistance;
        gameObject.SetActive(true);
    }

    public void deactivate()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Physics-based interception algorithm
        // Computes optimal heading to chase moving player
        
        // Direction to player
        Vector3 D = player.transform.position - gameObject.transform.position;
        D.Normalize();
        
        // Player velocity
        Vector3 v = player.GetComponent<CharacterController>().velocity;
        float s_player = v.magnitude;
        s_player = Mathf.Min(s_player, speed);
        v.Normalize();
        
        // Calculate perpendicular velocity component
        Vector3 v_perp = v - Vector3.Dot(v, D) * D;
        float sin_ph = v_perp.magnitude;
        float sin_th = sin_ph * s_player / speed;
        v_perp.Normalize();

        // Optimal interception velocity
        Vector3 w = (Mathf.Sqrt(1 - sin_th * sin_th) * D + sin_th * v) * speed;

        gameObject.transform.position += w * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider c)
    {
        if(c.gameObject == player)
        {
            // Deliver punishment on collision
            player.GetComponent<PlayerController>().deliverAirpuff();
            deactivate();
        } 
        else
        {
            Debug.Log(c.gameObject); 
            deactivate();
        }
    }
}
