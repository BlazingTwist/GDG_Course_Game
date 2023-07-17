using UnityEngine;
using UnityEngine.Tilemaps;


public class WallController : MonoBehaviour
{
    public TileBase visibleWallTile;
    public TileBase invisibleWallTile;
    public Tilemap tilemap;

    private bool wallVisible = true;

    // Aufruf dieser Methode, um die Wand zu entfernen
    public void RemoveWall()
    {
        if (wallVisible)
        {
            tilemap.SwapTile(visibleWallTile, invisibleWallTile);
            wallVisible = false;
        }else{
            tilemap.SwapTile(invisibleWallTile,visibleWallTile);
            wallVisible = true;
        }
    }
}
