using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestCodes : MonoBehaviour
{
    List<Card> cards = new List<Card>();
    // Start is called before the first frame update
    void Start()
    {
        string cardString = "";
        for(int i = 0; i < 10; i++)
        {
            //Card c = new Card(Card.SUIT.SPADE, Random.Range(2, 15));
            //cards.Add(c);
        }

        cards = cards.OrderBy(c => c.no).ToList();

        foreach(var c in cards)
        {
            cardString += c.suit.ToString() + c.no + ", ";
        }

        print(cardString);
        cardString = "";
        var continous = cards.Zip(cards.Skip(1), (a, b) => a.no + 1 == b.no ? a : null);
        foreach(Card c in continous)
        {
            if(c != null)
                cardString += c.suit.ToString() + c.no + ", ";
        }
        print(cardString);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
