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
    public int mypos;
    NetworkEndpoint m_network = NetworkEndpoint.LoopbackIpv4;

    [SerializeField]
    TMP_InputField IPinputF;

    public string PortNum;
    [SerializeField]
    TMP_InputField PortinputF;

    
    public void CreateServer()
    {
        GetComponent<ServerBehaviour>().port = PortNum;
        gameObject.GetComponent<ServerBehaviour>().enabled = true;
        UIManager.Instance.SetLobbyUI(false);
    }

    public void CreateClient()
    {
        GetComponent<ClientBehaviour>().port = PortNum;
        gameObject.GetComponent<ClientBehaviour>().enabled = true;
        UIManager.Instance.SetLobbyUI(false);

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
        GetComponent<ClientBehaviour>().nickName = nickName;

    }

    public void SetPortNum(string port)
    {
        PortNum = port;
    }
}
