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

    //���� Ŭ�� �����
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

    //Ŭ�� -> ���� ���� ������
    public void SendDatatoServer(List<byte> packet)
    {
        m_Client.SendReq(packet);
    }
    //���� -> ��� Ŭ�󿡰� ���� ������
    public void SendDatatoClientAll(DataStreamWriter writer)
    {
        m_Server.SendAcktoAll(writer);
    }
    //���� -> Ư�� Ŭ�󿡰� ���� ������
    public void SendDatatoClient(DataStreamWriter writer, int pos)
    {
        m_Server.SendAck(writer, pos);
    }



    //���� ���� ������ _ ����
/*    public void SendGameStart_Server()
    {
        m_Server.SendGameStart();
    }
*/
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
    }


}
