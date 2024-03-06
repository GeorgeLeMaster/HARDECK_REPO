using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAIBase : MonoBehaviour
{

    public Vector3Int currentPos;
    public GameObject gfx;

    public UnitSO unitSOtemplate;

    public UnitSO instanceSO;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    virtual public void Spawn(Vector3Int input) { }
}
