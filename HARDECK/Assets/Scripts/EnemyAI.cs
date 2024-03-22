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

    public void BeginTurn()
    {
        cooldown = 2f;

        currentUnitID = 0;

        units = GameManager.instance.EnemyUnits;
        unusedUnits = units;

        myTurn = true;
        //Debug.Log("Enemy turn start");

    }

    private void EndTurn()
    {
        myTurn = false;
        GameManager.instance.StartPlayerTurn();
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
        UnitAIBase unit = unusedUnits[currentUnitID];

        List<UnitAIBase> playerUnitsInRange = new List<UnitAIBase>();

        // find all in range enemy(player) units
        foreach (UnitAIBase playerUnit in GameManager.instance.PlayerUnits)
        {
            if (Vector3.Distance(unit.currentPos, playerUnit.currentPos) < unit.range)
            {
                playerUnitsInRange.Add(playerUnit);
            }
        }

        // find closest in range unit
        float closestRange = unit.range;
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

        currentUnitID++;


        cooldown = 1f;
    }
}
