using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardUIOBJ : MonoBehaviour
{

    public TextMeshProUGUI damageText;
    public TextMeshProUGUI moveSpeedText;
    public TextMeshProUGUI visionRadiusText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI nameText;


    public UnitData uData = null;

    private void Start()
    {
        DisplayInfo();
    }

    public void DisplayInfo()
    {
        if (uData == null)
        {
            uData = GameManager.instance.infintrymanUnitData;
        }

        damageText.text = uData.damage.ToString() + "±" + uData.damageMod.ToString();
        moveSpeedText.text = uData.moveSpeed.ToString();
        visionRadiusText.text = uData.visionRadius.ToString();
        hpText.text = uData.hp.ToString();

        nameText.text = uData.name;
    }
}
