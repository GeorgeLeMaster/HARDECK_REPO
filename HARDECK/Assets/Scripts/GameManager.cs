using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject arrowPrefab;
    public GameObject arrowParent;

    [Header("Map Data")]
    public TileInfo[,,] builtMap_TileInfo;


    [Header("Starting Units")]
    public List<GroundUnitAI> Units;
    private void Awake()
    {

        instance = this;

    }

    [Header("Game Components")]
    public int currentTurn = 1;

    // Start is called before the first frame update
    void Start()
    {
        PlayerManager.instance.StartTurn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SupplyMapInfo(int width, int length, int height)
    {
        builtMap_TileInfo = new TileInfo[width,length,height];
    }


    public void EndPlayerTurn()
    {
        UpdateTurnTimers();

        currentTurn += 1;
    }

    void UpdateTurnTimers()
    {

    }

    public void SpawnUnit(int unitID, Vector3Int pos)
    {

        GameObject unitType;
        UnitSO unitSO;

        switch (unitID)
        {
            case -1:
                unitType = Catalouge.instance.groundUnitObj;
                unitSO = Catalouge.instance.debugGroundUnit_SO;
                break;

            default:

                unitType = Catalouge.instance.groundUnitObj;
                unitSO = Catalouge.instance.debugGroundUnit_SO;
                break;
        }

        GameObject newUnit;
        newUnit = Instantiate(unitType);
        newUnit.GetComponent<UnitAIBase>().unitSO = unitSO;
        newUnit.GetComponent<UnitAIBase>().Spawn(pos);

    }
}
