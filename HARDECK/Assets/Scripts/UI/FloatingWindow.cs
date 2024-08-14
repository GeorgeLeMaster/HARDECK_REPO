using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloatingWindow : MonoBehaviour
{
    public bool locked;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleWindowLock()
    {
        locked = !locked;

        if (locked)
        {
            animator.SetBool("down", true);
        }
        else
        {
            animator.SetBool("down", false);


        }
    }
}
