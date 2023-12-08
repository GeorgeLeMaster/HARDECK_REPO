using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    }



    public void LoadMapFromFile()
    {
        Debug.Log("Loading Map from " + mapFilePath);

        Vector3 newPos = Vector3.zero;

        StreamReader reader = new StreamReader(mapFilePath);

        reader.ReadLine();

        GameObject newTile = Instantiate(tilePrefab, newPos, Quaternion.identity) as GameObject;
        newTile.transform.parent = GameObject.Find("Tiles").transform;

        while (reader.ReadLine() != null) 
        {
            string currentLine = reader.ReadLine();

            if (currentLine != null)
            {
                float xPos;
                float yPos;
                float zPos;

                char[] buffer = new char[currentLine.Length];

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

        foreach(Transform transform in tiles.GetComponentsInChildren<Transform>())
        {
            if (transform.gameObject.GetComponent<TileInfo>() != null)
            {
                writer.WriteLine(transform.position);
            }
        }

        writer.WriteLine("fileEnd");

        writer.Close();
    }

}
