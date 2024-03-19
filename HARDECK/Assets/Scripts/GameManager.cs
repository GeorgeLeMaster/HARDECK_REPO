using JetBrains.Annotations;
using System;
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

    public EnemyAI enemyAI;

    [Header("UI")]
    [Header("Player Interface")]
    public GameObject commitActionButton;
    public GameObject cancelActionButton;
    public GameObject endTurnButton;
    public TextMeshProUGUI commitActionText;
    public TextMeshProUGUI hitChanceText;
    public GameObject worldTextPopupPrefab;

    private bool controllsLocked;

    [Header("Menu")]
    public GameObject menuButtons;

    [Header("Selected Unit Display")]
    public GameObject selectedUnitDisplay_Obj;
    public GameObject selectedUnit_Indicator;
    public GameObject selectedUnit_ActionPipIndicator;
    public GameObject selectedUnit_MovementPipIndicator;
    public TextMeshProUGUI selectedUnitName_Text;
    public Image selectedUnit_PortraitColor;
    public Image selectedUnit_hpBarFillImage;
    public GameObject queuedPipIndicator;

    [Header("Enemy Unit Display")]
    public GameObject enemyUnitDisplay_Obj;
    public GameObject enemyUnit_Indicator;
    public TextMeshProUGUI enemyUnitName_Text;
    public Image enemyUnit_PortraitColor;
    public Image enemydUnit_hpBarFillImage;

    public LineRenderer moveLine;
    public GameObject enemyTurnIndicator;

    [Header("AllUnits")]
    public List<UnitAIBase> AllUnits;
    public List<UnitAIBase> PlayerUnits;
    public List<UnitAIBase> EnemyUnits;

    private void Awake()
    {
        instance = this;
    }

    [Header("Game Components")]
    public int currentTurn = 1;

    public UnitAIBase selectedUnit_Player;
    public UnitAIBase selectedUnit_Enemy;

    // Queued action info
    private int q_actionId;
    private UnitAIBase q_actingUnit;
    private Vector3Int q_actionPos;

    // Start is called before the first frame update
    void Start()
    {
        MapBuilder.instance.LoadMapV2();
        ResetActionQueue();
        UpdatePlayerActionGFX();

        foreach(UnitAIBase unit in GameObject.FindObjectsOfType<UnitAIBase>())
        {
            AllUnits.Add(unit);

            if (unit.alliance == 0)
            {
                PlayerUnits.Add(unit);
            }
            else
            {
                EnemyUnits.Add(unit);
            }
        }
        StartPlayerTurn();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!controllsLocked)
            {
                ResetActionQueue();
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
                {
                    if (hit.transform.GetComponent<TileInfo>() && selectedUnit_Player != null)
                    {
                        bool unoccupied = true;

                        foreach(UnitAIBase u in AllUnits)
                        {
                            if (u.currentPos == hit.transform.GetComponent<TileInfo>().tilemapPosition)
                            {
                                unoccupied = false;
                                break;
                            }
                        }

                        if (unoccupied)
                        {
                            selectedUnit_Enemy = null;
                            // Queue up move command for selected unit

                            if (selectedUnit_Player.movementPips > 0)
                            {
                                q_actionId = 0;
                                q_actingUnit = selectedUnit_Player;
                                q_actionPos = hit.transform.GetComponent<TileInfo>().tilemapPosition;
                            }
                        }
                    }
                    else if (hit.transform.GetComponent<UnitAIBase>())
                    {
                        // Cases where a unit is clicked
                        UnitAIBase hitUnit = hit.transform.GetComponent<UnitAIBase>();

                        // Determine alliance of clicked unit and fill appropriate stats
                        if (hitUnit.alliance == playerAllianceInt)
                        {
                            selectedUnit_Enemy = null;

                            if (selectedUnit_Player != null)
                            {
                                if (selectedUnit_Player == hitUnit)
                                {
                                    selectedUnit_Player = null;
                                }
                                else
                                {
                                    selectedUnit_Player = hitUnit;
                                }

                            }
                            else
                            {
                                selectedUnit_Player = hitUnit;
                            }
                        }
                        else if (hitUnit.alliance != playerAllianceInt)
                        {

                            if (selectedUnit_Enemy != null)
                            {
                                if (selectedUnit_Enemy == hitUnit)
                                {
                                    selectedUnit_Enemy = null;
                                }
                                else
                                {
                                    selectedUnit_Enemy = hitUnit;
                                }

                            }
                            else
                            {
                                selectedUnit_Enemy = hitUnit;
                            }


                        }


                        if (selectedUnit_Player != null && selectedUnit_Enemy != null && selectedUnit_Player.actionPips > 0)
                        {
                            // Enemy and Controlled Unit selected, queue attack
                            ResetActionQueue();
                            q_actionId = 1;
                            q_actingUnit = selectedUnit_Player;
                            q_actionPos = selectedUnit_Enemy.currentPos;
                        }
                        else
                        {
                            // Not enough units selected for a UvU combat action, reset the queue
                            ResetActionQueue();

                        }
                    }
                }
            }
            UpdatePlayerActionGFX();
        }
    }

    void ResetActionQueue()
    {
        q_actionId = -1;
        q_actingUnit = null;
        q_actionPos = new Vector3Int(-1, -1, -1);
    }

    public void UpdatePlayerActionGFX()
    {
        hitChanceText.gameObject.SetActive(false);
        // Sets unit displays positions
        if (selectedUnit_Enemy != null && selectedUnit_Player != null)
        {
            selectedUnitDisplay_Obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-255, 50, 0);
            enemyUnitDisplay_Obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(255, 50, 0);

        }
        else
        {
            selectedUnitDisplay_Obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 50, 0);
            enemyUnitDisplay_Obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 50, 0);
        }

        // Toggles on and off selected and enemy displays (and selection indicator)
        if (selectedUnit_Player != null)
        {
            selectedUnitDisplay_Obj.SetActive(true);

            selectedUnit_Indicator.SetActive(true);
            selectedUnit_Indicator.transform.SetParent(selectedUnit_Player.gfx.gameObject.transform);
            selectedUnit_Indicator.transform.localPosition = Vector3.zero + new Vector3(0, 0.25f, 0);

            selectedUnitName_Text.text = selectedUnit_Player.unitName;

            selectedUnit_hpBarFillImage.fillAmount = selectedUnit_Player.currentHealth / selectedUnit_Player.maxHealth;

            Color allianceColor = new Color(0, 1, 0);
            selectedUnit_PortraitColor.color = allianceColor;

            if (selectedUnit_Player.movementPips > 0)
            {
                selectedUnit_MovementPipIndicator.SetActive(true);

                if (q_actionId == 0)
                {
                    selectedUnit_MovementPipIndicator.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    queuedPipIndicator.SetActive(true);
                    queuedPipIndicator.GetComponent<RectTransform>().anchoredPosition = selectedUnit_MovementPipIndicator.GetComponent<RectTransform>().anchoredPosition;
                    queuedPipIndicator.GetComponent<RectTransform>().position = selectedUnit_MovementPipIndicator.GetComponent<RectTransform>().position;

                }
                else
                {
                    selectedUnit_MovementPipIndicator.transform.localScale = Vector3.one;


                }

            }
            else
            {
                selectedUnit_MovementPipIndicator.SetActive(false);
            }


            if (selectedUnit_Player.actionPips > 0)
            {
                selectedUnit_ActionPipIndicator.SetActive(true);

                if (q_actionId == 1)
                {
                    int hitChance = 0;
                    hitChance = GroundUnitAI.CalculateHitChance(selectedUnit_Player, selectedUnit_Enemy);

                    //hitChance = Math.Clamp(hitChance,1,19);
                    if (hitChance < 1)
                    {
                        hitChance = 1;
                    }

                    hitChanceText.gameObject.SetActive(true);
                    hitChanceText.text = (100-((hitChance)/20f*100)).ToString() + "%";
                    selectedUnit_ActionPipIndicator.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    queuedPipIndicator.SetActive(true);
                    queuedPipIndicator.GetComponent<RectTransform>().anchoredPosition = selectedUnit_ActionPipIndicator.GetComponent<RectTransform>().anchoredPosition;
                    queuedPipIndicator.GetComponent<RectTransform>().position = selectedUnit_ActionPipIndicator.GetComponent<RectTransform>().position;

                }
                else
                {
                    selectedUnit_ActionPipIndicator.transform.localScale = Vector3.one;
                    hitChanceText.gameObject.SetActive(false);

                }
            }
            else
            {
                selectedUnit_ActionPipIndicator.SetActive(false);
            }

            if (q_actionId < 0)
            {
                queuedPipIndicator.SetActive(false);

            }
        }
        else
        {
            selectedUnitDisplay_Obj.SetActive(false);

            selectedUnit_Indicator.SetActive(false);
            selectedUnit_Indicator.transform.SetParent(null);
            selectedUnit_Indicator.transform.position = Vector3.zero;
        }

        if (selectedUnit_Enemy != null)
        {
            enemyUnitDisplay_Obj.SetActive(true);

            enemyUnit_Indicator.SetActive(true);
            enemyUnit_Indicator.transform.SetParent(selectedUnit_Enemy.gfx.gameObject.transform);
            enemyUnit_Indicator.transform.localPosition = Vector3.zero;

            enemyUnitName_Text.text = selectedUnit_Enemy.unitName;

            enemydUnit_hpBarFillImage.fillAmount = selectedUnit_Enemy.currentHealth / selectedUnit_Enemy.maxHealth;

            Color allianceColor = new Color(1, 0, 0);
            enemyUnit_PortraitColor.color = allianceColor;
        }
        else
        {
            enemyUnitDisplay_Obj.SetActive(false);

            enemyUnit_Indicator.SetActive(false);
            enemyUnit_Indicator.transform.SetParent(null);
            enemyUnit_Indicator.transform.position = Vector3.zero;
        }

        // Assigns stats


        // In world UIs
        if (q_actionId != -1) 
        {
            commitActionButton.SetActive(true);
            cancelActionButton.SetActive(true);


            switch (q_actionId)
            {
                case 0:
                    // Move line logic
                    moveLine.gameObject.SetActive(true);
                    List<Vector3> path = new List<Vector3>();
                    path = MapBuilder.instance.GetPath(q_actionPos, q_actingUnit.currentPos);
                    moveLine.positionCount = path.Count;
                    //Debug.Log(path.Count);
                    for(int i = 0; i < path.Count; i++)
                    {
                        moveLine.SetPosition(i, path[i] + new Vector3(0, 0.1f, 0));
                    }

                    // Update Text
                    commitActionText.text = "MOVE";
                    commitActionButton.GetComponent<Image>().color = new Color(1, 0.86f,0.36f);

                    break;

                case 1:
                    moveLine.gameObject.SetActive(false);

                    commitActionText.text = "ATTACK";
                    commitActionButton.GetComponent<Image>().color = new Color(1, 0, 0);

                    break;

                default:
                    break;
            }

        }
        else
        {
            commitActionButton.SetActive(false);
            cancelActionButton.SetActive(false);
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
                if (q_actingUnit.movementPips > 0)
                {
                    q_actingUnit.Order(q_actionId, q_actionPos);
                }
                break;
            case 1:
                if (q_actingUnit.actionPips > 0)
                {
                    q_actingUnit.Order(q_actionId, q_actionPos);

                }
                break;

            default : break;
        }

        ResetActionQueue();
        UpdatePlayerActionGFX();
    }

    public void CancelAction()
    {
        ResetActionQueue();
        selectedUnit_Enemy = null;
        UpdatePlayerActionGFX();
    }

    // Trun state logic
    public void EndPlayerTurn()
    {
        selectedUnit_Player = null;
        selectedUnit_Enemy = null;
        UpdatePlayerActionGFX();
        enemyAI.BeginTurn();
        controllsLocked = true;
        enemyTurnIndicator.SetActive(true);
        endTurnButton.SetActive(false);
        
    }

    public void StartPlayerTurn()
    {
        foreach(UnitAIBase unit in PlayerUnits)
        {
            unit.actionPips = 1;
            unit.movementPips = 1;
        }
        UpdatePlayerActionGFX();
        controllsLocked = false;
        enemyTurnIndicator.SetActive(false);
        endTurnButton.SetActive(true);

    }


    // Menu UI logic

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void ToggleMenuUI()
    {
        menuButtons.SetActive(!menuButtons.activeInHierarchy);
    }
}
