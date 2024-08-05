using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public ServerBehaviour m_Server;
    public ClientBehaviour m_Client;

    NetworkEndpoint m_network = NetworkEndpoint.LoopbackIpv4;

    [SerializeField]
    TMP_InputField IPinputF;

    //서버 클라 만들기
    public void CreateServer(string IPAddr, string portNum)
    {
        m_Server = this.AddComponent<ServerBehaviour>();
        m_Server.Connect(IPAddr, portNum);
    }
    public void CreateClient(string IPAddr, string portNum)
    {
        m_Client = this.AddComponent<ClientBehaviour>();
        m_Client.Connect(IPAddr, portNum);
    }

    //클라 -> 서버 정보 보내기 (packet)
    public void SendDatatoServer<T>(T packet)
    {
        string jsonString = JsonUtility.ToJson(packet);
        byte[] byteData = Encoding.UTF8.GetBytes(jsonString);
        m_Client.SendReq(byteData);
    }

    //서버 -> 모든 클라에게 정보 보내기
    public void SendDatatoClientAll<T>(T packet)
    {
        string jsonString = JsonUtility.ToJson(packet);
        byte[] byteData = Encoding.UTF8.GetBytes(jsonString);
        m_Server.SendAcktoAll(byteData);
    }
    //서버 -> 특정 클라에게 정보 보내기
    public void SendDatatoClient<T>(T packet, NetworkConnection connection)
    {
        string jsonString = JsonUtility.ToJson(packet);
        byte[] byteData = Encoding.UTF8.GetBytes(jsonString);
        m_Server.SendAck(byteData, connection);
    }

    //서버 종료
    public void QuitServer()
    {
        Destroy(m_Server);
        m_Server = null;
    }

    public void DisconnectClient(int pos)
    {
        m_Server.DisconnectClient(pos); 
    }



/*
    //게임 시작 보내기 _ 서버
    public void SendGameStart_Server()
    {
        m_Server.SendGameStart();
    }

    //플레이어 카드정보 보내기 _ 서버
    public void SendCardInfo(int suit, int no, int pos)
    {
        string data = suit.ToString() + no.ToString();
        //m_Server.SendAck(3, data, m_Server.clientList[pos - 1].net);
    }

    //플레이어 카드 정보 받기 _ 클라이언트



    void Start()
    {
        IPinputF.text = m_network.Address.ToString();
    }

    public void SendFoldCheck_Client(int i)
    {
        m_Client.SendReq(2, i.ToString());
    }*/


}
