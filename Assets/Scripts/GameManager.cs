using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;


public class NetworkRecievedClient : UnityEvent<byte[]> { }
public class NetworkRecievedServer : UnityEvent<byte[]> { }

public class NetworkConnectEvent : UnityEvent { }

public class ClientNetworkDisconnectEvent : UnityEvent { }
public class ServerNetworkDisconnectEvent : UnityEvent { }


public class GameManager : MonoBehaviour
{
    public static NetworkRecievedClient m_networkClientRecievedEvent;
    public static NetworkRecievedServer m_networkServerRecievedEvent;

    public static NetworkConnectEvent m_networkServerConnectEvent;

    public static ClientNetworkDisconnectEvent m_clientNetworkDisconnectEvent;
    public static ServerNetworkDisconnectEvent m_serverNetworkDisconnectEvent;

    private void Awake()
    {
        if (m_networkClientRecievedEvent == null)
            m_networkClientRecievedEvent = new NetworkRecievedClient();
        m_networkClientRecievedEvent.AddListener(ReadRecievedClientData_Json);

        if (m_networkServerRecievedEvent == null)
            m_networkServerRecievedEvent = new NetworkRecievedServer();
        m_networkServerRecievedEvent.AddListener(ReadRecievedServerData_Json);

        if(m_networkServerConnectEvent == null)
            m_networkServerConnectEvent = new NetworkConnectEvent();
        m_networkServerConnectEvent.AddListener(SendMyInfotoServer);

        if (m_clientNetworkDisconnectEvent == null)
            m_clientNetworkDisconnectEvent = new ClientNetworkDisconnectEvent();
        m_clientNetworkDisconnectEvent.AddListener(ClientNetworkDisconnected);

        if (m_serverNetworkDisconnectEvent == null)
            m_serverNetworkDisconnectEvent = new ServerNetworkDisconnectEvent();
        m_serverNetworkDisconnectEvent.AddListener(ServerNetworkDisconnected);

        mapPackets = new Dictionary<PacketID, PacketFunc>()
        {
            { PacketID.REQ_JOINGAME, Req_JoinGame },
            { PacketID.REQ_QUIT, Req_Quit},
            { PacketID.REQ_CHANGESTATE, Req_ChangeState},
            { PacketID.ACK_JOIN_PLAYER, Ack_JoinPlayer},
            { PacketID.ACK_QUIT_SOMEBODY, Ack_QuitSomeBody},
            { PacketID.ACK_GAME_START, Ack_GameStart},
            { PacketID.ACK_PERSONAL_CARD, Ack_PersonalCard},
            { PacketID.ACK_TABLE_CARD, Ack_TableCard},
            { PacketID.ACK_ANOTHER_CARD, Ack_AnotherCard},
            { PacketID.ACK_PLAYER_STATE_INFO, Ack_PlayerStateInfo},
            { PacketID.ACK_WINNER_INFO, Ack_WinnerInfo}
        };
    }

    [SerializeField] PokerGame pokergame;
    [SerializeField] NetworkManager networkManager;
    [SerializeField] UIManager uiManager;

    const int COMMUNITYNUM = 5;

    PlayerInfo myInfo;
    public string nickName;
    public int mypos;

    public bool isServer;

    #region Setting Nickname, IP, Port
    //닉네임 설정
    public void SetNickName(string nickName)
    {
        this.nickName = nickName;
    }

    //IP 설정
    string IPv4;
    public void SetIPv4(string ipv4)
    {
        this.IPv4 = ipv4;
    }

    string portNum;
    //포트 번호 설정
    public void SetPortNum(string portNum)
    {
        this.portNum = portNum;
    }
    #endregion

    #region Server, Client Create
    //서버 만들기
    public void CreateServer()
    {
        networkManager.CreateServer(IPv4, portNum);
        
        uiManager.SetLobbyUI(false);

        uiManager.SetFoldBTNInteractable(false);
        uiManager.SetCheckBTNInteractable(false);

        AddPlayer(0, nickName);
        isServer = true;    
    }
    //클라 만들기
    public void CreateClient()
    {
        networkManager.CreateClient(IPv4, portNum);

        uiManager.SetLobbyUI(false);
        uiManager.SetStartGameBTN(false);

        uiManager.SetFoldBTNInteractable(false);
        uiManager.SetCheckBTNInteractable(false);

        isServer = false;
    }
    #endregion

    #region Server, Client Network Disconnected
    public void ClientNetworkDisconnected()
    {
        P_REQ_QuitGame packetQuit = new P_REQ_QuitGame((byte)mypos);
        networkManager.SendDatatoServer(packetQuit);

        playerInfo.Clear();
        pokergame.SetPlayerNicknameAll(playerInfo);

        uiManager.SetLobbyUI(true);
    }

    public void ServerNetworkDisconnected()
    {
        playerInfo.Clear();
        pokergame.SetPlayerNicknameAll(playerInfo);

        uiManager.SetLobbyUI(true);
    }
    #endregion



    //플레이어 추가
    public List<string> playerInfo = new List<string>();
    //public Dictionary<int, string> playerInfo = new Dictionary<int, string>();
    public void AddPlayer(int pos, string nickname)
    {
        playerInfo.Insert(pos, nickname);
        pokergame.SetPlayerNickname(pos, nickname);
        if(nickname == nickName)
        {
            this.mypos = pos;
        }
    }

    public void AddPlayerServer(string nickname)
    {
        playerInfo.Add(nickname);
        pokergame.SetPlayerNickname(playerInfo.Count - 1, nickname);
    }

    //내 자신 위치 찾기
    public void SetMyPos()
    {
        for(int i = 0; i < playerInfo.Count; i++)
        {
            if (playerInfo[i] == this.nickName)
            {
                mypos = i;
            }
        }
    }

    //자리 정리 (nickname)
    public void SetGameNicknames()
    {
        pokergame.SetPlayerNicknameAll(playerInfo);
    }

    //라운드 관리
    List<int> playerState = new List<int>();
    Rounds curRound;
    void InitPlayerState()
    {
        foreach (var player in playerInfo)
        {
            playerState.Add(-1);
        }
        curRound = Rounds.SETTING;
    }

    #region Fold Check BTN

    public void Fold_BTN()
    {
        if (isServer)
        {
            playerState[0] = 0;
            uiManager.SetFoldBTNInteractable(false);
            uiManager.SetCheckBTNInteractable(false);
            //send packet
            P_ACK_PlayerState packet = new P_ACK_PlayerState((byte)mypos, 0, 0);
            networkManager.SendDatatoClientAll(packet);
        }
        else
        {
            P_REQ_ChangeState statePacket = new P_REQ_ChangeState((byte)mypos, (byte)0, 0);
            networkManager.SendDatatoServer(statePacket);

            uiManager.SetFoldBTNInteractable(false);
            uiManager.SetCheckBTNInteractable(false);
        }
        pokergame.FoldPlayer(mypos);
    }
    public void Check_BTN()
    {
        if (isServer)
        {
            playerState[0] = 1;
            uiManager.SetFoldBTNInteractable(false);
            uiManager.SetCheckBTNInteractable(false);
            //send packet
            P_ACK_PlayerState packet = new P_ACK_PlayerState((byte)mypos, 1, 0);
            networkManager.SendDatatoClientAll(packet);
        }
        else
        {
            P_REQ_ChangeState statePacket = new P_REQ_ChangeState((byte)mypos, (byte)1, 0);
            networkManager.SendDatatoServer(statePacket);

            uiManager.SetFoldBTNInteractable(false);
            uiManager.SetCheckBTNInteractable(false);
        }

    }

    #endregion

    public void QuitGame_BTN()
    {
        if (isServer)
        {
            networkManager.QuitServer();
            uiManager.SetLobbyUI(true);
        }
        else
        {
            ClientNetworkDisconnected();
            //networkManager.QuitClient();
        }
    }

    public delegate void PacketFunc(dynamic dynamicData);
    private Dictionary<PacketID, PacketFunc> mapPackets;

    #region Server Methods

    int sendCommCard = 0;

    //클라에서 받을 stream 조작
    void ReadRecievedClientData_Json(byte[] data)
    {
        string jsonString = Encoding.UTF8.GetString(data);
        dynamic dynamicData = JsonConvert.DeserializeObject(jsonString);

        if(mapPackets.ContainsKey((PacketID)dynamicData.id))
            mapPackets[(PacketID)dynamicData.id](dynamicData);

/*        switch ((PacketID)dynamicData.id)
        {
            case PacketID.REQ_JOINGAME: Req_JoinGame(dynamicData); break;
            case PacketID.REQ_QUIT: Req_Quit(dynamicData); break;
            case PacketID.REQ_CHANGESTATE: Req_ChangeState(dynamicData); break;
        }*/
    }

    void Req_JoinGame(dynamic dynamicData)
    {
        string nickname = dynamicData.nickName;
        //test
        print(nickname);

        AddPlayerServer(nickname);
        int playerPos = playerInfo.Count - 1;

        //모든 클라에게 새 플레이어 정보 보내기
        P_ACK_JoinPlayer packet = new P_ACK_JoinPlayer((byte)(playerPos), nickname);
        for (int i = 1; i < playerInfo.Count; i++)
        {
            if (i != playerPos)
            {
                networkManager.SendDatatoClient(packet, i);

            }
        }

        //새 멤버한테 기존 사람들 정보 알려주기
        for (int i = 0; i < playerInfo.Count; i++)
        {
            P_ACK_JoinPlayer packet1 = new P_ACK_JoinPlayer((byte)i, playerInfo[i]);
            networkManager.SendDatatoClient(packet1, playerPos);
        }
    }
    void Req_Quit(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        networkManager.DisconnectClient(pos);

        playerInfo.RemoveAt(pos);
        pokergame.SetPlayerNicknameAll(playerInfo);
        if (playerInfo.Count > 1)
        {
            P_ACK_QuitSomebody packet1 = new P_ACK_QuitSomebody((byte)pos);
            networkManager.SendDatatoClientAll(packet1);
        }
    }
    void Req_ChangeState(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.player_index);
        int state = Convert.ToInt32(dynamicData.state);
        int price = dynamicData.price;

        if (state == 0)
        {
            playerState[pos] = 0;
            pokergame.FoldPlayer(pos);
        }
        else
        {
            playerState[pos] = 1;
        }

        if (pos == playerState.Count - 1)
        {
            //start next round
            curRound += 1;
            if (curRound == Rounds.FIRST)
            {
                for (int i = 0; i < 3; i++)
                {
                    Card c = pokergame.SetCommCard_Server(i);

                    P_ACK_TableCard packet1 = new P_ACK_TableCard((byte)(Convert.ToInt32(c.suit)), (byte)c.no);
                    networkManager.SendDatatoClientAll(packet1);
                }
                uiManager.SetFoldBTNInteractable(true);
                uiManager.SetCheckBTNInteractable(true);
            }
            else if (curRound == Rounds.SECOND)
            {
                Card c = pokergame.SetCommCard_Server(3);

                P_ACK_TableCard packet1 = new P_ACK_TableCard((byte)(Convert.ToInt32(c.suit)), (byte)c.no);
                networkManager.SendDatatoClientAll(packet1);

                uiManager.SetFoldBTNInteractable(true);
                uiManager.SetCheckBTNInteractable(true);
            }
            else if (curRound == Rounds.THIRD)
            {
                Card c = pokergame.SetCommCard_Server(4);

                P_ACK_TableCard packet1 = new P_ACK_TableCard((byte)(Convert.ToInt32(c.suit)), (byte)c.no);
                networkManager.SendDatatoClientAll(packet1);

                uiManager.SetFoldBTNInteractable(true);
                uiManager.SetCheckBTNInteractable(true);


            }
            else if (curRound == Rounds.FINALL)
            {
                List<PlayerInfo> playerCardInfo;
                int winnerIndex = pokergame.FindWinner(out playerCardInfo);

                P_ACK_Winner packetWin = new P_ACK_Winner((byte)winnerIndex);
                networkManager.SendDatatoClientAll(packetWin);
                for (int i = 0; i < playerState.Count; i++)
                {
                    if (playerState[i] == 1)
                    {
                        P_ACK_AnotherCard packet1 = new P_ACK_AnotherCard(
                            (byte)i, (byte)((int)playerCardInfo[i].Card1.suit), (byte)playerCardInfo[i].Card1.no);
                        networkManager.SendDatatoClientAll(packet1);

                        P_ACK_AnotherCard packet2 = new P_ACK_AnotherCard(
                            (byte)i, (byte)((int)playerCardInfo[i].Card2.suit), (byte)playerCardInfo[i].Card2.no);
                        networkManager.SendDatatoClientAll(packet2);

                    }
                }
            }


        }

        P_ACK_PlayerState packet = new P_ACK_PlayerState((byte)pos, (byte)state, price);
        networkManager.SendDatatoClientAll(packet);
    }

    //게임 시작 버튼
    public void GameStartBTN()
    {
        pokergame.SetCommunityCard_Server();

        P_ACK_GameStart packet = new P_ACK_GameStart();
        networkManager.SendDatatoClientAll(packet);

        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (i == 0)
                pokergame.SetPlayerCard_Host(playerInfo[i], i);
            else
            {
                List<int> cardinfo = pokergame.SetPlayerCard_Guest(playerInfo[i], i);

                P_ACK_PersonalCard packet1 = new P_ACK_PersonalCard((byte)cardinfo[0], (byte)cardinfo[1]);
                P_ACK_PersonalCard packet2 = new P_ACK_PersonalCard((byte)cardinfo[2], (byte)cardinfo[3]);
                networkManager.SendDatatoClient(packet1, i);
                networkManager.SendDatatoClient(packet2, i);

            }
        }

        //Fold, Check 버튼 활성화
        uiManager.SetFoldBTNInteractable(true);
        uiManager.SetCheckBTNInteractable(true);

        InitPlayerState(); //플레이어 현재 상태 초기화

    }


    #endregion

    #region Client Methods

    //클라에서 서버에게 자신의 정보 보내기
    public void SendMyInfotoServer()
    {
        P_REQ_JoinGame joinPacket = new P_REQ_JoinGame(nickName);
        networkManager.SendDatatoServer(joinPacket);
    }

    //서버에서 받을 stream 조작
    int recieveMyCard = 0;
    Card card1;
    Card card2;

    int recieveCommCard = 0;

    int recievePlayerCard = 0;
    void ReadRecievedServerData_Json(byte[] data)
    {
        string jsonString = Encoding.UTF8.GetString(data);
        dynamic dynamicData = JsonConvert.DeserializeObject(jsonString);

        if (mapPackets.ContainsKey((PacketID)dynamicData.id))
            mapPackets[(PacketID)dynamicData.id](dynamicData);


/*        switch ((PacketID)dynamicData.id)
        {
            case PacketID.ACK_JOIN_PLAYER: Ack_JoinPlayer(dynamicData); break;
            case PacketID.ACK_QUIT_SOMEBODY: Ack_QuitSomeBody(dynamicData); break;
            case PacketID.ACK_GAME_START: Ack_GameStart(dynamicData); break;
            case PacketID.ACK_PERSONAL_CARD: Ack_PersonalCard(dynamicData); break;
            case PacketID.ACK_TABLE_CARD: Ack_TableCard(dynamicData); break;
            case PacketID.ACK_ANOTHER_CARD: Ack_AnotherCard(dynamicData); break;
            case PacketID.ACK_PLAYER_STATE_INFO: Ack_PlayerStateInfo(dynamicData); break;
            case PacketID.ACK_WINNER_INFO: Ack_WinnerInfo(dynamicData); break;
        }*/
    }

    void Ack_JoinPlayer(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        string nickname = dynamicData.nickName;

        AddPlayer(pos, nickname);

        //test
        print(nickname);
    }
    void Ack_QuitSomeBody(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);

        playerInfo.RemoveAt(pos);
        SetMyPos();
        pokergame.SetPlayerNicknameAll(playerInfo);
    }
    void Ack_GameStart(dynamic dynamicData)
    {
        InitPlayerState();
        pokergame.InitCommunityCard_Client();
    }
    void Ack_PersonalCard(dynamic dynamicData)
    {
        int cSuit = Convert.ToInt32(dynamicData.shape);
        int cNO = Convert.ToInt32(dynamicData.number);
        recieveMyCard++;

        if (recieveMyCard == 1)
        {
            card1 = new Card((Card.SUIT)cSuit, cNO, false);
        }
        else if (recieveMyCard == 2)
        {
            card2 = new Card((Card.SUIT)cSuit, cNO, false);

            for (int i = 0; i < playerInfo.Count; i++)
            {
                if (i == mypos)
                {
                    pokergame.SetPlayerCard_Self(playerInfo[i], card1, card2, i);
                }
                else
                {
                    pokergame.SetPlayerCard_Other(playerInfo[i], i);
                }
            }
        }
    }
    void Ack_TableCard(dynamic dynamicData)
    {
        int cSuit = Convert.ToInt32(dynamicData.shape);
        int cNO = Convert.ToInt32(dynamicData.number);

        Card c = new Card((Card.SUIT)cSuit, cNO, true);
        pokergame.SetCommCard_Client(recieveCommCard, c);
        recieveCommCard++;

        if (recieveCommCard == 5)
            pokergame.SetResult_Client(mypos);
    }
    void Ack_AnotherCard(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        int cSuit = Convert.ToInt32(dynamicData.shape);
        int cNO = Convert.ToInt32(dynamicData.number);

        Card card = new Card((Card.SUIT)cSuit, cNO, false);

        if (recievePlayerCard == 0)
        {
            pokergame.ShowPlayerCard_Other(pos, recievePlayerCard, card);

            recievePlayerCard++;
        }
        else
        {
            pokergame.ShowPlayerCard_Other(pos, recievePlayerCard, card);

            recievePlayerCard = 0;
        }
    }
    void Ack_PlayerStateInfo(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        int state = Convert.ToInt32(dynamicData.state);
        int price = dynamicData.price;

        //fold
        if (state == 0)
        {
            playerState[pos] = 0;
            pokergame.FoldPlayer(pos);
        }
        //check
        else
        {
            playerState[pos] = 1;
        }

        if (pos + 1 == mypos)
        {
            curRound++;
            if (curRound == Rounds.THIRD)
            {
                //pokergame.SetResult_Client(mypos);
            }

            if (playerState[mypos] == 0)
            {
                //이전에 fold를 했으면 다시 폴드 주고 넘어감
                P_REQ_ChangeState statePacket = new P_REQ_ChangeState((byte)mypos, 0, 0);
                networkManager.SendDatatoServer(statePacket);
            }
            else
            {
                //Fold, Check 버튼 활성화
                uiManager.SetFoldBTNInteractable(true);
                uiManager.SetCheckBTNInteractable(true);
            }
        }
    }
    void Ack_WinnerInfo(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        pokergame.ShowWinner(pos);
    }

    #endregion
}
