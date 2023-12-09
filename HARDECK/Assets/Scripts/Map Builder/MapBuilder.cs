using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


[ExecuteInEditMode]
public class MapBuilder : MonoBehaviour
{
    public static MapBuilder instance;


    public Object tilePrefab;

    private string mapFilePath = "testMap.txt";


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

        Vector3 newPos = Vector3.zero;

        StreamReader reader = new StreamReader(mapFilePath);

        reader.ReadLine();
        reader.ReadLine();

        string currentLine;

        while ((currentLine = reader.ReadLine()) != "fileEnd") 
        {

            if (currentLine != null)
            {
                // PARSE LINE DATA INTO SEPERATE MEMBERS
                string[] dataMembers = currentLine.Split("*");

                // TRUNCATE POSITION VALUE TO READABLE DATA AND DELIMINATE
                dataMembers[0] = dataMembers[0].Substring(1, dataMembers[0].Length - 2);
                string[] posValues = dataMembers[0].Split(',');

                // ASSIGN POSITIONS FROM PARSED DATA
                float.TryParse(posValues[0], out float xPos);
                float.TryParse(posValues[1], out float yPos);
                float.TryParse(posValues[2], out float zPos);

                // INSTANTIATE TILE AT DESIRED POSITION AND SET PROPER PARENT
                newPos = new Vector3(xPos, yPos, zPos);
                GameObject newTile = Instantiate(tilePrefab, newPos, Quaternion.identity) as GameObject;
                newTile.transform.parent = GameObject.Find("Tiles").transform;

                // GRAB NEWTILE'S TileInfo SCRIPT
                TileInfo newTileInfo = newTile.GetComponent<TileInfo>();

                // PARSE AND ASSIGN RAMP STATS
                string[] rampData = dataMembers[1].Split(',');
                if (rampData[0] == "True")
                {

                    // IS A RAMP LOGIC
                    newTileInfo.isRamp = true;

                    if (rampData[1] == "North")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.North;
                    }
                    else if (rampData[1] == "East")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.East;
                    }
                    else if (rampData[1] == "South")
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.South;
                    }
                    else
                    {
                        newTileInfo.rampOrientation = TileInfo.Directions.West;
                    }


                }
                else
                {
                    // IS *NOT* A RAMP LOGIC
                    newTileInfo.isRamp = false;
                }
            }
        }


        Debug.Log("Map File Loaded");

        reader.Close();
    }
    
    public void SaveMapToFile()
    {
        Debug.Log("Saving map to " + mapFilePath);

        GameObject tiles = GameObject.Find("Tiles");

        StreamWriter writer = new StreamWriter(mapFilePath);

        writer.WriteLine("Tiles\n*");

        int numOfObjs = 0;

        foreach(Transform transform in tiles.GetComponentsInChildren<Transform>())
        {
            if (transform.gameObject.GetComponent<TileInfo>() != null)
            {
                TileInfo currentTileInfo = transform.gameObject.GetComponent<TileInfo>();

                string positionString = transform.position.ToString();
                string rampBool = currentTileInfo.isRamp.ToString();
                string rampOrientation = currentTileInfo.rampOrientation.ToString();

                writer.WriteLine($"{positionString}*{rampBool},{rampOrientation}");
                numOfObjs++;
            }
        }

        Debug.Log(numOfObjs);
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

}
