using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using TMPro;
using UnityEngine;


public class Card : IEquatable<Card>
{
    public enum SUIT { SPADE, HEART, DIAMOND, CLOVER };

    public SUIT suit { get; private set; }

    public int no { get; private set; }

    public bool isCommunity { get; set; }

        
    public Card(SUIT suit, int no, bool isCommunity)
    {
        this.suit = suit;
        this.no = no;
        this.isCommunity = isCommunity;
    }

    public bool Equals(Card other)
    {
        if (this is null) return false;
        if (other is null) return false;
        if(ReferenceEquals(this, other)) return true;
        return (this.suit == other.suit) && (this.no == other.no);
    }

}

public enum HAND
{
    ROYAL_FLUSH, STRAIGHT_FLUSH, FOUR_KIND, FUll_HOUSE,
    FLUSH, STRAIGHT, THREE_KIND, TWO_PAIR, PAIR, HIGH_CARD
}

public class PlayerInfo
{
    string player;
    public string Player
    {
        get { return player; }
    }

    Card card1;
    public Card Card1
    {
        get { return card1; }
    }
    Card card2;
    public Card Card2
    {
        get { return card2; }
    }
    public HAND playerHand;

    public Card highestCard;

    public PlayerInfo(string player, Card card1, Card card2)
    {
        this.player = player;
        this.card1 = card1;
        this.card2 = card2;
    }
}

public class PokerGame : MonoBehaviour
{

    const int COMMUNITYNUM = 5;
    const int MAXCARDNUM = 52;
    List<Card> Deck = new List<Card>();

    void InitDeck()
    {
        for(Card.SUIT suit = Card.SUIT.SPADE; suit <= Card.SUIT.CLOVER; suit++)
        {
            for(int i = 0; i < 13; i++)
            {
                Deck.Add(new Card(suit, i + 2, true));
            }
        }
    }

    Card[] GetRandCards(int numCard)
    {
        Card[] cards = new Card[numCard];
        for (int n = 0; n < numCard; n++)
        {
            int ranIndex = UnityEngine.Random.Range(0, Deck.Count);
            cards[n] = Deck[ranIndex];
            Deck.Remove(Deck[ranIndex]);
        }
        return cards;
    }


    public PokerCardPrefabs cardPrefabs;
    GameObject GetCardPrefab(int suit, int rank)
    {
        if (suit == 0)
            return cardPrefabs.SpadeCards[rank];
        else if (suit == 1)
            return cardPrefabs.HeartCards[rank];
        else if (suit == 2)
            return cardPrefabs.DiamodCards[rank];
        else if (suit == 3)
            return cardPrefabs.CloverCards[rank];
        else return null;
    }


    [SerializeField]
    int playerNum;

    [SerializeField]
    Transform[] cardPlaces;
    [SerializeField]
    GameObject[] playerPlace;

    [SerializeField]
    TextMeshProUGUI[] playerHand;

    Card[] communityCards = new Card[COMMUNITYNUM];
    public List<PlayerInfo> playersInfo = new List<PlayerInfo>();

    PlayerInfo winner;
    [SerializeField]
    TextMeshProUGUI winnerText;
    void Start()
    {
        InitDeck();
/*        communityCards = GetRandCards(COMMUNITYNUM);
        for (int i = 0; i < communityCards.Length; i++)
        {
            GameObject cObj = GetCardPrefab((int)communityCards[i].Suit, communityCards[i].NO - 2);
            var c = Instantiate(cObj, cardPlaces[i].position, 
                Quaternion.Euler(cObj.transform.rotation.eulerAngles + new Vector3(90, 0, 0)));
            //c.transform.Rotate(c.transform.rotation.eulerAngles + new Vector3(0, 0, 180));
        }*/

/*        playersInfo = new PlayerInfo[playerNum];
        for(int i = 0; i < playerNum; i++)
        {
            playersInfo[i] = new PlayerInfo((i+ 1).ToString(), GetRandCards(1)[0], GetRandCards(1)[0]);
            playersInfo[i].Card1.IsCommunity = false;
            playersInfo[i].Card2.IsCommunity = false;
            Find_Hand3(playersInfo[i]);

            //show player card
            GameObject cObj1 = GetCardPrefab((int)playersInfo[i].Card1.Suit, playersInfo[i].Card1.NO - 2);
            GameObject cObj2 = GetCardPrefab((int)playersInfo[i].Card2.Suit, playersInfo[i].Card2.NO - 2);
            Instantiate(cObj1, playerPlace[i].transform.GetChild(0));
            Instantiate(cObj2, playerPlace[i].transform.GetChild(1));

            //show player highest card
            GameObject cObjH = GetCardPrefab((int)playersInfo[i].highestCard.Suit, playersInfo[i].highestCard.NO - 2);
            Instantiate(cObjH, playerPlace[i].transform.GetChild(2));

            //show player hand
            playerPlace[i].transform.GetChild(3).GetComponent<TextMeshPro>().text = playersInfo[i].playerHand.ToString();
        }*/

/*        //winner
        PlayerInfo[] winners = playersInfo.Where(n => n.playerHand == playersInfo.Min(x => x.playerHand)).ToArray();
        if(winners.Length > 1)
        {
            winners = winners.OrderBy(c => c.highestCard.Suit).OrderByDescending(c => c.highestCard.NO).ToArray();
            winner = winners[0];
        }
        else { winner = winners[0]; }
        winnerText.text = "Winner : " + winner.Player;*/

    }

    //�÷��̾�� �г��� ȭ�鿡 ����
    public void SetPlayerNickname(int pos, string nickname)
    {
        playerPlace[pos].transform.GetChild(4).GetComponent<TextMeshPro>().text = nickname;
    }

    //Ŀ�´�Ƽ ī�� ���� _ ����(���� ��)
    public void SetCommunityCard_Server()
    {
        communityCards = GetRandCards(COMMUNITYNUM);
        for (int i = 0; i < communityCards.Length; i++)
        {
            GameObject cObj = GetCardPrefab((int)communityCards[i].suit, communityCards[i].no - 2);
            var c = Instantiate(cObj, cardPlaces[i].position,
                Quaternion.Euler(cObj.transform.rotation.eulerAngles + new Vector3(90, 0, 0)));
            //c.transform.Rotate(c.transform.rotation.eulerAngles + new Vector3(0, 0, 180));
        }
    }

    //Ŀ�´�Ƽ ī�� ���� _ Ŭ��(���� ��)
    public void SetCommunityCard_Client()
    {
        for (int i = 0; i < communityCards.Length; i++)
        {
            GameObject cObj = GetCardPrefab(0, 12);
            var c = Instantiate(cObj, cardPlaces[i].position,
                Quaternion.Euler(cObj.transform.rotation.eulerAngles + new Vector3(90, 0, 0)));
            //c.transform.Rotate(c.transform.rotation.eulerAngles + new Vector3(0, 0, 180));
        }
    }

/*    public void AddPlayerMe(int myIndex, int suit1, int NO1, int suit2, int NO2)
    {
        playersInfo.Add(new PlayerInfo((myIndex + 1).ToString(), 
            new Card((Card.SUIT)suit1, NO1, false), new Card((Card.SUIT)suit2, NO2, false)));
        Find_Hand3(playersInfo[myIndex]);

        //show player card
        GameObject cObj1 = GetCardPrefab((int)playersInfo[myIndex].Card1.suit, playersInfo[myIndex].Card1.no - 2);
        GameObject cObj2 = GetCardPrefab((int)playersInfo[myIndex].Card2.suit, playersInfo[myIndex].Card2.no - 2);
        Instantiate(cObj1, playerPlace[myIndex].transform.GetChild(0));
        Instantiate(cObj2, playerPlace[myIndex].transform.GetChild(1));

        //show player highest card
        GameObject cObjH = GetCardPrefab((int)playersInfo[myIndex].highestCard.suit, playersInfo[myIndex].highestCard.no - 2);
        Instantiate(cObjH, playerPlace[myIndex].transform.GetChild(2));

        //show player hand
        playerPlace[myIndex].transform.GetChild(3).GetComponent<TextMeshPro>().text = playersInfo[myIndex].playerHand.ToString();

    }

    public void AddPlayer(int playerIndex)
    {
        playersInfo.Add(new PlayerInfo((playerIndex + 1).ToString(), GetRandCards(1)[0], GetRandCards(1)[0]));
        playersInfo[playerIndex].Card1.isCommunity = false;
        playersInfo[playerIndex].Card2.isCommunity = false;
        Find_Hand3(playersInfo[playerIndex]);

        //show player card
        GameObject cObj1 = GetCardPrefab((int)playersInfo[playerIndex].Card1.suit, playersInfo[playerIndex].Card1.no - 2);
        GameObject cObj2 = GetCardPrefab((int)playersInfo[playerIndex].Card2.suit, playersInfo[playerIndex].Card2.no - 2);
        Instantiate(cObj1, playerPlace[playerIndex].transform.GetChild(0));
        Instantiate(cObj2, playerPlace[playerIndex].transform.GetChild(1));

        //show player highest card
        GameObject cObjH = GetCardPrefab((int)playersInfo[playerIndex].highestCard.suit, playersInfo[playerIndex].highestCard.no - 2);
        Instantiate(cObjH, playerPlace[playerIndex].transform.GetChild(2));

        //show player hand
        playerPlace[playerIndex].transform.GetChild(3).GetComponent<TextMeshPro>().text = playersInfo[playerIndex].playerHand.ToString();

    }*/

    //�� �÷��̾� ī�� ���� _ ����(��� ���� ��)
    public void DistributeCard(Dictionary<int, string> playerInfos, int playerNum)
    {
        for (int i = 0; i < playerNum; i++)
        {
            playersInfo.Add(new PlayerInfo(playerInfos[i], GetRandCards(1)[0], GetRandCards(1)[0]));
            playersInfo[i].Card1.isCommunity = false;
            playersInfo[i].Card2.isCommunity = false;
            Find_Hand3(playersInfo[i]);

            SetCard_Server(playersInfo[i], i);

/*            //show player card
            GameObject cObj1 = GetCardPrefab((int)playersInfo[i].Card1.suit, playersInfo[i].Card1.no - 2);
            GameObject cObj2 = GetCardPrefab((int)playersInfo[i].Card2.suit, playersInfo[i].Card2.no - 2);
            Instantiate(cObj1, playerPlace[i].transform.GetChild(0).position,
                Quaternion.Euler(playerPlace[i].transform.GetChild(0).rotation.eulerAngles + new Vector3(180, 0, 0)));
            Instantiate(cObj2, playerPlace[i].transform.GetChild(1).position,
                Quaternion.Euler(playerPlace[i].transform.GetChild(1).rotation.eulerAngles + new Vector3(180, 0, 0)));

            //show player highest card
            GameObject cObjH = GetCardPrefab((int)playersInfo[i].highestCard.suit, playersInfo[i].highestCard.no - 2);
            Instantiate(cObjH, playerPlace[i].transform.GetChild(2));

            //show player hand
            playerPlace[i].transform.GetChild(3).GetComponent<TextMeshPro>().text = playersInfo[i].playerHand.ToString();
*/

        }
    }
    public void SetCard_Server(PlayerInfo pInfo, int pos)
    {
        if(pInfo.Player == GameManager.Instance.nickName)
        {
            ShowCardFront((int)pInfo.Card1.suit, pInfo.Card1.no, pos);
        }
        else
        {
            ShowCardBack((int)pInfo.Card1.suit, pInfo.Card1.no, pos);

            //NetworkManager.Instance.SendCardInfo((int)pInfo.Card1.suit, pInfo.Card1.no, pos);
        }
    }
    void ShowCardFront(int suit, int no, int pos)
    {
        GameObject cObj1 = GetCardPrefab(suit, no - 2);
        GameObject cObj2 = GetCardPrefab(suit, no - 2);
        Instantiate(cObj1, playerPlace[pos].transform.GetChild(0).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(0).rotation.eulerAngles));
        Instantiate(cObj2, playerPlace[pos].transform.GetChild(1).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(1).rotation.eulerAngles));
    }
    void ShowCardBack(int suit, int no, int pos)
    {
        GameObject cObj1 = GetCardPrefab(suit, no - 2);
        GameObject cObj2 = GetCardPrefab(suit, no - 2);
        Instantiate(cObj1, playerPlace[pos].transform.GetChild(0).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(0).rotation.eulerAngles + new Vector3(180, 0, 0)));
        Instantiate(cObj2, playerPlace[pos].transform.GetChild(1).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(1).rotation.eulerAngles + new Vector3(180, 0, 0)));
    }


    //�� �÷��̾� ī�� ���� _ Ŭ��(�ڽ� ������ ��)


    public void ShowMyCard(PlayerInfo myInfo, int index)
    {
        //show player card
        GameObject cObj1 = GetCardPrefab((int)myInfo.Card1.suit, myInfo.Card1.no - 2);
        GameObject cObj2 = GetCardPrefab((int)myInfo.Card2.suit, myInfo.Card2.no - 2);
        Instantiate(cObj1, playerPlace[index + 1].transform.GetChild(0).position,
             Quaternion.Euler(playerPlace[index + 1].transform.GetChild(0).rotation.eulerAngles));
        Instantiate(cObj2, playerPlace[index + 1].transform.GetChild(1).position,
            Quaternion.Euler(playerPlace[index + 1].transform.GetChild(1).rotation.eulerAngles ));
        //Instantiate(cObj2, playerPlace[index + 1].transform.GetChild(1));

    }

    public void FindWinner()
    {
        //winner
        PlayerInfo[] winners = playersInfo.Where(n => n.playerHand == playersInfo.Min(x => x.playerHand)).ToArray();
        if (winners.Length > 1)
        {
            winners = winners.OrderBy(c => c.highestCard.suit).OrderByDescending(c => c.highestCard.no).ToArray();
            winner = winners[0];
        }
        else { winner = winners[0]; }
        winnerText.text = "Winner : " + winner.Player;
    }





    void Find_Hand3(PlayerInfo player)
    {
        List<Card> handCardsFlush = new List<Card>();
        List<Card> handCardsStrait = new List<Card>();
        List<Card> handCardsPair = new List<Card>();
        List<Card> cards = new List<Card>();
        for (int i = 0; i < communityCards.Length; i++)
        {
            cards.Add(communityCards[i]);
        }
        cards.Add(player.Card1);
        cards.Add(player.Card2);

        HAND handFlush = Find_FlushGroup(cards, out handCardsFlush);
        HAND handPair = HAND.HIGH_CARD;
        if(handFlush <= HAND.STRAIGHT_FLUSH)
        {
            player.playerHand = handFlush;
        }
        else if((handPair = Find_PairGroup(cards, out handCardsPair)) <= HAND.FUll_HOUSE)
        {
            player.playerHand = handPair;
        }
        else if(handFlush == HAND.FLUSH)
        {
            player.playerHand = handFlush;
        }
        else if(Find_Straight(cards, out handCardsStrait) == HAND.STRAIGHT)
        {
            player.playerHand = HAND.STRAIGHT;
        }
        else
        {
            player.playerHand = handPair;
        }

        if(handCardsFlush != null)
        {        
            player.highestCard = Find_PlayerInvolvedCard(handCardsFlush, player);
        }
        else if (handCardsStrait != null)
        {
            player.highestCard = Find_PlayerInvolvedCard(handCardsStrait, player);
        }
        else
        {
            player.highestCard = Find_PlayerInvolvedCard(handCardsPair, player);
        }
    }

    Card Find_PlayerInvolvedCard(List<Card> cards, PlayerInfo player)
    {
        Card playerCard;
        cards = cards.OrderBy(c => c.suit).OrderByDescending(c => c.no).ToList();
        foreach (Card card in cards)
        {
            if (!card.isCommunity)
            {
                playerCard = card;
                return playerCard;
            }
        }
        if(player.Card1.no > player.Card2.no)
        {
            return player.Card1;
        }
        else if(player.Card1.no < player.Card2.no)
            return player.Card2;
        else
        {
            if(player.Card1.suit > player.Card2.suit)
                return player.Card1;
            else
                return player.Card2;
        }
    }

    HAND Find_FlushGroup(List<Card> cards, out List<Card> handCards)
    {
        cards.Sort((c1, c2) => c1.no.CompareTo(c2.no));
        var sameSuitGroup = cards.GroupBy(c => c.suit).ToList();
        foreach(var sameSuit in sameSuitGroup)
        {
            if(sameSuit.Count() >= 5)
            {
                List<Card> sameSuitList = sameSuit.ToList();
                if ((sameSuitList[sameSuitList.Count - 5].no == 10) && (sameSuitList[sameSuitList.Count - 1].no == 14))
                {
                    handCards = sameSuitList.Skip(sameSuitList.Count - 5).ToList();
                    return HAND.ROYAL_FLUSH;
                }
                else if (sameSuit.Zip(sameSuit.Skip(4), (a, b) => a.no + 4 == b.no ? a : null).Where(n => n != null).Count() >= 1)
                {
                    Card firstCard = sameSuit.Zip(sameSuit.Skip(4), (a, b) => a.no + 4 == b.no ? a : null).Where(n => n != null).Last();
                    //handCards = sameSuitList.Skip(sameSuitList.IndexOf(firstCard)).ToList();
                    handCards = sameSuitList.GetRange(sameSuitList.IndexOf(firstCard), 5);

                    return HAND.STRAIGHT_FLUSH;
                }
                else
                {
                    handCards = sameSuitList;
                    return HAND.FLUSH;
                }
            }
        }
        handCards = null;
        return HAND.HIGH_CARD;
    }

    HAND Find_Straight(List<Card> cards, out List<Card> handCards)
    {
        cards.Sort((c1, c2) => c1.no.CompareTo(c2.no));
        //�ߺ� ���� ����
        List<Card> numsList = cards.GroupBy(n => n.no).Select(m => m.First()).ToList();
        //���� 5�� ���� ã��
        List<Card> continous = numsList.Zip(numsList.Skip(4), (a, b) => a.no + 4 == b.no ? a : null).Where(n => n != null).ToList();

        if (continous.Count >= 1)
        {
            Card firstCard = continous.Last();
            Card lastCard = cards.Where(n => n.no == firstCard.no).FirstOrDefault();
            //handCards = cards.Skip(cards.IndexOf(firstCard)).ToList();
            handCards = cards.GetRange(cards.IndexOf(firstCard), cards.IndexOf(lastCard));
            return HAND.STRAIGHT;
        }
        handCards = null;
        return HAND.HIGH_CARD;
    }

    HAND Find_PairGroup(List<Card> cards, out List<Card> handCards)
    {
        handCards = new List<Card>();
        int pair3 = 0;
        int pair2 = 0;
        var sameNumGroup = cards.GroupBy(x => x.no).ToList();
        foreach (var sameNum in sameNumGroup)
        {
            if (sameNum.Count() >= 4)
            {
                handCards = sameNum.ToList();
                return HAND.FOUR_KIND;

            }
            else if (sameNum.Count() >= 3)
            {
                handCards.AddRange(sameNum.ToList());
                pair3++;

            }
            else if (sameNum.Count() >= 2)
            {
                handCards.AddRange(sameNum.ToList());
                pair2++;
            }
        }

        if ((pair3 >= 1) && (pair2 >= 2))
            return HAND.FUll_HOUSE;
        else if (pair3 > 0)
            return HAND.THREE_KIND;
        else if (pair2 > 1)
            return HAND.TWO_PAIR;
        else if (pair2 > 0)
            return HAND.PAIR;

        handCards = cards;
        return HAND.HIGH_CARD;
    }
}
