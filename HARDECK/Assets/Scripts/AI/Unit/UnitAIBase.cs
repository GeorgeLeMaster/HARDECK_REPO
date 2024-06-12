using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitAIBase : MonoBehaviour
{

    public Vector3Int currentPos;
    public GameObject gfx;

    public UnitSO unitSO;

    public int alliance;

    public string unitName;

    public float currentHealth;
    public float maxHealth;

    public int actionPips;
    public int movementPips;

    public int moveSpeed;
    public float range;
    public float damage;
    public float damageMod;
    public float visionRadius;


    // ABILITY FLAGS
    [HideInInspector]
    public bool f_dugIn;  

    [HideInInspector]
    public List<AbilityObj> activeAbilities;

    public List<int> abilityInts;

    public GameObject fireGFX_prefab;

    public Animator animator;

    public GameObject minimapPip;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UseAbility(int input, UnitAIBase castingUnit = null, UnitAIBase targetedUnit = null, Vector3Int targetedPosition = default(Vector3Int))
    {

        if (activeAbilities == null)
        {
            activeAbilities = new List<AbilityObj>();
        }

        AbilityObj newAbilityObj = new AbilityObj();
        newAbilityObj.turnsRemaining = Abilities.abilityDescs[input].maxTurnsRemaining;
       // Debug.Log(activeAbilities);
        activeAbilities.Add(newAbilityObj);
        Abilities.UseAbility(input, castingUnit, targetedUnit, targetedPosition);
    }

    virtual public void Spawn(Vector3Int input, GameObject gfxInput) { }

    virtual public void Move(Vector3Int desiredPos) { }

    virtual public void Attack(UnitAIBase defender) { }

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

    public void TakeDamage(float input, Vector3 damageSourcePos)
    {

        GameObject obj = Instantiate(GameManager.instance.worldTextPopupPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        TextMeshProUGUI txt = obj.GetComponentsInChildren<TextMeshProUGUI>()[0];
        if (input <= 0)
        {
            txt.text = "MISS";
            txt.color = Color.white;
            return;
        }
        else
        {
            animator.SetTrigger("TakeDamage");
            txt.text = input.ToString();
            txt.color = Color.red;
        }

        currentHealth -= input;

        //Debug.Log(gameObject.name + $" took {input} damage");
        if (currentHealth <= 0)
        {
            // Disable Minimap Pip
            minimapPip.SetActive(false);

            Rigidbody[] childRbs = gfx.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody rb in childRbs)
            {
                rb.isKinematic = false;
            }

            // Add death force [CHANGE: MAKE DEATH SPECIFIC]
            int limb =  Random.Range(0, 1);
            Vector3 dir = childRbs[limb].transform.position - damageSourcePos;
            childRbs[limb].AddForce(dir.normalized * Random.Range(250, 700) * childRbs[limb].mass);
            childRbs[limb].AddForce(Vector3.up * Random.Range(-200,250) * childRbs[limb].mass);
            childRbs[limb].AddTorque(Vector3.up * Random.Range(-2500, 2500));

            // Remove from commanders unit list and update FOW

            this.GetComponent<GFXAnchor>().enabled = false;

            GameManager.instance.commanders[alliance].activeUnits.Remove(this);
            if (alliance == GameManager.instance.playerAllianceInt)
            {
                GameManager.instance.RefreshFOW();
            }
            // Turn off animator, logic, and colliders
            animator.enabled = false;
            foreach (Collider c in this.GetComponents<Collider>())
            {
                c.enabled = false;
            }
            this.enabled = false;
        }

        GameManager.instance.UpdatePlayerActionGFX();

    }

    

}
