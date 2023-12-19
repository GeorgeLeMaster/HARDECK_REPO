using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catalouge : MonoBehaviour
{
    public static Catalouge instance;

    public UnitSO[] deck_unitSOs;

    public GameObject groundUnitObj;

    public UnitSO debugGroundUnit_SO;
    public UnitSO debugArmouredUnit_SO;
    public UnitSO debugAirUnit_SO;

    private void Awake()
    {

        instance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
