using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NetworkRecievedClient : UnityEvent<DataStreamReader>
{

}
public class NetworkRecievedServer : UnityEvent<DataStreamReader>
{

}


public class GameManager : MonoBehaviour
{
    public static NetworkRecievedClient m_networkClientRecievedEvent;
    public static NetworkRecievedServer m_networkServerRecievedEvent;

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
    }

    [SerializeField] PokerGame pokergame;
    [SerializeField] NetworkManager networkManager;
    [SerializeField] UIManager uiManager;

    const int COMMUNITYNUM = 5;

    PlayerInfo myInfo;
    public string nickName;
    public int mypos;

    public bool isServer;


    //�г��� ����
    public void SetNickName(string nickName)
    {
        this.nickName = nickName;
    }

    string portNum;
    //��Ʈ ��ȣ ����
    public void SetPortNum(string portNum)
    {
        this.portNum = portNum;
    }


    //���� �����
    public void CreateServer()
    {
        networkManager.CreateServer(portNum);
        
        uiManager.SetLobbyUI(false);
        uiManager.SetSendMyInfoBTN(false);

        AddPlayer(nickName);
        isServer = true;    
    }
    //Ŭ�� �����
    public void CreateClient()
    {
        networkManager.CreateClient(portNum);

        uiManager.SetLobbyUI(false);
        uiManager.SetStartGameBTN(false);

        isServer = false;

        //networkManager.SendDatatoServer(writeClientData(0, nickName));
    }

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

    //Ŭ�󿡼� ���� stream ����
    public List<byte> writeClientData(int type, string data)
    {
        DataStreamWriter writer = new DataStreamWriter();
        List<byte> packet = new List<byte>();
        
        switch (type)
        {
            //�� �г��� ���� ������
            case 0:
                {
                    packet.AddRange(BitConverter.GetBytes(type));
                    packet.AddRange(Encoding.UTF8.GetBytes(data));
                    packet.InsertRange(0, BitConverter.GetBytes(packet.Count));

                    writer.WriteInt(type);
                    writer.WriteFixedString128(data);
                    break;
                }
        }
        return packet;

    }

    //�������� ���� stream ����
    DataStreamWriter writeServerData(int type, string data)
    {
        DataStreamWriter writer = new DataStreamWriter();
        switch (type)
        {
            case 0:
                {
                    //new Player Broadcast
                    writer.WriteInt(type);
                    writer.WriteInt(int.Parse(data.Substring(0, 1)));
                    writer.WriteFixedString128(data.Substring(1));
                    break;
                }

            case 2:
                {
                    //GameStart Broadcast
                    writer.WriteInt(type);
                    break;
                }

            case 3:
                {
                    //Personal Card Distribute
                    writer.WriteInt(type);
                    writer.WriteInt(int.Parse(data.Substring(0, 1)));   //suit
                    writer.WriteInt(int.Parse(data.Substring(1)));      //no
                    break;
                }
        }
        return writer;
    }

    //Ŭ�󿡼� ���� stream ����
    void ReadRecievedClientData(DataStreamReader stream)
    {
        //�޴°� Ȯ��
        NativeArray<byte> NAByte = new NativeArray<byte>();
        stream.ReadBytes(NAByte);
        print(NAByte);
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
                        if(i != playerInfo.Count - 1)
                            networkManager.SendDatatoClient(writeServerData(0, data), i);
                    }

                    *//*   //���� ���� ���� �ڸ��� ����ִ� ��ġ ã��
                       int pos;
                       for(int i = 0; i < networkManager.m_Server.clientList.Length; i++)
                       {
                           if (networkManager.m_Server.clientList[i] == null)
                           {
                               pos = i; break;
                           }
                       }

                       //������ ���� �ޱ�
                       networkManager.m_Server.clientList[pos] = new Client(newNickName, networkManager.m_Server.m_Connections[i]);
                       print("New Player at " + i + ", " + clientList[i].nickName);
                       GameManager.Instance.AddPlayer(i + 1, clientList[i].nickName);*//*


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

    //�������� ���� stream ����
    void ReadRecievedServerData(DataStreamReader stream)
    {
        switch (stream.ReadInt())
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
/*                    GameManager.Instance.SetMyInfoClient(nickName,
                        (int)stream.ReadUInt(), (int)stream.ReadUInt(),  //card1
                        (int)stream.ReadUInt(), (int)stream.ReadUInt()); //card2*/
                    break;
                }
        }
    }


    //���� ����
/*    public void GameStart_Server()
    {
        pokergame.SetCommunityCard_Server();
        //pokergame.DistributeCard(playerInfo, playerInfo.Count);
        networkManager.SendGameStart_Server();
    }*/
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
