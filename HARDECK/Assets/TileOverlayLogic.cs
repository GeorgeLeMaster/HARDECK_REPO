using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileOverlayLogic : MonoBehaviour
{
    public Image overlay;

    private string currentState = "none";
    
    [Header("Overlays")]
    public Sprite FOW;
    public Sprite deployable;

    public void SetOverlay(string state, bool snap = true)
    {
        if (currentState == state)
        {
            return;
        }

        switch (state)
        {
            case "FOW_showfow":
                overlay.sprite = FOW;
                overlay.color = new Color(1, 1, 1, 1);
                currentState = "FOW_showfow";
                break;

            case "FOW_hidefow":
                overlay.sprite = FOW;
                overlay.color = new Color(1,1,1,0);
                currentState = "FOW_hidefow";
                break;

            case "FOW_rem":
                overlay.sprite = FOW;
                overlay.color = new Color(1, 1, 1, 0.5f);
                currentState = "FOW_rem";
                break;

            case "Deployable":
                overlay.sprite = deployable;
                break;
        }
    }
}
