using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("Set In Inspector")]
    public Sprite suitJoker;
    public bool startFaceUp = false;
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;
    public Sprite[] faceSprites;
    public Sprite[] rankSprites;
    public Sprite cardBack;
    public Sprite cardFront;
    public GameObject prefabCard;
    public GameObject prefabSprite;

    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuites;

    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;
    private Sprite _tSp = null;


    [Header("Set Player")]
    public int playerCount;
    public List<Vector3> playerCardPosition;
    public List<Vector3> playerCardRotation;
    public GameObject playerDeckPrefab;

    public List<GameObject> playerDeck;
    public GameObject deckPrefab;
    public void Play(int pCount)
    {
        playerCount = pCount;
        int deckCount = Mathf.CeilToInt((float)cards.Count / (float)playerCount);     //카드 분배 개수
        Prospector.playerCount = playerCount;
        //  ※다시 플레이시 플레이어덱을 전체덱으로 옮기고 Clear하는 코드 필요

        for(int i = 0; i < playerCount; i++)    //카드 분배
        {
            GameObject tDeck;
            if (playerCount ==2&& i ==1)
                tDeck = Instantiate(playerDeckPrefab, playerCardPosition[2], Quaternion.Euler(playerCardRotation[2]));
            else
                tDeck = Instantiate(playerDeckPrefab, playerCardPosition[i], Quaternion.Euler(playerCardRotation[i]));
            tDeck.name = i.ToString();
            PlayerDeck playerdeck = tDeck.GetComponent<PlayerDeck>();
            for (int j = 0; j < deckCount; j++) {
                if (cards.Count == 0)
                {
                    break;
                }
                playerdeck.cards.Add(cards[0]);
                cards[0].transform.parent = tDeck.transform;
                cards.RemoveAt(0);
            }

            if (i == 0)         //플레이어인지 컴퓨터인지 확인(0번이 플레이어)
                playerdeck.isPlayer = true;
            else
                playerdeck.isPlayer = false;
            playerDeck.Add(tDeck);
        }
    }
    public void InitDeck(string deckXMLText)
    {
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = Instantiate(deckPrefab);
            deckAnchor = anchorGO.transform;
        }
        dictSuites = new Dictionary<string, Sprite>()
            {
            {"C", suitClub},
            {"D", suitDiamond},
            {"H", suitHeart},
            {"S", suitSpade},
            {"J", suitJoker}
            };
        ReadDeck(deckXMLText);

        MakeCards();
    }
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach (var cd in cardDefs)
        {
            if (cd.rank == rnk)
            {
                return cd;
            }
        }
        return null;
    }
    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (var s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }
        cardNames.Add("J" + 14);
        cards = new List<Card>();
        for (int i = 0; i < cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }
    private Card MakeCard(int cNum)
    {
        GameObject cgo = Instantiate(prefabCard) as GameObject;
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        card.deck = gameObject;

        if (card.suit == "D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }
        card.def = GetCardDefinitionByRank(card.rank);
        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }
    private void AddPips(Card card)
    {
        foreach (var pip in card.def.pips)
        {
            _tGO = Instantiate(prefabSprite);
            _tGO.transform.parent = card.transform;
            _tGO.transform.localPosition = pip.loc;
            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuites[card.suit];
            _tSR.sortingOrder = 1;
            card.pipGos.Add(_tGO);
        }
    }
    private void AddFace(Card card)
    {
        if (card.def.face == "")
        {
            return;
        }
        _tGO = Instantiate(prefabSprite);
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSp = GetFace(card.def.face + card.suit);
        _tSR.sprite = _tSp;
        _tSR.sortingOrder = 1;
        _tGO.transform.parent = card.transform;
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }
    private Sprite GetFace(string faceS)
    {
        foreach (var _tSP in faceSprites)
        {
            if (_tSP.name == faceS)
            {
                return _tSP;
            }
        }
        return null;
    }
    private void AddDecorators(Card card)
    {
        foreach (var deco in decorators)
        {
            if (deco.type == "suit")
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSR.sprite = dictSuites[card.suit];
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSR.sprite = rankSprites[card.rank];
                _tSR.color = card.color;
            }
            _tSR.sortingOrder = 1;
            _tGO.transform.parent = card.transform;
            _tGO.transform.localPosition = deco.loc;
            if (deco.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            _tGO.name = deco.type;
            card.decoGOs.Add(_tGO);
        }
    }
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);
        string s = "xml[0] decorator[0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        decorators = new List<Decorator>();
        PT_XMLHashList xDeocs = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i = 0; i < xDeocs.Count; i++)
        {
            deco = new Decorator();
            deco.type = xDeocs[i].att("type");
            deco.flip = (xDeocs[i].att("flip") == "1");
            deco.scale = float.Parse(xDeocs[i].att("scale"));
            deco.loc.x = float.Parse(xDeocs[i].att("x"));
            deco.loc.y = float.Parse(xDeocs[i].att("y"));
            deco.loc.z = float.Parse(xDeocs[i].att("z"));
            decorators.Add(deco);
        }

        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            CardDefinition cDef = new CardDefinition();
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator();
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }
    }
    private void AddBack(Card card)
    {
        _tGO = Instantiate(prefabSprite);
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.parent = card.transform;
        _tGO.transform.localPosition = Vector3.zero;
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;
        card.faceUp = startFaceUp;
    }

    static public void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();
        int ndx;
        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }
        oCards = tCards;
    }

}
