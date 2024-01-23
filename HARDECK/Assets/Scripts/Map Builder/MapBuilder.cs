using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class Flowfield
{
    public Flowfield()
    {
        tiles = MapBuilder.instance.Tiles;
    }

    public TileInfo orginPoint;

    public TileInfo[,,] tiles;

}

[ExecuteAlways]
public class MapBuilder : MonoBehaviour
{
    public static MapBuilder instance;

    private GameObject mapBuildLimitObj;

    public UnityEngine.Object tilePrefab;

    public string mapFilePath = "testMap2.txt";

    public int mapX;
    public int mapZ;
    public int mapY;

    public Flowfield[,,] Flowfields;


    public TileInfo[,,] Tiles;

    public Vector3Int to = new Vector3Int( 1, 0, 1 );
    public Vector3Int from = new Vector3Int( 3, 0, 3 );

    public void Start()
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


        Debug.ClearDeveloperConsole();
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
                //Debug.Log(newPos_tile);

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

            //foreach (Flowfield ff in Flowfields)
            //{
            //    if (ff != null)
            //    {
            //        Debug.Log(ff.orginPoint.tilemapPosition);
            //    }
            //}

             DrawPath(GetPath(to, from));

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

        foreach(TileInfo currentTile in Tiles)
        {
            Vector3Int pos = currentTile.tilemapPosition;
            //Debug.Log(pos);
            Flowfield ff = BuildFlowfield(pos);

            Flowfields[pos.x, pos.y, pos.z] = ff;
        }

    }

    public Flowfield BuildFlowfield(Vector3Int pos)
    {
        Flowfield result = new Flowfield();

        List<TileInfo> toCheck = new List<TileInfo>();

        // Check if tile is real
        if (Tiles[pos.x, pos.y, pos.z] != null)
        {

            // Grab current tile to build ff for
            TileInfo currentTile = result.tiles[pos.x, pos.y, pos.z];
            currentTile.pathCost = 0;
            result.orginPoint = currentTile;
            result.orginPoint.nextTile = null;

            // Add it to the toCheckList
            toCheck.Add(result.orginPoint);

            int killswitch = 0;

            // while the list to check isnt empty and we havent tripped the killswitch
            while (toCheck.Count() > 0 && killswitch < (10000))
            {
                TileInfo checkTile = toCheck.First();

                TileInfo newTile;

                for (int yOff = -1; yOff <= 1; yOff++)
                {
                    for (int zOff = -1; zOff <= 1; zOff++)
                    {
                        for (int xOff = -1; xOff < 1; xOff++)
                        {
                            // Inspecting each tile surrounding currentTile

                            // Make sure its not the center tile
                            if (!(zOff == 0 && xOff == 0))
                            {
                                // Coords of would-be checkTile
                                int newX = checkTile.tilemapPosition.x + xOff;
                                int newY = checkTile.tilemapPosition.y + yOff;
                                int newZ = checkTile.tilemapPosition.z + zOff;

                                // Make sure its not a null reference
                                if (IsValidTile(newX, newY, newZ))
                                {

                                    newTile = result.tiles[newX, newY, newZ];
                                    //newTile.tilemapPosition = new Vector3Int(newX, newY, newZ);
                                    // Mod Cost
                                    float modTileCost = 1;

                                    // Check if at angle
                                    if (zOff != 0 && xOff != 0)
                                    {
                                        modTileCost *= 1.5f;
                                    }

                                    // Check if up or down
                                    if (yOff == 1)
                                    {
                                        modTileCost *= 1.2f;
                                    }
                                    else if (yOff == -1)
                                    {
                                        modTileCost *= 0.8f;
                                    }

                                    // if up - checkTile must be ramp, flat is ok, if down - new tile must be ramp
                                    if ((yOff == 1 && checkTile.isRamp) || yOff == 0 || (yOff == -1 && newTile.isRamp))
                                    {
                                        if (newTile != result.orginPoint)
                                        {
                                            if (newTile.nextTile == null && newTile != result.orginPoint)
                                            {
                                                newTile.nextTile = checkTile;
                                                newTile.pathCost = modTileCost + checkTile.pathCost;
                                                if (newTile != result.orginPoint)
                                                toCheck.Add(newTile);
                                            }
                                            else
                                            {
                                                //if (checkTile.nextTile != null)
                                                //{
                                                //    Vector2 a = new Vector2(newTile.tilemapPosition.x, newTile.tilemapPosition.z) - new Vector2(checkTile.tilemapPosition.x, checkTile.tilemapPosition.z);
                                                //    Vector2 b = new Vector2(checkTile.tilemapPosition.x, checkTile.tilemapPosition.z) - new Vector2(checkTile.nextTile.tilemapPosition.x, checkTile.nextTile.tilemapPosition.z);

                                                //    a.Normalize();
                                                //    b.Normalize();

                                                //    if (a == b)
                                                //    {
                                                //        newTile.nextTile = checkTile;
                                                //    }
                                                //}

                                            }
                                        }
                                    }

                                }
                            }
                        } // Offset loops
                    } // " "
                } // " "

                toCheck.Remove(toCheck.First());

                killswitch++;

            } // toCheck loop

        }

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

    void DrawPath(List<Vector3> input )
    {
        foreach(Vector3 pos in input)
        {
            Debug.Log(pos);
        }

        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.alignment = LineAlignment.View;
        lr.SetWidth(0.2f, 0.2f);

        lr.positionCount = input.Count;

        for (int i = 0; i < input.Count; i++)
        {
            lr.SetPosition(i, input[i] + new Vector3(0, 0.25f, 0));
        }
    }

    List<Vector3> GetPath(Vector3Int fforigin, Vector3Int current)
    {
        fforigin = new Vector3Int(4, 0, 4);
        current = new Vector3Int(0, 0, 0);

        List<Vector3> result = new List<Vector3>();

        Flowfield flowfield = Flowfields[fforigin.x, fforigin.y, fforigin.z];

        TileInfo currentTile = flowfield.tiles[current.x, current.y, current.z];

        int killswitch = 0;


        while (currentTile.nextTile != null && killswitch < 10000)
        {
            result.Add(currentTile.tilemapPosition);

            currentTile = currentTile.nextTile;

            killswitch++;
        }

        return result;
    }

}
