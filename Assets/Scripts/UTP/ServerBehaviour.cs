using UnityEngine;
using UnityEngine.Assertions;

using Unity.Networking.Transport;
using Unity.VisualScripting;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using Unity.Collections;
using System.Net.Sockets;

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

    void Start()
    {
    }

    public void Connect(string IPAddr, string Port)
    {
        m_Driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.AnyIpv4;
        endpoint.Port = ushort.Parse(Port.AsSpan());

        //var endpoint = NetworkEndpoint.Parse(IPAddr, ushort.Parse(Port.AsSpan()));


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
                    byte[] packet = new byte[stream.Length];
                    NativeArray<byte> NAByte = new NativeArray<byte>(packet, Allocator.Persistent);
                    stream.ReadBytes(NAByte);
                    packet = NAByte.ToArray();

                    if (GameManager.m_networkClientRecievedEvent != null)
                        GameManager.m_networkClientRecievedEvent.Invoke(packet);

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

    //모든 클라에게 정보 보내기
    public void SendAcktoAll(byte[] packet)
    {
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                DataStreamWriter writer;
                m_Driver.BeginSend(m_Connections[i], out writer);
                NativeArray<byte> NAByte = new NativeArray<byte>(packet, Allocator.Persistent);
                writer.WriteBytes(NAByte);
                m_Driver.EndSend(writer);
            }
        }
    }

    //특정 클라에게 정보 보내기
    public void SendAck(byte[] packet, int pos)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connections[pos - 1], out writer);
        NativeArray<byte> NAByte = new NativeArray<byte>(packet, Allocator.Persistent);
        writer.WriteBytes(NAByte);
        m_Driver.EndSend(writer);
    }




/*    //게임 시작 보내기
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

    //플레이어 카드 보내기
    public void SendPlayerCardInfo(string data, int pos)
    {
        SendAck(3, data, clientList[pos - 1].net);
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
                    writer.WriteInt(type);
                    m_Driver.EndSend(writer);

                    //GameManager.Instance.GameStart_Server();
                    //GameManager.Instance.SetMyInfoServer(0);
                    break;
                }

            case 3:
                {
                    //Personal Card Distribute
                    DataStreamWriter writer;
                    m_Driver.BeginSend(connection, out writer);
                    writer.WriteInt(type);
                    writer.WriteInt(int.Parse(data.Substring(0, 1)));   //suit
                    writer.WriteInt(int.Parse(data.Substring(1)));      //no
                    m_Driver.EndSend(writer);

                    break;
                }
        }
    }*/
}
