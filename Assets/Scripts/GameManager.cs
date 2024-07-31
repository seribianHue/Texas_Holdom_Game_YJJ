using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NetworkRecievedClient : UnityEvent<byte[]>
{

}
public class NetworkRecievedServer : UnityEvent<byte[]>
{

}

public class NetworkConnectEvent : UnityEvent
{

}


public class GameManager : MonoBehaviour
{
    public static NetworkRecievedClient m_networkClientRecievedEvent;
    public static NetworkRecievedServer m_networkServerRecievedEvent;

    public static NetworkConnectEvent m_networkServerConnectEvent;

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

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

        if (m_networkClientRecievedEvent == null)
            m_networkClientRecievedEvent = new NetworkRecievedClient();
        m_networkClientRecievedEvent.AddListener(ReadRecievedClientData);

        if (m_networkServerRecievedEvent == null)
            m_networkServerRecievedEvent = new NetworkRecievedServer();
        m_networkServerRecievedEvent.AddListener(ReadRecievedServerData);

        if(m_networkServerConnectEvent == null)
            m_networkServerConnectEvent = new NetworkConnectEvent();
        m_networkServerConnectEvent.AddListener(SendMyInfotoServer);
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

        AddPlayer(nickName);
        isServer = true;    
    }
    //Ŭ�� �����
    public void CreateClient()
    {
        networkManager.CreateClient(IPv4, portNum);

        uiManager.SetLobbyUI(false);
        uiManager.SetStartGameBTN(false);

        isServer = false;
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


    #region Server Methods

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
                    packet.Add((byte)int.Parse(data.Substring(0, 1)));
                    packet.AddRange(Encoding.UTF8.GetBytes(data));
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
        }
        return packet.ToArray();

    }

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
