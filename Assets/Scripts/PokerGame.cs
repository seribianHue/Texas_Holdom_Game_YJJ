using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


class Card : IEquatable<Card>
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
    //Card[] Deck = new Card[MAXCARDNUM];

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

    int[,] PokerCardDeck = new int[4, 13];
    int[,] GetRandCard(int[,] pockerCardDeck, int numCard)
    {
        int[,] ranCards = new int[numCard, 2];

        for (int i = 0; i < numCard; i++)
        {
            bool isPossibleCard = false;
            int suit, rank = 0;
            do
            {
                suit = UnityEngine.Random.Range(0, 4);
                rank = UnityEngine.Random.Range(0, 13);
                if (pockerCardDeck[suit, rank] == 0)
                {
                    isPossibleCard = true;
                    pockerCardDeck[suit, rank] = 1;
                    ranCards[i, 0] = suit;
                    ranCards[i, 1] = rank;
                }
            } while (!isPossibleCard);
        }
        return ranCards;
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

    Player[] playerCards;

    int[,] ShareCards;

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
        communityCards = GetRandCards(10);

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

        print(playersInfo[0].playerHand.ToString());


        ShareCards = GetRandCard(PokerCardDeck, 5);
        for (int i = 0; i < ShareCards.GetLength(0); i++)
        {
            GameObject card = GetCardPrefab(ShareCards[i, 0], ShareCards[i, 1]);
            Instantiate(card, cardPlaces[i]);
            print(ShareCards[i, 0] + ", " + ShareCards[i, 1]);
        }

        playerCards = new Player[playerNum];
        for(int i = 0; i < playerNum; i++)
        {
            int[,] tempCard = GetRandCard(PokerCardDeck, 2);
            playerCards[i] = new Player();
            playerCards[i].card1[0] = tempCard[0, 0];
            playerCards[i].card1[1] = tempCard[0, 1]; 
            playerCards[i].card2[0] = tempCard[1, 0];
            playerCards[i].card2[1] = tempCard[1, 1];

            GameObject card1 = GetCardPrefab(playerCards[i].card1[0], playerCards[i].card1[1]);
            GameObject card2 = GetCardPrefab(playerCards[i].card2[0], playerCards[i].card2[1]);
            Instantiate(card1, playerPlace[i].transform.GetChild(0));
            Instantiate(card2, playerPlace[i].transform.GetChild(1));
        }
        for(int i = 0; i < playerNum; i++)
        {

            playerCards[i].playerHand = Search_Hand(playerCards[i], out playerCards[i].highestCard);
            playerPlace[i].transform.GetChild(3).GetComponent<TextMeshPro>().text = playerCards[i].playerHand.ToString();
            if (playerCards[i].highestCard[0] != -1)
                Instantiate(GetCardPrefab(playerCards[i].highestCard[0], playerCards[i].highestCard[1]), playerPlace[i].transform.GetChild(2));
            //Player[] highestPlayer = 
        }
    }

    enum HAND { ROYAL_FLUSH, STRAIGHT_FLUSH, FOUR_KIND, FUll_HOUSE, 
        FLUSH, STRAIGHT, THREE_KIND, TWO_PAIR, PAIR, HIGH_CARD}
    HAND Search_Hand(Player playerC, out int[] highestHandCard)
    {
        highestHandCard = new int[2] { -1, -1};

        int[,] curCard = new int[4, 13];
        curCard[playerC.card1[0], playerC.card1[1]]++;
        curCard[playerC.card2[0], playerC.card2[1]]++;

        int[] cardNums = new int[13];
        cardNums[playerC.card1[1]]++;
        cardNums[playerC.card2[1]]++;



        for (int i = 0; i < 5; i++)
        {
            curCard[ShareCards[i, 0], ShareCards[i, 1]] = 1;
            cardNums[ShareCards[i, 1]]++;
        }

        //Royal Flush
        int suit = -1;
        for (int i = 3; i >= 0; i--)
        {
            if ((curCard[i, 12] != 0) && (curCard[i, 11] != 0) && (curCard[i, 10] != 0)
                    && (curCard[i, 9] != 0) && (curCard[i, 8] != 0) && (curCard[i, 7] != 0))
            {
                for (int j = 0; j < 4; j++)
                {
                    if (playerC.card1[1] > playerC.card2[1])
                    {
                        if ((playerC.card1[0] == i) && (playerC.card1[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }
                        else if ((playerC.card2[0] == i) && (playerC.card2[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }
                    }
                    else
                    {
                        if ((playerC.card2[0] == i) && (playerC.card2[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }
                        else if ((playerC.card1[0] == i) && (playerC.card1[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }
                    }
                    break;

                }
                return HAND.ROYAL_FLUSH;
            }
            else
                continue;
        }

        //Straight Flush
        //Highest not done
        int count = 0;
        for (int i = 3; i >= 0; i--)
        {
            if (count >= 4)
                break;
            for (int j = 0; j < 13; j++)
            {
                if (curCard[i, j] == 1)
                {
                    try
                    {
                        if (curCard[i, j + 1] == 1)
                        {
                            count++;
                            if(count >= 4)
                            {
                                highestHandCard[0] = i;
                                highestHandCard[1] = j + 1;
                                break;
                            }
                        }
                        else
                            count = 0;
                    }
                    catch
                    {

                    }
                }
                else
                    count = 0;
            }
        }
        if (count >= 4)
        {
            return HAND.STRAIGHT_FLUSH;
        }

        //Four Of a Kind
        for (int i = 0; i < 13; i++)
        {
            if (cardNums[i] == 4)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (playerC.card1[1] > playerC.card2[1])
                    {
                        if ((playerC.card1[0] == i) && (playerC.card1[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }
                        else if ((playerC.card2[0] == i) && (playerC.card2[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }

                    }
                    else
                    {
                        if ((playerC.card2[0] == i) && (playerC.card2[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }
                        else if ((playerC.card1[0] == i) && (playerC.card1[1] == j))
                        {
                            highestHandCard[0] = i;
                            highestHandCard[1] = j;
                        }
                    }
                }
                return HAND.FOUR_KIND;
            }
        }


        //Full House
        bool same3 = false;
        int same3Num = -1;
        bool same2 = false;
        int same2Num = -1;
        for (int i = 0; i < 13; i++)
        {
            if (cardNums[i] == 3)
            {
                same3 = true;
                same3Num = i;
            }
            if (cardNums[i] == 2)
            {
                same2 = true;
                same2Num = i;
            }
        }
        if (same3 && same2)
        {
            if (playerC.card1[1] > playerC.card2[1])
            {
                if (((playerC.card1[1] == same2Num) || (playerC.card1[1] == same3Num)))
                {
                    highestHandCard[0] = playerC.card2[0];
                    highestHandCard[1] = playerC.card2[1];
                }
                else if (((playerC.card2[1] == same2Num) || (playerC.card2[1] == same3Num)))
                {
                    highestHandCard[0] = playerC.card1[0];
                    highestHandCard[1] = playerC.card1[1];
                }
            }
            else
            {
                if (((playerC.card2[1] == same2Num) || (playerC.card2[1] == same3Num)))
                {
                    highestHandCard[0] = playerC.card1[0];
                    highestHandCard[1] = playerC.card1[1];
                }
                else if (((playerC.card1[1] == same2Num) || (playerC.card1[1] == same3Num)))
                {
                    highestHandCard[0] = playerC.card2[0];
                    highestHandCard[1] = playerC.card2[1];
                }
            }

            return HAND.FUll_HOUSE;

        }

        //Flush
        int endPoint = -1;
        int flushNum = 0;
        for (int i = 3; i >= 0; i--)
        {
            for (int j = 0; j < 13; j++)
            {
                if (curCard[i, j] == 1)
                {
                    flushNum++;
                    if(flushNum >= 4)
                    {
                        endPoint = j;
                        break;
                    }

                }
                else
                    flushNum = 0;
            }
        }
        if (flushNum >= 4)
        {
            if (playerC.card1[1] > playerC.card2[1])
            {
                for(int i = endPoint; i > endPoint-5; i--)
                {
                    if (playerC.card1[1] == i)
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
                    }
                    else if (playerC.card2[1] == i)
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
                    }
                }
            }

            return HAND.FLUSH;

        }

        //Straight
        //Highest Card Not Done
        int straightNum = 0;
        for (int j = 0; j < 13; j++)
        {
            if (cardNums[j] > 0)
            {
                try
                {
                    if (cardNums[j + 1] > 0)
                    {
                        straightNum++;
                        if (straightNum >= 4)
                        {
                            if(playerC.card1[1] == j + 1)
                            {
                                highestHandCard[0] = playerC.card1[0];
                                highestHandCard[1] = playerC.card1[1];
                                return HAND.STRAIGHT;

                            }
                            else if (playerC.card2[1] == j + 1)
                            {
                                highestHandCard[0] = playerC.card2[0];
                                highestHandCard[1] = playerC.card2[0];
                                return HAND.STRAIGHT;
                            }


                        }
                    }
                    else
                        straightNum = 0;
                }
                catch
                {

                }
            }
            else
                straightNum = 0;
        }

        //Three of a Kind
        for(int i = 0; i < 13; i++)
        {
            if (cardNums[i] == 3)
            {
                if (playerC.card1[1] > playerC.card2[1])
                {
                    if (playerC.card1[1] == i)
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
                    }
                    else if (playerC.card2[1] == i)
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
                    }
                }
                else
                {
                    if ((playerC.card2[1] == i))
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
                    }
                    else if ((playerC.card1[1] == i))
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
                    }
                }
                return HAND.THREE_KIND;

            }
        }

        //Two Pair
        int[] pairNums = new int[13];
        int pair = 0;
        for (int i = 0; i < 13; i++)
        {
            if (cardNums[i] == 2)
            {
                pair++;
                pairNums[i]++;
            }
        }
        if (pair >= 2)
        {
            int[] pairsNum = new int[2];
            for(int i = 0; i < 13; i++)
            {
                int curIndex = 0;
                if(pairNums[i] > 0)
                {
                    pairsNum[curIndex] = i;
                    curIndex++;
                }
            }
            if (playerC.card1[1] > playerC.card2[1])
            {
                if ((playerC.card1[1] == pairsNum[0]) || (playerC.card1[1] == pairsNum[1]))
                {
                    highestHandCard[0] = playerC.card1[0];
                    highestHandCard[1] = playerC.card1[1];
                }
                else if ((playerC.card2[1] == pairsNum[0]) || (playerC.card2[1] == pairsNum[1]))
                {
                    highestHandCard[0] = playerC.card2[0];
                    highestHandCard[1] = playerC.card2[1];
                }
            }
            else
            {
                if ((playerC.card2[1] == pairsNum[0]) || (playerC.card2[1] == pairsNum[1]))
                {
                    highestHandCard[0] = playerC.card2[0];
                    highestHandCard[1] = playerC.card2[1];
                }
                else if ((playerC.card1[1] == pairsNum[0]) || (playerC.card1[1] == pairsNum[1]))
                {
                    highestHandCard[0] = playerC.card1[0];
                    highestHandCard[1] = playerC.card1[1];
                }
            }
            return HAND.TWO_PAIR;
        }

        if (pair == 1) 
        {
            int pairNum = -1;
            for (int i = 0; i < 13; i++)
            {
                if (pairNums[i] > 0)
                {
                    pairNum = i;
                    break;
                }
            }
            if (playerC.card1[1] > playerC.card2[1])
            {
                if (playerC.card1[1] == pairNum)
                {
                    highestHandCard[0] = playerC.card1[0];
                    highestHandCard[1] = playerC.card1[1];
                }
                else if (playerC.card2[1] == pairNum)
                {
                    highestHandCard[0] = playerC.card2[0];
                    highestHandCard[1] = playerC.card2[1];
                }
            }
            else
            {
                if (playerC.card2[1] == pairNum)
                {
                    highestHandCard[0] = playerC.card2[0];
                    highestHandCard[1] = playerC.card2[1];
                }
                else if (playerC.card1[1] == pairNum)
                {
                    highestHandCard[0] = playerC.card1[0];
                    highestHandCard[1] = playerC.card1[1];
                }
            }
            return HAND.PAIR; 
        }

        if (playerC.card1[1] > playerC.card2[1])
        {
            highestHandCard[0] = playerC.card1[0];
            highestHandCard[1] = playerC.card1[1];
        }
        else
        {
            highestHandCard[0] = playerC.card2[0];
            highestHandCard[1] = playerC.card2[1];
        }
        return HAND.HIGH_CARD;

    }

    void Find_Hand(PlayerInfo player)
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < communityCards.Length; i++)
        {
            cards.Add(communityCards[i]);
        }
        cards.Add(player.Card1);
        cards.Add(player.Card2);

        if (isRoyalFlush(cards))
            player.playerHand = HAND.ROYAL_FLUSH;
        else if (isStraightFlush(cards))
            player.playerHand = HAND.STRAIGHT_FLUSH;
        else if (isFourKind(cards))
            player.playerHand = HAND.FOUR_KIND;
        else if (isFullHouse(cards))
            player.playerHand = HAND.FUll_HOUSE;
        else if (isFlush(cards))
            player.playerHand = HAND.FLUSH;
        else if (isStraight(cards))
            player.playerHand = HAND.STRAIGHT;
        else if (isThreeeKind(cards))
            player.playerHand = HAND.THREE_KIND;
        else if (isTwoPair(cards))
            player.playerHand = HAND.TWO_PAIR;
        else if (isPair(cards))
            player.playerHand = HAND.PAIR;
        else
            player.playerHand = HAND.HIGH_CARD;
    }

    bool isRoyalFlush(List<Card> cards)
    {
        cards.Sort((c1, c2) => c1.NO.CompareTo(c2.NO));

                  
        for (Card.SUIT suit = Card.SUIT.SPADE; suit <= Card.SUIT.CLOVER; suit++)
        {
            List<Card> sameSuit = cards.FindAll(c => c.Suit == suit);

            if ((sameSuit.Count >= 5) && (sameSuit[sameSuit.Count - 5].NO == 10) && (sameSuit[sameSuit.Count - 1].NO == 14))
            {
                return true;
            }

            /*
            for (int i = 0; i < sameSuit.Count - 4; i++)
            {
                if ((sameSuit[i].NO == 10) && (sameSuit[i + 4].NO == 14))
                {
                    return true;
                }
            }
            */
        }
        return false;
    }

    bool isStraightFlush(List<Card> cards)
    {
        cards.Sort((c1, c2) => c1.NO.CompareTo(c2.NO));

        for (Card.SUIT suit = Card.SUIT.SPADE; suit <= Card.SUIT.CLOVER; suit++)
        {
            List<Card> sameSuit = cards.FindAll(c => c.Suit == suit);
            for (int i = 0; i < sameSuit.Count - 4; i++)
            {
                if (sameSuit[i + 4].NO == sameSuit[i].NO + 4)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool isFourKind(List<Card> cards)
    {
        for (int i = 0; i < 13; i++)
        {
            List<Card> sameNum = cards.FindAll(c => c.NO == i + 2);
            if(sameNum.Count >= 4) { return true; }
        }
        return false;
    }

    bool isFullHouse(List<Card> cards)
    {
        bool is3 = false;
        bool is2 = false;
        for (int i = 0; i < 13; i++)
        {
            List<Card> sameNum = cards.FindAll(c => c.NO == i + 2);
            if (sameNum.Count >= 3) { is3 = true; }
            else if (sameNum.Count == 2) { is2 = true; }
        }
        if(is3 && is2) {  return true; }
        else {  return false; }
    }

    bool isFlush(List<Card> cards)
    {
        for (Card.SUIT suit = Card.SUIT.SPADE; suit <= Card.SUIT.CLOVER; suit++)
        {
            List<Card> sameSuit = cards.FindAll(c => c.Suit == suit);
            if(sameSuit.Count >= 5) { return true; }
        }
        return false;
    }

    bool isStraight(List<Card> cards)
    {
        int[] cardsNum = new int[13];
        for (int i = 0; i < cards.Count; i++)
            cardsNum[cards[i].NO - 2]++;

        for (int i = 0; i < cardsNum.Length - 4; i++)
        {
            if ((cardsNum[i] > 0) && (cardsNum[i + 1] > 0) && (cardsNum[i + 2] > 0)
                && (cardsNum[i + 3] > 0) && (cardsNum[i + 4] > 0))
            {
                return true;
            }
        }
        return false;
    }

    bool isThreeeKind(List<Card> cards)
    {
        for (int i = 0; i < 13; i++)
        {
            List<Card> sameNum = cards.FindAll(c => c.NO == i + 2);
            if (sameNum.Count >= 3) { return true; }
        }
        return false;
    }

    bool isTwoPair(List<Card> cards)
    {
        int pair = 0;
        for (int i = 0; i < 13; i++)
        {
            List<Card> sameNum = cards.FindAll(c => c.NO == i + 2);
            if (sameNum.Count >= 2) { pair++; }
        }
        if(pair >= 2) { return true; }
        else { return false; }
    }

    bool isPair(List<Card> cards)
    {
        for (int i = 0; i < 13; i++)
        {
            List<Card> sameNum = cards.FindAll(c => c.NO == i + 2);
            if (sameNum.Count >= 2) { return true; }
        }
        return false;
    }

    void FindHighestCard(List<Card> cards, PlayerInfo player)
    {
        Card involvedCard1 = null;
        Card involvedCard2 = null;
        foreach (Card card in cards)
        {
            if (card == player.Card1)
            {
                involvedCard1 = card;
            }
            if (card == player.Card2)
            {
                involvedCard2 = card;
            }
        }

        if (player.Card1.NO > player.Card2.NO)
        {
            player.highestCard = involvedCard1 is null ? involvedCard1 : involvedCard2;
        }
        else
        {
            player.highestCard = involvedCard2 is null ? involvedCard2 : involvedCard1;
        }
    }
}
