using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    public GameObject player;
    public Vector3 teleportVector;
    public GameObject loomingStimulus;

    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject == player)
        {
            player.GetComponent<PlayerController>().teleport(teleportVector);
            loomingStimulus.SetActive(false);
        }
    }
}
