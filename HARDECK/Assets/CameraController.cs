using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {


        if (Input.GetMouseButton(2))
        {
            Cursor.visible = false;
            transform.position += new Vector3(transform.forward.x * 15, 0, transform.forward.z * 15) * -Input.GetAxis("Mouse Y") * Time.deltaTime;
            transform.position += new Vector3(transform.right.x * 15, 0, transform.right.z * 15) * -Input.GetAxis("Mouse X") * Time.deltaTime;

        }
        else
        {
            Cursor.visible = true;
        }
    }
}
