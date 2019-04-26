using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public List<Image> PackList = new List<Image>();

    public List<Sprite> PackNormalList = new List<Sprite>();
    public List<Sprite> PackSelectedList = new List<Sprite>();

    // Use this for initialization
    void Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        SelectPack(0);
    }

    // Update is called once per frame
    void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            Application.Quit();
        }
	}

    public void SelectPack(int index)
    {
        PlayerPrefs.SetInt("Pack", index);

        for(int i = 0; i < PackList.Count; i++)
        {
            if(i == index)
            {
                //PackList[i].sprite = PackSelectedList[i];
                PackList[i].transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                //PackList[i].sprite = PackNormalList[i];
                PackList[i].transform.localScale = Vector3.one * 1.0f;
            }
        }
    }
}
