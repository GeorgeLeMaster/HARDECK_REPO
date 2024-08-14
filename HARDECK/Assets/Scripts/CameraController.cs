using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    private float sizeClamp = 10;

    private float xPivotPos;
    private float zPivotPos;

    private float xMax, zMax;

    Vector3 lockedPos;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        GameObject mbl = GameObject.Find("MapBuildLimit");

        xMax = mbl.transform.position.x;
        zMax = mbl.transform.position.z;

        xPivotPos = transform.position.x;
        zPivotPos = transform.position.z;

        foreach (Structure str in GameManager.instance.commanders[GameManager.instance.playerAllianceInt].ownedStructures)
        {
            if (str.playerHq)
            {
                transform.position = new Vector3(str.transform.position.x, 0, str.transform.position.z);
                break;
            }
        }

        lockedPos = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        sizeClamp -= Input.mouseScrollDelta.y;
        sizeClamp = Mathf.Clamp(sizeClamp, 5, 25);

        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sizeClamp, Time.deltaTime * 5);

        if (Input.GetMouseButton(2))
        {



            transform.position += new Vector3(transform.forward.x * 15, 0, transform.forward.z * 15) * -Input.GetAxis("Mouse Y") * Time.deltaTime * 1.4f;
            transform.position += new Vector3(transform.right.x * 15, 0, transform.right.z * 15) * -Input.GetAxis("Mouse X") * Time.deltaTime;

            lockedPos = new Vector3(Mathf.Clamp(transform.position.x, 0, MapBuilder.instance.mapX), 0, Mathf.Clamp(transform.position.z, 0, MapBuilder.instance.mapZ));

            transform.position = lockedPos;

        }
        else if (Input.GetMouseButton(1))
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
        }

    }
}
