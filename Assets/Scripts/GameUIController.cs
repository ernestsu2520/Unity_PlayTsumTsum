using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour {
    private Text ComboText;//Combo文字
    private Text Time_txt; //計時器文字
    private GameObject FeverTimeTxt;
    private GameObject FeverBar;
    private float FeverBarTarget_X;

    //分數
    private int TargetScore;
    private float NowScore;
    private GameObject ScoreTxt;
    private int TargetFeverScore;
    private float NowFeverScore;
    private GameObject FeverScoreTxt;
    [Header("分數遞增速度")]public float ScoreAddSpeed;

    private GameObject GameOverPanel;

    void Awake()
    {
        ComboText = GameObject.Find("Combo").GetComponent<Text>();
        Time_txt = GameObject.Find("Timer/Text").GetComponent<Text>();
        FeverTimeTxt = GameObject.Find("FeverBar/Text");
        FeverBar = GameObject.Find("FeverBar/FeverBarInner");
        GameOverPanel = GameObject.Find("GameOverPanel");
        FeverBarTarget_X = 1;

        ScoreTxt = GameObject.Find("ScoreGroup/Score");
        FeverScoreTxt = GameObject.Find("ScoreGroup/FeverScore");
        FeverScoreTxt.SetActive(false);
        FeverTimeTxt.SetActive(false);
    }

    void Update()
    {
        FeverTimeUI_Update();
        Score_Update();
        FeverScore_Update();
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
            FeverTimeTxt.SetActive(true);
            FeverBar.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            FeverTimeTxt.SetActive(false);
            FeverBar.GetComponent<Image>().color = Color.white;
        }
    }

    //開關遊戲結束頁面
    public void GameOverPanelControl(bool IsShow)
    {
        GameOverPanel.SetActive(IsShow);
    }

    void Score_Update() {
        if (NowScore < TargetScore)
        {
            NowScore = Mathf.Lerp(NowScore, TargetScore, Time.deltaTime * ScoreAddSpeed);
            ScoreTxt.GetComponent<Text>().text = string.Format("{0:#,0}", NowScore);
        }
    }
    public void RefreshScore(int targetScore)
    {
        TargetScore = targetScore;
    }


    public void FeverScore_Update()
    {
        if (NowFeverScore < TargetFeverScore)
        {
            NowFeverScore = Mathf.Lerp(NowFeverScore, TargetFeverScore,Time.deltaTime * ScoreAddSpeed);
            FeverScoreTxt.GetComponent<Text>().text = string.Format("{0:#,0}", NowFeverScore);
        }
    }
    public void RefreshFeverScore(int targetFeverScore)
    {
        if (targetFeverScore == 0)
        {
            FeverScoreTxt.SetActive(false);
        }
        else
        {
            FeverScoreTxt.SetActive(true);
        }
        TargetFeverScore = targetFeverScore;
    }
}
