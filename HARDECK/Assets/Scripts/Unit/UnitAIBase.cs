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

    public int actionPips;
    public int movementPips;

    public float damage;
    public float range;

    public GameObject fireGFX_prefab;

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

    virtual public void Attack(Vector3Int desiredPos) { }

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

    public void TakeDamage(float input)
    {
        currentHealth -= input;

        Debug.Log(gameObject.name + $" took {input} damage");
        if (currentHealth <= 0)
        {
            if (GameManager.instance.selectedUnit_Player == this)
            {
                GameManager.instance.selectedUnit_Player = null;
            }
            else if(GameManager.instance.selectedUnit_Enemy == this)
            {
                GameManager.instance.selectedUnit_Enemy = null;
            }
            Destroy(gameObject);
            
        }
        GameManager.instance.UpdatePlayerActionGFX();

    }

}
