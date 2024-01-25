using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class TileInfo_Class
{
    public Vector3Int tilemapPosition;

    public bool isRamp;
    public TileInfo.Directions rampOrientation;

    public TileInfo_Class nextTile = null;
    public float pathCost;
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
    }

    public void Start()
    {



        //Debug.ClearDeveloperConsole();
        //LoadMapFromFile();
    }

    public void LoadMapFromFile()
    {

        // Find mapBuildLimitObj
        mapBuildLimitObj = GameObject.Find("MapBuildLimit");

        // Clears old map 
        ClearMap();

        Debug.Log("Loading Map from " + mapFilePath);

        // Declares Vars
        Vector3 newPos_gfx = Vector3.zero;
        Vector3Int newPos_tile = Vector3Int.zero;

        // Create and open reader
        StreamReader reader = new StreamReader(mapFilePath);

        // Jump to data
        reader.ReadLine();
        reader.ReadLine();

        // Readline AND SET POSITION OF MapBuildLimitObj
        // ALSO ASSIGN mapX, mapY, AND mapZ
        string[] limitObjPos = reader.ReadLine().Split(",");
        float.TryParse(limitObjPos[0], out float limX);
        float.TryParse(limitObjPos[1], out float limY);
        float.TryParse(limitObjPos[2], out float limZ);
        mapX = Mathf.FloorToInt(limX);
        mapY = Mathf.FloorToInt(limY);
        mapZ = Mathf.FloorToInt(limZ);

        Vector3 limPos = new Vector3(mapX, mapY, mapZ);
        mapBuildLimitObj.transform.position = limPos;

        // JUMP LINE
        reader.ReadLine();

        // PREP TO READ TILE DATA
        Tiles = new TileInfo[mapX + 1, mapY + 1, mapZ + 1];
        string currentLine;

        while ((currentLine = reader.ReadLine()) != "fileEnd") 
        {

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
                        newTileInfo.rampOrientation = TileInfo.Directions.Forwards;
                    }
                    else if (rampData[1] == "Right")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.Right;
                    }
                    else if (rampData[1] == "Backwards")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.Backwards;
                    }
                    else
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.Left;
                    }


                }
                else
                {
                    // IS *NOT* A RAMP LOGIC
                    newTileInfo.isRamp = false;
                }

                // PARSE AND ASSIGN TILEMAP POS
                string[] tilemapValues = dataMembers[2].Split(",");

                float.TryParse(posValues[0], out float xPos_tile);
                float.TryParse(posValues[1], out float yPos_tile);
                float.TryParse(posValues[2], out float zPos_tile);

                // INSTANTIATE TILE AT DESIRED POSITION AND SET PROPER PARENT
                newPos_tile = new Vector3Int((int)xPos_tile, (int)yPos_tile, (int)zPos_tile);
               // Debug.Log(newPos_tile);

                newTileInfo.tilemapPosition = newPos_tile;

                if (newPos_tile.x < mapX && newPos_tile.y < mapY && newPos_tile.z < mapZ) {
                }

                // ASSIGN NAME
                string rampString = "Tile";
                if (rampData[0] == "True") { rampString = "Ramp"; }
                newTile.name = $"{rampString}-{xPos_tile},{yPos_tile},{zPos_tile}";

                // SLOT INTO Tiles ARRAY

                Tiles[newPos_tile.x, newPos_tile.y, newPos_tile.z] = newTileInfo;

            }
        }


        Debug.Log("Map File Loaded");

        reader.Close();

        if (Application.isPlaying)
        {
            BuildFlowfields();

            //foreach(TileInfo tile in Tiles)
            //{
            //    Debug.Log(tile.tilemapPosition);
            //}

            //foreach (Flowfield ff in Flowfields)
            //{
            //    int i = 0;
            //    foreach (TileInfo tile in ff.tiles)
            //    {
            //        if (tile.nextTile != null)
            //        {
            //            Debug.Log($"{tile.tilemapPosition}, {tile.nextTile.tilemapPosition}");

            //        }
            //    }

            //    if (ff != null)
            //    {
            //        Debug.Log($"{ff.originPoint.tilemapPosition}, {i}");
            //    }
            //}

            //DrawFlowfield(Flowfields[3, 0, 3]);

        }
        // Decide what is and isnt pathable

        //Debug.Log(MapBuilder.instance.pathableMapTiles.Length);

    }

    public void SaveMapToFile()
    {

        // DEBUG INIT
        Debug.Log("Saving map to " + mapFilePath);

        // FIND PARENT OBJ
        GameObject tiles = GameObject.Find("Tiles");

        // OPEN WRITER AND TITLE FILE
        StreamWriter writer = new StreamWriter(mapFilePath);
        mapBuildLimitObj = GameObject.Find("MapBuildLimit");
        string sizeLine = mapBuildLimitObj.transform.position.ToString();
        sizeLine = sizeLine.Substring(1, sizeLine.Length - 2);

        writer.WriteLine($"Tiles\n*\n{sizeLine}\n*");

        // RECORDS TILE OBJ POSITIONS AND RAMP INFO AS RECORDED IN TILEINFO
        foreach(Transform transform in tiles.GetComponentsInChildren<Transform>())
        {
            if (transform.gameObject.GetComponent<TileInfo>() != null)
            {
                

                TileInfo currentTileInfo = transform.gameObject.GetComponent<TileInfo>();

                if (currentTileInfo.tilemapPosition.y <= mapY)
                {

                    string positionString = transform.position.ToString();
                    positionString = positionString.Substring(1, positionString.Length - 2);

                    string rampBool = currentTileInfo.isRamp.ToString();

                    string rampOrientation = currentTileInfo.rampOrientation.ToString();

                    string tilemapPos = currentTileInfo.tilemapPosition.ToString();
                    tilemapPos = tilemapPos.Substring(1, tilemapPos.Length - 2);


                    writer.WriteLine($"{positionString}*{rampBool},{rampOrientation}*{tilemapPos}");
                }
            }
        }

        writer.WriteLine("fileEnd");

        writer.Close();
    }

    public void ClearMap()
    {
        GameObject tiles = GameObject.Find("Tiles");

        DestroyImmediate(tiles);

        GameObject newTilesObj = new GameObject();
        newTilesObj.name = "Tiles";
        newTilesObj.transform.parent = GameObject.Find("Environment").transform;

        
    }

    public void BuildFlowfields()
    {


        // Initalize checklist

        Debug.Log($"{mapX}, {mapY}, {mapZ}");
        // Initalize flowfields array

        Flowfields = new Flowfield[mapX+1, mapY+1, mapZ+1];
        //Debug.Log(Flowfields.Length);

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

    }

    public Flowfield BuildFlowfield(TileInfo origin)
    {
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
           // Debug.Log(origin.tilemapPosition);
            TileInfo_Class currentTile = result.tiles[origin.tilemapPosition.x, origin.tilemapPosition.y, origin.tilemapPosition.z];
            currentTile.pathCost = 0;
           // result.originPoint = currentTile;
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
                                            newTile.nextTile = checkTile;
                                            //newTile.nextTile = result.originPoint;

                                        }
                                        else
                                        {
                                            //newTile.nextTile = result.originPoint;

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

    public void DrawFlowfield(Flowfield ff)
    {
        GameObject ap = GameObject.Find("ArrowParent");

        DestroyImmediate(ap);

        GameObject newAp = new GameObject();
        newAp.name = "ArrowParent";
        newAp.transform.parent = GameObject.Find("Environment").transform;
        

        //Debug.Log(ff.originPoint.tilemapPosition);
        foreach(TileInfo_Class tile in ff.tiles)
        {
            if (tile != null)
            {
                if (tile.tilemapPosition != ff.originPoint.tilemapPosition && tile.nextTile != null)
                {
                    GameObject arrow = Instantiate(GameManager.instance.arrowPrefab);
                    arrow.transform.position = tile.tilemapPosition;
                    arrow.transform.LookAt(tile.nextTile.tilemapPosition);
                    arrow.transform.parent = newAp.transform;
                }
            }
        }
    }

    void DrawPath(List<Vector3> input )
    {
        //Debug.Log(input.Count());
        //foreach(Vector3 pos in input)
        //{
        //    Debug.Log(pos);
        //}

        LineRenderer lr;

        if (gameObject.GetComponent<LineRenderer>())
        {
            lr = gameObject.GetComponent<LineRenderer>();
        }
        else
        {
            lr = gameObject.AddComponent<LineRenderer>();
        }

        lr.alignment = LineAlignment.View;
        lr.startWidth = 0.05f;
        lr.numCapVertices = 3;
        lr.numCornerVertices = 3;

        lr.positionCount = input.Count;

        for (int i = 0; i < input.Count; i++)
        {
            lr.SetPosition(i, input[i] + new Vector3(0, 0.1f, 0));
        }
    }

    List<Vector3> GetPath(Vector3Int to, Vector3Int from)
    {


        List<Vector3> result = new List<Vector3>();

        Flowfield ff = Flowfields[to.x, to.y, to.z];


        TileInfo_Class currentTile = ff.tiles[to.x, to.y, to.z];


        int killswitch = 0;


        while (currentTile.nextTile != null && killswitch < 1000)
        {
            result.Add(currentTile.tilemapPosition);
            Debug.Log(currentTile);
            if (currentTile.nextTile != null)
            currentTile = currentTile.nextTile;

            killswitch++;
        }

       // Debug.Log(result.Count);
        return result;
    }

    bool TilesConnected(TileInfo_Class a, TileInfo_Class b)
    {
        TileInfo_Class rampTile;
        TileInfo_Class flatTile;

        if (a.isRamp && b.isRamp)
        {
            return true;
        }

        if (a.isRamp)
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
            return true;
        }

        Vector3Int upPos = rampTile.tilemapPosition;
        Vector3Int downPos = rampTile.tilemapPosition;

        if (rampTile.rampOrientation == TileInfo.Directions.Forwards)
        {
            upPos += new Vector3Int(0,0,-1);
            downPos += new Vector3Int(0, -1, 1);

        }
        else if (rampTile.rampOrientation == TileInfo.Directions.Right)
        {
            upPos += new Vector3Int(-1, 0, 0);
            downPos += new Vector3Int(1, -1, 0);

        }
        else if (rampTile.rampOrientation == TileInfo.Directions.Backwards)
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

}
