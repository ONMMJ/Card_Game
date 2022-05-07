using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{

    int d, j, k;
    [Header("Set Dynamically")]
    public string suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";
    public List<GameObject> decoGOs = new List<GameObject>();
    public List<GameObject> pipGos = new List<GameObject>();
    public GameObject back;
    public CardDefinition def;

    public GameObject deck;
    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Turn()         //카드를 선택하는 함수(게임의 한 턴)
    {
        PlayerDeck pDeck = transform.parent.GetComponent<PlayerDeck>();  //previousPlayerDeck
        pDeck.cards.Remove(GetComponent<Card>());     //기존의 플레이어덱에 카드 리스트에서 제거
        GameObject newDeck = deck.GetComponent<Deck>().playerDeck[Prospector.playerTurnNum];    //현재 턴의 플레이어덱 불러오기

        transform.parent = newDeck.transform;   //플레이어덱 이동
        PlayerDeck tDeck = newDeck.GetComponent<PlayerDeck>();  //tempPlayerDeck
        tDeck.cards.Add(GetComponent<Card>());

        if (tDeck.isPlayer)      //플레이어면 앞면 컴퓨터면 뒷면
            faceUp = true;
        else
            faceUp = false;

        StartCoroutine(LocalMoveTo(new Vector3((tDeck.cards.Count - 1) * tDeck.cardInterval, 0, (tDeck.cards.Count - 1) * -0.1f), 0.5f)); //플레이어덱으로 카드 위치 이동
        StartCoroutine(PickCard(tDeck, pDeck, 3f));
    }

    virtual public void OnMouseUpAsButton()     //플레이어는 클릭으로 턴을 넘김
    {
        if (transform.parent.GetComponent<PlayerDeck>() == null)    //버려진 카드 클릭 시 return
            return;
        if (!transform.parent.GetComponent<PlayerDeck>().isMouseActive)   //마우스 클릭이 활성화 되어있지 않다면 return
            return;
        Turn();
    }

    IEnumerator PickCard(PlayerDeck tDeck, PlayerDeck pDeck, float time)      //1초뒤 섞기
    {
        FindObjectOfType<Prospector>().PDMouseActiveOFF();
        tDeck.SetSortOrder();
        tDeck.setBack();
        pDeck.SetSortOrder();
        pDeck.setBack();
        yield return new WaitForSeconds(1f);
        tDeck.StartCoroutine(tDeck.SameCardDrop((x, y, z) => //카드 유효 확인 후 섞기
        {
            d = x;
            j = y;
            k = z;
        }));
        pDeck.PDShuffle();  //카드뽑은 유저 카드 섞기
        yield return new WaitForSeconds(time);
        Debug.Log(d);
        Debug.Log(j);
        Debug.Log(k);
        FindObjectOfType<Prospector>().NextTurn(d, j, k);
    }

    public void DropCard()
    {
        StartCoroutine(MoveTo(Vector3.zero, 1f));
        back.SetActive(false);
    }


    public IEnumerator LocalMoveTo(Vector3 toPos, float time)       //toPos 로컬위치로 time 시간동안 이동
    {
        float count = 0;
        Vector3 wasPos = gameObject.transform.localPosition;
        while (true)
        {
            count += Time.deltaTime;
            gameObject.transform.localPosition = Vector3.Lerp(wasPos, toPos, count/time);

            if (count >= time)
            {
                gameObject.transform.localPosition = toPos;
                break;
            }
            yield return null;
        }
    }
    public IEnumerator MoveTo(Vector3 toPos, float time)                   //toPos 월드위치로 time 시간동안 이동  
    {
        float count = 0;
        Vector3 wasPos = gameObject.transform.position;
        while (true)
        {
            count += Time.deltaTime;
            gameObject.transform.position = Vector3.Lerp(wasPos, toPos, count/time);

            if (count >= time)
            {
                gameObject.transform.position = toPos;
                break;
            }
            yield return null;
        }
    }
}

[System.Serializable]
public class Decorator
{
    public string type;
    public Vector3 loc;
    public bool flip = false;
    public float scale = 1f;
}
[System.Serializable]
public class CardDefinition
{
    public string face;
    public int rank;
    public List<Decorator> pips = new List<Decorator>();
}
