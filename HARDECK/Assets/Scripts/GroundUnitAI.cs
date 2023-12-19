using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundUnitAI : UnitAIBase 
{

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
        mapCoords = input;
        transform.position = mapCoords;

        gfx = Instantiate(unitSO.gfx, this.transform);


    }
}
