using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour {
    private Text ComboText;//Combo文字
    private Text Time_txt; //計時器文字
    private GameObject FeverBar;
    private float FeverBarTarget_X;

    private GameObject GameOverPanel;

    void Awake()
    {
        ComboText = GameObject.Find("Combo").GetComponent<Text>();
        Time_txt = GameObject.Find("Timer/Text").GetComponent<Text>();
        FeverBar = GameObject.Find("FeverBar/FeverBarInner");
        GameOverPanel = GameObject.Find("GameOverPanel");
        FeverBarTarget_X = 1;
    }

    void Update()
    {
        FeverTimeUI_Update();
    }

    public void RefreshComboUI(int combo)
    {
        if (combo <= 0)
        {
            ComboText.gameObject.SetActive(false);
        }
        else if (combo == 1)
        {
            ComboText.gameObject.SetActive(true);
            ComboText.text = combo.ToString() + " <size=40>Combo</size>";
        }
        else
        {
            ComboText.text = combo.ToString() + " <size=40>Combo</size>";
        }
    }

    public void RefreshTimerUI(float time)
    {
        Time_txt.text = Mathf.Floor(time).ToString();
    } 

    //Fever讀條
    void FeverTimeUI_Update()
    {
        FeverBar.GetComponent<RectTransform>().localPosition = Vector3.Lerp(FeverBar.GetComponent<RectTransform>().localPosition, new Vector3(FeverBarTarget_X, 0,0), Time.deltaTime);
    }
    public void RefreshFeverTarget(float target,float max,bool InFever)
    {
        FeverBarTarget_X =  FeverBar.GetComponent<RectTransform>().rect.width * ( -1 + target / max);
        if (InFever)
        {
            FeverBar.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            FeverBar.GetComponent<Image>().color = Color.white;
        }
    }

    //開關遊戲結束頁面
    public void GameOverPanelControl(bool IsShow)
    {
        GameOverPanel.SetActive(IsShow);
    }
}
