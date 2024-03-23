using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MapBuilderUI : MonoBehaviour
{

    public void LoadMapFromFile()
    {
        //MapBuilder.instance.LoadMapFromFile();
        PlayerPrefs.SetInt("pref_selectedMapId", GameObject.Find("MapBuilder").GetComponent<MapBuilder>().selectedMapID);
        GameObject.Find("MapBuilder").GetComponent<MapBuilder>().LoadMapFromFile();
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
