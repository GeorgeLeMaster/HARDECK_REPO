using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class Flowfield
{
    public Flowfield() { }
    public Flowfield(int x, int y, int z)
    {

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


    [SerializeField] public TileInfo[,,] Tiles;


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

        BuildFlowfields();

        Debug.Log("Map File Loaded");

        reader.Close();

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

                string positionString = transform.position.ToString();
                positionString = positionString.Substring(1, positionString.Length - 2);

                string rampBool = currentTileInfo.isRamp.ToString();

                string rampOrientation = currentTileInfo.rampOrientation.ToString();

                string tilemapPos = currentTileInfo.tilemapPosition.ToString();
                tilemapPos = tilemapPos.Substring(1, tilemapPos.Length - 2);


                writer.WriteLine($"{positionString}*{rampBool},{rampOrientation}*{tilemapPos}");
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
        Flowfields = new Flowfield[mapX+1, mapY+1, mapZ+1];


    }
/*
    public Flowfield GenerateFlowfield(Vector2Int orginPoint)
    {

        Flowfield result = new Flowfield();

        mapX = (int)mapBuildLimitObj.transform.position.x;
        mapY = (int)mapBuildLimitObj.transform.position.y;
        mapZ = (int)mapBuildLimitObj.transform.position.z;

        result.orginPoint = pathableMapTiles[mapX, mapY, mapZ];
        //Debug.Log(result.orginPoint.voxCoord);
        // Copies blank map data
        for (int x = 0; x < mapX; x++)
        {
            for (int y = 0; y < mapY; y++)
            {
                result.voxels[x, y] = new Voxel();

                result.voxels[x, y].voxCoord = currentMapData.voxels[x, y].voxCoord;

                result.voxels[x, y].tileCost = currentMapData.voxels[x, y].tileCost;
                result.voxels[x, y].pathCost = currentMapData.voxels[x, y].pathCost;

                result.voxels[x, y].isDest = currentMapData.voxels[x, y].isDest;
                result.voxels[x, y].isRamp = currentMapData.voxels[x, y].isRamp;

                result.voxels[x, y].terrainObj = currentMapData.voxels[x, y].terrainObj;

            }
        }

        // Builds Paths
        //  -   Work outwards, left->right, top->down
        //  -   If checkedVox has no nextVox, assign to currentVox and increment path cost
        //  -   If checkedVox HAS a nextVox, skip
        //  -   EXCEPTION: If assigning currentVox as nextVox would align currentVox and nextVox, do so

        // Stores the voxels yet to check
        List<Voxel> toCheck = new List<Voxel>();

        toCheck.Add(result.orginPoint);

        int killswitch = 0;

        while (toCheck.Count() > 0)
        {
            for (int yOff = -1; yOff <= 1; yOff++)
            {
                for (int xOff = -1; xOff <= 1; xOff++)
                {
                    // Make sure its not the center vox
                    if (!(yOff == 0 && xOff == 0))
                    {
                        int newX = toCheck.First().voxCoord.x + xOff;
                        int newY = toCheck.First().voxCoord.y + yOff;

                        // Ensure new coord is valid
                        if (IsValidCoord(newX, newY) == true && CheckLevel(result.voxels[newX, newY], toCheck.First()) == true && result.orginPoint != result.voxels[newX, newY])
                        {

                            float modTileCost = result.voxels[newX, newY].tileCost;

                            if (yOff != 0 && xOff != 0)
                            {
                                modTileCost *= 1.5f;
                            }

                            // nextVox rule check and assignment
                            if (result.voxels[newX, newY].nextVoxel == null && toCheck.First() != null)
                            {
                                result.voxels[newX, newY].nextVoxel = toCheck.First();

                                // Calculate _voxel pathCost
                                result.voxels[newX, newY].pathCost = toCheck.First().pathCost + modTileCost;
                            }
                            else
                            {
                                Vector2 a = toCheck.First().voxCoord - result.voxels[newX, newY].voxCoord;
                                Vector2 b = toCheck.First().nextVoxel.voxCoord - toCheck.First().voxCoord;



                                a.Normalize();
                                b.Normalize();

                                if (a == b)
                                {
                                    result.voxels[newX, newY].nextVoxel = toCheck.First();

                                    // Calculate _voxel pathCost
                                    result.voxels[newX, newY].pathCost = toCheck.First().pathCost + modTileCost;
                                }
                                else
                                {
                                    // This should mean that _voxel already has a nextVox and assigning it to toCheck.first()
                                    // would not align it with the currentVox's bearing
                                    // If this is correct, do not touch _voxel's nextVox or pathCost

                                }
                            }

                            // Try to make range circular rather than square 
                            // BROKEN
                            //if (xOff != 0 && yOff != 0)
                            //{
                            //    result.voxels[newX, newY].pathCost += 1;
                            //}

                            // Add to toCheck list, provided it hs not already been checked and isnot already in the list
                            if (!result.voxels[newX, newY].isChecked && !toCheck.Contains(result.voxels[newX, newY]))
                            {
                                toCheck.Add(result.voxels[newX, newY]);
                            }
                        }

                    }
                }
            }
            //Debug.Log("!");

            toCheck.First().isChecked = true;
            toCheck.Remove(toCheck.First());

            // Prevent endless looping in case my code is shoddy
            if (killswitch >= 10000)
            {
                Debug.Log("Killswitch");
                break;
            }
            else
            {
                killswitch++;
            }
        }

        return result;
    }
*/
    void OnDrawGizmos()
    {
        mapBuildLimitObj = GameObject.Find("MapBuildLimit");

        mapX = (int)mapBuildLimitObj.transform.position.x;
        mapY = (int)mapBuildLimitObj.transform.position.y;
        mapZ = (int)mapBuildLimitObj.transform.position.z;


        Gizmos.color = Color.green;

        Vector3 pos_1 = new Vector3(-0.5f       , -1, -0.5f);
        Vector3 pos_2 = new Vector3(-0.5f       , -1, mapZ + 0.5f);
        Vector3 pos_3 = new Vector3(mapX + 0.5f , -1, mapZ + 0.5f);
        Vector3 pos_4 = new Vector3(mapX + 0.5f , -1, -0.5f);

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

}
