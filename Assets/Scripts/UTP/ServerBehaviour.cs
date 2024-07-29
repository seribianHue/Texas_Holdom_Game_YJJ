using UnityEngine;
using UnityEngine.Assertions;

using Unity.Networking.Transport;
using Unity.VisualScripting;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using Unity.Collections;

public class Client
{
    public string nickName;
    public NetworkConnection net;

    public Client(string nickName, NetworkConnection net)
    {
        this.nickName = nickName;
        this.net = net;
    }   
}

public class ServerBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    public string port;

    public Client[] clientList = new Client[7];

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4;
        //endpoint.Port = 9000;
        endpoint.Port = ushort.Parse(port.AsSpan());

        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        //Accept new Connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    switch (stream.ReadUInt())
                    {
                        //New Player Enter
                        case 0:
                            {
                                string newNickName = stream.ReadFixedString128().ToString();
                                

                                //기존 사람들에게 새로운 사람 정보 알려주기
                                for (int j = 0; j < clientList.Length; j++)
                                {
                                    if (clientList[j] != null)
                                    {
                                        string newPlayerData = (i + 1).ToString() + newNickName;
                                        SendAck(0, newPlayerData, m_Connections[j]);
                                    }
                                }


                                //서버가 정보 받기
                                clientList[i] = new Client(newNickName, m_Connections[i]);
                                print("New Player at " + i + ", " + clientList[i].nickName);
                                GameManager.Instance.AddPlayer(i + 1, clientList[i].nickName);


                                //새 멤버한테 기존 사람들 정보 알려주기
                                foreach (var player in GameManager.Instance.playerInfo)
                                {
                                    string playerData = player.Key.ToString() + player.Value;
                                    SendAck(0, playerData, clientList[i].net);
                                }

                                break;
                            }
                        //Fold Check recieve
                        case 2:
                            {

                                break;
                            }
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }

    public void SendMsg()
    {
        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamWriter writer3;
            m_Driver.BeginSend(m_Connections[i], out writer3);
            writer3.WriteUInt(212121212);
            m_Driver.EndSend(writer3);
        }
    }

    public void SendGameStart()
    {
        for (int i = 0; i < clientList.Length; i++)
        {
            if (clientList[i] != null)
            {
                SendAck(2, "", clientList[i].net);
            }
        }
    }

    public void SendAck(int type, string data, NetworkConnection connection)
    {
        switch (type)
        {
            case 0:
                {
                    //new Player Broadcast
                    DataStreamWriter writer3;
                    m_Driver.BeginSend(connection, out writer3);
                    writer3.WriteInt(type);
                    writer3.WriteInt(int.Parse(data.Substring(0,1)));
                    writer3.WriteFixedString128(data.Substring(1));
                    m_Driver.EndSend(writer3);
                    break;
                }

            case 2:
                {
                    //GameStart Broadcast
                    DataStreamWriter writer;
                    m_Driver.BeginSend(connection, out writer);
                    writer.WriteUInt((uint)type);
                    m_Driver.EndSend(writer);
                    //GameManager.Instance.GameStart_Server();
                    //GameManager.Instance.SetMyInfoServer(0);
                    break;
                }

            case 3:
                {
                    //Personal Card Distribute
                    for (int i = 0; i < clientList.Length; i++)
                    {
                        if (clientList[i] != null)
                        {
                            DataStreamWriter writer;
                            m_Driver.BeginSend(m_Connections[i], out writer);
                            writer.WriteUInt((uint)type);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card1.suit);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card1.no);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card2.suit);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card2.no);
                            m_Driver.EndSend(writer);
                        }
                    }
                    break;
                }
        }
    }
}
