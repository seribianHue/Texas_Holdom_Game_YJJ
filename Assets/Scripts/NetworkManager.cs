using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] ServerBehaviour m_Server;
    [SerializeField] ClientBehaviour m_Client;

    public string nickName;
    NetworkEndpoint m_network = NetworkEndpoint.LoopbackIpv4;

    [SerializeField]
    TMP_InputField IPinputF;

    public string PortNum;
    [SerializeField]
    TMP_InputField PortinputF;

    public void CreateServer()
    {
        m_Server.CreatServer(PortNum);
    }

    public void CreateClient()
    {
        m_Client.CreateClient(PortNum);
        m_Client.SendPacket(0, nickName);
    }
    void Start()
    {
        IPinputF.text = m_network.Address.ToString();
    }


    void Update()
    {
        
    }

    public void SetNickName(string nickName)
    {
        this.nickName = nickName;
    }

    public void SetPortNum(string port)
    {
        PortNum = port;
    }
}
