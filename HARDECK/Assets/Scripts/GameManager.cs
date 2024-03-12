using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject arrowPrefab;
    public GameObject arrowParent;

    public int playerAllianceInt;

    [Header("UI")]
    [Header("Player Interface")]
    public GameObject commitActionButton;
    public TextMeshProUGUI commitActionText;

    public GameObject noUnitSelectedText;

    public GameObject selectedUnitName_Obj;
    public TextMeshProUGUI selectedUnitName_Text;

    public GameObject selectedUnit_hpBar;
    public Image selectedUnit_hpBarFillImage; 

    public LineRenderer moveLine;
    public GameObject selectedUnitIndicator;

    public GameObject unitPortrait;

    [Header("Starting Units")]
    public List<UnitAIBase> Units;
    private void Awake()
    {
        instance = this;
    }

    [Header("Game Components")]
    public int currentTurn = 1;

    public bool controllsLocked;

    public UnitAIBase selectedUnit;

    // Queued action info
    private int q_actionId;
    private UnitAIBase q_actingUnit;
    private Vector3Int q_actionPos;

    // Start is called before the first frame update
    void Start()
    {
        MapBuilder.instance.LoadMapFromFile();
        ResetActionQueue();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            //DrawFFArrowField();

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                if (hit.transform.GetComponent<TileInfo>() && selectedUnit != null)
                {
                    // Queue up move command for selected unit
                    q_actionId = 0;
                    q_actingUnit = selectedUnit;
                    q_actionPos = hit.transform.GetComponent<TileInfo>().tilemapPosition;
                }
                else if (hit.transform.GetComponent<UnitAIBase>())
                {
                    // Cases where a unit is clicked
                    UnitAIBase hitUnit = hit.transform.GetComponent<UnitAIBase>();

                    if (selectedUnit != null)
                    {
                        if (hitUnit == selectedUnit)
                        {
                            Debug.Log("Unit Deselected");
                            // if weve clicked the currently selected unit
                            ResetActionQueue();
                            selectedUnit = null;
                        }
                        else if (hitUnit.alliance == playerAllianceInt)
                        {
                            // if weve clicked a unit we controll but is not currently selected
                            Debug.Log("Selected New Unit");
                            ResetActionQueue();
                            selectedUnit = hitUnit;
                        }
                    }
                    else if (hitUnit.alliance == playerAllianceInt)
                    {
                        // if weve clicked a unit we controll but is not currently selected
                        Debug.Log("Selected New Unit");
                        ResetActionQueue();
                        selectedUnit = hitUnit;
                    }
                }
            }

            UpdatePlayerActionGFX();
        }
    }

    void StartPlayerTurn()
    {
        ResetActionQueue();
    }

    void ResetActionQueue()
    {
        q_actionId = -1;
        q_actingUnit = null;
        q_actionPos = new Vector3Int(-1, -1, -1);
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

    public void UpdatePlayerActionGFX()
    {

        if (selectedUnit != null)
        {
            noUnitSelectedText.SetActive(false);
            selectedUnitName_Obj.SetActive(true);

            selectedUnitIndicator.SetActive(true);
            selectedUnitIndicator.transform.SetParent(selectedUnit.gfx.gameObject.transform);
            selectedUnitIndicator.transform.localPosition = Vector3.zero;

            selectedUnitName_Text.text = selectedUnit.unitName;

            selectedUnit_hpBar.SetActive(true);
            selectedUnit_hpBarFillImage.fillAmount = selectedUnit.currentHealth / selectedUnit.maxHealth;

            unitPortrait.SetActive(true);

        }
        else
        {
            noUnitSelectedText.SetActive(true);
            selectedUnitName_Obj.SetActive(false);

            selectedUnitIndicator.SetActive(false);
            selectedUnitIndicator.transform.SetParent(null);
            selectedUnitIndicator.transform.position = Vector3.zero;

            selectedUnit_hpBar.SetActive(false);

            unitPortrait.SetActive(false);

        }

        // UIs
        if (q_actionId != -1) 
        {
            commitActionButton.SetActive(true);

            switch (q_actionId)
            {
                case 0:
                    moveLine.gameObject.SetActive(true);
                    List<Vector3> path = new List<Vector3>();
                    path = MapBuilder.instance.GetPath(q_actionPos, q_actingUnit.currentPos);
                    moveLine.positionCount = path.Count;
                    //Debug.Log(path.Count);
                    for(int i = 0; i < path.Count; i++)
                    {
                        moveLine.SetPosition(i, path[i] + new Vector3(0, 0.1f, 0));
                    }

                    commitActionText.text = "MOVE";

                    break;

                default:
                    break;
            }

        }
        else
        {
            commitActionButton.SetActive(false);
            moveLine.gameObject.SetActive(false);
        }
    }

    public void CommitAction()
    {
        // action IDs:
        // 0 - move


        switch (q_actionId)
        {
            case 0:
                q_actingUnit.Order(q_actionId, q_actionPos);
                break;

            default : break;
        }

        ResetActionQueue();
        UpdatePlayerActionGFX();
    }


}
