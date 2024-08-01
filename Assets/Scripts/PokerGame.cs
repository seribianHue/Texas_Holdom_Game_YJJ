using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using TMPro;
using UnityEngine;
using static Card;


public class Card : IEquatable<Card>
{
    public enum SUIT { SPADE, HEART, DIAMOND, CLOVER };

    public SUIT suit { get; private set; }

    public int no { get; private set; }

    public bool isCommunity { get; set; }

    public GameObject cardObj;
        
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
        set { card1 = value; }
    }
    Card card2;
    public Card Card2
    {
        get { return card2; }
        set { card2 = value; }
    }
    public HAND playerHand;

    public Card highestCard;

    public bool isFold;

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

    //플레이어들 닉네임 화면에 설정
    public void SetPlayerNickname(int pos, string nickname)
    {
        playerPlace[pos].transform.GetChild(4).GetComponent<TextMeshPro>().text = nickname;
    }

    //커뮤니티 카드 설정 _ 서버(정보 유)
    public void SetCommunityCard_Server()
    {
        communityCards = GetRandCards(COMMUNITYNUM);
        for (int i = 0; i < communityCards.Length; i++)
        {
            GameObject cObj = GetCardPrefab((int)communityCards[i].suit, communityCards[i].no - 2);
            var c = Instantiate(cObj, cardPlaces[i].position,
                Quaternion.Euler(cObj.transform.rotation.eulerAngles + new Vector3(90, 0, 0)));
            communityCards[i].cardObj = c;
        }
    }

    //커뮤니티 카드 보이기 _ 서버
    public Card SetCommCard_Server(int index)
    {
        Destroy(communityCards[index].cardObj);
        GameObject cObj = GetCardPrefab((int)communityCards[index].suit, communityCards[index].no - 2);
        var c = Instantiate(cObj, cardPlaces[index]);
        communityCards[index].cardObj = c;
        return communityCards[index];
    }

    //커뮤니티 카드 설정 _ 클라(정보 무)
    public void InitCommunityCard_Client()
    {
        for (int i = 0; i < communityCards.Length; i++)
        {
            communityCards[i] = new Card((Card.SUIT)0, 14, true);
            GameObject cObj = GetCardPrefab(0, 12);
            var c = Instantiate(cObj, cardPlaces[i].position,
                Quaternion.Euler(cObj.transform.rotation.eulerAngles + new Vector3(90, 0, 0)));
            communityCards[i].cardObj = c;
        }
    }

    //커뮤니티 카드 보이기 _ 클라
    public void SetCommCard_Client(int index, Card card)
    {
        Destroy(communityCards[index].cardObj);
        communityCards[index] = card;
        GameObject cObj = GetCardPrefab((int)card.suit, card.no - 2);
        var c = Instantiate(cObj, cardPlaces[index]);
        communityCards[index].cardObj = c;
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

    //각 플레이어 카드 설정 _ 서버(모든 정보 유)
    public void SetPlayerCard_Host(string nickname, int pos)
    {
        Card c1 = GetRandCards(1)[0];
        Card c2 = GetRandCards(1)[0];

        playersInfo.Add(new PlayerInfo(nickname, c1, c2));
        playersInfo[pos].Card1.isCommunity = false;
        playersInfo[pos].Card2.isCommunity = false;
        Find_Hand3(playersInfo[pos]);

        ShowCardFront(playersInfo[pos], pos);
    }
    //서버장 이외의 카드 설정
    public List<int> SetPlayerCard_Guest(string nickname, int pos)
    {
        Card c1 = GetRandCards(1)[0];
        Card c2 = GetRandCards(1)[0];

        playersInfo.Add(new PlayerInfo(nickname, c1, c2));
        playersInfo[pos].Card1.isCommunity = false;
        playersInfo[pos].Card2.isCommunity = false;
        Find_Hand3(playersInfo[pos]);

        ShowCardFront(playersInfo[pos], pos);
        //ShowCardBack(playersInfo[pos], pos);

        List<int> cardInfo = new List<int>()
                    { ((int)playersInfo[pos].Card1.suit), playersInfo[pos].Card1.no,
                    (int) playersInfo[pos].Card2.suit, playersInfo[pos].Card2.no};

        return cardInfo;
    }

    //폴드한 플레이어 설정
    public void FoldPlayer(int pos)
    {
        playersInfo[pos].isFold = true;
    }

    //모든 플레이어 카드 계산
    public void SetResultAll()
    {
        for(int i = 0; i < playersInfo.Count; i++)
        {
            Find_Hand3(playersInfo[i]);
            playerPlace[i].transform.GetChild(3).GetComponent<TextMeshPro>().text
                = playersInfo[i].playerHand.ToString();
            Card highCard = playersInfo[i].highestCard;
            GameObject cObj = GetCardPrefab((int)highCard.suit, highCard.no - 2);
            var c = Instantiate(cObj, playerPlace[i].transform.GetChild(2));
        }
    }

    //승자 계산
    public int FindWinner(out List<PlayerInfo> playerCardInfo)
    {
        PlayerInfo[] winners = playersInfo.Where(n => n.playerHand == playersInfo.Min(x => x.playerHand))
            .ToArray();
        /*        if (winners.Length > 1)
                {
                    winners = winners.OrderBy(c => c.highestCard.suit).OrderByDescending(c => c.highestCard.no).ToArray();
                    for(int i = 0; i < winners.Length; i++)
                    {
                        if (!winners[i].isFold)
                        {
                            winner = winners[i];
                            break;
                        }
                    }

                }
                else { winner = winners[0]; }*/

        winners = winners.OrderBy(c => c.highestCard.suit).OrderByDescending(c => c.highestCard.no)
            .ToArray();
        for (int i = 0; i < winners.Length; i++)
        {
            if (!winners[i].isFold)
            {
                winner = winners[i];
                break;
            }
        }
        winnerText.text = "Winner : " + winner.Player;
        playerCardInfo = playersInfo;
        return playersInfo.FindIndex(n => n.Player == winner.Player);
    }

    //승자 보여주기 _ 클라
    public void ShowWinner(int pos)
    {
        winner = playersInfo[pos];
        winnerText.text = "Winner : " + winner.Player;
    }

    //카드 보여주기 _ 앞, 뒤
    void ShowCardFront(PlayerInfo playerinfo, int pos)
    {
        GameObject cObj1 = GetCardPrefab((int)playerinfo.Card1.suit, playerinfo.Card1.no - 2);
        GameObject cObj2 = GetCardPrefab((int)playerinfo.Card2.suit, playerinfo.Card2.no - 2);
        Instantiate(cObj1, playerPlace[pos].transform.GetChild(0).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(0).rotation.eulerAngles));
        Instantiate(cObj2, playerPlace[pos].transform.GetChild(1).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(1).rotation.eulerAngles));
    }
    void ShowCardBack(PlayerInfo playerinfo, int pos)
    {
        GameObject cObj1 = GetCardPrefab((int)playerinfo.Card1.suit, playerinfo.Card1.no - 2);
        GameObject cObj2 = GetCardPrefab((int)playerinfo.Card2.suit, playerinfo.Card2.no - 2);
        Instantiate(cObj1, playerPlace[pos].transform.GetChild(0).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(0).rotation.eulerAngles + new Vector3(180, 0, 0)));
        Instantiate(cObj2, playerPlace[pos].transform.GetChild(1).position,
            Quaternion.Euler(playerPlace[pos].transform.GetChild(1).rotation.eulerAngles + new Vector3(180, 0, 0)));
    }


    //각 플레이어 카드 설정 _ 클라(자신 정보만 유)
    public void SetPlayerCard_Self(string nickname, Card c1, Card c2, int pos)
    {
        playersInfo.Add(new PlayerInfo(nickname, c1, c2));

        ShowCardFront(playersInfo[pos], pos);
    }

    //클라 자신 패 계산, 높은 카드 보이기
    public void SetResult_Client(int mypos)
    {
        Find_Hand3(playersInfo[mypos]);
        playerPlace[mypos].transform.GetChild(3).GetComponent<TextMeshPro>().text
            = playersInfo[mypos].playerHand.ToString();
        Card highCard = playersInfo[mypos].highestCard;
        GameObject cObj = GetCardPrefab((int)highCard.suit, highCard.no - 2);
        var c = Instantiate(cObj, playerPlace[mypos].transform.GetChild(2));
    }

    //다른 사람 정보 모르게 각 플레이어 카드 설정
    public void SetPlayerCard_Other(string nickname, int pos)
    {
        playersInfo.Add(new PlayerInfo(nickname, new Card((Card.SUIT)0, 14, true),
            new Card((Card.SUIT)0, 14, true)));

        ShowCardBack(playersInfo[pos], pos);
    }

    //다른 플레이어 카드 설정 _ 클라
    public void ShowPlayerCard_Other(int pos, int cardnum, Card card)
    {
        if(cardnum == 0)
        {
            playersInfo[pos].Card1 = card;
            Destroy(playersInfo[pos].Card1.cardObj);
            GameObject cObj = GetCardPrefab((int)playersInfo[pos].Card1.suit, playersInfo[pos].Card1.no - 2);
            var c = Instantiate(cObj, playerPlace[pos].transform.GetChild(0));
            playersInfo[pos].Card1.cardObj = c;
        }
        else
        {
            playersInfo[pos].Card2 = card;
            Destroy(playersInfo[pos].Card2.cardObj);
            GameObject cObj = GetCardPrefab((int)playersInfo[pos].Card2.suit, playersInfo[pos].Card2.no - 2);
            var c = Instantiate(cObj, playerPlace[pos].transform.GetChild(1));
            playersInfo[pos].Card2.cardObj = c;
        }
    }


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
        //중복 숫자 제거
        List<Card> numsList = cards.GroupBy(n => n.no).Select(m => m.First()).ToList();
        //연속 5개 숫자 찾기
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
