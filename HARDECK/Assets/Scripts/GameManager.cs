using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TurnState
{
    Drawing,
    Deployment,
    Action,
    Enemy
}

public class Commander
{
    public int allianceInt;
    public List<Structure> structures;
    public List<UnitAIBase> units;

}

public class UnitData
{
    public UnitData(int mv, int vr, int d, int dm, int r, int h, string n)
    {
        moveSpeed = mv;
        visionRadius = vr;
        damage = d;
        damageMod = dm;
        range = r;
        hp = h;
        name = n;
    }

    public int moveSpeed;
    public int visionRadius;
    public int damage;
    public int damageMod;
    public int range;
    public int hp;

    public string name;
}

public class GameManager : MonoBehaviour
{
    public UnitData infintrymanUnitData = new UnitData(8, 8, 4, 2, 12, 6, "Infantryman");

    public UnitData unitDataToSpawn;

    public static GameManager instance;

    public TurnState turnState;

    public int playerAllianceInt;

    public EnemyAI enemyAI;

    public Material playerMat;
    public Material enemyMat;

    public Color playerColor;
    public Color enemyColor;

    [Header("UI")]
    [Header("Player Interface")]
    public GameObject commitActionButton;
    public GameObject cancelActionButton;
    public GameObject endTurnButton;
    public GameObject movementTooFarText;
    public GameObject outOfRangeText;
    public TextMeshProUGUI commitActionText;
    public TextMeshProUGUI hitChanceText;
    public GameObject worldTextPopupPrefab;

    public bool controllsLocked;
    public bool skipDeployment;

    [Header("Decks")]
    public GameObject phaseOne;
    public GameObject phaseTwo;
    public GameObject phaseThree;

    public GameObject selectedDeckImage;

    public Sprite groundDeckSprite;
    public Sprite heavyDeckSprite;
    public Sprite airDeckSprite;

    [HideInInspector]
    public List<GFXOBJContainter> lastFOWTiles;
    [HideInInspector]
    public List<GFXOBJContainter> currentFOWTiles;

    public GameObject selectedTileIndicator;

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

    public List<Structure> structures;

    public List<TileOverlayLogic> deployableTiles;

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
        //ResetActionQueue();
        //UpdatePlayerActionGFX();

        playerMat.color = playerColor;
        enemyMat.color = enemyColor;


        foreach (UnitAIBase unit in GameObject.FindObjectsOfType<UnitAIBase>())
        {
            AllUnits.Add(unit);

            if (unit.alliance == playerAllianceInt)
            {
                PlayerUnits.Add(unit);
            }
            else
            {
                EnemyUnits.Add(unit);
            }
        }
        foreach (UnitAIBase u in EnemyUnits)
        {
            u.gfx.SetActive(false);

        }


        //StartPlayerTurn();
        //APFOW();

    }



    // Update is called once per frame
    void Update()
    {
        // Deployment State logic
        // Action state logic
        if (Input.GetMouseButtonDown(0) && turnState == TurnState.Deployment && !EventSystem.current.IsPointerOverGameObject())
        {

            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("Overlay");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask))
            {
                //Debug.Log("hit");
                if (hit.transform.GetComponent<TileOverlayLogic>())
                {
                    if (unitDataToSpawn != null && deployableTiles.Contains(hit.transform.GetComponent<TileOverlayLogic>()))
                    {
                        GameObject newUnit = Instantiate(Resources.Load("Units/OBJs/GroundUnitPrefab") as GameObject);
                        UnitAIBase newAi = newUnit.GetComponent<UnitAIBase>();
                        
                        newAi.moveSpeed = unitDataToSpawn.moveSpeed;
                        newAi.damage = unitDataToSpawn.damage;
                        newAi.damageMod = unitDataToSpawn.damageMod;
                        newAi.alliance = 0;
                        newAi.range = unitDataToSpawn.range;
                        newAi.visionRadius = unitDataToSpawn.visionRadius;
                        newAi.maxHealth = unitDataToSpawn.hp;
                        newAi.currentHealth = unitDataToSpawn.hp;

                        Vector3Int pos = new Vector3Int( (int)hit.transform.GetComponent<TileOverlayLogic>().transform.position.x, (int)hit.transform.GetComponent<TileOverlayLogic>().transform.position.y, (int)hit.transform.GetComponent<TileOverlayLogic>().transform.position.z);

                        newAi.Spawn(pos, Resources.Load("Units/GFX/GFX_Infantryman") as GameObject);

                        AllUnits.Add(newAi);
                        PlayerUnits.Add(newAi);

                        turnState = TurnState.Action;
                        controllsLocked = false;
                        phaseThree.SetActive(false);
                        phaseTwo.SetActive(false);
                        APFOW();
                    }
                }
            }

        }

        // Action state logic
        if (Input.GetMouseButtonDown(0) && turnState == TurnState.Action && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!controllsLocked)
            {
                ResetActionQueue();
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Tile", "Unit");
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask))
                {
                    if (hit.transform.GetComponent<TileInfo>())
                    {
                        selectedTileIndicator.transform.position = hit.transform.GetComponent<TileInfo>().tilemapPosition;
                        selectedTileIndicator.SetActive(true);
                    }
                    else
                    {
                        selectedTileIndicator.SetActive(false);

                    }

                    if (hit.transform.GetComponent<TileInfo>() && selectedUnit_Player != null)
                    {
                        bool unoccupied = true;

                        foreach (UnitAIBase u in AllUnits)
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
                        else if (hitUnit.alliance != playerAllianceInt && hitUnit.gfx.activeInHierarchy)
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

        RaycastHit conHit;
        LayerMask conMask = LayerMask.GetMask("Tile", "Unit");
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out conHit, Mathf.Infinity, conMask) && turnState == TurnState.Action)
        {
            if (conHit.transform.GetComponent<TileInfo>())
            {
                selectedTileIndicator.SetActive(true);

                selectedTileIndicator.transform.position = conHit.transform.GetComponent<TileInfo>().tilemapPosition;
            }
            else
            {
                selectedTileIndicator.SetActive(false);
            }
        }
        else
        {
            selectedTileIndicator.SetActive(false);
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

            Color allianceColor = playerMat.color;
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

            Color allianceColor = enemyMat.color;
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

        movementTooFarText.SetActive(false);
        outOfRangeText.SetActive(false);
        commitActionButton.SetActive(false);
        cancelActionButton.SetActive(false);
        // In world UIs
        if (q_actionId != -1) 
        {
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
                        moveLine.SetPosition(i, path[i] + new Vector3(0, 0.025f, 0));
                        
                    }

                    Flowfield ff = MapBuilder.instance.Flowfields[q_actingUnit.currentPos.x, q_actingUnit.currentPos.y, q_actingUnit.currentPos.z];
                    float cost = ff.tiles[q_actionPos.x, q_actionPos.y, q_actionPos.z].pathCost;

                    Color goodColor = new Color(0,1,0,1);
                    Color badColor = new Color(0, 1, 0, 0.2f);


                    if (cost <= q_actingUnit.moveSpeed && cost != -1)
                    {
                        commitActionText.text = "MOVE";
                        commitActionButton.GetComponent<Image>().color = new Color(1, 0.86f, 0.36f);
                        moveLine.startColor = goodColor;
                        moveLine.endColor = goodColor;
                        commitActionButton.SetActive(true);
                        cancelActionButton.SetActive(true);
                        movementTooFarText.SetActive(false);
                    }
                    else
                    {

                        movementTooFarText.SetActive(true);
                        commitActionButton.GetComponent<Image>().color = new Color(0.8f, 0.86f, 0.16f);
                        moveLine.startColor = badColor;
                        moveLine.endColor = badColor;
                    }


                    break;

                case 1:
                    if (Vector3.Distance(q_actionPos, q_actingUnit.currentPos) <= q_actingUnit.range)
                    {
                        moveLine.gameObject.SetActive(false);
                        commitActionButton.SetActive(true);
                        cancelActionButton.SetActive(true);
                        outOfRangeText.SetActive(false);
                        commitActionText.text = "ATTACK";
                        commitActionButton.GetComponent<Image>().color = new Color(1, 0, 0);
                    }
                    else
                    {
                        outOfRangeText.SetActive(true);


                    }

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

    public void APFOW()
    {
        List<Vector3> hideSpots = new List<Vector3>();
        List<Vector3> showSpots = new List<Vector3>();

        // clear out last frame list, set = to current frame list

        lastFOWTiles = new List<GFXOBJContainter>(currentFOWTiles);

        currentFOWTiles.Clear();

        foreach (UnitAIBase u in PlayerUnits)
        {
            if (u != null && u.currentHealth > 0)
            {
                MapBuilder.instance.UpdateFOW(u);
            }
        }

        foreach (Structure st in structures)
        {
            if (st.ownerId == playerAllianceInt)
            {
                Collider[] structTiles = Physics.OverlapBox(st.tilemapPos, new Vector3(st.WidthHeight.x, 1, st.WidthHeight.y), Quaternion.identity, LayerMask.GetMask("OBJFinder"));
                //if (st.tilemapPos.x + x >= 0 && st.tilemapPos.x + x < MapBuilder.instance.mapX - 1 && st.tilemapPos.x + x >= 0 && st.tilemapPos.x + x < MapBuilder.instance.mapX - 1) 
                //{
                foreach (Collider obj in structTiles)
                {
                    currentFOWTiles.Add(obj.GetComponent<GFXOBJContainter>());
                }

                // }


            }
        }

        if (lastFOWTiles.Count > 0)
        {
            foreach (GFXOBJContainter t in lastFOWTiles)
            {
                if (t != null)
                {
                    t.SetState(2);
                    //Debug.Log("!");
                    hideSpots.Add(t.transform.position);
                }
            }
        }

        if (currentFOWTiles.Count > 0)
        {
            foreach (GFXOBJContainter t in currentFOWTiles)
            {
                if (t != null)
                {
                    t.SetState(1);
                    showSpots.Add(t.transform.position);
                }
            }
        }

        foreach (GFXOBJContainter t in currentFOWTiles)
        {

           // t.SetState(1);
            showSpots.Add(t.transform.position);

        }

        foreach (UnitAIBase u in EnemyUnits)
        {
            if (showSpots.Contains(u.currentPos))
            {
                u.gfx.SetActive(true);
            }
            else if (hideSpots.Contains(u.currentPos))
            {
                u.gfx.SetActive(false);

            }
        }

        foreach (Structure st in structures)
        {
            if (showSpots.Contains(st.tilemapPos) && st.ownerId != playerAllianceInt)
            {
                st.ShowGFX();
            }
            else if (st.ownerId != playerAllianceInt)
            {
                st.HideGFX();
            }
        }
    }

    public void StartPlayerTurn()
    {


        turnState = TurnState.Drawing;
       // Debug.Log("start");

        foreach(Structure st in structures)
        {
            st.CheckOwnership();
        }

        foreach(UnitAIBase unit in PlayerUnits)
        {
            unit.actionPips = 1;
            unit.movementPips = 1;
        }
        controllsLocked = false;
        enemyTurnIndicator.SetActive(false);
        endTurnButton.SetActive(true);
        selectedUnit_Player = null;
        selectedUnit_Enemy = null;
        q_actionId = -1;
        UpdatePlayerActionGFX();

        controllsLocked = true;

        if (skipDeployment == true)
        {
            turnState = TurnState.Action;
            controllsLocked = false;
            return;
        }
        else
        {

            PromptDrawPhaseOne();
        }
    }


    // Menu UI logic

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ToggleMenuUI()
    {
        menuButtons.SetActive(!menuButtons.activeInHierarchy);
    }

    private IEnumerator firstFOWupdate()
    {
        structures.Clear();
        foreach (Structure s in GameObject.Find("Structures").GetComponentsInChildren<Structure>())
        {
            structures.Add(s);
        }
        yield return new WaitForEndOfFrame();
        lastFOWTiles.Clear();
        currentFOWTiles.Clear();
        APFOW();
    }

    public void HighlightDeployableTiles(int type)
    {
        foreach (Structure s in structures)
        {
            if (s.ownerId == playerAllianceInt && ((type == 0 && s.provideGround) || (type == 1 && s.provideHeavy) || (type == 2 && s.provideAir)))
            {
                for (int x = -s.WidthHeight.x; x <= s.WidthHeight.x; x++)
                {
                    for (int z = -s.WidthHeight.y; z <= s.WidthHeight.y; z++)
                    {
                        int newX = s.tilemapPos.x + x;
                        int newY = s.tilemapPos.z + z;
                        if (MapBuilder.instance.FOWtiles[newX,s.tilemapPos.y, newY] != null)
                        {
                            MapBuilder.instance.FOWtiles[newX, s.tilemapPos.y, newY].GetComponent<TileOverlayLogic>().SetOverlay("Deployable");
                            deployableTiles.Add(MapBuilder.instance.FOWtiles[newX, s.tilemapPos.y, newY].GetComponent<TileOverlayLogic>());
                        }
                    }
                }
            }
        }
    }

    public void PromptDrawPhaseOne()
    {
        phaseOne.SetActive(true);
        phaseTwo.SetActive(true);
        phaseThree.SetActive(true);
        phaseOne.SetActive(true);
        phaseTwo.SetActive(false);
        phaseThree.SetActive(false);

        phaseOne.GetComponent<Animator>().ResetTrigger("GT2");

    }

    public void GoBetweenPhaseTwo(int input)
    {
        StartCoroutine(PromptDrawPhaseTwo(input));
    }

    public void GoBetweenPhaseThree(int input)
    {
        StartCoroutine(PromptDrawPhaseThree(input));

    }

    public IEnumerator PromptDrawPhaseTwo(int input)
    {
        phaseOne.SetActive(true);
        phaseTwo.SetActive(false);
        phaseThree.SetActive(false);

        switch(input)
        {
            case 1:
                selectedDeckImage.GetComponent<Image>().sprite = groundDeckSprite;
                break;
            case 2:
                selectedDeckImage.GetComponent<Image>().sprite = heavyDeckSprite;
                break;
            case 3:
                selectedDeckImage.GetComponent<Image>().sprite = airDeckSprite;
                break;
        }

        phaseOne.GetComponent<Animator>().SetTrigger("GT2");
        yield return new WaitForSeconds(0.5f);

        phaseOne.SetActive(false);
        phaseTwo.SetActive(true);
        phaseThree.SetActive(false);

       // phaseTwo.

    }

    public IEnumerator PromptDrawPhaseThree(int input)
    {
        phaseOne.SetActive(false);
        phaseTwo.SetActive(true);
        phaseThree.SetActive(false);

        if (input == 0)
        {
            phaseTwo.GetComponent<Animator>().SetTrigger("DrawDeck");
            yield return new WaitForSeconds(0.5f);
            phaseTwo.SetActive(false);

            phaseThree.SetActive(true);


        }
        else
        {
            phaseTwo.GetComponent<Animator>().SetTrigger("DrawGeneric");
            yield return new WaitForSeconds(0.5f);
            unitDataToSpawn = infintrymanUnitData;
            HighlightDeployableTiles(0);
            turnState = TurnState.Deployment;
        }

       // yield return new WaitForSeconds(1f);

       
    }

    public void SkipDeployment()
    {
        phaseOne.SetActive(false);
        phaseTwo.SetActive(false);
        phaseThree.SetActive(false);

        turnState = TurnState.Action;
        controllsLocked = false;
    }
}
