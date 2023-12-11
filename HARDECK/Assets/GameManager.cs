using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Map Data")]
    public TileInfo[,,] builtMap_TileInfo;


    [Header("Starting Units")]
    public List<GroundUnitAI> Units;
    private void Awake()
    {
        if (instance != null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SupplyMapInfo(int width, int length, int height)
    {
        builtMap_TileInfo = new TileInfo[width,length,height];
    }
}
