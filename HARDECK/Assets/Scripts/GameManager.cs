using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject arrowPrefab;
    public GameObject arrowParent;


    [Header("Starting Units")]
    public List<GroundUnitAI> Units;
    private void Awake()
    {
        instance = this;
    }

    [Header("Game Components")]
    public int currentTurn = 1;

    public GroundUnitAI testUnit;

    // Start is called before the first frame update
    void Start()
    {
        MapBuilder.instance.LoadMapFromFile();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //DrawFFArrowField();

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                if (hit.transform.GetComponent<TileInfo>())
                {
                    testUnit.Order(0, hit.transform.GetComponent<TileInfo>().tilemapPosition);
                }
            }
        }
    }

    void DrawFFArrowField()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        {
            if (hit.transform.GetComponent<TileInfo>())
            {
                Vector3Int clickedPos = hit.transform.GetComponent<TileInfo>().tilemapPosition;

                Debug.Log(clickedPos);
                MapBuilder.instance.DrawFlowfield(MapBuilder.instance.Flowfields[clickedPos.x, clickedPos.y, clickedPos.z]);
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }
    }


}
