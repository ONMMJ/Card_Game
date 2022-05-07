using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Prospector : MonoBehaviour
{
    static public Prospector S;

    static public int playerTurnNum;
    static public int playerCount;
    int turnDirection;
    public GameObject playButton;
    public GameObject reStartButton;
    [Header("Set in Inspector")]
    public TextAsset deckXML;
    [Header("Set Dynamically")]
    public Deck deck;
    void Awake()
    {
        S = this;
    }

    public void Play()
    {
        if (FindObjectOfType<InputCount>().IF.text == "")
            return;
        else
            playerCount = int.Parse(FindObjectOfType<InputCount>().IF.text);
        playButton.SetActive(false);
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        Card c;
        for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        {
            c = deck.cards[cNum];
            c.transform.localPosition = Vector3.zero;
        }

        deck.Play(playerCount);
        foreach (var tPD in deck.playerDeck)
        {
            tPD.GetComponent<PlayerDeck>().StartGame();
        }
        turnDirection = 1;  //1: ������, 2:������
        playerTurnNum = 0;  //0�� �÷��̾�
    }

    public void ReStart()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    public void PDMouseActiveOFF()         //���콺 Ŭ�� ��Ȱ��ȭ(AI������ �ƴ� ������� �����̳� �߰����� �� ���� �� �ʿ�)
    {
        foreach (var PD in deck.playerDeck)
        {
            PD.GetComponent<PlayerDeck>().isMouseActive = false;
        }
    }

    public void PDMouseActiveON(int direction)         //���콺 Ŭ�� Ȱ��ȭ(AI������ �ƴ� ������� �����̳� �߰����� �� ���� �� �ʿ�)
    {
        foreach(var PD in deck.playerDeck)
        {
            PD.GetComponent<PlayerDeck>().isMouseActive = false;
        }
        if (playerTurnNum != 0)
            return;
        deck.playerDeck[NextIndex(direction, 1, 0)].GetComponent<PlayerDeck>().isMouseActive = true;
    }

    int NextIndex(int direction, int jump, int king)     //���� ���� ��ȣ(direction 1: ����, -1: ���� / jump 1: 1�÷��̾� ��, 2: 2�÷��̾� �� / king 0: �״��, 1: �� �� ��)
    {
        int Index = playerTurnNum;
        if (king == 1)
            return playerTurnNum %= playerCount;

        if (direction == 1)
            Index = (playerTurnNum + jump) % playerCount;
        else if (direction == -1)
            Index = (playerTurnNum + playerCount - jump) % playerCount;

        return Index;
    }

    public void NextTurn(int direction, int jump, int king)      //���� ���ʷ� �ѱ��(direction 1: ����, -1: ���� / jump 1: 1�÷��̾� ��, 2: 2�÷��̾� �� / king 0: �״��, 1: �� �� ��)
    {
        int nowPlayerNum;
        int nextPlayerNum;

        if (deck.playerDeck[0].GetComponent<PlayerDeck>().cards.Count == 0 && deck.playerDeck[0].GetComponent<PlayerDeck>().isPlayer)
        {
            GameEnd("You Win");
            return;
        }

        nowPlayerNum = playerTurnNum;

        if (deck.playerDeck[playerTurnNum].GetComponent<PlayerDeck>().cards.Count == 0)     //���� ������ �÷��̾��� ī�尡 �������� ����
        {
            deck.playerDeck.RemoveAt(playerTurnNum);
            playerCount--;
            playerTurnNum %= playerCount;
            if (deck.playerDeck[playerTurnNum].GetComponent<PlayerDeck>().cards.Count == 0)  //���� ������ �÷��̾��� ���� �÷��̾� ī�尡 �������� ����
            {
                deck.playerDeck.RemoveAt(playerTurnNum);
                playerCount--;
                turnDirection *= direction;
                if (turnDirection == 1)
                    playerTurnNum %= playerCount + jump - 1;
                else if (turnDirection == -1)
                    playerTurnNum = NextIndex(turnDirection, jump, 0);

            }
            else if (deck.playerDeck[NextIndex(turnDirection, 1, 0)].GetComponent<PlayerDeck>().cards.Count == 0)  //���� ������ �÷��̾��� ���� �÷��̾� ī�尡 �������� ����
            {
                deck.playerDeck.RemoveAt(NextIndex(turnDirection, 1, 0));
                playerCount--;
                turnDirection *= direction;
                playerTurnNum = NextIndex(turnDirection, jump, 0);
            }
            else        //���� ������ ���� �÷��̾��� ī�尡 �������� �ʾ��� ��
            {
                turnDirection *= direction;
                if (turnDirection == 1)
                    playerTurnNum %= playerCount + jump - 1;
                else if (turnDirection == -1)
                    playerTurnNum = NextIndex(turnDirection, jump, 0);
            }
        }
        else if (deck.playerDeck[NextIndex(turnDirection, 1, 0)].GetComponent<PlayerDeck>().cards.Count == 0)  //���� ������ �÷��̾��� ���� �÷��̾� ī�尡 �������� ����
        {
            deck.playerDeck.RemoveAt(NextIndex(turnDirection, 1, 0));
            playerCount--;

            turnDirection *= direction;
            playerTurnNum = NextIndex(turnDirection, jump, king);
        }
        else
        {
            turnDirection *= direction;
            playerTurnNum = NextIndex(turnDirection, jump, king);
        }


        if (playerCount == 1)
        {
            GameEnd("You Lose");
            return;
        }

        nextPlayerNum = playerTurnNum;

        deck.playerDeck[nowPlayerNum].GetComponent<PlayerDeck>().crown.SetActive(false);
        deck.playerDeck[nextPlayerNum].GetComponent<PlayerDeck>().crown.SetActive(true);

        PDMouseActiveON(turnDirection);
        if (playerTurnNum != 0)     //�÷��̾�(0)�� �ƴ� �� ��ǻ�Ͱ� ����
            ComTurn(turnDirection);
    }

    void ComTurn(int direction)      //��ǻ�� �ڵ� ����
    {
        PlayerDeck nPD = deck.playerDeck[NextIndex(direction, 1, 0)].GetComponent<PlayerDeck>();   //NextPlayerDeck
        nPD.cards[Random.Range(0, nPD.cards.Count)].Turn();
    }

    void GameEnd(string WoL)    //Win or Lose
    {
        reStartButton.SetActive(true);
        reStartButton.transform.Find("WoLText").GetComponent<Text>().text = WoL;
    }
}
