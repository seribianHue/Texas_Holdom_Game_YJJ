using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Unity.Networking.Transport;


public class NetworkRecievedClient : UnityEvent<byte[]> { }
public class NetworkRecievedServer : UnityEvent<byte[]> { }

public class ClientNetworkConnectEvent : UnityEvent { }
public class ServerNetworkConnectEvent : UnityEvent<NetworkConnection> { }

public class ClientNetworkDisconnectEvent : UnityEvent { }
public class ServerNetworkDisconnectEvent : UnityEvent { }


public class GameManager : MonoBehaviour
{
    public static NetworkRecievedClient m_networkClientRecievedEvent;
    public static NetworkRecievedServer m_networkServerRecievedEvent;

    public static ClientNetworkConnectEvent m_networkClientConnectEvent;
    public static ServerNetworkConnectEvent m_networkServerConnectEvent;


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

        if(m_networkClientConnectEvent == null)
            m_networkClientConnectEvent = new ClientNetworkConnectEvent();
        m_networkClientConnectEvent.AddListener(SendMyInfotoServer);
        if(m_networkServerConnectEvent == null)
            m_networkServerConnectEvent = new ServerNetworkConnectEvent();
        m_networkServerConnectEvent.AddListener(RecieveConnectionInfo);

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
    const int MAXPLAYERNUM = 8;

    Player myInfo;
    public string nickName;
    public int mypos;
    

    public Player[] players = new Player[MAXPLAYERNUM];

    public bool isServer;

    #region Setting Nickname, myInfo, IP, Port
    //닉네임 설정
    public void SetNickName(string nickName)
    {
        this.nickName = nickName;
    }

    public void SetMyInfo(string nickname, string UUID)
    {
        myInfo = new Player(nickname, UUID);
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

    string GetMyUUID()
    {
        return SystemInfo.deviceUniqueIdentifier;
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

        myInfo = new Player(nickName, GetMyUUID());
        AddPlayer(0, nickName, myInfo.UUID);
        isServer = true;    
    }
    //클라 만들기
    public void CreateClient()
    {
        myInfo = new Player(nickName, GetMyUUID());
        isServer = false;

        networkManager.CreateClient(IPv4, portNum);

        uiManager.SetLobbyUI(false);
        uiManager.SetStartGameBTN(false);

        uiManager.SetFoldBTNInteractable(false);
        uiManager.SetCheckBTNInteractable(false);
    }
    #endregion

    #region Server, Client Network Disconnected
    public void ClientNetworkDisconnected()
    {
        P_REQ_QuitGame packetQuit = new P_REQ_QuitGame((byte)mypos);
        networkManager.SendDatatoServer(packetQuit);

        for(int i = 0; i < players.Length; i++)
        {
            players[i] = null;
        }
        pokergame.SetPlayerNicknameNull();

        uiManager.SetLobbyUI(true);
    }

    public void ServerNetworkDisconnected()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i] = null;
        }
        pokergame.SetPlayerNicknameNull();

        uiManager.SetLobbyUI(true);
    }
    #endregion



    //플레이어 추가
    public void AddPlayer(int pos, string nickname, string UUID)
    {
        players[pos] = new Player(nickname, UUID);
        pokergame.SetPlayerNickname(pos, nickname);
        if(myInfo.UUID == UUID)
        {
            myInfo = players[pos];
            mypos = pos;
        }
    }

    int FindEmptyPlayerPlace()
    {
        int pos = -1;
        for(int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
               return i;
        }
        return pos;
    }

    public void AddPlayerServer(int pos, string nickname, string UUID)
    {
        players[pos] = new Player(nickname, UUID);
        pokergame.SetPlayerNickname(pos, nickname);
    }

    //게임 시작시 현재 플레이어 순서 정리
    List<int> playOrderList = new List<int>();
    void PlayOrderSet(int startPos)
    {
        for(int i = startPos; i < players.Length; i++)
        {
            if(players[i] != null)
                playOrderList.Add(i);
        }
        for (int i = 0; i < startPos; i++)
        {
            if (players[i] != null)
                playOrderList.Add(i);
        }
    }

    //내 자신 위치 찾기
    /*    public void SetMyPos()
        {
            for(int i = 0; i < playerInfo.Count; i++)
            {
                if (playerInfo[i] == this.nickName)
                {
                    mypos = i;
                }
            }
        }*/

    //자리 정리 (nickname)
    /*    public void SetGameNicknames()
        {
            pokergame.SetPlayerNicknameAll(playerInfo);
        }*/

    //라운드 관리
    Rounds curRound;

/*    List<int> playerState = new List<int>();
    void InitPlayerState()
    {
        foreach (var player in playerInfo)
        {
            playerState.Add(-1);
        }
        curRound = Rounds.SETTING;
    }*/

    #region Fold Check BTN

    public void Fold_BTN()
    {
        if (isServer)
        {
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
        myInfo.isFold = true;
        //pokergame.FoldPlayer(mypos);
    }
    public void Check_BTN()
    {
        if (isServer)
        {
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
    }

    NetworkConnection tmpNet;
    public void RecieveConnectionInfo(NetworkConnection connection)
    {
        tmpNet = connection;
    }

    void Req_JoinGame(dynamic dynamicData)
    {
        string nickname = dynamicData.nickName;
        string UUID = dynamicData.UUID;
        //test
        print(nickname + UUID);

        int pos = FindEmptyPlayerPlace();
        AddPlayerServer(pos, nickname, UUID);
        players[pos].connection = tmpNet;
        //int playerPos = playerInfo.Count - 1;

        //모든 클라에게 새 플레이어 정보 보내기
        P_ACK_JoinPlayer packet = new P_ACK_JoinPlayer(Convert.ToByte(pos), nickname, UUID);
        for (int i = 1; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            if (i != pos)
            {
                networkManager.SendDatatoClient(packet, players[i].connection);
            }
        }

        //새 멤버한테 기존 사람들 정보 알려주기
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            P_ACK_JoinPlayer packet1 = new P_ACK_JoinPlayer((byte)i, players[i].nickname, players[i].UUID);
            networkManager.SendDatatoClient(packet1, players[pos].connection);
        }
    }
    void Req_Quit(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        networkManager.DisconnectClient(pos);

        players[pos] = null;
        pokergame.ErasePlayerNickanme(pos);
        if (players.Length > 1)
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
            players[pos].isFold = true;
        }

        if (pos == playOrderList[playOrderList.Count - 1])
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
                int winnerIndex = pokergame.FindWinner(players);

                P_ACK_Winner packetWinner = new P_ACK_Winner((byte)winnerIndex);
                networkManager.SendDatatoClientAll(packetWinner);
                for (int i = 0; i < players.Length; i++)
                {
                    if(players[i] == null) 
                        continue;
                    if (players[i].isFold == false)
                    {
                        P_ACK_AnotherCard packet1 = new P_ACK_AnotherCard(
                            (byte)i, (byte)((int)players[i].card1.suit), (byte)players[i].card1.no);
                        networkManager.SendDatatoClientAll(packet1);

                        P_ACK_AnotherCard packet2 = new P_ACK_AnotherCard(
                            (byte)i, (byte)((int)players[i].card2.suit), (byte)players[i].card2.no);
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
        PlayOrderSet(0);

        P_ACK_GameStart packet = new P_ACK_GameStart();
        networkManager.SendDatatoClientAll(packet);

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            if (i == 0)
                pokergame.SetPlayerCard_Host(players[i], i);
            else
            {
                List<int> cardinfo = pokergame.SetPlayerCard_Guest(players[i], i);

                P_ACK_PersonalCard packet1 = new P_ACK_PersonalCard((byte)cardinfo[0], (byte)cardinfo[1]);
                P_ACK_PersonalCard packet2 = new P_ACK_PersonalCard((byte)cardinfo[2], (byte)cardinfo[3]);
                networkManager.SendDatatoClient(packet1, players[i].connection);
                networkManager.SendDatatoClient(packet2, players[i].connection);

            }
        }

        //Fold, Check 버튼 활성화
        uiManager.SetFoldBTNInteractable(true);
        uiManager.SetCheckBTNInteractable(true);

        //InitPlayerState(); //플레이어 현재 상태 초기화

    }


    #endregion

    #region Client Methods

    //클라에서 서버에게 자신의 정보 보내기
    public void SendMyInfotoServer()
    {
        P_REQ_JoinGame joinPacket = new P_REQ_JoinGame(myInfo.nickname, myInfo.UUID);
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
    }

    void Ack_JoinPlayer(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        string nickname = dynamicData.nickName;
        string UUID = dynamicData.UUID;

        AddPlayer(pos, nickname, UUID);

        //test
        print(nickname);
    }
    void Ack_QuitSomeBody(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);

        players[pos] = null;
        pokergame.SetDisconnectedNicknameNull(pos);
    }
    void Ack_GameStart(dynamic dynamicData)
    {
        pokergame.InitCommunityCard_Client();
        PlayOrderSet(0);
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

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                    continue;

                if (i == mypos)
                {
                    players[i].card1 = card1;
                    players[i].card2 = card2;
                    pokergame.SetPlayerCard_Self(players[i], i);
                }
                else
                {
                    pokergame.SetPlayerCard_Other(players[i], i);
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
            pokergame.SetResult_Client(myInfo, mypos);
    }
    void Ack_AnotherCard(dynamic dynamicData)
    {
        int pos = Convert.ToInt32(dynamicData.index);
        int cSuit = Convert.ToInt32(dynamicData.shape);
        int cNO = Convert.ToInt32(dynamicData.number);

        Card card = new Card((Card.SUIT)cSuit, cNO, false);

        if (recievePlayerCard == 0)
        {
            pokergame.ShowPlayerCard_Other(players[pos], recievePlayerCard, card, pos);

            recievePlayerCard++;
        }
        else
        {
            pokergame.ShowPlayerCard_Other(players[pos], recievePlayerCard, card, pos);

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
            players[pos].isFold = true;
            //pokergame.FoldPlayer(pos);
        }

        if ((playOrderList.IndexOf(pos) < playOrderList.Count - 1) 
            && (playOrderList[playOrderList.IndexOf(pos) + 1] == mypos))
        {
            curRound++;
            if (curRound == Rounds.THIRD)
            {
                //pokergame.SetResult_Client(mypos);
            }

            if (players[mypos].isFold == true)
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
        pokergame.ShowWinner(players[pos]);
    }

    #endregion
}
