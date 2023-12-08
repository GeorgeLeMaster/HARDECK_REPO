using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class MapBuilder : MonoBehaviour
{
    public static MapBuilder instance;


    public Object tilePrefab;


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



    public void BuildMap()
    {
        Debug.Log("Building Map...");

        Vector3 newPos = Vector3.zero;

        GameObject newTile = Instantiate(tilePrefab, newPos, Quaternion.identity) as GameObject;
    }

}
