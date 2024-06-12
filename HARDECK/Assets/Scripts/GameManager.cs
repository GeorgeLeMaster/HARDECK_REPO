using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
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
    public Commander()
    {
        ownedStructures = new List<Structure>();
        activeUnits = new List<UnitAIBase>();

        visableEnemyUnits = new List<UnitAIBase>();
        visableGFXOBJs = new List<GFXOBJContainter>();
        visableTiles = new List<Vector3Int>();

        deck = new List<UnitAIBase>();

        knownStructures = new List<Structure>();
    }

    public int allianceInt;
    public Color allianceColor;

    public int ccCap;
    public int ccCurrent;

    public List<Structure> ownedStructures;
    public List<UnitAIBase> activeUnits;

    public List<UnitAIBase> deck;

    public List<UnitAIBase> visableEnemyUnits;
    public List<GFXOBJContainter> visableGFXOBJs;
    public List<Vector3Int> visableTiles;

    public GFXOBJContainter[] GFXOBJs;
    public List<Structure> knownStructures;
}

public class UnitData
{
    public UnitData(int mv, int vr, int d, int dm, int r, int h, string n, List<int> abilityInts)
    {
        moveSpeed = mv;
        visionRadius = vr;
        damage = d;
        damageMod = dm;
        range = r;
        hp = h;
        name = n;
        this.abilityInts = abilityInts;
    }

    public int moveSpeed;
    public int visionRadius;
    public int damage;
    public int damageMod;
    public int range;
    public int hp;

    public string name;

    public List<int> abilityInts;

}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public TurnState turnState;

    public UnitData infintrymanUnitData = new UnitData(8, 8, 4, 2, 12, 6, "Infantryman", new List<int>{0,1});

    public UnitData unitDataToSpawn;

    public int playerAllianceInt;
    public Material playerMat;

    public Color commanderColor_0;
    public Color commanderColor_1;

    public EnemyAI enemyAI;

    public List<Commander> commanders;

    [Header("UI")]
    [Header("Player Interface")]
    public GameObject commitActionButton;
    public GameObject cancelActionButton;
    public GameObject endTurnButton;
    public TextMeshProUGUI commitActionText;
    public TextMeshProUGUI actionInfoText;
    public GameObject worldTextPopupPrefab;

    public Image abilityIcon;
    public TextMeshProUGUI abilityName;
    public TextMeshProUGUI abilityDesc;
    public Sprite[] abilityIconFiles;

    public bool controllsLocked;
    public bool skipDeployment;

    public GameObject cursor;

    private Canvas gameUI_Canvas;

    [Header("Decks")]
    public GameObject phaseOne;
    public GameObject phaseTwo;
    public GameObject phaseThree;

    public GameObject selectedDeckImage;

    public Sprite groundDeckSprite;
    public Sprite heavyDeckSprite;
    public Sprite airDeckSprite;

    [HideInInspector]
    public List<GFXOBJContainter> lastGFXOBJs;
    [HideInInspector]
    public List<GFXOBJContainter> currentGFXOBJs;

    public GameObject selectedTileIndicator;

    [Header("Menu")]
    public GameObject menuButtons;

    [Header("Selected Unit Display")]
    public GameObject abilityDisplay;
    public GameObject selectedUnitDisplay_Obj;
    public GameObject selectedUnit_Indicator;
    public GameObject selectedUnit_ActionPipIndicator;
    public GameObject selectedUnit_MovementPipIndicator;
    public TextMeshProUGUI selectedUnitName_Text;
    public Image selectedUnit_PortraitColor;
    public Image selectedUnit_hpBarFillImage;
    public int selectedAbilityInt;

    [Header("Enemy Unit Display")]
    public GameObject enemyUnitDisplay_Obj;
    public GameObject enemyUnit_Indicator;
    public TextMeshProUGUI enemyUnitName_Text;
    public Image enemyUnit_PortraitColor;
    public Image enemydUnit_hpBarFillImage;

    public LineRenderer moveLine;
    public GameObject enemyTurnIndicator;

    public List<TileOverlayLogic> deployableTiles;

    [Header("Game Components")]
    public int currentTurn = 1;

    public UnitAIBase selectedUnit_Player = null;
    public UnitAIBase selectedUnit_Enemy = null;
    public Vector3Int selectedPosition = new Vector3Int(-1,-1,-1);


    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create a player commander and an enemy commander [LATER CHANGE FOR MAP SPECIFIC IMPLAMENTATION]
        Commander playerCommander = new Commander();
        playerCommander.allianceColor = commanderColor_0;
        playerCommander.allianceInt = 0;

        Commander enemyCommander = new Commander();
        enemyCommander.allianceColor = commanderColor_1;
        enemyCommander.allianceInt = 1;

        commanders = new List<Commander>();
        commanders.Add(playerCommander);
        commanders.Add(enemyCommander);

        gameUI_Canvas = GameObject.Find("GameUI").GetComponent<Canvas>();
        Cursor.visible = false;
        GameObject.Find("CursorColor").GetComponent<Image>().color = commanderColor_0;

        MapBuilder.instance.LoadMapV2();

        playerMat.color = commanderColor_0;

        foreach(Commander c in commanders)
        {
            c.GFXOBJs = new GFXOBJContainter[MapBuilder.instance.GFXOBJs.Length];

            for(int i = 0; i < c.GFXOBJs.Length; i++)
            {
                c.GFXOBJs[i] = new GFXOBJContainter();
            }

            CopyStates(c.GFXOBJs, MapBuilder.instance.GFXOBJs);
        }

        // Load icon files
        abilityIconFiles = Resources.LoadAll<Sprite>("Sprites/AbilityIcons");

        StartNewTurn();

    }



    // Update is called once per frame
    void Update()
    {

        // Deployment State logic
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
                        newAi.alliance = playerAllianceInt;
                        newAi.range = unitDataToSpawn.range;
                        newAi.visionRadius = unitDataToSpawn.visionRadius;
                        newAi.maxHealth = unitDataToSpawn.hp;
                        newAi.currentHealth = unitDataToSpawn.hp;
                        newAi.unitName = unitDataToSpawn.name;
                        newAi.abilityInts = unitDataToSpawn.abilityInts;

                        Vector3Int pos = new Vector3Int( (int)hit.transform.GetComponent<TileOverlayLogic>().transform.position.x, (int)hit.transform.GetComponent<TileOverlayLogic>().transform.position.y, (int)hit.transform.GetComponent<TileOverlayLogic>().transform.position.z);

                        newAi.Spawn(pos, Resources.Load("Units/GFX/GFX_Infantryman") as GameObject);

                        commanders[newAi.alliance].activeUnits.Add(newAi);

                        ClearOverlay("Structures");
                        turnState = TurnState.Action;
                        controllsLocked = false;
                        phaseThree.SetActive(false);
                        phaseTwo.SetActive(false);
                        GameManager.instance.UpdateVisableFOW();
                    }
                }
            }

        }

        // Action state logic
        if (Input.GetMouseButtonDown(0) && turnState == TurnState.Action && !EventSystem.current.IsPointerOverGameObject())
        {
            
            // If controlls are not locked
            if (!controllsLocked)
            {
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Tile", "Unit");
                // Raycast out, returns true if a tile or unit is selected
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask))
                {

                    // If an empty tile is clicked and a player unit is selected
                    if (hit.transform.GetComponent<TileInfo>() && selectedUnit_Player != null)
                    {
                        // Deselects enemy, if one was selected
                        selectedUnit_Enemy = null;
                        // Queue up move command for selected unit
                        selectedPosition = hit.transform.GetComponent<TileInfo>().tilemapPosition;

                    }
                    // If unit is selected
                    else if (hit.transform.GetComponent<UnitAIBase>())
                    {
                        // Cases where a unit is clicked
                        UnitAIBase hitUnit = hit.transform.GetComponent<UnitAIBase>();

                        // Determine alliance of clicked unit and fill appropriate stats
                        if (hitUnit.alliance == playerAllianceInt)
                        {
                            selectedUnit_Enemy = null;

                            if (selectedUnit_Player == null)
                            {
                                selectedUnit_Player = hitUnit;

                            }
                            else if (selectedUnit_Player != hitUnit)
                            {
                                selectedUnit_Player = hitUnit;

                            }
                            else
                            {
                                selectedUnit_Player = null;
                            }
                        }
                        // If clicked on non-allied unit that is visable
                        else if (hitUnit.alliance != playerAllianceInt && hitUnit.gfx.activeInHierarchy)
                        {
                            if (selectedUnit_Enemy == null)
                            {
                                selectedUnit_Enemy = hitUnit;
                            }
                            else if (selectedUnit_Enemy != hitUnit)
                            {
                                selectedUnit_Enemy = hitUnit;
                            }
                            else
                            {
                                selectedUnit_Enemy = null;
                            }
                        }
                        selectedPosition = new Vector3Int(-1, -1, -1);

                    }
                    else
                    {
                        selectedPosition = new Vector3Int(-1, -1, -1);
                    }


                    // If player and enemy units are selected, queue the attack action [CHANGE: TURN INTO ABILITY SYSTEM]
                    if (selectedUnit_Player != null)
                    {
                        if (selectedUnit_Enemy != null && selectedUnit_Player.actionPips > 0)
                        {
                            selectedAbilityInt = 0;
                            selectedPosition = new Vector3Int(-1, -1, -1);

                        }
                        else if (selectedPosition.x > -1 && selectedUnit_Player.movementPips > 0)
                        {
                            selectedAbilityInt = 1;
                        }
                        else
                        {
                            selectedAbilityInt = 2;
                            selectedPosition = new Vector3Int(-1, -1, -1);

                        }
                    }
                    else
                    {
                        selectedAbilityInt = -1;
                        selectedPosition = new Vector3Int(-1, -1, -1);

                    }
                }
            }
            UpdatePlayerActionGFX();
        }


        // If a unit is selected, can move, and the pointed to tile is within range, display the tile indicator [CHANGE: SCRAP THIS SHIT AND MAKE IT BETTER]
        RaycastHit conHit;
        LayerMask conMask = LayerMask.GetMask("Tile", "Unit");
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out conHit, Mathf.Infinity, conMask) && turnState == TurnState.Action && !EventSystem.current.IsPointerOverGameObject() && selectedUnit_Player != null)
        {
            if (conHit.transform.GetComponent<TileInfo>() && selectedUnit_Player.movementPips > 0)
            {
                Vector3Int pos = conHit.transform.GetComponent<TileInfo>().tilemapPosition;
                Vector3Int uPos = selectedUnit_Player.currentPos;
                if (MapBuilder.instance.Flowfields[uPos.x, uPos.y, uPos.z].tiles[pos.x, pos.y, pos.z].pathCost <= selectedUnit_Player.moveSpeed)
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
        else
        {
            selectedTileIndicator.SetActive(false);
        }
    }

    public void UpdatePlayerActionGFX()
    {
        // Disable Everything
        actionInfoText.gameObject.SetActive(false);
        commitActionButton.SetActive(false);
        cancelActionButton.SetActive(false);

        // Toggles on and off selected and enemy displays (and selection indicator)
        if (selectedUnit_Player != null)
        {
            // Shows ability display 
            if (selectedAbilityInt >= 0)
            {
                abilityDisplay.SetActive(true);
                abilityName.text = Abilities.abilityDescs[selectedAbilityInt].abName;
                abilityDesc.text = Abilities.abilityDescs[selectedAbilityInt].abDescription;
                abilityIcon.sprite = abilityIconFiles[selectedAbilityInt];
            }

            // Shows unit display
            selectedUnitDisplay_Obj.SetActive(true);
            selectedUnitName_Text.text = selectedUnit_Player.unitName;
            selectedUnit_hpBarFillImage.fillAmount = selectedUnit_Player.currentHealth / selectedUnit_Player.maxHealth;
            Color allianceColor = playerMat.color;
            selectedUnit_PortraitColor.color = allianceColor;

            // Shows and snaps indicator
            selectedUnit_Indicator.SetActive(true);
            selectedUnit_Indicator.transform.SetParent(selectedUnit_Player.gfx.gameObject.transform);
            selectedUnit_Indicator.transform.localPosition = Vector3.zero + new Vector3(0, 0.025f, 0);

            // Show or hide pips
            if (selectedUnit_Player.movementPips > 0)
            {
                selectedUnit_MovementPipIndicator.SetActive(true);
            }
            else
            {
                selectedUnit_MovementPipIndicator.SetActive(false);
            }
            if (selectedUnit_Player.actionPips > 0)
            {
                selectedUnit_ActionPipIndicator.SetActive(true);
            }
            else
            {
                selectedUnit_ActionPipIndicator.SetActive(false);
            }

        }
        else
        {
            // No player controlled unit is selected, hide displays and indicators
            selectedUnitDisplay_Obj.SetActive(false);
            abilityDisplay.SetActive(false);
            selectedUnit_Indicator.SetActive(false);
            selectedUnit_Indicator.transform.SetParent(null);
            selectedUnit_Indicator.transform.position = Vector3.zero;
        }

        // Same as above (kinda) but for enemy
        if (selectedUnit_Enemy != null)
        {
            enemyUnitDisplay_Obj.SetActive(true);

            enemyUnit_Indicator.SetActive(true);
            enemyUnit_Indicator.transform.SetParent(selectedUnit_Enemy.gfx.gameObject.transform);
            enemyUnit_Indicator.transform.localPosition = Vector3.zero;

            enemyUnitName_Text.text = selectedUnit_Enemy.unitName;

            enemydUnit_hpBarFillImage.fillAmount = selectedUnit_Enemy.currentHealth / selectedUnit_Enemy.maxHealth;

            Color allianceColor = commanders[selectedUnit_Enemy.alliance].allianceColor;
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
        if (selectedAbilityInt != -1) 
        {
            AbilityDescriptor ab = Abilities.abilityDescs[selectedAbilityInt];

            if (Abilities.CheckAbilityCost(selectedUnit_Player, selectedAbilityInt) && Abilities.CheckAbilityParameters(selectedUnit_Player, selectedAbilityInt))
            {
                commitActionButton.SetActive(true);
                commitActionButton.GetComponent<Image>().color = ab.color;
                actionInfoText.gameObject.SetActive(false);
                commitActionText.text = ab.abName;
            }
            else
            {
                commitActionButton.SetActive(false);
                actionInfoText.gameObject.SetActive(true);
                if (!Abilities.CheckAbilityCost(selectedUnit_Player, selectedAbilityInt))
                {
                    actionInfoText.text = ab.costMessage;
                }
                else
                {
                    actionInfoText.text = ab.parametersMessage;
                }
            }


            // Move line check
            if (selectedAbilityInt != 1 || selectedPosition == new Vector3Int(-1, -1, -1))
            {
                moveLine.gameObject.SetActive(false);
               
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

        selectedUnit_Player.UseAbility(selectedAbilityInt, selectedUnit_Player, selectedUnit_Enemy, selectedPosition);

        selectedAbilityInt = -1;
        UpdatePlayerActionGFX();
    }

    public void CancelAction()
    {
        selectedAbilityInt = -1;
        selectedUnit_Enemy = null;
        UpdatePlayerActionGFX();
    }

    public void StartNewTurn()
    {

        turnState = TurnState.Drawing;

        selectedAbilityInt = -1;

        foreach (UnitAIBase unit in commanders[playerAllianceInt].activeUnits)
        {
            unit.actionPips = 1;
            unit.movementPips = 1;

            if (unit.activeAbilities != null)
            {
                if (unit.activeAbilities.Count > 0)
                {
                    int count = unit.activeAbilities.Count;
                    for (int i = 0; i < count; i++)
                    {
                        AbilityObj obj = unit.activeAbilities[i];
                        obj.turnsRemaining--;

                        if (obj.turnsRemaining <= 0)
                        {
                            Abilities.EndAbility(obj.referencedAbility, unit);
                            unit.activeAbilities.Remove(obj);
                            i--;
                            count--;
                        }
                        else
                        {
                            Abilities.TickAbility(obj.referencedAbility, unit);
                            obj.turnsRemaining--;
                        }

                    }
                }
            }

        }

        foreach (Structure s in GameObject.Find("Structures").GetComponentsInChildren<Structure>())
        {
            s.CheckOwnership();
            s.HideGFX();
        }

        lastGFXOBJs.Clear();
        //currentGFXOBJs.Clear();
        RefreshFOW();
        GameManager.instance.UpdateVisableFOW();

        controllsLocked = false;
        enemyTurnIndicator.SetActive(false);
        endTurnButton.SetActive(true);
        selectedUnit_Player = null;
        selectedUnit_Enemy = null;
        selectedAbilityInt = -1;
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

    // Trun state logic
    public void EndPlayerTurn()
    {

        CopyStates(commanders[playerAllianceInt].GFXOBJs, MapBuilder.instance.GFXOBJs);

        foreach(Structure s in commanders[playerAllianceInt].ownedStructures)
        {
            s.HideGFX();
        }

        foreach (UnitAIBase u in commanders[playerAllianceInt].activeUnits)
        {
            u.gfx.SetActive(false);
        }

        selectedAbilityInt = -1;
        selectedPosition = new Vector3Int(-1, -1, -1);
        selectedUnit_Player = null;
        selectedUnit_Enemy = null;
        UpdatePlayerActionGFX();

        //[CHANGE: DO NOT HARD CODE FOR ALLIENCE INTS]
        //if(playerAllianceInt == 0)
        //{
        //    enemyAI.BeginTurn(1);

        //}else
        //{
        //    enemyAI.BeginTurn(0);

        //}



        controllsLocked = true;
        enemyTurnIndicator.SetActive(true);
        endTurnButton.SetActive(false);


        // Cycles through commanders
        if (playerAllianceInt < commanders.Count-1)
        {
            playerAllianceInt++;
        }
        else
        {
            playerAllianceInt = 0;
        }

        StartNewTurn();
        
    }

    public void UpdateVisableFOW()
    {
        Commander cInput = GameManager.instance.commanders[playerAllianceInt];

        // clear out last frame list, set = to current frame list

        currentGFXOBJs = cInput.visableGFXOBJs;

        lastGFXOBJs = new List<GFXOBJContainter>(currentGFXOBJs);

        currentGFXOBJs.Clear();

        foreach (UnitAIBase u in cInput.activeUnits)
        {
            u.gfx.SetActive(true);

            if (u != null && u.currentHealth > 0)
            {
                MapBuilder.instance.UpdateFOW(u);
            }
        }

        foreach (Structure st in cInput.ownedStructures)
        {
            st.ShowGFX();
            if (!commanders[playerAllianceInt].knownStructures.Contains(st))
            {
                commanders[playerAllianceInt].knownStructures.Add(st);
            }

            Collider[] structTiles = Physics.OverlapBox(st.tilemapPos, new Vector3(st.WidthHeight.x, 1, st.WidthHeight.y), Quaternion.identity, LayerMask.GetMask("OBJFinder"));

            foreach (Collider obj in structTiles)
            {
                currentGFXOBJs.Add(obj.GetComponent<GFXOBJContainter>());
            }

        }

        if (lastGFXOBJs.Count > 0)
        {
            foreach (GFXOBJContainter t in lastGFXOBJs)
            {
                if (t != null)
                {
                    t.SetState(2);
                }
            }
        }

        if (currentGFXOBJs.Count > 0)
        {
            foreach (GFXOBJContainter t in currentGFXOBJs)
            {
                if (t != null)
                {
                    t.SetState(1);
                }
            }
        }

        foreach (Commander c in commanders)
        {
            if (c.allianceInt != playerAllianceInt)
            {
                foreach (UnitAIBase u in c.activeUnits)
                {
                    if (cInput.visableTiles.Contains(u.currentPos))
                    {
                        u.gfx.SetActive(true);
                    }
                    else
                    {
                        u.gfx.SetActive(false);

                    }
                }

            }
        }

        foreach (Structure s in GameObject.Find("Structures").GetComponentsInChildren<Structure>())
        {
            if (cInput.visableTiles.Contains(s.tilemapPos) && s.ownerAlliance != playerAllianceInt)
            {
                s.ShowGFX();
                if (!commanders[playerAllianceInt].knownStructures.Contains(s))
                {
                    Debug.Log("Adding");
                    commanders[playerAllianceInt].knownStructures.Add(s);
                }
            }
            else if (s.ownerAlliance != playerAllianceInt && !commanders[playerAllianceInt].knownStructures.Contains(s))
            {
                s.HideGFX();
            }
        }


    }

    public void RefreshFOW()
    {

        //foreach (GFXOBJContainter c in MapBuilder.instance.GFXOBJs)
        //{
        //    c.SetState(0);
        //}

        //CopyStates(MapBuilder.instance.GFXOBJs, commanders[playerAllianceInt].GFXOBJs);

        for (int i = 0; i < MapBuilder.instance.GFXOBJs.Length; i++)
        {
            MapBuilder.instance.GFXOBJs[i].SetState(commanders[playerAllianceInt].GFXOBJs[i].state);
        }

        //commanders[playerAllianceInt].visableTiles.Clear();
        //foreach (UnitAIBase u in commanders[playerAllianceInt].activeUnits)
        //{
        //    MapBuilder.instance.UpdateFOW(u);
        //}

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

    public void HighlightDeployableTiles(int type)
    {
        foreach (Structure s in commanders[playerAllianceInt].ownedStructures)
        {
            if (((type == 0 && s.provideGround) || (type == 1 && s.provideHeavy) || (type == 2 && s.provideAir)))
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

    public void ClearOverlay(string input)
    {
        switch (input)
        {
            case "Structures":

                foreach (Structure s in commanders[playerAllianceInt].ownedStructures)
                {

                    for (int x = -s.WidthHeight.x; x <= s.WidthHeight.x; x++)
                    {
                        for (int z = -s.WidthHeight.y; z <= s.WidthHeight.y; z++)
                        {
                            int newX = s.tilemapPos.x + x;
                            int newY = s.tilemapPos.z + z;
                            if (MapBuilder.instance.FOWtiles[newX, s.tilemapPos.y, newY] != null)
                            {
                                MapBuilder.instance.FOWtiles[newX, s.tilemapPos.y, newY].GetComponent<TileOverlayLogic>().SetOverlay("FOW_hidefow");
                                deployableTiles.Clear();
                            }
                        }
                    }

                }

                break;

            default:

                break;
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

    public void ToggleSkipDeployment()
    {
        skipDeployment = !skipDeployment;
    }

    public void ChangeSelectedAbilityInt(bool positive)
    {
        if (selectedUnit_Player != null)
        {
            int desieredValue = selectedAbilityInt;

            if (positive)
            {
                desieredValue++;
            }
            else
            {
                desieredValue--;
            }

            if (desieredValue > selectedUnit_Player.abilityInts.Count - 1)
            {
                desieredValue = 0;
            }
            else if (desieredValue < 0)
            {
                desieredValue = selectedUnit_Player.abilityInts.Count - 1;
            }

            selectedAbilityInt = desieredValue;

            //AttackAbilityIcon
            UpdatePlayerActionGFX();
        }
    }

    private void LateUpdate()
    {
        ManageCursor();
    }

    private void ManageCursor()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)gameUI_Canvas.transform, Input.mousePosition, gameUI_Canvas.worldCamera, out pos);

        cursor.transform.position = gameUI_Canvas.transform.TransformPoint(pos);
    }

    private void CopyStates(GFXOBJContainter[] to, GFXOBJContainter[] from)
    {

        for (int i = 0; i < to.Length; i++)
        {
            to[i].state = from[i].state;
        }
    }
}
