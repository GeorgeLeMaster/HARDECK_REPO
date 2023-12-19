using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class UnitSO : ScriptableObject
{

    public string unitName;

    public GameObject gfx;

    public UnitAIBase unitAI;
}
