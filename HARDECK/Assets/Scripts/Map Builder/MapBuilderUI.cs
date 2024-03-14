using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MapBuilderUI : MonoBehaviour
{

    public void LoadMapFromFile()
    {
        //MapBuilder.instance.LoadMapFromFile();
        GameObject.Find("MapBuilder").GetComponent<MapBuilder>().LoadMapFromFile();
    }

    public void LoadMapV2()
    {
        //MapBuilder.instance.LoadMapFromFile();
        GameObject.Find("MapBuilder").GetComponent<MapBuilder>().LoadMapV2();
    }

    public void SaveMapToFile()
    {
        GameObject.Find("MapBuilder").GetComponent<MapBuilder>().SaveMapToFile();
    }

    public void ClearMap()
    {
        GameObject.Find("MapBuilder").GetComponent<MapBuilder>().ClearMap();

    }
}
