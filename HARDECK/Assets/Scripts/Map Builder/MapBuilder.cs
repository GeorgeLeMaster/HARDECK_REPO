using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


[ExecuteInEditMode]
public class MapBuilder : MonoBehaviour
{
    public static MapBuilder instance;

    private GameObject mapBuildLimitObj;

    public Object tilePrefab;

    private string mapFilePath = "testMap2.txt";

    public int mapX;
    public int mapZ;
    public int mapY;
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
        ClearMap();

        Debug.Log("Loading Map from " + mapFilePath);

        Vector3 newPos_gfx = Vector3.zero;
        Vector3Int newPos_tile = Vector3Int.zero;

        StreamReader reader = new StreamReader(mapFilePath);

        reader.ReadLine();
        reader.ReadLine();

        // SET POSITION OF MapBuildLimitObj
        string[] limitObjPos = reader.ReadLine().Split(",");
        float.TryParse(limitObjPos[0], out float limX);
        float.TryParse(limitObjPos[1], out float limY);
        float.TryParse(limitObjPos[2], out float limZ);
        Vector3 limPos = new Vector3(limX, limY, limZ);
        mapBuildLimitObj.transform.position = limPos;

        // INITALIZE TileInfoArray


        reader.ReadLine();

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
                Debug.Log(newPos_tile);

                newTileInfo.tilemapPosition = newPos_tile;



                // ASSIGN NAME
                string rampString = "Tile";
                if (rampData[0] == "True") { rampString = "Ramp"; }
                newTile.name = $"{rampString}-{xPos_tile},{yPos_tile},{zPos_tile}";

            }
        }

        Debug.Log("Map File Loaded");

        reader.Close();
    }
    
    public void SaveMapToFile()
    {

        // DEBUG INIT
        Debug.Log("Saving map to " + mapFilePath);

        // FIND PARENT OBJ
        GameObject tiles = GameObject.Find("Tiles");

        // OPEN WRITER AND TITLE FILE
        StreamWriter writer = new StreamWriter(mapFilePath);
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


    void OnDrawGizmos()
    {
        mapBuildLimitObj = GameObject.Find("MapBuildLimit");

        mapX = (int)mapBuildLimitObj.transform.position.x;
        mapY = (int)mapBuildLimitObj.transform.position.y;
        mapZ = (int)mapBuildLimitObj.transform.position.z;

        Vector3 orginPoint = new Vector3(-0.5f, 0, -0.5f);

        Gizmos.color = Color.green;

        Gizmos.DrawLine(orginPoint, new Vector3(mapX + 0.5f, 0, -0.5f));
        Gizmos.DrawLine(orginPoint, new Vector3(-0.5f, mapY + 1f, -0.5f));
        Gizmos.DrawLine(orginPoint, new Vector3(-0.5f, 0, mapZ + 0.5f));

    }

}
