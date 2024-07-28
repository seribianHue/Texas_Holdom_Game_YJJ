using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance {  get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    const int COMMUNITYNUM = 5;

    public int playerNum = 1;

    PlayerInfo myInfo;

    [SerializeField] public PokerGame pokergame;
    [SerializeField] ServerBehaviour serverBehaviour;
    [SerializeField] NetworkManager networkManager;

    public void AddPlayer()
    {
        playerNum++;
    }

    List<string> playerNames = new List<string>();
    public void GameStart()
    {
        playerNames.Add(networkManager.nickName);
        foreach(var client in serverBehaviour.clientList)
        {
            if(client != null)
                playerNames.Add(client.nickName);
        }
        pokergame.DistributeCard(playerNames, playerNum);
        serverBehaviour.SendAck(3, "");
    }

    public void SetMyInfoServer(int index)
    {
        myInfo = pokergame.playersInfo[index];
    }
    
    public void SetMyInfoClient(string name, int suit1, int no1, int suit2, int no2)
    {
        Card c1 = new Card((Card.SUIT)suit1, no1, false);
        Card c2 = new Card((Card.SUIT)suit2, no2, false);
        myInfo = new PlayerInfo(name, c1, c2);
        pokergame.ShowMyCard(myInfo, networkManager.mypos);
    }

    public void SendServerFold()
    {
        networkManager.SendServerFoldCheck(0);
    }
    public void SendServerCheck()
    {
        networkManager.SendServerFoldCheck(1);
    }



    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
