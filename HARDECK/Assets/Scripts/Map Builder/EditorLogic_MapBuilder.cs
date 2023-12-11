using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapBuilder))]
public class EditorLogic_MapBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        MapBuilder mapBuilder = (MapBuilder)target;

        mapBuilder.tilePrefab = EditorGUILayout.ObjectField("Tile Prefab", mapBuilder.tilePrefab, typeof(Object), true);

        mapBuilder.mapFilePath = EditorGUILayout.TextField("Map File Path", mapBuilder.mapFilePath);


        if (GUILayout.Button("Load Map from File"))
        {
            mapBuilder.LoadMapFromFile();
        }

        if (GUILayout.Button("Save Map to File"))
        {
            mapBuilder.SaveMapToFile();

        }

        if (GUILayout.Button("Clear Map"))
        {
            mapBuilder.ClearMap();
        }
    }

}
