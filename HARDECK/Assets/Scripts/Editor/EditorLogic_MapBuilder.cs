using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapBuilderUI))]
public class EditorLogic_MapBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        MapBuilderUI mapBuilder = (MapBuilderUI)target;



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
