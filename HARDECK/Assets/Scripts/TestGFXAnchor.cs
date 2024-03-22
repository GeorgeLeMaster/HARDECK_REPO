using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGFXAnchor : MonoBehaviour
{

    public GameObject gfxAnchor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LayerMask mask = LayerMask.GetMask("GFXEnvironment");

        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3 (0, 1f, 0), Vector3.down, out hit, 10f, mask))
        {
            gfxAnchor.transform.position = hit.point;
        }
    }
}
