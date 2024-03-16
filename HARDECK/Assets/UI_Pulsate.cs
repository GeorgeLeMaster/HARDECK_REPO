using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Pulsate : MonoBehaviour
{
    private Image image;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0)
        {
            timer = 255;
        }

        timer -= Time.deltaTime * 250;
        image.color = new Color(1,1,1, timer / 255);
    }
}
