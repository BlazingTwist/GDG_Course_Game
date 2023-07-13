using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRepeater : MonoBehaviour
{
    public Tilemap tilemap;
    public float repeatInterval = 10f; // Die Distanz, nach der die Tilemap wiederholt wird
    private Transform player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        // Überprüfe, ob der Spieler die Tilemap in eine Wiederholungszone betreten hat
        if (Mathf.Abs(player.position.x - transform.position.x) > repeatInterval)
        {
            // Verschiebe die Tilemap um das Wiederholungsintervall
            Vector3 offset = new Vector3(repeatInterval * Mathf.Sign(player.position.x - transform.position.x), 0f, 0f);
            transform.position += offset;

            // Kopiere die Kollisionen der Tilemap in die verschobene Position
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] tiles = tilemap.GetTilesBlock(bounds);
            tilemap.SetTilesBlock(bounds, tiles);
        }
    }
}
