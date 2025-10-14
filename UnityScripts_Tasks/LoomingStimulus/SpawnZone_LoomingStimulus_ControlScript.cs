using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Spawn Zone - Looming Stimulus Control Script
 * 
 * Manages when and where looming stimuli appear during experiments.
 * Implements habituation period (minimum safe crossings) before threat
 * introduction, with probabilistic spawning.
 * 
 * Experimental Design:
 * - Allows initial safe trials for habituation
 * - Probabilistic spawning prevents predictability
 * - Single stimulus at a time (prevents overwhelming)
 * - Resets spawn eligibility on zone exit
 */

public class SpawnZone_LoomingStimulus_ControlScript : MonoBehaviour
{
    public GameObject player;
    public GameObject loomingStimulus;
    public float spawnChance = 0.01f;        // Probability per second
    public bool loom_already_spawned = false;
    public int min_safe_crossings = 2;        // Habituation period (trials)

    void OnTriggerStay(Collider other)
    {
        int crossings = player.GetComponent<PlayerController>().number_of_teleports;

        if (other.gameObject == player && !loom_already_spawned && crossings > min_safe_crossings)
        {
            // Probabilistic spawning (only if not already active)
            if(Random.Range(0.0f, 1.0f) < spawnChance * Time.deltaTime && !loomingStimulus.activeSelf)
            {
                loom_already_spawned = true;
                loomingStimulus.GetComponent<LoomingStimulusControlScript>().activate();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            // Reset spawn eligibility when player leaves zone
            loom_already_spawned = false;
            loomingStimulus.GetComponent<LoomingStimulusControlScript>().deactivate();
        }
    }
}
