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

    public float Score;
    public float FeverScore;

    [Header("FeverTime開啟顆數")] public int FeverTimeBallMax;
    [Header("當前消除顆數(會隨時間遞減)")] public int FeverTimeBallCount;
    [Header("控制幾秒後結束FeverTime")]public float FeverRemainTime;
    [Header("FeverTime自動遞減時間")] public float FeverBallDecreaseTime;
    private float FeverBallDecreaseTimeCount; //紀錄FeverTime遞減時間;
    private bool IsInFeverTime;
    private int FeverTimeScore;

    [Header("氣泡爆破大小")] public float bubbleRadious;
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

   
    void SpawnBall()  //Call the function to spawn ball form "BallPool.cs" 
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
        if (hit2D)
        {
            if (hit2D.collider.gameObject.tag == "Ball")
            {
                target_type = hit2D.collider.gameObject.GetComponent<BallBasic>().type + 1;
                AddToChain(hit2D.collider.gameObject);
                IsLineRender = true; //將路徑實現成線段
            }
            else if (hit2D.collider.gameObject.tag == "BubbleBall")
            {
                DrawSphere(hit2D.collider.gameObject.transform.position);
                GetComponent<MagicBubblePool>().ReCycleBubble(hit2D.collider.gameObject);
            }
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
        int link = selectBalls.Count;
        if (link >= 3)
        {
            SetCombo();  //Combo數增加

            Vector3 tmp_BubblePos = new Vector3() ;

            //基本分計算
            int tempBasicScore = 0;
            foreach (GameObject ball in selectBalls.ToArray())
            {
                RemoveToChain(selectBalls.IndexOf(ball));

                //累加基本分
                tempBasicScore += ball.GetComponent<BallBasic>().BasicScore;
                tmp_BubblePos = ball.transform.position;
                GetComponent<BallPool>().RecycleBall(ball);
                NowBallNumber--;
            }

            GetComponent<MagicBubblePool>().SpawnMagicBubble(link, tmp_BubblePos);

            ScoreCoculate(tempBasicScore, link); //加分計算

            //Fever計算
            AddFeverCount(link);
        }
        else
        {
            foreach (GameObject ball in selectBalls.ToArray())
            {
                RemoveToChain(selectBalls.IndexOf(ball));
            }
        }
        target_type = 0;
        IsLineRender = false;
    }

    void ValidateBall(GameObject ball) //驗證球的狀態 
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
    void AddToChain(GameObject ball) //加入選擇陣列中 
    {
        selectBalls.Add(ball);
        ball.GetComponent<Animator>().SetBool("selected", true);
    }
    void RemoveToChain(int index) //從選擇陣列移除 
    {
        selectBalls[index].GetComponent<Animator>().SetBool("selected", false);
        selectBalls.RemoveAt(index);
    }

    float CalculateDis(Vector3 NextPos) //計算兩點距離 
    {
        return Vector3.Distance(selectBalls[selectBalls.Count - 1].transform.position,NextPos);
    }
    void RenderTheLine() //控制畫線 
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
    void Timer() //倒數計時 
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

    #region Bubble
    void DrawSphere(Vector3 original)
    {
        SetCombo();  //Combo數增加

        Collider2D[] bubbleColliders =  Physics2D.OverlapCircleAll(original, bubbleRadious);

        if (bubbleColliders.Length > 0)
        {
            //基本分計算
            int tempBasicScore = 0;
            foreach (Collider2D bubbleCollider in bubbleColliders)
            {
                //累加基本分
                if (bubbleCollider.gameObject.tag == "Bubble")
                {
                    tempBasicScore += bubbleCollider.gameObject.GetComponent<BallBasic>().BasicScore;
                    GetComponent<BallPool>().RecycleBall(bubbleCollider.gameObject);
                    NowBallNumber--;
                }
               
            }
        }
    
    }
    #endregion

    #region Combo
    void SetCombo() //Combo數增加
    {
        IsInComboTime = true;
        NowComboRemainTime = ComboRemainTime;
        ComboCount++;
        GetComponent<GameUIController>().RefreshComboUI(ComboCount);
    }
    void ComboTimer() //Combo計時
    {
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
    #endregion

    #region FeverTime
    void AddFeverCount(int add) //FeverTime處理 
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
            time += 5; //進入FeverTime 增加5秒
            FeverBallDecreaseTimeCount = FeverRemainTime;
        }
        FeverCounAutoDecrease();
    }
    void FeverCounAutoDecrease() //FeverTime累積條自動遞減
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
                FeverTimeEndAddScore();
            }
        }
    }
    void FeverTimeEndAddScore() //FeverTime結束之分數計算 
    {
        Score += FeverScore * GetComboRate(ComboCount);
        FeverScore = 0;
        GetComponent<GameUIController>().RefreshScore(Mathf.FloorToInt(Score));
        GetComponent<GameUIController>().RefreshFeverScore(Mathf.FloorToInt(FeverScore));
    }
    #endregion
   
    #region 分數計算相關
    void ScoreCoculate(int BasicScore,int link) //分數計算 
    {
        if (!IsInFeverTime)
        {
            Score += (BasicScore + GetLinkBasicScore(link)) * GetComboRate(ComboCount);
            GetComponent<GameUIController>().RefreshScore(Mathf.FloorToInt(Score));
        }
        else
        {
            FeverScore += (BasicScore * 3 + GetLinkBasicScore(link));
            GetComponent<GameUIController>().RefreshFeverScore(Mathf.FloorToInt(FeverScore));
        }  
    }
    int GetLinkBasicScore(int link) //基本分數轉換 
    {
        return (link >= 3 ? 300 : 0)
                + (link >= 4 ? 400 : 0)
                + (link >= 5 ? 600 : 0)
                + (link >= 6 ? 800 : 0)
                + (link >= 7 ? 1000 : 0)
                + (link >= 8 ? (link >= 11 ? 3 : link - 7) * 1500 : 0)
                + (link >= 11 ? (link >= 15 ? 4 : link - 10) * 2000 : 0)
                + (link >= 15 ? (link >= 20 ? 5 : link - 14) * 2500 : 0)
                + (link >= 20 ? (link >= 30 ? 10 : link - 19) * 3000 : 0)
                + (link >= 30 ? (link >= 36 ? 6 : link - 29) * 3500 : 0);
    }
    float GetComboRate(int combo)  //加成倍率轉換
    {
        return 1.10f + 0.1f * (combo > 48 ? 48 : combo);
    }
    #endregion

    public void ChangeScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
