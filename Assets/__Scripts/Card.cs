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
    
    public void Turn()         //ī�带 �����ϴ� �Լ�(������ �� ��)
    {
        PlayerDeck pDeck = transform.parent.GetComponent<PlayerDeck>();  //previousPlayerDeck
        pDeck.cards.Remove(GetComponent<Card>());     //������ �÷��̾�� ī�� ����Ʈ���� ����
        GameObject newDeck = deck.GetComponent<Deck>().playerDeck[Prospector.playerTurnNum];    //���� ���� �÷��̾ �ҷ�����

        transform.parent = newDeck.transform;   //�÷��̾ �̵�
        PlayerDeck tDeck = newDeck.GetComponent<PlayerDeck>();  //tempPlayerDeck
        tDeck.cards.Add(GetComponent<Card>());

        if (tDeck.isPlayer)      //�÷��̾�� �ո� ��ǻ�͸� �޸�
            faceUp = true;
        else
            faceUp = false;

        StartCoroutine(LocalMoveTo(new Vector3((tDeck.cards.Count - 1) * tDeck.cardInterval, 0, (tDeck.cards.Count - 1) * -0.1f), 0.5f)); //�÷��̾���� ī�� ��ġ �̵�
        StartCoroutine(PickCard(tDeck, pDeck, 3f));
    }

    virtual public void OnMouseUpAsButton()     //�÷��̾�� Ŭ������ ���� �ѱ�
    {
        if (transform.parent.GetComponent<PlayerDeck>() == null)    //������ ī�� Ŭ�� �� return
            return;
        if (!transform.parent.GetComponent<PlayerDeck>().isMouseActive)   //���콺 Ŭ���� Ȱ��ȭ �Ǿ����� �ʴٸ� return
            return;
        Turn();
    }

    IEnumerator PickCard(PlayerDeck tDeck, PlayerDeck pDeck, float time)      //1�ʵ� ����
    {
        FindObjectOfType<Prospector>().PDMouseActiveOFF();
        tDeck.SetSortOrder();
        tDeck.setBack();
        pDeck.SetSortOrder();
        pDeck.setBack();
        yield return new WaitForSeconds(1f);
        tDeck.StartCoroutine(tDeck.SameCardDrop((x, y, z) => //ī�� ��ȿ Ȯ�� �� ����
        {
            d = x;
            j = y;
            k = z;
        }));
        pDeck.PDShuffle();  //ī����� ���� ī�� ����
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


    public IEnumerator LocalMoveTo(Vector3 toPos, float time)       //toPos ������ġ�� time �ð����� �̵�
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
    public IEnumerator MoveTo(Vector3 toPos, float time)                   //toPos ������ġ�� time �ð����� �̵�  
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
