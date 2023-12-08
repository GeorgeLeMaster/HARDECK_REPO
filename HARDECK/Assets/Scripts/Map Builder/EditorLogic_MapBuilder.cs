using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapBuilder))]
public class EditorLogic_MapBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        MapBuilder mapBuilder = (MapBuilder)target;

        mapBuilder.tilePrefab = EditorGUILayout.ObjectField(mapBuilder.tilePrefab, typeof(Object), true);

        if (GUILayout.Button("Build Map"))
        {
            mapBuilder.BuildMap();
        }

        if (GUILayout.Button("Save Map"))
        {
            mapBuilder.BuildMap();
        }
    }
}
