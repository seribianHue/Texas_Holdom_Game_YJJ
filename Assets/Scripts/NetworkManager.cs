using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    public void SendDatatoServer(byte[] packet)
    {
        m_Client.SendReq(packet);
    }

    //���� -> ��� Ŭ�󿡰� ���� ������
    public void SendDatatoClientAll(byte[] packet)
    {
        m_Server.SendAcktoAll(packet);
    }
    //���� -> Ư�� Ŭ�󿡰� ���� ������
    public void SendDatatoClient(byte[] packet, int pos)
    {
        m_Server.SendAck(packet, pos);
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
