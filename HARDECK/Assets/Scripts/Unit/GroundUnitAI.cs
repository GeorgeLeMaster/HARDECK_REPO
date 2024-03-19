using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundUnitAI : UnitAIBase 
{


    // Start is called before the first frame update
    void Start()
    {
        transform.position = currentPos;
        currentHealth = maxHealth;

    }

    // Update is called once per frame
    void Update()
    {
        //SnapGFXtoGround();
    }

    override public void Spawn(Vector3Int input)
    {
        currentPos = input;
        transform.position = currentPos;

        gfx = Instantiate(unitSOtemplate.gfx, this.transform);

    }

    override public void Order(int orderID, Vector3Int desiredPos)
    {
        UnitAIBase defender = null ;
        foreach(UnitAIBase unit in GameManager.instance.AllUnits)
        {
            if (unit.currentPos == desiredPos) defender = unit;
        }

        switch (orderID)
        {
            case 0:
                Move(desiredPos);
                break;
            case 1:
                if (defender != null)
                {
                    Attack(defender);
                }
                break;
        }
    }

    override public void Move(Vector3Int desiredPos) 
    {
        movementPips -= 1;

        List<Vector3Int> pathPositions = new List<Vector3Int>();

        Flowfield ff = MapBuilder.instance.Flowfields[desiredPos.x, desiredPos.y, desiredPos.z];

        TileInfo_Class checkPos = ff.tiles[currentPos.x, currentPos.y, currentPos.z];


        while(checkPos.tilemapPosition != desiredPos)
        {
            pathPositions.Add(checkPos.tilemapPosition);
            checkPos = checkPos.nextTile;
        }
        pathPositions.Add(checkPos.tilemapPosition);
        Debug.Log(checkPos.pathCost);
        checkPos = checkPos.nextTile;
        

        StartCoroutine( Move_Coroutine(pathPositions) );

    }

    override public void Attack(UnitAIBase defender)
    {
        actionPips -= 1;

        int roll = Random.Range(1,21);
        Debug.Log(roll + ", " + CalculateHitChance(this, defender));

        bool hits = false;

        if (roll == 20)
        {
            hits = true;
            Debug.Log("Hit");

        }
        else if (roll == 1)
        {
            hits = false;
            Debug.Log("Miss");

        }
        else if (roll >= CalculateHitChance(this, defender))
        {
            hits = true;
            Debug.Log("Hit");

        }
        else
        {
            hits = false;
            Debug.Log("Miss");

        }


            GameObject fireFX = Instantiate(fireGFX_prefab);
            fireFX.transform.position = transform.position + new Vector3(0, 0.35f, 0);
            fireFX.transform.LookAt(defender.currentPos + new Vector3(0, 0.35f, 0));
            Destroy(fireFX, 2f);

            UnitAIBase attackedUnit = null;
            foreach (UnitAIBase unit in GameManager.instance.AllUnits)
            {
                if (unit == defender)
                {
                    attackedUnit = unit;
                    break;
                }
            }

        if (attackedUnit != null)
        {
            if (hits)
            {
                attackedUnit.TakeDamage(damage);
            }
            else
            {
                attackedUnit.TakeDamage(-1);
            }
        }
        
    }

    IEnumerator Move_Coroutine(List<Vector3Int> pathPositions)
    {
        Vector3Int nextPos = pathPositions.First();
        pathPositions.Remove(nextPos);
        currentPos = nextPos;

        while (nextPos != null)
        {
            Vector3 moveVec = nextPos - transform.position;
            moveVec.Normalize();
            transform.position += (moveVec*Time.deltaTime*3);

            if (Vector3.Distance(transform.position, nextPos) < 0.1f)
            {
                if (pathPositions.Count > 0)
                {
                    nextPos = pathPositions.First();
                    pathPositions.Remove(nextPos);
                    currentPos = nextPos;
                }
                else
                {
                    break;
                }
            }
            yield return new WaitForEndOfFrame();

        }

    }

    static public int CalculateHitChance(UnitAIBase attacker, UnitAIBase defender)
    {
        int result = 5;

        // DnD style roll, 20 always succseeding, 1 always missing

        if (attacker.currentPos.y > defender.currentPos.y)
        {
            result -= 2;
        }
        else if (attacker.currentPos.y == defender.currentPos.y)
        {
            result += 2;

        }
        else
        {
            result += 6;
        }

        return result;
    }
    
}
