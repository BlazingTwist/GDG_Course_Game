using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
     public Transform player;
     public float teleportX;
     public float teleportY;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Teleportiere den Player
            TeleportPlayer();
        }
    }
    private void TeleportPlayer()
    {

        player.position = new Vector3(teleportX, teleportY, 0f);
    }

}
