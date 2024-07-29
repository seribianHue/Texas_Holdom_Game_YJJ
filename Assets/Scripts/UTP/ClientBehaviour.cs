using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using System.IO;
using System;
using System.Text;
using Unity.VisualScripting;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;

    public string port;

    public string nickName;
    public int myPos;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndpoint.LoopbackIpv4;
        //endpoint.Port = 9000;
        endpoint.Port = ushort.Parse(port.AsSpan());

        m_Connection = m_Driver.Connect(endpoint);
    }

    void Update()
    {
        //연결 성공 다시 확인
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!Done)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                if (GameManager.m_networkServerRecievedEvent != null)
                    GameManager.m_networkServerRecievedEvent.Invoke(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnect form server");
                m_Connection = default(NetworkConnection);
            }

        }
    }

    public void SendMsg()
    {
        DataStreamWriter writer3;
        m_Driver.BeginSend(m_Connection, out writer3);
        writer3.WriteUInt(2);
        m_Driver.EndSend(writer3);
    }

    public void SendReq(DataStreamWriter writer)
    {
        DataStreamWriter tempWriter;
        m_Driver.BeginSend(m_Connection, out tempWriter);
        tempWriter = writer;
        m_Driver.EndSend(tempWriter);
    }

    public void SendReq(int type, string data)
    {
        switch (type)
        {
            case 0:
                {
                    DataStreamWriter writer;
                    m_Driver.BeginSend(m_Connection, out writer);
                    writer.WriteUInt((uint)type);
                    writer.WriteFixedString128(data);
                    m_Driver.EndSend(writer);
                    break;
                }
            case 2:
                {
                    DataStreamWriter writer;
                    m_Driver.BeginSend(m_Connection, out writer);
                    writer.WriteUInt((uint)type);
                    writer.WriteUInt((uint)myPos);
                    writer.WriteUInt(uint.Parse(data));
                    m_Driver.EndSend(writer);
                    break;
                }
        }


    }

}
