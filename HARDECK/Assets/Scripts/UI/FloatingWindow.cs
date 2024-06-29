using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloatingWindow : MonoBehaviour
{
    public bool locked;

    public Canvas canvas;

    private Vector2 offset;

    public Image lockedImage;

    public Image ColorBar;

    private Sprite lockedSprite;
    private Sprite unlockedSprite;

    // Start is called before the first frame update
    void Start()
    {
        ColorBar.color = GameManager.instance.commanderColor_0;

        lockedSprite = Resources.Load("Sprites/Lock_Locked") as Sprite;
        unlockedSprite = Resources.Load("Sprites/Lock_Unlocked.png") as Sprite;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DragHandler(BaseEventData data)
    {
        if (!locked)
        {
            PointerEventData pointerData = data as PointerEventData;

            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, pointerData.position + offset, canvas.worldCamera, out pos);


            transform.position = canvas.transform.TransformPoint(pos);

        }    
    }

    public void CalcOffset(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;
        offset = (Vector2)transform.position - pointerData.position;

    }

    public void ToggleWindowLock()
    {
        locked = !locked;

        if (locked)
        {
          //  lockedSprite = "UNLOCK";
          lockedImage.sprite = Resources.Load("Sprites/Lock_Locked") as Sprite;
        }
        else
        {
            //  lockedSprite.text = "LOCK";
            lockedImage.sprite = Resources.Load("Sprites/Lock_Unlocked") as Sprite;

        }
    }
}
