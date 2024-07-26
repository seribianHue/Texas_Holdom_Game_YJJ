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
                        case 0:
                            {
                                clientList[i] = new Client(stream.ReadFixedString128().ToString(), m_Connections[i]);
                                
                                print("New Player at " + i + ", " + clientList[i].nickName);
                                GameManager.Instance.AddPlayer();
                                
                                string data = i.ToString() + clientList[i].nickName;
                                SendAck(0, data);
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

    public void SendAck(int type, string data)
    {
        switch (type)
        {
            case 0:
                {
                    //new Player Broadcast
                    for (int i = 0; i < clientList.Length; i++)
                    {
                        if(clientList[i] != null)
                        {
                            DataStreamWriter writer3;
                            m_Driver.BeginSend(m_Connections[i], out writer3);
                            writer3.WriteUInt((uint)type);
                            writer3.WriteFixedString128(data);
                            m_Driver.EndSend(writer3);
                            
                        }

                    }
                    break;
                }

            case 2:
                {
                    //GameStart Broadcast
                    for (int i = 0; i < clientList.Length; i++)
                    {
                        if (clientList[i] != null)
                        {
                            DataStreamWriter writer;
                            m_Driver.BeginSend(m_Connections[i], out writer);
                            writer.WriteUInt((uint)type);
                            m_Driver.EndSend(writer);
                        }
                    }
                    GameManager.Instance.GameStart();
                    GameManager.Instance.SetMyInfoServer(0);
                    break;
                }

            case 3:
                {
                    //Card Distribute
                    for (int i = 0; i < clientList.Length; i++)
                    {
                        if (clientList[i] != null)
                        {
                            DataStreamWriter writer;
                            m_Driver.BeginSend(m_Connections[i], out writer);
                            writer.WriteUInt((uint)type);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card1.Suit);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card1.NO);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card2.Suit);
                            writer.WriteUInt((uint)GameManager.Instance.pokergame.playersInfo[i + 1].Card2.NO);
                            m_Driver.EndSend(writer);
                        }
                    }
                    break;
                }
        }
    }
}
