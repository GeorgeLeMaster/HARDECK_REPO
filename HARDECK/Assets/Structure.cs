using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public Vector2Int WidthHeight;

    public Vector3Int tilemapPos;

    public bool provideGround;
    public bool provideHeavy;
    public bool provideAir;

    public int ownerId;

    private void Start()
    {
        LineRenderer lr = GetComponent<LineRenderer>();

        lr.SetPosition(0, new Vector3( WidthHeight.x + 0.5f, 0.1f,  WidthHeight.y + 0.5f));
        lr.SetPosition(1, new Vector3(-WidthHeight.x - 0.5f, 0.1f,  WidthHeight.y + 0.5f));
        lr.SetPosition(2, new Vector3(-WidthHeight.x - 0.5f, 0.1f, -WidthHeight.y - 0.5f));
        lr.SetPosition(3, new Vector3( WidthHeight.x + 0.5f, 0.1f, -WidthHeight.y - 0.5f));
        lr.SetPosition(4, new Vector3( WidthHeight.x + 0.5f, 0.1f,  WidthHeight.y + 0.5f));

        switch (ownerId)
        {
            case 0:
                lr.startColor = GameManager.instance.playerColor;
                lr.endColor = GameManager.instance.playerColor;
                break;
            case 1:
                lr.startColor = GameManager.instance.enemyColor;
                lr.endColor = GameManager.instance.enemyColor;
                break;
            default:
                lr.startColor = Color.white;
                lr.endColor = Color.white;
                break;
        }
    }

    public void CheckOwnership()
    {
        for (int x = -WidthHeight.x; x < WidthHeight.x + 1; x++)
        {
            for (int z = -WidthHeight.y; z < WidthHeight.y + 1; z++)
            {
                TileInfo newTile = MapBuilder.instance.Tiles[tilemapPos.x + x, tilemapPos.y, tilemapPos.z + z];


            }
        }
    }


}
