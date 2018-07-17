using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPool : MonoBehaviour {
    private Queue<GameObject> ballPool = new Queue<GameObject>();

    public GameObject BasicBall_prefab;

    [System.Serializable]
    public struct BallBasicInfo
    {
        public Sprite ball_sptite;
        public int BasicScore;
    }

    public BallBasicInfo[] balls_info;
    
    public Sprite[] Ball_sprites;


    [Header("生成最大數量")]
    public int PoolMaxBallNumber;

    [Header("遊戲當前總數")]
    public int BallIntotal;

    void Awake()
    {
        for (int i = 0; i < PoolMaxBallNumber; i++)
        {
           GameObject tmp_go = InstantiatelBall();
            ballPool.Enqueue(tmp_go);
        }
    }

    GameObject InstantiatelBall()
    {
        GameObject tmp_go = Instantiate<GameObject>(BasicBall_prefab);
        RandomBallInfo(tmp_go);
        BallIntotal++;
        return tmp_go;
    }

    public void SpawnBall(Vector3 pos)
    {
        GameObject tmp_go;
        if (ballPool.Count <= 0)
        {
            tmp_go = InstantiatelBall();
        }
        else
        {
            tmp_go = ballPool.Dequeue();
        }
        tmp_go.transform.position = pos;
        tmp_go.SetActive(true);
    }

    public void RecycleBall(GameObject recycle)
    {
        RandomBallInfo(recycle);
        ballPool.Enqueue(recycle);
    }

    void RandomBallInfo(GameObject Input_Ball)
    {
        Input_Ball.SetActive(false);
        Input_Ball.GetComponent<BallBasic>().type = Random.Range(0, balls_info.Length);
        Input_Ball.GetComponent<SpriteRenderer>().sprite = balls_info[Input_Ball.GetComponent<BallBasic>().type].ball_sptite;
    }
}
