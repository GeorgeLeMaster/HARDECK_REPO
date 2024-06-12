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

    public int ownerAlliance;

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

        switch (ownerAlliance)
        {
            case 0:
                lr.startColor = GameManager.instance.commanderColor_0;
                lr.endColor = GameManager.instance.commanderColor_0;
                break;
            case 1:
                lr.startColor = GameManager.instance.commanderColor_1;
                lr.endColor = GameManager.instance.commanderColor_1;
                break;
            default:
                lr.startColor = Color.white;
                lr.endColor = Color.white;
                break;
        }
    }

    public void CheckOwnership()
    {
        List<int> capAlliances = new List<int>();

        foreach (Commander c in GameManager.instance.commanders)
        {
            foreach (UnitAIBase u in c.activeUnits)
            {
                if (CheckInBorder(u) == true && !capAlliances.Contains(u.alliance))
                {
                    capAlliances.Add(u.alliance);
                }
            }
        }

        // Check Alliances
        // Must be one lone alliance for something to change
        if (capAlliances.Count == 1)
        {
            if (ownerAlliance != capAlliances[0])
            {
                if (ownerAlliance == 10)
                {
                    // give to cap alliance
                    GameManager.instance.commanders[capAlliances[0]].ownedStructures.Add(this);
                    ownerAlliance = capAlliances[0];
                }
                else
                {
                    // take from enemy
                    GameManager.instance.commanders[ownerAlliance].ownedStructures.Remove(this);
                    ownerAlliance = 10;
                }
            }
        }



        LineRenderer lr = GetComponent<LineRenderer>();

        switch (ownerAlliance)
        {
            case 0:
                lr.startColor = GameManager.instance.commanderColor_0;
                lr.endColor = GameManager.instance.commanderColor_0;
                break;
            case 1:
                lr.startColor = GameManager.instance.commanderColor_1;
                lr.endColor = GameManager.instance.commanderColor_1;
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


        LineRenderer lr = GetComponent<LineRenderer>();
        lr.enabled = false;

        buildingGfx.SetActive(false);

        hideGFX_h.SetActive(true);



    }

    private bool CheckInBorder(UnitAIBase uInput)
    {
        bool result = false;

        if (Mathf.Abs(uInput.currentPos.x - tilemapPos.x) <= WidthHeight.x && Mathf.Abs(uInput.currentPos.z - tilemapPos.z) <= WidthHeight.y && uInput.currentPos.y == tilemapPos.y)
        {
            result = true;
        }

        return result;
    }
}
