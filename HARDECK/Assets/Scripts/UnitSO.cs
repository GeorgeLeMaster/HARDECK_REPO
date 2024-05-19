using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class UnitSO : ScriptableObject
{

    public string unitName;

    public GameObject gfxPrefab;

    [Header("Stats")]
    public int moveSpeed;

    public int range;

    public int damage;
    public int damageMod;

    public int maxHealth;

    public int visionRadius;
}
