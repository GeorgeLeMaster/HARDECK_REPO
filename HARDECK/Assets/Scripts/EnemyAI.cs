using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    private float cooldown;

    private List<UnitAIBase> units;
    private List<UnitAIBase> unusedUnits;

    private bool myTurn;

    private int currentUnitID;

    public Vector3Int assaultPosition;

    // Start is called before the first frame update
    void Start()
    {
        myTurn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (myTurn)
        {
            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
            }
            else
            {
                if (currentUnitID-1 < unusedUnits.Count())
                {
                    NextAction();
                }
                else
                {
                    //Debug.Log("Enemy turn end");

                    EndTurn();
                }
            }
        }
    }

    public void BeginTurn(int commanderToUse)
    {
        cooldown = 2f;

        currentUnitID = 0;

        units = GameManager.instance.commanders[commanderToUse].activeUnits;
        unusedUnits = units;

        myTurn = true;
        //Debug.Log("Enemy turn start");

    }

    private void EndTurn()
    {
        myTurn = false;
        GameManager.instance.StartNewTurn();
    }

    private void NextAction()
    {
        if (currentUnitID > unusedUnits.Count()-1)
        {
            currentUnitID++;
           // EndTurn();

            return;
        }
        while (unusedUnits[currentUnitID] == null)
        {
            currentUnitID++;
        }
        // Grab new unit
        UnitAIBase unit = unusedUnits[currentUnitID];

        // Attack Logic
        List<UnitAIBase> playerUnitsInRange = new List<UnitAIBase>();

        // find all in range enemy(player) units
        foreach (UnitAIBase playerUnit in GameManager.instance.commanders[GameManager.instance.playerAllianceInt].activeUnits)
        {
            if (Vector3.Distance(unit.currentPos, playerUnit.currentPos) < unit.visionRadius)
            {
                playerUnitsInRange.Add(playerUnit);
            }
        }

        // find closest in range unit
        float closestRange = unit.visionRadius;
        UnitAIBase closestUnit = null;
        foreach (UnitAIBase playerUnit in playerUnitsInRange)
        {
            if (Vector3.Distance(playerUnit.currentPos, unit.currentPos) < closestRange && playerUnit != null)
            {
                closestUnit = playerUnit;
                closestRange = Vector3.Distance(playerUnit.currentPos, unit.currentPos);
            }
        }

        if (closestUnit != null)
        {
            unit.Attack(closestUnit);
        }

        // Movement Logic


        //// get path to unit position
        //List<Vector3> path = MapBuilder.instance.GetPath(assaultPosition, unit.currentPos);

        //Vector3 destination = assaultPosition;

        //float cost = 0;
        //// iterate through path until max move is found
        //for (int i = 0; i < path.Count; i++)
        //{
        //    cost += 2;
        //    if (cost >= unit.moveSpeed)
        //    {
        //        destination = path[i];
        //        break;
        //    }
        //}

        //unit.Move(new Vector3Int((int)destination.x, (int)destination.y, (int)destination.z));
        //unit.movementPips -= 1;

        currentUnitID++;


        cooldown = 1f;
    }
}
