using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    private float sizeClamp = 3;

    private float xPivotPos;
    private float zPivotPos;

    private float xMax, zMax;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        GameObject mbl = GameObject.Find("MapBuildLimit");

        xMax = mbl.transform.position.x;
        zMax = mbl.transform.position.z;

        xPivotPos = transform.position.x;
        zPivotPos = transform.position.z;

        //foreach(Structure str in GameManager.instance.structures)
        //{
        //    if (str.playerHq)
        //    {
        //        transform.position = new Vector3(str.transform.position.x, 0, str.transform.position.z);
        //        break;
        //    }
        //}
    }

    // Update is called once per frame
    void LateUpdate()
    {
        sizeClamp -= Input.mouseScrollDelta.y;
        sizeClamp = Mathf.Clamp(sizeClamp, 2, 12);

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, sizeClamp, Time.deltaTime * 5);

        if (Input.GetMouseButton(2))
        {
            
            transform.position += new Vector3(transform.forward.x * 15, 0, transform.forward.z * 15) * -Input.GetAxis("Mouse Y") * Time.deltaTime;
            transform.position += new Vector3(transform.right.x * 15, 0, transform.right.z * 15) * -Input.GetAxis("Mouse X") * Time.deltaTime;

        }
        else if (Input.GetMouseButton(1))
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
        }

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;

        }

    }
}
