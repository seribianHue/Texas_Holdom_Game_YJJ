using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance {  get { return instance; } }

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] public ServerBehaviour m_Server;
    [SerializeField] public ClientBehaviour m_Client;

    NetworkEndpoint m_network = NetworkEndpoint.LoopbackIpv4;

    [SerializeField]
    TMP_InputField IPinputF;

    public string PortNum;
    [SerializeField]
    TMP_InputField PortinputF;

    //서버 클라 만들기
    public void CreateServer()
    {
        GetComponent<ServerBehaviour>().port = PortNum;
        gameObject.GetComponent<ServerBehaviour>().enabled = true;

        UIManager.Instance.SetLobbyUI(false);

        GameManager.Instance.AddPlayer(0, GameManager.Instance.nickName);
        GameManager.Instance.isServer = true;
    }
    public void CreateClient()
    {
        GetComponent<ClientBehaviour>().port = PortNum;
        gameObject.GetComponent<ClientBehaviour>().enabled = true;

        UIManager.Instance.SetLobbyUI(false);
        UIManager.Instance.SetStartGameBTN(false);
        
        GameManager.Instance.isServer = false;

        GetComponent<ClientBehaviour>().nickName = GameManager.Instance.nickName;
    }

    //게임 시작 보내기 _ 서버
    public void SendGameStart_Server()
    {
        m_Server.SendGameStart();
    }

    //플레이어 카드정보 보내기 _ 서버
    public void SendCardInfo(int suit, int no, int pos)
    {
        string data = suit.ToString() + no.ToString();
        m_Server.SendAck(3, data, m_Server.clientList[pos - 1].net);
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
        GameManager.Instance.nickName = nickName;

    }

    public void SetPortNum(string port)
    {
        PortNum = port;
    }

    public void SendFoldCheck_Client(int i)
    {
        m_Client.SendReq(2, i.ToString());
    }


}
