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

    PlayerInfo myInfo;
    public string nickName;
    public int mypos;

    [SerializeField] public PokerGame pokergame;
    [SerializeField] NetworkManager networkManager;

    public bool isServer;

    //플레이어 추가
    public Dictionary<int, string> playerInfo = new Dictionary<int, string>();
    public void AddPlayer(int pos, string nickname)
    {
        playerInfo.Add(pos, nickname);
        pokergame.SetPlayerNickname(pos, nickname);
        if(nickname == nickName)
        {
            mypos = pos;
        }
    }

    //게임 시작
    public void GameStart_Server()
    {
        pokergame.SetCommunityCard_Server();
        pokergame.DistributeCard(playerInfo, playerInfo.Count);
        networkManager.SendGameStart_Server();
    }
    public void GameStart_Client()
    {
        pokergame.SetCommunityCard_Client();
    }

    //플레이어 카드 보내고 받기
    public void SendPlayerCard(int suit, int no)
    {
        string data = suit.ToString() + no.ToString();
        //networkManager.SendCardInfo(3, data, );
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
        pokergame.ShowMyCard(myInfo, mypos);
    }

    public void SendServerFold()
    {
        networkManager.SendFoldCheck_Client(0);
    }
    public void SendServerCheck()
    {
        networkManager.SendFoldCheck_Client(1);
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
