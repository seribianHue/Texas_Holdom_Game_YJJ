using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance {  get { return instance; } }

    private void Awake()
    {
        instance = this;
    }

    public ServerBehaviour m_Server;
    public ClientBehaviour m_Client;

    NetworkEndpoint m_network = NetworkEndpoint.LoopbackIpv4;

    [SerializeField]
    TMP_InputField IPinputF;

    //서버 클라 만들기
    public void CreateServer(string portNum)
    {
        m_Server = this.AddComponent<ServerBehaviour>();
        m_Server.port = portNum;
        //gameObject.GetComponent<ServerBehaviour>().enabled = true;
    }
    public void CreateClient(string portNum)
    {
        m_Client = this.AddComponent<ClientBehaviour>();
        m_Client.port = portNum;
        //gameObject.GetComponent<ClientBehaviour>().enabled = true;
    }

    //클라 -> 서버 정보 보내기
    public void SendDatatoServer(List<byte> packet)
    {
        m_Client.SendReq(packet);
    }
    //서버 -> 모든 클라에게 정보 보내기
    public void SendDatatoClientAll(DataStreamWriter writer)
    {
        m_Server.SendAcktoAll(writer);
    }
    //서버 -> 특정 클라에게 정보 보내기
    public void SendDatatoClient(DataStreamWriter writer, int pos)
    {
        m_Server.SendAck(writer, pos);
    }



    //게임 시작 보내기 _ 서버
/*    public void SendGameStart_Server()
    {
        m_Server.SendGameStart();
    }
*/
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
    }


}
