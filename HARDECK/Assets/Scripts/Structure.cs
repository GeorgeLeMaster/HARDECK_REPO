using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public Vector2Int WidthHeight;

    public Vector3Int tilemapPos;

    public bool provideGround;
    public bool provideHeavy;
    public bool provideAir;

    public int ownerId;

    public bool playerHq;

    public GameObject buildingGfx;
    public GameObject hideGFX_h;
    public GameObject hideGFX_d;


    bool discovered = false;

    private void Start()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        float yOff = 0.05f;
        lr.SetPosition(0, new Vector3( WidthHeight.x + 0.5f, yOff,  WidthHeight.y + 0.5f));
        lr.SetPosition(1, new Vector3(-WidthHeight.x - 0.5f, yOff,  WidthHeight.y + 0.5f));
        lr.SetPosition(2, new Vector3(-WidthHeight.x - 0.5f, yOff, -WidthHeight.y - 0.5f));
        lr.SetPosition(3, new Vector3( WidthHeight.x + 0.5f, yOff, -WidthHeight.y - 0.5f));
        lr.SetPosition(4, new Vector3( WidthHeight.x + 0.5f, yOff,  WidthHeight.y + 0.5f));

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
        LayerMask mask = LayerMask.GetMask("Unit");

        Collider[] c = Physics.OverlapBox(transform.position, new Vector3(WidthHeight.x, 1, WidthHeight.y), Quaternion.identity, mask); 
        bool playerUnit = false;
        bool enemyUnit = false;
        
        foreach(Collider u in c)
        {
            UnitAIBase unit = u.GetComponent<UnitAIBase>();

            if (unit.alliance == 0)
            {
                playerUnit = true;
            }
            else
            {
                enemyUnit = true;
            }

            if (playerUnit && enemyUnit)
            {
                break;
            }
        }

        if (ownerId == -1)
        {
            // if neutral
            if (playerUnit && !enemyUnit)
            {
                ownerId = 0;
            }
            else if (enemyUnit && !playerUnit)
            {
                ownerId = 1;
            }
        }
        else if (ownerId == 0)
        {
            // owned by player
            if (enemyUnit && !playerUnit)
            {
                ownerId = -1;
            }
        }
        else if (ownerId == 1)
        {
            // owned by enemy
            if (!enemyUnit && playerUnit)
            {
                ownerId = -1;
            }
        }

        LineRenderer lr = GetComponent<LineRenderer>();

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

    public void ShowGFX()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.enabled = true;
        discovered = true;

        buildingGfx.SetActive(true);

        hideGFX_h.SetActive(false);
    }

    public void HideGFX()
    {

        if (!discovered)
        {
            LineRenderer lr = GetComponent<LineRenderer>();
            lr.enabled = false;

            buildingGfx.SetActive(false);

            hideGFX_h.SetActive(true);

        }

    }
}
