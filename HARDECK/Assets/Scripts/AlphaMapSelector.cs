using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AlphaMapSelector : MonoBehaviour
{

    public TMPro.TMP_Dropdown mapDropdown;

    int selection = 0;

    // Start is called before the first frame update
    void Start()
    {
        List<string> names = new List<string>();
        List<TextAsset> maps = Resources.LoadAll<TextAsset>("MapFiles").ToList();

        foreach (TextAsset m in maps)
        {
            names.Add(m.name);
        }
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(names);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadGameScene()
    {
        selection = mapDropdown.value;
        Debug.Log(selection);
        PlayerPrefs.SetInt("pref_selectedMapId", selection);
        SceneManager.LoadScene("GameScene");
    }
}
