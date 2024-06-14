using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GFXOBJContainter : MonoBehaviour
{
    public string tilesetName;
    public int objId;

    private MeshRenderer[] gfxMeshes;
    private Material[] gfxMaterials;

    private Material fowmat;

    public int state = 0;

    public GFXOBJContainter()
    {
    }

    private void Awake()
    {
        fowmat = Resources.Load("FOWMAT") as Material;

        gfxMeshes = gameObject.GetComponentsInChildren<MeshRenderer>();

        gfxMaterials = new Material[gfxMeshes.Length];

        for (int i = 0; i < gfxMaterials.Length; i++) 
        {
            gfxMaterials[i] = gfxMeshes[i].material;
        }
    }

    private void Update()
    {

    }

    public void SetState(int input)
    {
        state = input;
        if (input == 0)
        {
            foreach (MeshRenderer m in gfxMeshes)
            {
                m.material = fowmat;
            }
            
        }
        else if (input == 1)
        {
            for (int i = 0; i < gfxMeshes.Length; i++)
            {
                gfxMeshes[i].material = gfxMaterials[i];
                gfxMeshes[i].material.color = new Color(1, 1, 1, 1f);

            }
        }
        else if (input == 2)
        {

            for (int i = 0; i < gfxMeshes.Length; i++)
            {
                gfxMeshes[i].material = gfxMaterials[i];
                gfxMeshes[i].material.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            }

        }


    }
}
