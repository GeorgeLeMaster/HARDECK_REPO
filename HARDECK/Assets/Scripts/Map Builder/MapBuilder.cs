using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class TileInfo_Class
{
    public Vector3Int tilemapPosition;

    public bool isRamp;
    public TileInfo.Directions rampOrientation;

    public TileInfo_Class nextTile = null;
    public float pathCost = -1f;
    public bool isChecked = false;

    public TileInfo_Class() { }

    public TileInfo_Class(TileInfo input)
    {
        tilemapPosition = input.tilemapPosition;
        isRamp = input.isRamp;
        pathCost = input.pathCost;
        isChecked = input.isChecked;

        rampOrientation = input.rampOrientation;
    }
}

public class Flowfield
{

    public Flowfield(TileInfo op)
    {
        tiles = new TileInfo_Class[MapBuilder.instance.mapX+1, MapBuilder.instance.mapY + 1, MapBuilder.instance.mapZ + 1];

        foreach(TileInfo tile in MapBuilder.instance.Tiles)
        {
            if (tile != null)
            {
                Vector3Int pos = tile.tilemapPosition;

                if (tiles[pos.x, pos.y, pos.z] == null)
                {
                    tiles[pos.x, pos.y, pos.z] = new TileInfo_Class(tile);
                }
            }
        }

        originPoint = new TileInfo_Class(op);
    }

    public TileInfo_Class originPoint = null;

    public TileInfo_Class[,,] tiles;

}


[ExecuteAlways]
public class MapBuilder : MonoBehaviour
{
    public static MapBuilder instance;

    private GameObject mapBuildLimitObj;

    public UnityEngine.Object tilePrefab;

    public string mapFilePath;

    public int mapX;
    public int mapZ;
    public int mapY;

    public Flowfield[,,] Flowfields;


    public TileInfo[,,] Tiles;

    public GameObject FOWPrefab;
    public GameObject[,,] FOWtiles;

    [Header("Map File Components")]
    public TextAsset[] MapFiles;
    public int selectedMapID;

    public GameObject[] GFXOBJ_TileSets;

    private void Awake()
    {
        // Singleton Logic
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        MapFiles = Resources.LoadAll<TextAsset>("MapFiles");
        GFXOBJ_TileSets = Resources.LoadAll<GameObject>("GFXOBJs");
    }

    public void Start()
    {

        //LoadMapV2();
        //Debug.ClearDeveloperConsole();
        //LoadMapFromFile();
    }


    public void LoadMapV2()
    {
        ClearMap();
        if (Application.isEditor)
        {
            PlayerPrefs.SetInt("pref_selectedMapId", selectedMapID);
        }
        selectedMapID = PlayerPrefs.GetInt("pref_selectedMapId");
        // Find mapBuildLimitObj
        mapBuildLimitObj = GameObject.Find("MapBuildLimit");
        Debug.Log("Loading map from " + MapFiles[selectedMapID].name + ".txt");
        // Get Text Asset from Database
        TextAsset mapTextAsset = MapFiles[selectedMapID];
        //Debug.Log(mapTextAsset);

        // Declares Vars
        Vector3 newPos_gfx = Vector3.zero;
        Vector3Int newPos_tile = Vector3Int.zero;

        // Split by line
        string[] lines = mapTextAsset.text.Split('\n');

        int currentLineInt = 0;

        // Readline AND SET POSITION OF MapBuildLimitObj
        // ALSO ASSIGN mapX, mapY, AND mapZ
        string[] limitObjPos = lines[2].Split(",");
        float.TryParse(limitObjPos[0], out float limX);
        float.TryParse(limitObjPos[1], out float limY);
        float.TryParse(limitObjPos[2], out float limZ);
        mapX = Mathf.FloorToInt(limX);
        mapY = Mathf.FloorToInt(limY);
        mapZ = Mathf.FloorToInt(limZ);

        Vector3 limPos = new Vector3(mapX, mapY, mapZ);
        mapBuildLimitObj.transform.position = limPos;

        // PREP TO READ TILE DATA
        Tiles = new TileInfo[mapX + 1, mapY + 1, mapZ + 1];
        string currentLine;

        currentLineInt = 4;
        while (!(currentLine = lines[currentLineInt]).Contains("fileEnd"))
        {

            //Debug.Log(currentLine);
            if (currentLine != null)
            {
                // PARSE LINE DATA INTO SEPERATE MEMBERS
                string[] dataMembers = currentLine.Split("*");

                // DELIMINATE
                string[] posValues = dataMembers[0].Split(',');

                // ASSIGN POSITIONS FROM PARSED DATA
                float.TryParse(posValues[0], out float xPos_gfx);
                float.TryParse(posValues[1], out float yPos_gfx);
                float.TryParse(posValues[2], out float zPos_gfx);



                // INSTANTIATE TILE AT DESIRED POSITION AND SET PROPER PARENT
                newPos_gfx = new Vector3(xPos_gfx, yPos_gfx, zPos_gfx);
                GameObject newTile = Instantiate(tilePrefab, newPos_gfx, Quaternion.identity) as GameObject;
                newTile.transform.parent = GameObject.Find("Tiles").transform;

                // GRAB NEWTILE'S TileInfo SCRIPT
                TileInfo newTileInfo = newTile.GetComponent<TileInfo>();

                // PARSE AND ASSIGN RAMP STATS
                string[] rampData = dataMembers[1].Split(',');
                if (rampData[0] == "True")
                {
                    // IS A RAMP LOGIC
                    newTileInfo.isRamp = true;

                    // DETERMINES RAMP ORIENTATION
                    if (rampData[1] == "Forwards")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.PosZ;
                    }
                    else if (rampData[1] == "Right")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.NegX;
                    }
                    else if (rampData[1] == "Backwards")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.NegZ;
                    }
                    else
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.PosX;
                    }


                }
                else
                {
                    // IS *NOT* A RAMP LOGIC
                    newTileInfo.isRamp = false;
                }

                // PARSE AND ASSIGN TILEMAP POS

                float.TryParse(posValues[0], out float xPos_tile);
                float.TryParse(posValues[1], out float yPos_tile);
                float.TryParse(posValues[2], out float zPos_tile);

                // INSTANTIATE TILE AT DESIRED POSITION AND SET PROPER PARENT
                newPos_tile = new Vector3Int((int)xPos_tile, (int)yPos_tile, (int)zPos_tile);
                // Debug.Log(newPos_tile);

                newTileInfo.tilemapPosition = newPos_tile;

                if (newPos_tile.x < mapX && newPos_tile.y < mapY && newPos_tile.z < mapZ)
                {
                }

                // ASSIGN NAME
                string rampString = "Tile";
                if (rampData[0] == "True") { rampString = "Ramp"; }
                newTile.name = $"{rampString}-{xPos_tile},{yPos_tile},{zPos_tile}";

                // SLOT INTO Tiles ARRAY

                Tiles[newPos_tile.x, newPos_tile.y, newPos_tile.z] = newTileInfo;

            }
            currentLineInt++;
        }

        // Scenery objs--------------------------------------------------------------------------------------------
        currentLineInt++;

        GameObject sceneryObj = GameObject.Find("Scenery");

        while (!(currentLine = lines[currentLineInt]).Contains("sceneEnd") && currentLine != "")
        {

            string[] dataMembers = currentLine.Split("*");

            //0: string
            //1: id
            //2: pos
            //3: rot
            //4: scale
            dataMembers[2] = dataMembers[2].Substring(1, dataMembers[2].Length - 2);
            dataMembers[3] = dataMembers[3].Substring(1, dataMembers[3].Length - 2);

            GameObject[] tilesetObjs = Resources.LoadAll<GameObject>($"GFXOBJs/{dataMembers[0]}");

            string[] posValuesS = dataMembers[2].Split(",");
            float.TryParse(posValuesS[0], out float xPos_gfx);
            float.TryParse(posValuesS[1], out float yPos_gfx);
            float.TryParse(posValuesS[2], out float zPos_gfx);
            
            Vector3 pos = new Vector3(xPos_gfx, yPos_gfx, zPos_gfx);

            string[] rotValuesS = dataMembers[3].Split(",");
            float.TryParse(rotValuesS[0], out float xRot_gfx);
            float.TryParse(rotValuesS[1], out float yRot_gfx);
            float.TryParse(rotValuesS[2], out float zRot_gfx);

            Vector3 rot = new Vector3(xRot_gfx, yRot_gfx, zRot_gfx);

            GameObject newObj =  Instantiate(tilesetObjs[int.Parse(dataMembers[1])], pos, Quaternion.Euler(new Vector3(rot.x, rot.y, rot.z)));
            newObj.transform.SetParent(sceneryObj.transform);
            newObj.transform.localScale = ParseStringToVector3(dataMembers[4]);
         //   Debug.Log($"{dataMembers[0]},{dataMembers[1]},{dataMembers[2]},{dataMembers[3]},{dataMembers[4]}");
            currentLineInt++;
            if (currentLineInt > 10000)
            {

                break;
            }
        }


        //Structures-----------------------------------------------------------------------------------------------
        currentLineInt++;

        GameObject sObj = GameObject.Find("Structures");
        GameObject structurePrefab = Resources.Load("Structures/StructureTest") as GameObject;

        while (!(currentLine = lines[currentLineInt]).Contains("structEnd") && currentLine != "")
        {
            //Debug.Log(currentLine);
            string[] dataMembers = currentLine.Split("*");
            dataMembers[0] = dataMembers[0].Substring(1, dataMembers[0].Length-2);
            dataMembers[1] = dataMembers[1].Substring(1, dataMembers[1].Length - 2);

            string[] posValues = dataMembers[0].Split(",");

            int.TryParse(posValues[0], out int xPos_s);
            int.TryParse(posValues[1], out int yPos_s);
            int.TryParse(posValues[2], out int zPos_s);

            GameObject obj = Instantiate(structurePrefab, new Vector3(xPos_s, yPos_s, zPos_s), Quaternion.identity);


            Structure sLogic = obj.GetComponent<Structure>();

            sLogic.tilemapPos = new Vector3Int(xPos_s, yPos_s, zPos_s);
            posValues = dataMembers[1].Split(",");
            int.TryParse(posValues[0], out int w);
            int.TryParse(posValues[1], out int h);

            sLogic.WidthHeight = new Vector2Int(w,h);

            //Debug.Log(dataMembers[2]+dataMembers[3]+dataMembers[4]);
            sLogic.ownerId = int.Parse(dataMembers[2]);
            sLogic.provideGround = bool.Parse(dataMembers[3]);
            sLogic.provideHeavy = bool.Parse(dataMembers[4]);
            sLogic.provideAir = bool.Parse(dataMembers[5]);

            obj.transform.position = sLogic.tilemapPos;
            obj.transform.SetParent(sObj.transform);
            //GameManager.instance.structures.Add(sLogic);

            currentLineInt++;
            if (currentLineInt > 10000)
            {

                break;
            }
        }

        //Units------------------------------------------------------------------------------------------------------
        currentLineInt++;

        GameObject uObj = GameObject.Find("Units");
        GameObject guPrefab = Resources.Load("Units/OBJs/GroundUnitPrefab") as GameObject;
        // unit SO data (name, moveSpeed, range, damage, damageMod, maxHealth)

        while (!(currentLine = lines[currentLineInt]).Contains("unitEnd") && currentLine != "")
        {
            string[] dataMembers = currentLine.Split("*");

            int aInt = int.Parse(dataMembers[1]);
            string uName = dataMembers[2];

            int.TryParse(dataMembers[3], out int ms);
            float.TryParse(dataMembers[4], out float r);
            float.TryParse(dataMembers[5], out float d);
            float.TryParse(dataMembers[6], out float dm);
            float.TryParse(dataMembers[7], out float mh);

            GameObject newUnit = Instantiate(guPrefab);
            newUnit.transform.parent = uObj.transform;

            UnitAIBase ai = newUnit.GetComponent<UnitAIBase>();

            ai.moveSpeed = ms;
            ai.range = r;
            ai.damage = d;
            ai.damageMod = dm;
            ai.maxHealth = mh;
            ai.currentHealth = mh;

            ai.visionRadius = ms;

            ai.alliance = aInt;
            ai.unitName = uName;

            ai.unitSO = Resources.Load("Units/SOs/Infantryman") as UnitSO;

            Vector3 pos = ParseStringToVector3(dataMembers[8]);
            ai.currentPos = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);

            ai.Spawn(ai.currentPos, Resources.Load("Units/GFX/GFX_InfantryMan") as GameObject);

            currentLineInt++;
            if (currentLineInt > 10000)
            {

                break;
            }
        }

        Debug.Log("Map File Loaded");

        if (Application.isPlaying)
        {
            // Inits flowfields array and builds one for each valid tile
            Flowfields = new Flowfield[mapX + 1, mapY + 1, mapZ + 1];
            for (int y = 0; y <= mapY; y++)
            {
                for (int z = 0; z <= mapZ; z++)
                {
                    for (int x = 0; x <= mapX; x++)
                    {
                        TileInfo currentTile = Tiles[x, y, z];
                        if (currentTile != null)
                        {
                            Flowfield ff = BuildFlowfield(currentTile);
                            Flowfields[currentTile.tilemapPosition.x, currentTile.tilemapPosition.y, currentTile.tilemapPosition.z] = ff;
                        }
                    }
                }
            }




            // Builds Overlay
            GameObject fowParent = GameObject.Find("FOW");
            //  Build FOW
            FOWtiles = new GameObject[mapX + 1, mapY + 1, mapZ + 1];
            foreach (TileInfo t in Tiles)
            {
                if (t != null)
                {
                    GameObject newFT = Instantiate(FOWPrefab, t.tilemapPosition, Quaternion.identity);
                    FOWtiles[t.tilemapPosition.x, t.tilemapPosition.y, t.tilemapPosition.z] = newFT;
                    newFT.transform.SetParent(fowParent.transform);
                    newFT.gameObject.GetComponent<TileOverlayLogic>().SetOverlay("FOW_hidefow");
                }
            }

            GameManager.instance.currentFOWTiles = new List<GFXOBJContainter>();
            GameManager.instance.lastFOWTiles = new List<GFXOBJContainter>();

            GameManager.instance.StartCoroutine("firstFOWupdate");
            GameManager.instance.StartPlayerTurn();

        }
    }

    public void SaveMapToFile()
    {
        mapFilePath = "Assets/Resources/MapFiles/" + MapFiles[selectedMapID].name + ".txt";
        // DEBUG INIT
        Debug.Log("Saving map to " + mapFilePath);

        // Find parent and data OBJs
        GameObject tiles = GameObject.Find("Tiles");
        mapBuildLimitObj = GameObject.Find("MapBuildLimit");

        // Init data string; this string is our entire file. Is that a good coding practice? Maybe!
        string dataToSave = "";

        // HEADER
        dataToSave += "Tiles\n*\n";

        // Determine size of map and write to file
        string sizeLine = mapBuildLimitObj.transform.position.ToString();
        sizeLine = sizeLine.Substring(1, sizeLine.Length - 2);
        dataToSave += sizeLine + "\n*\n";

        // Save tile info to file, one tile per line
        foreach (Transform transform in tiles.GetComponentsInChildren<Transform>())
        {
            if (transform.gameObject.GetComponent<TileInfo>() != null)
            {
                // init new tile info
                TileInfo currentTileInfo = transform.gameObject.GetComponent<TileInfo>();

                if (currentTileInfo.tilemapPosition.y <= mapY)
                {
                    string positionString = transform.position.ToString();
                    positionString = positionString.Substring(1, positionString.Length - 2);

                    string rampBool = currentTileInfo.isRamp.ToString();

                    string rampOrientation = currentTileInfo.rampOrientation.ToString();

                    string tilemapPos = currentTileInfo.tilemapPosition.ToString();
                    tilemapPos = tilemapPos.Substring(1, tilemapPos.Length - 2);

                    dataToSave += ($"{positionString}*{rampBool},{rampOrientation}*{tilemapPos}\n");
                }
            }
        }

        // End pathfinding tiles section
        dataToSave += "fileEnd\n";

        // Now we're doing the scenery objects
        // objects will have a position, rotation, scale, folders string, and id string

        // Find scenery parent obj
        GameObject sceneryObj = GameObject.Find("Scenery");

        foreach (Transform t in sceneryObj.GetComponentsInChildren<Transform>())
        {
            GameObject obj = t.gameObject;
            GFXOBJContainter c = obj.GetComponent<GFXOBJContainter>();

            if (obj != null && c != null)
            {
                string newLine = $"{c.tilesetName}*{c.objId}*{t.position}*{t.rotation.eulerAngles}*{t.localScale}\n";
                dataToSave += newLine;
            }
        }

        dataToSave += "sceneEnd\n";

        // Save structure data
        // needs to save: structure pos, widthHeight, ownerInt, grantsBools
        GameObject structObj = GameObject.Find("Structures");

        foreach (Structure st in structObj.GetComponentsInChildren<Structure>())
        {
            if (st != null)
                dataToSave += $"{st.tilemapPos}*{st.WidthHeight}*{st.ownerId}*{st.provideGround}*{st.provideHeavy}*{st.provideAir}\n";
        }

        dataToSave += "structEnd\n";
        // Save unit data for later spawning
        // Order of data:
        // unit SO id
        // unit SO data (name, moveSpeed, range, damage, damageMod, maxHealth)
        GameObject unitObj = GameObject.Find("Units");

        foreach (UnitAIBase t in unitObj.GetComponentsInChildren<UnitAIBase>())
        {
            if (t != null)
            {
                dataToSave += $"{t.unitSO.name}*{t.alliance.ToString()}*{t.unitName}*{t.moveSpeed}*{t.range}*{t.damage}*{t.damageMod}*{t.maxHealth}*{t.currentPos}\n";
            }
        }


        dataToSave += "unitEnd\n";

        // Open writer, write dataToSave string, close reader
        Debug.Log(mapFilePath);
        StreamWriter writer = new StreamWriter(mapFilePath);
        writer.WriteLine(dataToSave);
        writer.Close();
    }

    public void ClearMap()
    {
        // DESTROY AND REPLACE EVERYTHING
        GameObject tiles = GameObject.Find("Tiles");
        DestroyImmediate(tiles);
        GameObject newTilesObj = new GameObject();
        newTilesObj.name = "Tiles";
        newTilesObj.transform.parent = GameObject.Find("Environment").transform;
       // newTilesObj.hideFlags = HideFlags.HideInHierarchy;

        GameObject scenery = GameObject.Find("Scenery");
        DestroyImmediate(scenery);
        GameObject newSceneryObj = new GameObject();
        newSceneryObj.name = "Scenery";
        newSceneryObj.transform.parent = GameObject.Find("Environment").transform;

        GameObject fow = GameObject.Find("FOW");
        DestroyImmediate(fow);
        GameObject newFOWObj = new GameObject();
        newFOWObj.name = "FOW";
        newFOWObj.transform.parent = GameObject.Find("Environment").transform;

        GameObject st = GameObject.Find("Structures");
        DestroyImmediate(st);
        GameObject newSobj = new GameObject();
        newSobj.name = "Structures";
        newSobj.transform.parent = GameObject.Find("Environment").transform;

        GameObject u = GameObject.Find("Units");
        DestroyImmediate(u);
        GameObject newU = new GameObject();
        newU.name = "Units";
        newU.transform.parent = GameObject.Find("Units").transform;
    }

    public Flowfield BuildFlowfield(TileInfo origin)
    {
        // Builds a flowfield for a single tile
        Flowfield result = new Flowfield(origin);

        foreach(TileInfo_Class tile in result.tiles)
        {
            if (tile != null)
            {
                tile.nextTile = null;
                tile.isChecked = false;
            }
        }

        // Check if tile is real
        if (Tiles[origin.tilemapPosition.x, origin.tilemapPosition.y, origin.tilemapPosition.z] != null)
        {

            List<TileInfo_Class> toCheck = new List<TileInfo_Class>();

            TileInfo_Class checkTile;

            // Grab current tile to build ff for
            TileInfo_Class currentTile = result.tiles[origin.tilemapPosition.x, origin.tilemapPosition.y, origin.tilemapPosition.z];
            currentTile.pathCost = 0;
            result.originPoint.nextTile = null;

            // Add it to the toCheckList
            toCheck.Add(result.originPoint);
            //Debug.Log(toCheck.First().tilemapPosition);
            int killswitch = 0;

            // while the list to check isnt empty and we havent tripped the killswitch
            while (toCheck.Count() > 0)
            {
                //Debug.Log(toCheck.Count());
                checkTile = toCheck.First();

                for (int yOff = -1; yOff <= 1; yOff++)
                {
                    for (int xOff = -1; xOff <= 1; xOff++)
                    {
                        for (int zOff = -1; zOff <= 1; zOff++)
                        {
                            int newX = checkTile.tilemapPosition.x + xOff;
                            int newY = checkTile.tilemapPosition.y + yOff;
                            int newZ = checkTile.tilemapPosition.z + zOff;

                            if (IsValidTile(newX, newY, newZ))
                            {
                                TileInfo_Class newTile = result.tiles[newX, newY, newZ];

                                if ((yOff == -1 && checkTile.isRamp || yOff == 0 || (yOff == 1 && newTile.isRamp)) && (!(zOff == 0 && xOff == 0)) && checkTile != null)
                                {
                                    if (TilesConnected(checkTile, newTile))
                                    {

                                        if (newTile.nextTile == null)
                                        {
                                            float pathMod = 1f;
                                            if (xOff != 0 && zOff != 0)
                                            {
                                                pathMod++;
                                            }
                                            newTile.pathCost = checkTile.pathCost + pathMod;
                                            newTile.nextTile = checkTile;
                                        }
                                        else
                                        {
                                            if (checkTile.nextTile != null)
                                            {
                                                Vector3 a = checkTile.tilemapPosition - newTile.tilemapPosition;
                                                Vector3 b = checkTile.nextTile.tilemapPosition - checkTile.tilemapPosition;

                                                a = new Vector3(a.x, 0, a.z);
                                                b = new Vector3(b.x, 0, b.z);

                                                a.Normalize();
                                                b.Normalize();

                                                if (a == b)
                                                {
                                                    float pathMod = 1f;
                                                    if (xOff != 0 && zOff != 0)
                                                    {
                                                        pathMod += 0.34f;
                                                    }
                                                    newTile.pathCost = checkTile.pathCost + pathMod;
                                                    newTile.nextTile = checkTile;
                                                }
                                            }


                                        }

                                        if (newTile.isChecked == false && !toCheck.Contains(newTile))
                                        {

                                            toCheck.Add(newTile);
                                        }

                                    }
                                }
                            }
                        } // Offset loops
                    } // " "
                } // " "

                checkTile.isChecked = true;
                toCheck.Remove(toCheck.First());
                
                killswitch++;

                if (killswitch > (10000))
                {
                    Debug.Log("Killswitch");
                    break;
                }
            } // toCheck loop
        }

        result.originPoint.nextTile = null;
        return result;
    }
    private bool IsValidTile(int x, int y, int z)
    {
        bool result = true;

        if (mapX < x || mapY < y || mapZ < z || x < 0 || y < 0 || z < 0)
        {
            return false;
        }

        if (Tiles[x, y, z] == null)
        {
            return false;
        }

        return result;
    }

    void OnDrawGizmos()
    {
        mapBuildLimitObj = GameObject.Find("MapBuildLimit");

        mapX = (int)mapBuildLimitObj.transform.position.x;
        mapY = (int)mapBuildLimitObj.transform.position.y;
        mapZ = (int)mapBuildLimitObj.transform.position.z;


        Gizmos.color = Color.green;

        Vector3 pos_1 = new Vector3(-0.5f       , 0, -0.5f);
        Vector3 pos_2 = new Vector3(-0.5f       , 0, mapZ + 0.5f);
        Vector3 pos_3 = new Vector3(mapX + 0.5f , 0, mapZ + 0.5f);
        Vector3 pos_4 = new Vector3(mapX + 0.5f , 0, -0.5f);

        Vector3 pos_5 = new Vector3(-0.5f       , mapY, -0.5f);
        Vector3 pos_6 = new Vector3(-0.5f       , mapY, mapZ + 0.5f);
        Vector3 pos_7 = new Vector3(mapX + 0.5f , mapY, mapZ + 0.5f);
        Vector3 pos_8 = new Vector3(mapX + 0.5f , mapY, -0.5f);

        Gizmos.DrawLine(pos_1, pos_2);
        Gizmos.DrawLine(pos_2, pos_3);
        Gizmos.DrawLine(pos_3, pos_4);
        Gizmos.DrawLine(pos_4, pos_1);

        Gizmos.DrawLine(pos_5, pos_6);
        Gizmos.DrawLine(pos_6, pos_7);
        Gizmos.DrawLine(pos_7, pos_8);
        Gizmos.DrawLine(pos_8, pos_5);

        Gizmos.DrawLine(pos_1, pos_5);
        Gizmos.DrawLine(pos_2, pos_6);
        Gizmos.DrawLine(pos_3, pos_7);
        Gizmos.DrawLine(pos_4, pos_8);

    }

    public List<Vector3> GetPath(Vector3Int to, Vector3Int from)
    {
        

        List<Vector3> pathPositions = new List<Vector3>();

        Flowfield ff = MapBuilder.instance.Flowfields[to.x, to.y, to.z];

        TileInfo_Class checkPos = ff.tiles[from.x, from.y, from.z];

        if (checkPos.pathCost != -1)
        {

            Vector3 offset = new Vector3(0, 0.05f, 0);
            while (checkPos.tilemapPosition != to)
            {
                if (checkPos.isRamp)
                {
                    offset = new Vector3(0, -0.3f, 0);
                }
                else
                {
                    offset = new Vector3(0, 0.05f, 0);
                }

                pathPositions.Add(checkPos.tilemapPosition + offset);
                checkPos = checkPos.nextTile;
            }

            if (checkPos.isRamp)
            {
                offset = new Vector3(0, -0.3f, 0);
            }
            else
            {
                offset = new Vector3(0, 0.05f, 0);
            }

            pathPositions.Add(checkPos.tilemapPosition + offset);
            checkPos = checkPos.nextTile;

            // Debug.Log(result.Count);

        }

        return pathPositions;
    }

    bool TilesConnected(TileInfo_Class a, TileInfo_Class b)
    {
        TileInfo_Class rampTile = new TileInfo_Class();
        TileInfo_Class flatTile = new TileInfo_Class();

        if (a.isRamp && b.isRamp)
        {
            if (a.rampOrientation == b.rampOrientation)
            {
                return true;
            }
        }
        else if (a.isRamp)
        {
            rampTile = a;
            flatTile = b;
        }
        else if (b.isRamp)
        {
            rampTile = b;
            flatTile = a;
        }
        else
        {
            int xOff = a.tilemapPosition.x - b.tilemapPosition.x;
            int zOff = a.tilemapPosition.z - b.tilemapPosition.z;

            if (xOff == 0 || zOff == 0)
            {
                return true;
            }

            Vector3Int newPosA = a.tilemapPosition + new Vector3Int(-xOff, 0, 0);
            Vector3Int newPosB = a.tilemapPosition + new Vector3Int(0, 0, -zOff);

            if (Tiles[newPosA.x, newPosA.y, newPosA.z] != null && Tiles[newPosB.x, newPosB.y, newPosB.z] != null)
            {
                return true;
            }

            return false;

            //return true;
        }

        Vector3Int upPos = rampTile.tilemapPosition;
        Vector3Int downPos = rampTile.tilemapPosition;

        if (rampTile.rampOrientation == TileInfo.Directions.PosZ)
        {
            upPos += new Vector3Int(0, 0, -1);
            downPos += new Vector3Int(0, -1, 1);

        }
        else if (rampTile.rampOrientation == TileInfo.Directions.PosX)
        {
            upPos += new Vector3Int(-1, 0, 0);
            downPos += new Vector3Int(1, -1, 0);

        }
        else if (rampTile.rampOrientation == TileInfo.Directions.NegZ)
        {
            upPos += new Vector3Int(0, 0, 1);
            downPos += new Vector3Int(0, -1, -1);

        }
        else
        {
            upPos += new Vector3Int(1, 0, 0);
            downPos += new Vector3Int(-1, -1, 0);
        }

        if (flatTile.tilemapPosition == upPos || flatTile.tilemapPosition == downPos)
        {
            return true;
        }

        return false;
    }

    public void UpdateFOW(UnitAIBase input)
    {
        //Debug.Log("a");
        float yOff = 0.0f;
        Vector3 pos = input.currentPos + new Vector3(0,0.5f,0);
        LayerMask mask = LayerMask.GetMask("OBJFinder");

        Collider[] fowTiles = Physics.OverlapCapsule(new Vector3(pos.x, -10, pos.z), new Vector3(pos.x, 10, pos.z), input.visionRadius, mask);

        //Debug.Log(fowTiles.Length);
        //Debug.Log()
        foreach (Collider c in fowTiles)
        {
            Vector3 castPoint = c.transform.position + new Vector3(0, yOff, 0);
            //c.transform.position+new Vector3(0, yOff, 0)
            // raycst from input pos to c pos, if hit something, nothing, if not, hit tile
            Vector3 dir = castPoint - pos;
         //  dir.Normalize();
            float dist = Vector3.Distance(pos, castPoint);
            if (!GameManager.instance.currentFOWTiles.Contains(c.gameObject.GetComponent<GFXOBJContainter>()))
            {
                RaycastHit hit;
                if (!Physics.Raycast(pos, dir.normalized, out hit, dist, mask))
                {
                    GameManager.instance.currentFOWTiles.Add(c.gameObject.GetComponent<GFXOBJContainter>());
                }
                else
                {
                    if (hit.transform.gameObject == c.gameObject)
                    {
                        GameManager.instance.currentFOWTiles.Add(c.gameObject.GetComponent<GFXOBJContainter>());

                    }
                    else if (Vector3.Distance(hit.transform.position, c.transform.position) < 1f)
                    {
                        GameManager.instance.currentFOWTiles.Add(c.gameObject.GetComponent<GFXOBJContainter>());

                    }
                }
            }

        }
    }

    private Vector3 ParseStringToVector3(string input)
    {
      //  Debug.Log(input);
        Vector3 result = Vector3.one;

        input = input.Substring(1, input.Length - 3);

        string[] dataMembers = input.Split(',');
        //Debug.Log(input);
        float.TryParse(dataMembers[0], out result.x);

        float.TryParse(dataMembers[1], out result.y);

        float.TryParse(dataMembers[2], out result.z);

        return result;
    }
}
