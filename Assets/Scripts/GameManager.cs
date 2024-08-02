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

        AddPlayer(nickName);
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
        networkManager.SendDatatoServer(writeClientData_Json(1, mypos.ToString()));
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
    public void AddPlayer(string nickname)
    {
        playerInfo.Add(nickname);
        int pos = playerInfo.FindIndex(n => n.Equals(nickname));
        pokergame.SetPlayerNickname(pos, nickname);
        if(nickname == nickName)
        {
            mypos = pos;
        }
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
            
            string info = mypos.ToString() + 0.ToString() + 0.ToString();
            networkManager.SendDatatoClientAll(writeServerData_Json(6, info));
        }
        else
        {
            string info = mypos.ToString() + 0.ToString() + 0.ToString();
            networkManager.SendDatatoServer(writeClientData_Json(2, info));

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

            string info = mypos.ToString() + 1.ToString() + 0.ToString();
            networkManager.SendDatatoClientAll(writeServerData_Json(6, info));
        }
        else
        {
            string info = mypos.ToString() + 1.ToString() + 0.ToString();
            networkManager.SendDatatoServer(writeClientData_Json(2, info));

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


    #region Server Methods

    int sendCommCard = 0;
    //서버에서 보낼 stream 제작 with Json
    byte[] writeServerData_Json(int id, string data)
    {
        byte[] byteData = new byte[0];
        switch ((ServerToClient)id)
        {
            case ServerToClient.ACK_JOIN_PLAYER:
                {
                    P_ACK_JoinPlayer packet =
                        new P_ACK_JoinPlayer((byte)id,
                        (byte)int.Parse(data.Substring(0, 1)),  //pos
                        data.Substring(1));                     //nickname

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ServerToClient.ACK_QUIT_SOMEBODY:
                {
                    P_ACK_QuitSomebody packet =
                        new P_ACK_QuitSomebody((byte)id, 
                        (byte)int.Parse(data.Substring(0, 1))); //pos

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ServerToClient.ACK_GAME_START:
                {
                    P_ACK_GameStart packet =
                        new P_ACK_GameStart((byte)id);

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ServerToClient.ACK_PERSONAL_CARD:
                {
                    P_ACK_PersonalCard packet = 
                        new P_ACK_PersonalCard((byte)id,        //id
                        (byte)int.Parse(data.Substring(0, 1)),  //suit
                        (byte)int.Parse(data.Substring(1)));    //no

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ServerToClient.ACK_TABLE_CARD:
                {
                    P_ACK_TableCard packet =
                        new P_ACK_TableCard((byte)id,           //id
                        (byte)int.Parse(data.Substring(0, 1)),  //suit
                        (byte)int.Parse(data.Substring(1)));    //no

                    //test
                    sendCommCard++;
                    if (sendCommCard == 5)
                    {
                        pokergame.SetResultAll();
                    }

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ServerToClient.ACK_ANOTHER_CARD:
                {
                    P_ACK_AnotherCard packet =
                        new P_ACK_AnotherCard((byte)id,
                        (byte)int.Parse(data.Substring(0, 1)),  //player index
                        (byte)int.Parse(data.Substring(1, 1)),  //suit
                        (byte)int.Parse(data.Substring(2)));    //no

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ServerToClient.ACK_PLAYER_STATE_INFO:
                {
                    P_ACK_PlayerState packet =
                        new P_ACK_PlayerState((byte)id,
                        (byte)int.Parse(data.Substring(0, 1)),  //pos
                        (byte)int.Parse(data.Substring(1, 1)),  //fold or check
                        int.Parse(data.Substring(2)));          //price

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ServerToClient.ACK_WINNER_INFO:
                {
                    P_ACK_Winner packet =
                        new P_ACK_Winner((byte)id,
                        (byte)int.Parse(data.Substring(0, 1))); //winner pos

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
        }
        return byteData;
    }

    //클라에서 받을 stream 조작
    void ReadRecievedClientData_Json(byte[] data)
    {
        string jsonString = Encoding.UTF8.GetString(data);
        dynamic dynamicData = JsonConvert.DeserializeObject(jsonString);
        //dynamic dynamicData = JsonSerializer.CreateJsonReader().FromJson<dynamic>(jsonString);

        switch ((ClientToServer)dynamicData.id)
        {
            case ClientToServer.REQ_JOINGAME:
                {
                    string nickname = dynamicData.nickName;
                    //test
                    print(nickname);

                    AddPlayer(nickname);
                    pokergame.SetPlayerNickname(playerInfo.Count - 1, nickname);

                    //모든 클라에게 새 플레이어 정보 보내기
                    string newPlayerData = (playerInfo.Count - 1).ToString() + nickname;
                    for (int i = 1; i < playerInfo.Count; i++)
                    {
                        if (i != playerInfo.Count - 1)
                        {
                            networkManager.SendDatatoClient(writeServerData_Json(0, newPlayerData), i);

                        }
                    }

                    //새 멤버한테 기존 사람들 정보 알려주기
                    for (int i = 0; i < playerInfo.Count; i++)
                    {
                        string playerData = i.ToString() + playerInfo[i];
                        networkManager.SendDatatoClient
                            (writeServerData_Json(0, playerData), playerInfo.Count - 1);
                    }

                    break;
                }
            case ClientToServer.REQ_QUIT:
                {
                    int pos = Convert.ToInt32(dynamicData.index);
                    networkManager.DisconnectClient(pos);

                    playerInfo.RemoveAt(pos);
                    pokergame.SetPlayerNicknameAll(playerInfo);
                    if (playerInfo.Count > 1)
                        networkManager.SendDatatoClientAll(writeServerData_Json(1, pos.ToString()));
                    break;
                }
            case ClientToServer.REQ_CHANGESTATE:
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
                                string info = ((int)c.suit).ToString() + c.no.ToString();
                                networkManager.SendDatatoClientAll(writeServerData_Json(4, info));
                            }
                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);
                        }
                        else if (curRound == Rounds.SECOND)
                        {
                            Card c = pokergame.SetCommCard_Server(3);
                            string info = ((int)c.suit).ToString() + c.no.ToString();
                            networkManager.SendDatatoClientAll(writeServerData_Json(4, info));

                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);
                        }
                        else if (curRound == Rounds.THIRD)
                        {
                            Card c = pokergame.SetCommCard_Server(4);
                            string info = ((int)c.suit).ToString() + c.no.ToString();
                            networkManager.SendDatatoClientAll(writeServerData_Json(4, info));

                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);


                        }
                        else if (curRound == Rounds.FINALL)
                        {
                            List<PlayerInfo> playerCardInfo;
                            int winnerIndex = pokergame.FindWinner(out playerCardInfo);
                            networkManager.SendDatatoClientAll(writeServerData_Json(7, winnerIndex.ToString()));
                            for (int i = 0; i < playerState.Count; i++)
                            {
                                if (playerState[i] == 1)
                                {
                                    string card1 = i.ToString() + ((int)playerCardInfo[i].Card1.suit).ToString()
                                        + playerCardInfo[i].Card1.no.ToString();
                                    networkManager.SendDatatoClientAll(writeServerData_Json(5, card1));

                                    string card2 = i.ToString() + ((int)playerCardInfo[i].Card2.suit).ToString()
                                        + playerCardInfo[i].Card2.no.ToString();
                                    networkManager.SendDatatoClientAll(writeServerData_Json(5, card2));

                                }
                            }
                        }


                    }

                    string stateData = pos.ToString() + state.ToString() + price.ToString();

                    networkManager.SendDatatoClientAll(writeServerData_Json(6, stateData));
                    break;

                }
        }
    }

    //게임 시작 버튼
    public void GameStartBTN()
    {
        pokergame.SetCommunityCard_Server();
        
        networkManager.SendDatatoClientAll(writeServerData_Json(2, ""));

        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (i == 0)
                pokergame.SetPlayerCard_Host(playerInfo[i], i);
            else
            {
                List<int> cardinfo = pokergame.SetPlayerCard_Guest(playerInfo[i], i);
                string card1 = cardinfo[0].ToString() + cardinfo[1].ToString();
                string card2 = cardinfo[2].ToString() + cardinfo[3].ToString();

                networkManager.SendDatatoClient(writeServerData_Json(3, card1), i);
                networkManager.SendDatatoClient(writeServerData_Json(3, card2), i);

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
        //networkManager.SendDatatoServer(writeClientData(0, nickName));
        networkManager.SendDatatoServer(writeClientData_Json(0, nickName));
    }

    //클라에서 보낼 packet 제작
    public byte[] writeClientData_Json(int id, string data)
    {
        byte[] byteData = new byte[0];
        
        switch((ClientToServer)id)
        {
            case ClientToServer.REQ_JOINGAME:
                {
                    P_REQ_JoinGame packet =
                        new P_REQ_JoinGame((byte)id,
                        data);  //nickname

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ClientToServer.REQ_QUIT:
                {
                    P_REQ_QuitGame packet =
                        new P_REQ_QuitGame((byte)id,
                        (byte)int.Parse(data.Substring(0, 1))); //mypos

                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
            case ClientToServer.REQ_CHANGESTATE:
                {
                    P_REQ_ChangeState packet =
                        new P_REQ_ChangeState((byte)id,
                        (byte)int.Parse(data.Substring(0, 1)),  //pos
                        (byte)int.Parse(data.Substring(1, 1)),  //fold or check
                        int.Parse(data.Substring(2)));          //price
                    
                    string jsonData = JsonUtility.ToJson(packet);
                    byteData = Encoding.UTF8.GetBytes(jsonData);
                    break;
                }
        }

        return byteData;
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

        switch ((ServerToClient)dynamicData.id)
        {
            case ServerToClient.ACK_JOIN_PLAYER:
                {
                    int pos = Convert.ToInt32(dynamicData.index);
                    string nickname = dynamicData.nickName;

                    AddPlayer(nickname);

                    //test
                    print(nickname);
                    break;
                }
            case ServerToClient.ACK_QUIT_SOMEBODY:
                {
                    int pos = Convert.ToInt32(dynamicData.index);

                    playerInfo.RemoveAt(pos);
                    SetMyPos();
                    pokergame.SetPlayerNicknameAll(playerInfo);
                    break;
                }
            case ServerToClient.ACK_GAME_START:
                {
                    InitPlayerState();
                    pokergame.InitCommunityCard_Client();
                    break;
                }
            case ServerToClient.ACK_PERSONAL_CARD:
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


                    break;
                }
            case ServerToClient.ACK_TABLE_CARD:
                {
                    int cSuit = Convert.ToInt32(dynamicData.shape);
                    int cNO = Convert.ToInt32(dynamicData.number);

                    Card c = new Card((Card.SUIT)cSuit, cNO, true);
                    pokergame.SetCommCard_Client(recieveCommCard, c);
                    recieveCommCard++;

                    if (recieveCommCard == 5)
                        pokergame.SetResult_Client(mypos);


                    break;
                }
            case ServerToClient.ACK_ANOTHER_CARD:
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
                    break;
                }
            case ServerToClient.ACK_PLAYER_STATE_INFO:
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
                            string stateData = mypos.ToString() + 0.ToString() + 0.ToString();
                            networkManager.SendDatatoServer(writeClientData_Json(2, stateData));
                        }
                        else
                        {
                            //Fold, Check 버튼 활성화
                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);
                        }
                    }
                    break;
                }
            case ServerToClient.ACK_WINNER_INFO:
                {
                    int pos = Convert.ToInt32(dynamicData.index);
                    pokergame.ShowWinner(pos);
                    break;
                }
        }
    }

    #endregion
}
