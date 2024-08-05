using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using System.IO;
using System;
using System.Text;
using Unity.VisualScripting;
using System.Collections.Generic;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;



    void Start()
    {

    }

    public void Connect(string IPAddr, string Port)
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        //나한테 할려면 127.0.0.1
        var endpoint = NetworkEndpoint.Parse(IPAddr, ushort.Parse(Port.AsSpan()));
        //endpoint.Port = 9000;
        //endpoint.Port = ushort.Parse(port.AsSpan());

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
                if (GameManager.m_networkClientConnectEvent != null)
                    GameManager.m_networkClientConnectEvent.Invoke();
                Debug.Log("We are now connected to the server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                byte[] packet = new byte[stream.Length];
                NativeArray<byte> NAByte = new NativeArray<byte>(packet, Allocator.Persistent);
                stream.ReadBytes(NAByte);
                packet = NAByte.ToArray();

                if (GameManager.m_networkServerRecievedEvent != null)
                    GameManager.m_networkServerRecievedEvent.Invoke(packet);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnect form server");
                DisconnectNet();
            }

        }
    }

    //서버에게 byte array packet 보내기
    public void SendReq(byte[] packet)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        NativeArray<byte> NAByte = new NativeArray<byte>(packet, Allocator.Persistent);
        writer.WriteBytes(NAByte);
        m_Driver.EndSend(writer);
    }

    private void OnDestroy()
    {
        //DisconnectNet();
    }

    public void DisconnectNet()
    {
        if (GameManager.m_clientNetworkDisconnectEvent != null)
            GameManager.m_clientNetworkDisconnectEvent.Invoke();

        m_Connection = default(NetworkConnection);
        m_Driver.Dispose();
        Destroy(this);
    }



    /*    public void SendMsg()
        {
            DataStreamWriter writer3;
            m_Driver.BeginSend(m_Connection, out writer3);
            writer3.WriteUInt(2);
            m_Driver.EndSend(writer3);
        }

        //서버에게 데이터 보내기
        public void SendReq(List<byte> packet)
        {
            DataStreamWriter writer;
            m_Driver.BeginSend(m_Connection, out writer);
            NativeArray<byte> NAByte = new NativeArray<byte>(packet.ToArray(), Allocator.Persistent);
            writer.WriteBytes(NAByte);
            m_Driver.EndSend(writer);
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


        }*/

}
