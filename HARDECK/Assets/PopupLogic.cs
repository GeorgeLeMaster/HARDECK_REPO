using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupLogic : MonoBehaviour
{
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0, Time.deltaTime, 0);
        transform.LookAt(Camera.main.transform.position);
    }
}
