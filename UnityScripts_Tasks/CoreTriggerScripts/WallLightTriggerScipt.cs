using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallLightTriggerScipt : MonoBehaviour
{
    public GameObject player;

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            player.GetComponent<PlayerController>().deliverAirpuff();
        }
    }
}
