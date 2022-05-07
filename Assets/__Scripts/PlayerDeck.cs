using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public List<Card> cards;
    public SpriteRenderer[] spriteRenderers;
    public bool isPlayer;
    public bool isMouseActive = false;
    public float cardInterval = 0.7f; //ī�� ����
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

    public void StartGame()     //���� ���� �� �ʱ�ȭ
    {
        StartCoroutine(SinglyCardSet());
        SetSortOrder();
    }

    void CardSet()                      //�Ѳ����� ī�� ����
    {
        Card c;
        for (int cNum = 0; cNum < cards.Count; cNum++)
        {
            c = cards[cNum];
            c.StartCoroutine(c.LocalMoveTo(new Vector3(cNum * cardInterval, 0, cNum * -0.1f), 1f)); //Z���� Ŭ���� ���� ������ �߰�
            c.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    IEnumerator SinglyCardSet()         //�ϳ��� ī�� ����
    {
        Card c;
        for (int cNum = 0; cNum < cards.Count; cNum++)
        {
            c = cards[cNum];
            yield return c.StartCoroutine(c.LocalMoveTo(new Vector3(cNum * cardInterval, 0, cNum * -0.1f), 0.3f)); //Z���� Ŭ���� ���� ������ �߰�
            c.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        setBack();
        yield return StartCoroutine(SameCardDrop());    //(���⼭���� �߰�) �ߺ�ī�� ������ �ٽ� ����
        yield return StartCoroutine(SetMouseActive());
    }
    public IEnumerator SameCardDrop()         //�ߺ� ī�� ������
    {
        Card c1;
        Card c2;
        for (int c1Num = 0; c1Num < cards.Count - 1; c1Num++)
        {
            c1 = cards[c1Num];
            for (int c2Num = c1Num + 1; c2Num < cards.Count; c2Num++)
            {
                c2 = cards[c2Num];
                if (c1.rank == c2.rank)     //���� ���ڸ� ����
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
    public IEnumerator SameCardDrop(System.Action<int,int,int> rule)         //�ߺ� ī�� ������
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
                if (c1.rank == c2.rank)     //���� ���ڸ� ����
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
        c.StartCoroutine(c.MoveTo(Vector3.zero, 1f)); //���� ī��� 0,0 ��ǥ�� ����
        c.transform.rotation = Quaternion.Euler(Vector3.zero);
        c.faceUp = true;
        cards.Remove(c);
        c.transform.parent = FindObjectOfType<Deck>().deckAnchor;
        c.transform.parent.GetComponent<SortLayout>().SetSortOrder();
    }

    void SetIndex()     //ī�� ����Ʈ��� �ڽ� ������Ʈ ������
    {
        for(int i = 0; i < cards.Count; i++)
        {

            cards[i].transform.SetSiblingIndex(i);
        }
    }

    public void SetSortOrder()      //�׸��� ���ĺ����� �ʰ� ���̾� ����
    {
        foreach (var c in cards)
        {
            c.faceUp = false;
        }
        SetIndex();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        int sOrd = 0;
        sOrd += 1000 * int.Parse(gameObject.name);  //(�߰�) �÷��̾� ���� ���̾� ���̸� ���ؼ�
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

    public void setBack()       //�ڽ��� �ո� ��ǻ�ʹ� �޸�
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

    public void PDShuffle()     //ī�� ����
    {
        Deck.Shuffle(ref cards);
        CardSet();
        SetSortOrder();
        setBack();
    }
}
