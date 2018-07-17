using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePlayController : MonoBehaviour {
    [Header("物件生成線")]
    public float left;
    public float right;
    public float height;

    [Header("遊戲暫停")]
    public bool IsStop;

    [Header("畫面最大數量")] public int MaxBallNumber;
    [Header("當前數量")] public int NowBallNumber;

    [Header("消除類別")] public int target_type;
    private List<GameObject> selectBalls = new List<GameObject>(); //消除的物件List
    //畫線
    private bool IsLineRender;
    private LineRenderer lineRenderer;

    [Header("連線距離")] public float LinkDis;

    [Header("計時器")]
    public float time;
   

    [Header("Combo持續時間")] public float ComboRemainTime;
    [Header("Combo數")] public int ComboCount;
    private bool IsInComboTime;
    private float NowComboRemainTime; //Combo持續剩餘時間

    public int Score;

    [Header("FeverTime開啟顆數")] public int FeverTimeBallMax;
    [Header("當前消除顆數(會隨時間遞減)")] public int FeverTimeBallCount;
    [Header("控制幾秒後結束FeverTime")]public float FeverRemainTime;
    [Header("FeverTime自動遞減時間")] public float FeverBallDecreaseTime;
    private float FeverBallDecreaseTimeCount; //紀錄FeverTime遞減時間;
    private bool IsInFeverTime;
    private int FeverTimeScore;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(left, height, 0), new Vector3(right, height, 0));
    }
#endif
    void Awake()
    {
        IsLineRender = false;
        lineRenderer = GetComponent<LineRenderer>();
        IsStop = false;
    }

    void Start()
    {
        GetComponent<GameUIController>().GameOverPanelControl(false);
    }

    // Update is called once per frame
    void Update () {
        if (!IsStop)
        {
            SpawnBall();
            CursorDetect();
            RenderTheLine();
            Timer();
            ComboTimer();
            FeverTime();
        }
    }

    //Call the function to spawn ball form "BallPool.cs"
    void SpawnBall()
    {
        if (NowBallNumber < MaxBallNumber)
        {
            GetComponent<BallPool>().SpawnBall(new Vector3(Random.Range(left,right),height,0));
            NowBallNumber++;
        }
    }

    void CursorDetect()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0))
        {
            GetInputDown(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            GetInput(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
           GetInputUp();
        }
#endif
#if UNITY_ANDROID

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            GetInputDown(Input.GetTouch(0).position);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            GetInput(Input.GetTouch(0).position);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            GetInputUp();
        }
#endif
    }

    //選取球的基本動作處理
    void GetInputDown(Vector3 InputPos)
    {
        Vector3 m_pos = Camera.main.ScreenToWorldPoint(InputPos);
        RaycastHit2D hit2D = Physics2D.Raycast(m_pos, Vector2.zero);
        if (hit2D && hit2D.collider.gameObject.tag == "Ball")
        {
            target_type = hit2D.collider.gameObject.GetComponent<BallBasic>().type + 1;
            AddToChain(hit2D.collider.gameObject);
            IsLineRender = true; //將路徑實現成線段
        }
    }
    void GetInput(Vector3 InputPos)
    {
        Vector3 m_pos = Camera.main.ScreenToWorldPoint(InputPos);
        RaycastHit2D hit2D = Physics2D.Raycast(m_pos, Vector2.zero);
        if (hit2D && hit2D.collider.gameObject.tag == "Ball")
        {
            ValidateBall(hit2D.collider.gameObject);
        }
    }
    void GetInputUp()
    {
        //確認消除數大於等於3時，開始回收tsum
        bool StartRecycle = false;
        if (selectBalls.Count >= 3)
        {
            StartRecycle = true;

            //Combo數增加
            IsInComboTime = true;
            NowComboRemainTime = ComboRemainTime;
            ComboCount++;
            GetComponent<GameUIController>().RefreshComboUI(ComboCount);

            //Fever計算
            AddFeverCount(selectBalls.Count);
        }
        foreach (GameObject ball in selectBalls.ToArray())
        {
            RemoveToChain(selectBalls.IndexOf(ball));
            if (StartRecycle)
            {
                GetComponent<BallPool>().RecycleBall(ball);
                NowBallNumber--;
            }
        }

        target_type = 0;

        IsLineRender = false;
    }

    //驗證球的狀態
    void ValidateBall(GameObject ball)
    {
        if (ball.GetComponent<BallBasic>().type + 1 == target_type)
        {
            if (selectBalls.Contains(ball))
            {
                for (int i = selectBalls.Count - 1; i >= 1; i--)
                {
                    if (selectBalls[i] == ball)
                    {
                        break;
                    }
                    RemoveToChain(i);
                }
            }
            else if (!selectBalls.Contains(ball) && CalculateDis(ball.transform.position) <= LinkDis)
            {
                AddToChain(ball);
            }
        }
    }
    //加入選擇陣列中
    void AddToChain(GameObject ball)
    {
        selectBalls.Add(ball);
        ball.GetComponent<Animator>().SetBool("selected", true);
    }
    //從選擇陣列移除
    void RemoveToChain(int index)
    {
        selectBalls[index].GetComponent<Animator>().SetBool("selected", false);
        selectBalls.RemoveAt(index);
    }

    //計算兩點距離
    float CalculateDis(Vector3 NextPos)
    {
        return Vector3.Distance(selectBalls[selectBalls.Count - 1].transform.position,NextPos);
    }

    //控制畫線
    void RenderTheLine()
    {
        if (IsLineRender)
        {
            lineRenderer.positionCount = selectBalls.Count;
            for (int i = 0; i < selectBalls.Count; i++)
            {
                lineRenderer.SetPosition(i, selectBalls[i].transform.position);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

    //倒數計時
    void Timer()
    {
        time -= Time.deltaTime;
        if (time <= 0)
        {
            time = 0;
            GetComponent<GameUIController>().GameOverPanelControl(true);
            IsStop = true;
        }
        GetComponent<GameUIController>().RefreshTimerUI(time);
    }

    //Combo計時
    void ComboTimer() {
        if (IsInComboTime)
        {
            NowComboRemainTime -= Time.deltaTime;
            if (NowComboRemainTime <= 0)
            {
                IsInComboTime = false;
                ComboCount = 0;
                GetComponent<GameUIController>().RefreshComboUI(ComboCount);
            }
        }
    }

    //FeverTime處理
    void AddFeverCount(int add)
    {
        if (!IsInFeverTime)
        {
            FeverTimeBallCount += add;
            GetComponent<GameUIController>().RefreshFeverTarget(FeverTimeBallCount, FeverTimeBallMax, IsInFeverTime);
        }
    }
    void FeverTime()
    {
        if (FeverTimeBallCount >= FeverTimeBallMax)
        {
            FeverTimeBallCount = 0;
            IsInFeverTime = true;
            FeverBallDecreaseTimeCount = FeverRemainTime;
        }
        FeverCounAutoDecrease();
    }
    void FeverCounAutoDecrease()
    {
        if (!IsInFeverTime)
        {
            FeverBallDecreaseTimeCount -= Time.deltaTime;
            if (FeverBallDecreaseTimeCount <= 0)
            {
                FeverBallDecreaseTimeCount = FeverBallDecreaseTime;
                FeverTimeBallCount--;
                if (FeverTimeBallCount == -1)
                {
                    FeverTimeBallCount = 0;
                }
                GetComponent<GameUIController>().RefreshFeverTarget(FeverTimeBallCount, FeverTimeBallMax, IsInFeverTime);
            }
        }
        else
        {
            FeverBallDecreaseTimeCount -= Time.deltaTime;
            GetComponent<GameUIController>().RefreshFeverTarget(FeverBallDecreaseTimeCount, FeverRemainTime, IsInFeverTime);
            if (FeverBallDecreaseTimeCount <= 0)
            {
                IsInFeverTime = false;
            }
        }
    }

    int BasicScoreCaculate(int link)
    {
        return (link >= 3 ? 0 : 300)
                + (link >= 4 ? 0 : 400)
                + (link >= 5 ? 0 : 600)
                + (link >= 6 ? 0 : 800)
                + (link >= 7 ? 0 : 1000)
                + (link >= 8 ? 0 : (link >= 11 ? 3 : link - 7) * 1500)
                + (link >= 11 ? 0 : (link >= 15 ? 4 : link - 10) * 2000)
                + (link >= 15 ? 0 : (link >= 20 ? 5 : link - 14) * 2500)
                + (link >= 20 ? 0 : (link >= 30 ? 10 : link - 19) * 3000)
                + (link >= 30 ? 0 : (link >= 36 ? 6 : link - 29) * 3500);
    }

    float ComboRateCaculate(int combo)
    {
        return 1.10f + 0.1f * (combo > 48 ? 48 : combo);
    }

    public void ChangeScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
