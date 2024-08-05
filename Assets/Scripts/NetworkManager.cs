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

    //���� Ŭ�� �����
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

    //Ŭ�� -> ���� ���� ������ (packet)
    public void SendDatatoServer<T>(T packet)
    {
        string jsonString = JsonUtility.ToJson(packet);
        byte[] byteData = Encoding.UTF8.GetBytes(jsonString);
        m_Client.SendReq(byteData);
    }

    //���� -> ��� Ŭ�󿡰� ���� ������
    public void SendDatatoClientAll<T>(T packet)
    {
        string jsonString = JsonUtility.ToJson(packet);
        byte[] byteData = Encoding.UTF8.GetBytes(jsonString);
        m_Server.SendAcktoAll(byteData);
    }
    //���� -> Ư�� Ŭ�󿡰� ���� ������
    public void SendDatatoClient<T>(T packet, NetworkConnection connection)
    {
        string jsonString = JsonUtility.ToJson(packet);
        byte[] byteData = Encoding.UTF8.GetBytes(jsonString);
        m_Server.SendAck(byteData, connection);
    }

    //���� ����
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
    //���� ���� ������ _ ����
    public void SendGameStart_Server()
    {
        m_Server.SendGameStart();
    }

    //�÷��̾� ī������ ������ _ ����
    public void SendCardInfo(int suit, int no, int pos)
    {
        string data = suit.ToString() + no.ToString();
        //m_Server.SendAck(3, data, m_Server.clientList[pos - 1].net);
    }

    //�÷��̾� ī�� ���� �ޱ� _ Ŭ���̾�Ʈ



    void Start()
    {
        IPinputF.text = m_network.Address.ToString();
    }

    public void SendFoldCheck_Client(int i)
    {
        m_Client.SendReq(2, i.ToString());
    }*/


}
