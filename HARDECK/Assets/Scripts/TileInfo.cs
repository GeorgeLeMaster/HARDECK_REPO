using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TileInfo : MonoBehaviour
{

    public Vector3Int tilemapPosition;

    public bool isRamp;
    public int rampOrientation;

    public GameObject invalidGFX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (invalidGFX != null)
        {
            if (transform.position.x < 0 || transform.position.z < 0)
            {
                invalidGFX.SetActive(true);
            }
            else
            {
                invalidGFX.SetActive(false);
            }
        }
    }
}
