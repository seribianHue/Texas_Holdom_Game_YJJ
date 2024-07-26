using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using System.IO;
using System;
using System.Text;
using UnityEngine.XR;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;

    public string port;

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
                uint value = 2;
                m_Driver.BeginSend(m_Connection, out var writer);
                writer.WriteUInt(value);
                m_Driver.EndSend(writer);


            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log("Got the value = " + value + " back from the server");
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

    public void SendReq(int type, string data)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        writer.WriteUInt((uint)type);
        writer.WriteFixedString128(data);
        m_Driver.EndSend(writer);
    }

}
