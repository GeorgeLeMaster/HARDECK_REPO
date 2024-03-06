using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundUnitAI : UnitAIBase 
{

    List<Vector3Int> path;
    bool acting;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void Spawn(Vector3Int input)
    {
        currentPos = input;
        transform.position = currentPos;

        gfx = Instantiate(unitSOtemplate.gfx, this.transform);

    }

    public void Order(int orderID, Vector3Int desiredPos)
    {
        switch (orderID)
        {
            case 0:
                Move(desiredPos);
                break;
        }
    }

    private void Move(Vector3Int desiredPos) 
    {


        List<Vector3Int> pathPositions = new List<Vector3Int>();

        Flowfield ff = MapBuilder.instance.Flowfields[desiredPos.x, desiredPos.y, desiredPos.z];

        TileInfo_Class checkPos = ff.tiles[currentPos.x, currentPos.y, currentPos.z];
        

        while(checkPos.tilemapPosition != desiredPos)
        {
            pathPositions.Add(checkPos.tilemapPosition);
            checkPos = checkPos.nextTile;
        }
        pathPositions.Add(checkPos.tilemapPosition);
        checkPos = checkPos.nextTile;
        StartCoroutine( Move_Coroutine(pathPositions) );
        acting = true;

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

            if (Vector3.Distance(transform.position, nextPos) < 0.01f)
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
}
