using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundUnitAI : UnitAIBase 
{


    // Start is called before the first frame update
    void Start()
    {
        //Spawn(currentPos,unitSO,alliance);

    }

    // Update is called once per frame
    void Update()
    {

    }

    override public void Spawn(Vector3Int input, GameObject gfxInput)
    {
        // Snap To Position
        currentPos = input;
        transform.position = currentPos;



        GameObject gfxobj = Instantiate(gfxInput, gfx.transform);
        UnitGFXContainer gfxc = gfxobj.GetComponent<UnitGFXContainer>();
        if (Application.isPlaying)
        {
            if (alliance == 0)
            {
                gfxc.gfx.GetComponent<Renderer>().material.SetColor("_AllianceColor", GameManager.instance.commanderColor_0);
            }
            else
            {
                gfxc.gfx.GetComponent<Renderer>().material.SetColor("_AllianceColor", GameManager.instance.commanderColor_1);
            }
        }

        animator = gfxc.animator;

        //Material mat = gfxobj.GetComponentInChildren<MeshRenderer>().material;
        //mat.SetColor("AllianceColor", new Color(0,0,0,1));

        actionPips = 1;
        movementPips = 1;

        Rigidbody[] childRbs = gfx.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in childRbs)
        {
            rb.isKinematic = true;
        }
    }

    override public void Move(Vector3Int desiredPos) 
    {
  

        List<Vector3Int> pathPositions = new List<Vector3Int>();

        Flowfield ff = MapBuilder.instance.Flowfields[desiredPos.x, desiredPos.y, desiredPos.z];

        TileInfo_Class checkPos = ff.tiles[currentPos.x, currentPos.y, currentPos.z];

        //Debug.Log(checkPos.pathCost);


        if (checkPos.pathCost <= moveSpeed)
        {

            while (checkPos.tilemapPosition != desiredPos)
            {
                pathPositions.Add(checkPos.tilemapPosition);
                checkPos = checkPos.nextTile;
            }
            pathPositions.Add(checkPos.tilemapPosition);
            //Debug.Log(checkPos.pathCost);
            checkPos = checkPos.nextTile;


            StartCoroutine(Move_Coroutine(pathPositions));
        }
    }

    override public void Attack(UnitAIBase defender)
    {
        StartCoroutine(Attack_Coroutine(defender));

    }

    IEnumerator Move_Coroutine(List<Vector3Int> pathPositions)
    {
        float gfxMoveSpeed = 2.75f;
        GameManager.instance.controllsLocked = true;
        Vector3Int nextPos = pathPositions.First();
        pathPositions.Remove(nextPos);
        currentPos = nextPos;

        animator.SetTrigger("Move");


        while (nextPos != null)
        {
            Vector3 moveVec = nextPos - transform.position;
            moveVec.Normalize();
            transform.position += (moveVec*Time.deltaTime* gfxMoveSpeed);

            if (Vector3.Distance(transform.position, nextPos) < 0.1f)
            {
                if (pathPositions.Count > 0)
                {
                    nextPos = pathPositions.First();
                    pathPositions.Remove(nextPos);
                    currentPos = nextPos;
                    gfx.transform.LookAt(new Vector3(nextPos.x, gfx.transform.position.y, nextPos.z));
                    GameManager.instance.commanders[this.alliance].visableTiles.Clear();
                    MapBuilder.instance.UpdateFOW(this);
                    GameManager.instance.UpdateVisableFOW();


                }
                else
                {
                    break;
                }
            }
            yield return new WaitForEndOfFrame();  

        }
        GameManager.instance.controllsLocked = false;
        animator.SetTrigger("Idle");


    }

    IEnumerator Attack_Coroutine(UnitAIBase defender)
    {
        UnitAIBase attackedUnit = defender;

        gfx.transform.LookAt(new Vector3(attackedUnit.currentPos.x, gfx.transform.position.y, attackedUnit.currentPos.z));

        animator.SetTrigger("QueueAttack");
        int roll = Random.Range(1, 21);
        //Debug.Log(roll + ", " + CalculateHitChance(this, defender));

        bool hits = false;

        if (roll == 20)
        {
            hits = true;

        }
        else if (roll == 1)
        {
            hits = false;

        }
        else if (roll >= CalculateHitChance(this, defender))
        {
            hits = true;

        }
        else
        {
            hits = false;

        }



        yield return new WaitForSeconds(1f);
        animator.SetTrigger("CommitAttack");

        GameObject fireFX = Instantiate(fireGFX_prefab);
        fireFX.transform.position = transform.position + new Vector3(0, 0.5f, 0);
        fireFX.transform.LookAt(defender.currentPos + new Vector3(0, 0.35f, 0));
        Destroy(fireFX, 2f);

        //foreach (UnitAIBase unit in GameManager.instance.AllUnits)
        //{
        //    if (unit == defender)
        //    {
        //        attackedUnit = unit;
        //        break;
        //    }
        //}


        if (attackedUnit != null)
        {
            if (hits)
            {
                attackedUnit.TakeDamage(damage + (int)Mathf.Floor(Random.Range(-damageMod, damageMod + 0.99f)), this.transform.position);
            }
            else
            {
                attackedUnit.TakeDamage(-1, this.transform.position);
            }
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
