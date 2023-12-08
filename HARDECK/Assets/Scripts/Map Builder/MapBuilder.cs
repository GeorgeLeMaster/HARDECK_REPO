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


    public void Awake()
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
                Debug.Log(currentLine);

                currentLine = currentLine.Substring(1, currentLine.Length - 2);

                string[] col = currentLine.Split(',');

                float.TryParse(col[0], out float xPos);
                float yPos = float.Parse(col[1]);
                float zPos = float.Parse(col[2]);

                newPos = new Vector3(xPos, yPos, zPos);

                GameObject newTile = Instantiate(tilePrefab, newPos, Quaternion.identity) as GameObject;
                newTile.transform.parent = GameObject.Find("Tiles").transform;

                //Debug.Log($"{xPos}, {yPos}, {zPos} ");

            }
        }


        Debug.Log("Map File Loaded");

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
                writer.WriteLine(transform.position);
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
