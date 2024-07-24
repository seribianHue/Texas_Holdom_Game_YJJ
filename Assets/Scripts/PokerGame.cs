using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class Card : IEquatable<Card>
{
    public enum SUIT { SPADE, HEART, DIAMOND, CLOVER };

    SUIT suit;
    public SUIT Suit
    {
        get { return suit; }
    }
    int no;
    public int NO 
    {
        get { return no; }
    }
        
    public Card(SUIT suit, int no)
    {
        this.suit = suit;
        this.no = no;
    }

    public bool Equals(Card other)
    {
        if (this is null) return false;
        if (other is null) return false;
        if(ReferenceEquals(this, other)) return true;
        return (this.suit == other.suit) && (this.no == other.no);
    }

}


public class PokerGame : MonoBehaviour
{
    const int MAXCARDNUM = 52;
    List<Card> Deck = new List<Card>();

    void InitDeck()
    {
        for(Card.SUIT suit = Card.SUIT.SPADE; suit <= Card.SUIT.CLOVER; suit++)
        {
            for(int i = 0; i < 13; i++)
            {
                Deck.Add(new Card(suit, i + 2));
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

    class Player
    {
        public int[] card1 = new int[2];
        public int[] card2 = new int[2];

        public HAND playerHand;

        public int[] highestCard = new int[2];
        
    }

    class PlayerInfo
    {
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

        public PlayerInfo(Card card1, Card card2)
        {
            this.card1 = card1;
            this.card2 = card2;
        }
    }

    [SerializeField]
    int playerNum;

    [SerializeField]
    Transform[] cardPlaces;
    [SerializeField]
    GameObject[] playerPlace;

    [SerializeField]
    TextMeshProUGUI[] playerHand;

    Card[] communityCards;
    PlayerInfo[] playersInfo;
    void Start()
    {
        InitDeck();
        communityCards = GetRandCards(5);

        foreach (var card in communityCards)
        {
            print(card.Suit.ToString() + ", " + card.NO);
        }

        playersInfo = new PlayerInfo[playerNum];
        for(int i = 0; i < playerNum; i++)
        {
            playersInfo[i] = new PlayerInfo(GetRandCards(1)[0], GetRandCards(1)[0]);
        }

        foreach (var player in playersInfo)
        {
            print(player.Card1.Suit.ToString() + ", " + player.Card1.NO);
            print(player.Card2.Suit.ToString() + ", " + player.Card2.NO);
        }

        Find_Hand3(playersInfo[0]);
        print(playersInfo[0].playerHand.ToString());
    }

    enum HAND { ROYAL_FLUSH, STRAIGHT_FLUSH, FOUR_KIND, FUll_HOUSE, 
        FLUSH, STRAIGHT, THREE_KIND, TWO_PAIR, PAIR, HIGH_CARD }

    void Find_Hand3(PlayerInfo player)
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < communityCards.Length; i++)
        {
            cards.Add(communityCards[i]);
        }
        cards.Add(player.Card1);
        cards.Add(player.Card2);

        HAND handFlush = Find_FlushGroup(cards);
        HAND handPair = HAND.HIGH_CARD;
        if(handFlush <= HAND.STRAIGHT_FLUSH)
        {
            player.playerHand = handFlush;
        }
        else if((handPair = Find_PairGroup(cards)) <= HAND.FUll_HOUSE)
        {
            player.playerHand = handPair;
        }
        else if(handFlush == HAND.FLUSH)
        {
            player.playerHand = handFlush;
        }
        else if(Find_Straight(cards) == HAND.STRAIGHT)
        {
            player.playerHand = HAND.STRAIGHT;
        }
        else
        {
            player.playerHand = handPair;
        }
    }

    HAND Find_FlushGroup(List<Card> cards)
    {
        cards.Sort((c1, c2) => c1.NO.CompareTo(c2.NO));
        var sameSuitGroup = cards.GroupBy(c => c.Suit).ToList();
        foreach(var sameSuit in sameSuitGroup)
        {
            if(sameSuit.Count() >= 5)
            {
                List<Card> sameSuitList = sameSuit.ToList();
                if ((sameSuitList[sameSuitList.Count - 5].NO == 10) && (sameSuitList[sameSuitList.Count - 1].NO == 14))
                {
                    return HAND.ROYAL_FLUSH;
                }
                else if (sameSuit.Zip(sameSuit.Skip(4), (a, b) => a.NO + 4 == b.NO ? a : null).Where(n => n != null).Count() >= 1)
                {
                    return HAND.STRAIGHT_FLUSH;
                }
                else
                {
                    return HAND.FLUSH;
                }
            }
        }
        return HAND.HIGH_CARD;
    }

    HAND Find_Straight(List<Card> cards)
    {
        cards.Sort((c1, c2) => c1.NO.CompareTo(c2.NO));
        //중복 숫자 제거
        List<Card> numsList = cards.GroupBy(n => n.NO).Select(m => m.First()).ToList();
        //연속 5개 숫자 찾기
        List<Card> continous = numsList.Zip(numsList.Skip(4), (a, b) => a.NO + 4 == b.NO ? a : null).Where(n => n != null).ToList();

        if (continous.Count >= 1)
            return HAND.STRAIGHT;
        return HAND.HIGH_CARD;
    }

    HAND Find_PairGroup(List<Card> cards)
    {
        int pair3 = 0;
        int pair2 = 0;
        var sameNumGroup = cards.GroupBy(x => x.NO).ToList();
        foreach (var sameNum in sameNumGroup)
        {
            if (sameNum.Count() >= 4)
                return HAND.FOUR_KIND;
            else if (sameNum.Count() >= 3)
                pair3++;
            else if (sameNum.Count() >= 2)
                pair2++;
        }

        if ((pair3 >= 1) && (pair2 >= 2))
            return HAND.FUll_HOUSE;
        else if (pair3 > 0)
            return HAND.THREE_KIND;
        else if (pair2 > 1)
            return HAND.TWO_PAIR;
        else if (pair2 > 0)
            return HAND.PAIR;

        return HAND.HIGH_CARD;
    }
}
