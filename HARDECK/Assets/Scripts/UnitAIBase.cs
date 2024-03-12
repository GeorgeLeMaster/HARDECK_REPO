using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAIBase : MonoBehaviour
{

    public Vector3Int currentPos;
    public GameObject gfx;

    public UnitSO unitSOtemplate;

    public UnitSO instanceSO;

    public int alliance;

    public string unitName;

    public float currentHealth;
    public float maxHealth;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    virtual public void Spawn(Vector3Int input) { }

    virtual public void Order(int orderID, Vector3Int desiredPos) { }

    virtual public void Move(Vector3Int desiredPos) { }

    public void SnapGFXtoGround()
    {
        LayerMask mask = LayerMask.GetMask("GFXEnvironment");


        Vector3 castPos = transform.position + new Vector3(0,2,0);
        RaycastHit hit;
        if (Physics.Raycast(castPos, Vector3.down, out hit, 10f, mask))
        {
            gfx.transform.position = hit.transform.position;
        }
    }

}
