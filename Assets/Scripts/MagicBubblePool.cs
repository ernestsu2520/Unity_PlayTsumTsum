using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBubblePool : MonoBehaviour {
    private Queue<GameObject> magicBubblePool = new Queue<GameObject>();
    public int SpawnCount;
    public GameObject Bubble_prefab;
    public Sprite[] BubbleSprites;

    [System.Serializable]
    public struct BubbleRate
    {
        public float normal_bubble;
        public float time_bubble;
        public float star_bubble;
        public float coin_bubble;
        public float score_bubble;
    }

    [Header("各類泡泡生成機率")]
    public BubbleRate[] bubbleRate;

    private void OnDrawGizmos()
    {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GameObject.Find("_GM").GetComponent<GamePlayController>().bubbleRadious);
    }

    void Awake()
    {
        for (int i = 0; i < SpawnCount; i++)
        {
            GameObject go = Instantiate<GameObject>(Bubble_prefab);
            magicBubblePool.Enqueue(go);
            go.SetActive(false);
        }
    }

    public void SpawnMagicBubble(int link, Vector3 lastBallPos) //製作魔法泡泡
    {
        if (link >= 7 && link <= 21)
        {
            GameObject go;
            if (magicBubblePool.Count > 0)
            {
                go = magicBubblePool.Dequeue();    
            }
            else
            {
                go = Instantiate(Bubble_prefab);
            }

            int ran = Random.Range(0, 100);
            float normal_rate = bubbleRate[link - 7].normal_bubble;
            float time_rate = bubbleRate[link - 7].time_bubble + normal_rate;
            float star_rate = bubbleRate[link - 7].star_bubble + time_rate;
            float coin_rate = bubbleRate[link - 7].coin_bubble + star_rate;
            float score_rate = bubbleRate[link - 7].score_bubble + coin_rate;

            if (ran < normal_rate && bubbleRate[link - 7].normal_bubble != 0)
            {
                go.GetComponent<SpriteRenderer>().sprite = BubbleSprites[0];
                go.GetComponent<BubbleBasic>().type = 0;
            }
            else if (ran < time_rate && bubbleRate[link - 7].time_bubble != 0)
            {
                go.GetComponent<SpriteRenderer>().sprite = BubbleSprites[1];
                go.GetComponent<BubbleBasic>().type = 1;
            }
            else if (ran < star_rate && bubbleRate[link - 7].star_bubble != 0)
            {
                go.GetComponent<SpriteRenderer>().sprite = BubbleSprites[2];
                go.GetComponent<BubbleBasic>().type = 2;
            }
            else if (ran < coin_rate && bubbleRate[link - 7].coin_bubble != 0)
            {
                go.GetComponent<SpriteRenderer>().sprite = BubbleSprites[3];
                go.GetComponent<BubbleBasic>().type = 3;
            }
            else if (ran < score_rate && bubbleRate[link - 7].score_bubble != 0)
            {
                go.GetComponent<SpriteRenderer>().sprite = BubbleSprites[4];
                go.GetComponent<BubbleBasic>().type = 4;
            }

            go.transform.position = lastBallPos;
            go.SetActive(true);
        }
    }

    public void ReCycleBubble(GameObject recycle)
    {
        magicBubblePool.Enqueue(recycle);
        recycle.SetActive(false);
    }
}
