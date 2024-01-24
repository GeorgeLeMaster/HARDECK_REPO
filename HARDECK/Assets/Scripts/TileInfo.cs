using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TileInfo : MonoBehaviour
{

    public TileInfo() { }

    public enum Directions
    {
        Forwards = 0,
        Right = 90,
        Backwards = 180,
        Left = 270
    }

    [Header("Memebers")]
    public Vector3Int tilemapPosition;

    public bool isRamp;
    public Directions rampOrientation;

    public TileInfo nextTile = null;
    public float pathCost;
    public bool isChecked = false;

    [Header("GFX Members")]
    public GameObject GFXAnchor;
    private GameObject existingGFX;

    public GameObject tileGFX_prefab;
    public GameObject rampGFX_prefab;

    public GameObject invalidGFX;

    // Start is called before the first frame update
    void Start()
    {
        if (isRamp)
        {
            existingGFX = Instantiate(rampGFX_prefab, GFXAnchor.transform);
        }
        else
        {
            existingGFX = Instantiate(tileGFX_prefab, GFXAnchor.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (invalidGFX != null)
        {
            if (transform.position.x < 0 || transform.position.y < 0 || transform.position.z < 0)
            {
                invalidGFX.SetActive(true);
            }
            else
            {
                invalidGFX.SetActive(false);
            }
        }

        Vector3 desiredRot = Vector3.zero;

        desiredRot.y = (float)rampOrientation;

        existingGFX.transform.rotation = Quaternion.Euler(desiredRot);

        if (transform.hasChanged)
        {
            UpdateTilemapPos();

            string rampString = "Tile";
            if (isRamp == true) { rampString = "Ramp"; }
            this.gameObject.name = $"{rampString}-{tilemapPosition.x},{tilemapPosition.y},{tilemapPosition.z}";

            if (existingGFX != null)
                DestroyImmediate(existingGFX);

            if (isRamp)
            {

                existingGFX = Instantiate(rampGFX_prefab, GFXAnchor.transform);

                desiredRot.y = (float)rampOrientation;

                existingGFX.transform.rotation = Quaternion.Euler(desiredRot);
            }
            else
            {

                existingGFX = Instantiate(tileGFX_prefab, GFXAnchor.transform);
            }
        }
    }

    private void UpdateTilemapPos ()
    {
        tilemapPosition = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }

    public void CopyStats(TileInfo input)
    {
        tilemapPosition = input.tilemapPosition;

        isRamp = input.isRamp;
        rampOrientation = input.rampOrientation;

        nextTile = input.nextTile;
        pathCost = input.pathCost;
    }
    
}
