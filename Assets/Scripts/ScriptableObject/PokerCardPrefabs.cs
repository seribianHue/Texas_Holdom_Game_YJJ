using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PokerCardPrefabs", menuName = "Scriptable Object/PokerCard", order = int.MaxValue)]
public class PokerCardPrefabs : ScriptableObject
{
    [SerializeField]
    public GameObject[] SpadeCards = new GameObject[13];
    [SerializeField]
    public GameObject[] HeartCards = new GameObject[13];
    [SerializeField]
    public GameObject[] DiamodCards = new GameObject[13];
    [SerializeField]
    public GameObject[] CloverCards = new GameObject[13];

}
