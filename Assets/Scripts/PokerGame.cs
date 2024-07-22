using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PokerGame : MonoBehaviour
{
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
                suit = Random.Range(0, 4);
                rank = Random.Range(0, 13);
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
        else
            return cardPrefabs.CloverCards[rank];
    }

    class PlayerCards
    {
        public int[] card1 = new int[2];
        public int[] card2 = new int[2];

        
    }

    public int playerNum;
    PlayerCards[] playerCards;

    int[,] ShareCards;

    [SerializeField]
    Transform[] cardPlaces;
    [SerializeField]
    GameObject[] playerPlace;

    [SerializeField]
    TextMeshProUGUI[] playerHand;
    void Start()
    {

        ShareCards = GetRandCard(PokerCardDeck, 5);
        for (int i = 0; i < ShareCards.GetLength(0); i++)
        {
            GameObject card = GetCardPrefab(ShareCards[i, 0], ShareCards[i, 1]);
            Instantiate(card, cardPlaces[i]);
            print(ShareCards[i, 0] + ", " + ShareCards[i, 1]);
        }

        playerCards = new PlayerCards[playerNum];
        for(int i = 0; i < playerNum; i++)
        {
            int[,] tempCard = GetRandCard(PokerCardDeck, 2);
            playerCards[i] = new PlayerCards();
            playerCards[i].card1[0] = tempCard[0, 0];
            playerCards[i].card1[1] = tempCard[0, 1]; 
            playerCards[i].card2[0] = tempCard[1, 0];
            playerCards[i].card2[1] = tempCard[1, 1];

            GameObject card1 = GetCardPrefab(playerCards[i].card1[0], playerCards[i].card1[1]);
            GameObject card2 = GetCardPrefab(playerCards[i].card2[0], playerCards[i].card2[1]);
            Instantiate(card1, playerPlace[i].transform.GetChild(0));
            Instantiate(card2, playerPlace[i].transform.GetChild(1));
        }
        HAND highestHand = HAND.HIGH_CARD;
        for(int i = 0; i < playerNum; i++)
        {
            int[] highestPlayerCard = new int[2];
            HAND playerHand = Search_Hand(playerCards[i], out highestPlayerCard);
            if (playerHand < highestHand)
                highestHand = playerHand;
            playerPlace[i].transform.GetChild(2).GetComponent<TextMeshPro>().text = playerHand.ToString();
        }
    }

    enum HAND { ROYAL_FLUSH, STRAIGHT_FLUSH, FOUR_KIND, FUll_HOUSE, 
        FLUSH, STRAIGHT, THREE_KIND, TWO_PAIR, PAIR, HIGH_CARD}
    HAND Search_Hand(PlayerCards playerC, out int[] highestHandCard)
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
        for(int i = 0; i < 4; i++)
        {
            if ((curCard[i, 12] != 0) && (curCard[i, 11] != 0) && (curCard[i, 10] != 0)
                    && (curCard[i, 9] != 0) && (curCard[i, 8] != 0) && (curCard[i, 7] != 0))
            {
                for (int j = 0; j < 5; j++)
                {
                    if (playerC.card1[1] > playerC.card2[1])
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
                    else
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

                }
                return HAND.ROYAL_FLUSH;
            }
            else
                continue;
        }

        //Straight Flush
        //Highest not done
        int count = 0;
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 13; j++)
            {
                if (curCard[i, j] == 1)
                {
                    try
                    {
                        if (curCard[i, j + 1] == 1)
                            count++;
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
                    else
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
            for (int j = 0; j < 4; j++)
            {
                if (playerC.card1[1] > playerC.card2[1])
                {
                    if (((playerC.card2[0] == same2Num) || (playerC.card2[0] == same3Num)) && (playerC.card2[1] == j))
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
                    }
                    else if (((playerC.card1[0] == same2Num) || (playerC.card1[0] == same3Num)) && (playerC.card1[1] == j))
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
                    }
                }
                else
                {
                    if (((playerC.card1[0] == same2Num) || (playerC.card1[0] == same3Num)) && (playerC.card1[1] == j))
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
                    }
                    else if (((playerC.card2[0] == same2Num) || (playerC.card2[0] == same3Num)) && (playerC.card2[1] == j))
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
                    }
                }
            }

            return HAND.FUll_HOUSE;

        }

        //Flush
        int endPoint = -1;
        int flushNum = 0;
        for(int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                if (curCard[i, j] == 1)
                {
                    flushNum++;
                    endPoint = j;
                }
            }
            flushNum = 0;
        }
        if (flushNum >= 4)
        {
            if (playerC.card1[0] > playerC.card2[0])
            {
                for(int i = endPoint; i > endPoint-5; i--)
                {
                    if (playerC.card2[1] == i)
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
                    }
                    else if (playerC.card1[1] == i)
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
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
                        straightNum++;
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
        if (straightNum >= 4)
            return HAND.STRAIGHT;

        //Three of a Kind
        for(int i = 0; i < 13; i++)
        {
            if (cardNums[i] == 3)
            {
                if (playerC.card1[0] > playerC.card2[0])
                {
                    if (playerC.card2[1] == i)
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
                    }
                    else if (playerC.card1[1] == i)
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
                    }
                }
                else
                {
                    if ((playerC.card1[1] == i))
                    {
                        highestHandCard[0] = playerC.card1[0];
                        highestHandCard[1] = playerC.card1[1];
                    }
                    else if ((playerC.card2[1] == i))
                    {
                        highestHandCard[0] = playerC.card2[0];
                        highestHandCard[1] = playerC.card2[1];
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
            if (playerC.card1[0] > playerC.card2[0])
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
            else
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
            if (playerC.card1[0] > playerC.card2[0])
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
            else
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
            return HAND.PAIR; 
        }

        if (playerC.card1[0] > playerC.card2[0])
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



}
