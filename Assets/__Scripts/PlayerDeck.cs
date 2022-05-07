using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public List<Card> cards;
    public SpriteRenderer[] spriteRenderers;
    public bool isPlayer;
    public bool isMouseActive = false;
    public float cardInterval = 0.7f; //카드 간격
    public GameObject crown;
    void Start()
    {
        if (isPlayer)
            crown.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()     //게임 시작 시 초기화
    {
        StartCoroutine(SinglyCardSet());
        SetSortOrder();
    }

    void CardSet()                      //한꺼번에 카드 정렬
    {
        Card c;
        for (int cNum = 0; cNum < cards.Count; cNum++)
        {
            c = cards[cNum];
            c.StartCoroutine(c.LocalMoveTo(new Vector3(cNum * cardInterval, 0, cNum * -0.1f), 1f)); //Z축은 클릭을 위해 간격을 추가
            c.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    IEnumerator SinglyCardSet()         //하나씩 카드 정렬
    {
        Card c;
        for (int cNum = 0; cNum < cards.Count; cNum++)
        {
            c = cards[cNum];
            yield return c.StartCoroutine(c.LocalMoveTo(new Vector3(cNum * cardInterval, 0, cNum * -0.1f), 0.3f)); //Z축은 클릭을 위해 간격을 추가
            c.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        setBack();
        yield return StartCoroutine(SameCardDrop());    //(여기서부터 추가) 중복카드 버리고 다시 세팅
        yield return StartCoroutine(SetMouseActive());
    }
    public IEnumerator SameCardDrop()         //중복 카드 버리기
    {
        Card c1;
        Card c2;
        for (int c1Num = 0; c1Num < cards.Count - 1; c1Num++)
        {
            c1 = cards[c1Num];
            for (int c2Num = c1Num + 1; c2Num < cards.Count; c2Num++)
            {
                c2 = cards[c2Num];
                if (c1.rank == c2.rank)     //같은 숫자면 실행
                {
                    DropCard(c1);
                    DropCard(c2);
                    yield return new WaitForSeconds(1f);
                    c1Num--;
                    break;
                }
            }
        }
        PDShuffle();
    }
    public IEnumerator SameCardDrop(System.Action<int,int,int> rule)         //중복 카드 버리기
    {
        Card c1;
        Card c2;
        int ruleNum = 1;
        for (int c1Num = 0; c1Num < cards.Count - 1; c1Num++)
        {
            c1 = cards[c1Num];
            for (int c2Num = c1Num + 1; c2Num < cards.Count; c2Num++)
            {
                c2 = cards[c2Num];
                if (c1.rank == c2.rank)     //같은 숫자면 실행
                {
                    DropCard(c1);
                    DropCard(c2);
                    ruleNum = c1.rank;
                    yield return new WaitForSeconds(1f);
                    c1Num--;
                    break;
                }
            }
        }
        if (ruleNum == 11)
            rule(1, 2, 0);
        else if (ruleNum == 12)
            rule(-1, 1, 0);
        else if (ruleNum == 13)
            rule(1, 1, 1);
        else
            rule(1, 1, 0);
        PDShuffle();
    }
    IEnumerator SetMouseActive()
    {
        if ((Prospector.playerTurnNum + 1) % Prospector.playerCount == int.Parse(gameObject.name))
            isMouseActive = true;
        yield return null;
    }
    void DropCard(Card c)
    {
        c.StartCoroutine(c.MoveTo(Vector3.zero, 1f)); //같은 카드는 0,0 좌표로 버림
        c.transform.rotation = Quaternion.Euler(Vector3.zero);
        c.faceUp = true;
        cards.Remove(c);
        c.transform.parent = FindObjectOfType<Deck>().deckAnchor;
        c.transform.parent.GetComponent<SortLayout>().SetSortOrder();
    }

    void SetIndex()     //카드 리스트대로 자식 오브젝트 재정렬
    {
        for(int i = 0; i < cards.Count; i++)
        {

            cards[i].transform.SetSiblingIndex(i);
        }
    }

    public void SetSortOrder()      //그림이 곂쳐보이지 않게 레이어 설정
    {
        foreach (var c in cards)
        {
            c.faceUp = false;
        }
        SetIndex();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        int sOrd = 0;
        sOrd += 1000 * int.Parse(gameObject.name);  //(추가) 플레이어 별로 레이어 차이를 위해서
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var tSR = spriteRenderers[i];
            if (tSR.GetComponent<Card>() != null)
            {
                sOrd += 3;
                tSR.sortingOrder = sOrd;
                continue;
            }
            switch (tSR.gameObject.name)
            {
                case "back":
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "face":
                default:
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    public void setBack()       //자신은 앞면 컴퓨터는 뒷면
    {
        Card c;
        for (int cNum = 0; cNum < cards.Count; cNum++)
        {
            c = cards[cNum];
            if (isPlayer)
                c.faceUp = true;
            else
                c.faceUp = false;
        }
    }

    public void PDShuffle()     //카드 섞기
    {
        Deck.Shuffle(ref cards);
        CardSet();
        SetSortOrder();
        setBack();
    }
}
