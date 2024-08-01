using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

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
        m_networkClientRecievedEvent.AddListener(ReadRecievedClientData);

        if (m_networkServerRecievedEvent == null)
            m_networkServerRecievedEvent = new NetworkRecievedServer();
        m_networkServerRecievedEvent.AddListener(ReadRecievedServerData);

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
    //�г��� ����
    public void SetNickName(string nickName)
    {
        this.nickName = nickName;
    }

    //IP ����
    string IPv4;
    public void SetIPv4(string ipv4)
    {
        this.IPv4 = ipv4;
    }

    string portNum;
    //��Ʈ ��ȣ ����
    public void SetPortNum(string portNum)
    {
        this.portNum = portNum;
    }
    #endregion

    #region Server, Client Create
    //���� �����
    public void CreateServer()
    {
        networkManager.CreateServer(IPv4, portNum);
        
        uiManager.SetLobbyUI(false);
        uiManager.SetSendMyInfoBTN(false);

        uiManager.SetFoldBTNInteractable(false);
        uiManager.SetCheckBTNInteractable(false);

        AddPlayer(nickName);
        isServer = true;    
    }
    //Ŭ�� �����
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
        networkManager.SendDatatoServer(writeClientData(1, mypos.ToString()));
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

    //Ŭ�󿡼� �������� �ڽ��� ���� ������
    public void SendMyInfotoServer()
    {
        networkManager.SendDatatoServer(writeClientData(0, nickName));
    }

    //�÷��̾� �߰�
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

    //�ڸ� ���� with nickname and pos
    public void SetGameNicknames()
    {
        pokergame.SetPlayerNicknameAll(playerInfo);
    }



    //���� ����
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
            networkManager.SendDatatoClientAll(writeServerData(6, info));
        }
        else
        {
            string info = mypos.ToString() + 0.ToString() + 0.ToString();
            networkManager.SendDatatoServer(writeClientData(2, info));

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
            networkManager.SendDatatoClientAll(writeServerData(6, info));
        }
        else
        {
            string info = mypos.ToString() + 1.ToString() + 0.ToString();
            networkManager.SendDatatoServer(writeClientData(2, info));

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
    //�������� ���� stream ����
    byte[] writeServerData(int type, string data)
    {
        List<byte> packet = new List<byte>();

        switch (type)
        {
            //�÷��̾� ���� (pos, nickname) ������
            case 0:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //pos
                    packet.AddRange(Encoding.UTF8.GetBytes(data)); //nickname
                    break;
                }
            //Player Out Broadcast
            case 1:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //pos
                    break;
                }
            //Game Start Broadcast
            case 2:
                {
                    packet.Add((byte)type);
                    break;
                }
            //Pass Card Info to Player
            case 3:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1)));
                    packet.Add((byte)int.Parse(data.Substring(1)));

                    break;
                }
            //Send Community Card Info to Players
            case 4:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //suit
                    packet.Add((byte)int.Parse(data.Substring(1))); //no
                    sendCommCard++;
                    if(sendCommCard == 5)
                    {
                        pokergame.SetResultAll();
                    }
                    break;
                }
            //Send lived Player's card
            case 5:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //player index
                    packet.Add((byte)int.Parse(data.Substring(1, 1))); //suit
                    packet.Add((byte)int.Parse(data.Substring(2))); //no
                    break;
                }

            //Send All Client about the state
            case 6:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //pos
                    packet.Add((byte)int.Parse(data.Substring(1, 1))); //fold or check
                    packet.AddRange(BitConverter.GetBytes(int.Parse(data.Substring(2)))); //price

                    break;
                }

            //send Winner pos
            case 7:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //winner pos

                    break;
                }
        }
        return packet.ToArray();

    }

    //Ŭ�󿡼� ���� stream ����
    void ReadRecievedClientData(byte[] packet)
    {
        //�޴°� Ȯ��
        List<byte> packetList = packet.ToList();

        byte type = packetList[0];
        byte[] data = packetList.GetRange(1, packet.Length - 1).ToArray();

        switch (type)
        {
            //New Player Enter
            case 0:
                {
                    string nickname = Encoding.UTF8.GetString(data, 0, data.Length);
                    AddPlayer(nickname);
                    pokergame.SetPlayerNickname(playerInfo.Count - 1, nickname);

                    //��� Ŭ�󿡰� �� �÷��̾� ���� ������
                    string newPlayerData = (playerInfo.Count - 1).ToString() + nickname;
                    for (int i = 1; i < playerInfo.Count; i++)
                    {
                        if (i != playerInfo.Count - 1)
                            networkManager.SendDatatoClient(writeServerData(0, newPlayerData), i);
                    }

                    //�� ������� ���� ����� ���� �˷��ֱ�
                    for (int i = 0; i < playerInfo.Count; i++)
                    {
                        string playerData = i.ToString() + playerInfo[i];
                        networkManager.SendDatatoClient
                            (writeServerData(0, playerData), playerInfo.Count - 1);
                    }
                    
                    //test
                    print(type);
                    print(nickname);
                    break;
                }
            //Client Connection Lost
            case 1:
                {
                    int pos = Convert.ToInt32(data[0]);
                    networkManager.DisconnectClient(pos);

                    playerInfo.RemoveAt(pos);
                    pokergame.SetPlayerNicknameAll(playerInfo);
                    if(playerInfo.Count > 1)
                        networkManager.SendDatatoClientAll(writeServerData(1, pos.ToString()));
                    break;
                }
            //Fold or Check
            case 2:
                {
                    int pos = Convert.ToInt32(data[0]);
                    int state = Convert.ToInt32(data[1]);
                    int price = BitConverter.ToInt32(data.Skip(2).ToArray());

                    if(state == 0)
                    {
                        playerState[pos] = 0;
                        pokergame.FoldPlayer(pos);
                    }
                    else
                    {
                        playerState[pos] = 1;
                    }

                    if(pos == playerState.Count - 1)
                    {
                        //start next round
                        curRound += 1;
                        if(curRound == Rounds.FIRST)
                        {
                            for(int i = 0; i < 3; i++)
                            {
                                Card c = pokergame.SetCommCard_Server(i);
                                string info = ((int)c.suit).ToString() + c.no.ToString();
                                networkManager.SendDatatoClientAll(writeServerData(4, info));
                            }
                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);
                        }
                        else if (curRound == Rounds.SECOND)
                        {
                            Card c = pokergame.SetCommCard_Server(3);
                            string info = ((int)c.suit).ToString() + c.no.ToString();
                            networkManager.SendDatatoClientAll(writeServerData(4, info));

                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);
                        }
                        else if (curRound == Rounds.THIRD)
                        {
                            Card c = pokergame.SetCommCard_Server(4);
                            string info = ((int)c.suit).ToString() + c.no.ToString();
                            networkManager.SendDatatoClientAll(writeServerData(4, info));

                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);


                        }
                        else if(curRound == Rounds.FINALL)
                        {
                            List<PlayerInfo> playerCardInfo;
                            int winnerIndex = pokergame.FindWinner(out playerCardInfo);
                            networkManager.SendDatatoClientAll(writeServerData(7, winnerIndex.ToString()));
                            for (int i = 0; i < playerState.Count; i++)
                            {
                                if (playerState[i] == 1)
                                {
                                    string card1 = i.ToString() + ((int)playerCardInfo[i].Card1.suit).ToString()
                                        + playerCardInfo[i].Card1.no.ToString();
                                    networkManager.SendDatatoClientAll(writeServerData(5, card1));

                                    string card2 = i.ToString() + ((int)playerCardInfo[i].Card2.suit).ToString()
                                        + playerCardInfo[i].Card2.no.ToString();
                                    networkManager.SendDatatoClientAll(writeServerData(5, card2));

                                }
                            }
                        }


                    }

                    string stateData = pos.ToString() + state.ToString() + price.ToString();

                    networkManager.SendDatatoClientAll(writeServerData(6, stateData));
                    break;
                }
        }


        /*        switch (stream.ReadInt())
                {
                    //New Player Enter
                    case 0:
                        {
                            string newNickName = stream.ReadFixedString128().ToString();
                            AddPlayer(newNickName);

                            //���� ����鿡�� ���ο� ��� ���� �˷��ֱ�
                            string data = (playerInfo.Count - 1).ToString() + newNickName;
                            for (int i = 0; i < playerInfo.Count; i++)
                            {
                                if (i != playerInfo.Count - 1)
                                    networkManager.SendDatatoClient(writeServerData(0, data), i);
                            }

                            //���� ���� ���� �ڸ��� ����ִ� ��ġ ã��
                            int pos;
                            for (int i = 0; i < networkManager.m_Server.clientList.Length; i++)
                            {
                                if (networkManager.m_Server.clientList[i] == null)
                                {
                                    pos = i; break;
                                }
                            }

                            //������ ���� �ޱ�
                            networkManager.m_Server.clientList[pos] = new Client(newNickName, networkManager.m_Server.m_Connections[i]);
                            print("New Player at " + i + ", " + clientList[i].nickName);
                            GameManager.Instance.AddPlayer(i + 1, clientList[i].nickName);


                            //�� ������� ���� ����� ���� �˷��ֱ�
                            for (int i = 0; i < playerInfo.Count; i++)
                            {
                                string playerData = i.ToString() + playerInfo[i];
                                networkManager.SendDatatoClient(writeServerData(0, data), playerInfo.Count - 1);
                            }

                            break;
                        }
                    //Fold Check recieve
                    case 2:
                        {

                            break;
                        }
                }*/
    }

    public void TestSendToClient()
    {
        byte[] packet = new byte[] { 0, 1, 2, 3, 4 };
        networkManager.SendDatatoClientAll(packet);
    }

    public void GameStartBTN()
    {
        pokergame.SetCommunityCard_Server();
        
        networkManager.SendDatatoClientAll(writeServerData(2, ""));

        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (i == 0)
                pokergame.SetPlayerCard_Host(playerInfo[i], i);
            else
            {
                List<int> cardinfo = pokergame.SetPlayerCard_Guest(playerInfo[i], i);
                string card1 = cardinfo[0].ToString() + cardinfo[1].ToString();
                string card2 = cardinfo[2].ToString() + cardinfo[3].ToString();

                networkManager.SendDatatoClient(writeServerData(3, card1), i);
                networkManager.SendDatatoClient(writeServerData(3, card2), i);

            }
        }

        //Fold, Check ��ư Ȱ��ȭ
        uiManager.SetFoldBTNInteractable(true);
        uiManager.SetCheckBTNInteractable(true);

        InitPlayerState(); //�÷��̾� ���� ���� �ʱ�ȭ

    }


    #endregion

    

    #region Client Methods

    //Ŭ�󿡼� ���� packet ����
    public byte[] writeClientData(int type, string data)
    {
        List<byte> packet = new List<byte>();

        switch (type)
        {
            //�� �г��� ���� ������
            case 0:
                {
                    packet.Add((byte)type);
                    packet.AddRange(Encoding.UTF8.GetBytes(data));
                    break;
                }
            //���� ����
            case 1:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //pos

                    break;
                }
            //Fold or Check or somthing
            case 2:
                {
                    packet.Add((byte)type);
                    packet.Add((byte)int.Parse(data.Substring(0, 1))); //pos
                    packet.Add((byte)int.Parse(data.Substring(1, 1))); //fold or check
                    packet.AddRange(BitConverter.GetBytes(int.Parse(data.Substring(2)))); //price
                    break;
                }
        }
        return packet.ToArray();

    }

    int recieveMyCard = 0;
    Card card1;
    Card card2;

    int recieveCommCard = 0;

    int recievePlayerCard = 0;
    //�������� ���� stream ����
    void ReadRecievedServerData(byte[] packet)
    {
        List<byte> packetList = packet.ToList();

        byte type = packetList[0];  
        byte[] data = packetList.GetRange(1, packet.Length - 1).ToArray();
        

        switch (type)
        {
            //Player Info Getting
            case 0:
                {
                    int pos = Convert.ToInt32(data[0]);
                    string nickname = Encoding.UTF8.GetString(data).Substring(2);
                    //int pos = int.Parse(dataString.Substring(0, 1));
                    //string nickname = dataString.Substring(1);

                    AddPlayer(nickname);

                    //test
                    print(nickname);
                    break;
                }
            //Someone Got Out
            case 1:
                {
                    int pos = Convert.ToInt32(data[0]);

                    playerInfo.RemoveAt(pos);
                    SetMyPos();
                    pokergame.SetPlayerNicknameAll(playerInfo);
                    break;
                }
            //Game Start
            case 2:
                {
                    InitPlayerState();
                    pokergame.InitCommunityCard_Client();
                    break;
                }
            //My Card Get
            case 3:
                {
                    int cSuit = Convert.ToInt32(data[0]);
                    int cNO = Convert.ToInt32(data[1]);
                    recieveMyCard++;

                    if(recieveMyCard == 1)
                    {
                        card1 = new Card((Card.SUIT)cSuit, cNO, false);
                    }
                    else if (recieveMyCard == 2)
                    {
                        card2 = new Card((Card.SUIT)cSuit, cNO, false);

                        for(int i = 0; i < playerInfo.Count; i++)
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
            //Community Card Set
            case 4:
                {
                    int cSuit = Convert.ToInt32(data[0]);
                    int cNO = Convert.ToInt32(data[1]);
                    
                    Card c = new Card((Card.SUIT)cSuit, cNO, true);
                    pokergame.SetCommCard_Client(recieveCommCard, c);
                    recieveCommCard++;

                    if(recieveCommCard == 5)
                        pokergame.SetResult_Client(mypos);


                    break;
                }
            //Got Lived Player's card
            case 5:
                {
                    int pos = Convert.ToInt32(data[0]);
                    int cSuit = Convert.ToInt32(data[1]);
                    int cNO = Convert.ToInt32(data[2]);

                    Card card = new Card((Card.SUIT)cSuit, cNO, false);

                    if(recievePlayerCard == 0)
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
            //someone has bet
            case 6:
                {
                    int pos = Convert.ToInt32(data[0]);
                    int state = Convert.ToInt32(data[1]);
                    int price = BitConverter.ToInt32(data.Skip(2).ToArray());

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
                        if(curRound == Rounds.THIRD)
                        {
                            //pokergame.SetResult_Client(mypos);
                        }

                        if (playerState[mypos] == 0)
                        {
                            //������ fold�� ������ �ٽ� ���� �ְ� �Ѿ
                            string stateData = mypos.ToString() + 0.ToString() + 0.ToString();
                            networkManager.SendDatatoServer(writeClientData(2, stateData));
                        }
                        else
                        {
                            //Fold, Check ��ư Ȱ��ȭ
                            uiManager.SetFoldBTNInteractable(true);
                            uiManager.SetCheckBTNInteractable(true);
                        }
                    }
                    break;
                }
            //get winner pos
            case 7:
                {
                    int pos = Convert.ToInt32(data[0]);
                    pokergame.ShowWinner(pos);
                    break;
                }

        }

        /*        switch (stream.ReadInt())
                {
                    case 0:
                        {
                            //����� ���� �ޱ�
                            int pos = stream.ReadInt();
                            string nickname = stream.ReadFixedString128().ToString();

                            AddPlayer(nickname);
                            break;
                        }
                    case 2:
                        {
                            GameStart_Client();
                            break;
                        }
                    case 3:
                        {
                            GameManager.Instance.SetMyInfoClient(nickName,
                                (int)stream.ReadUInt(), (int)stream.ReadUInt(),  //card1
                                (int)stream.ReadUInt(), (int)stream.ReadUInt()); //card2
                            break;
                        }
                }*/
    }



    #endregion




    /*    //���� ����
        public void GameStart_Server()
        {
            pokergame.SetCommunityCard_Server();
            //pokergame.DistributeCard(playerInfo, playerInfo.Count);
            networkManager.SendGameStart_Server();
        }
        public void GameStart_Client()
        {
            pokergame.SetCommunityCard_Client();
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
        }*/
}
