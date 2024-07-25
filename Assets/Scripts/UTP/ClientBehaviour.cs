using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using System.IO;
using System;
using System.Text;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;
    public bool IsConnected;


    void Start()
    {
    }


    private void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        if(isClientCreated)
        {
            //���� ���� �ٽ� Ȯ��
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
                    IsConnected = true;
                    SendUint(2);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    uint recievedUint = RecievedUint(stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client got disconnect form server");
                    m_Connection = default(NetworkConnection);
                }
            }
        }
    }


    public bool isClientCreated;
    public void CreateClient(string port)
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        //loopback ipv4 = �ڱ� �ڽ� ��Ī ip 127.0.0.1 (���ÿ��� ���� ������ ����, �ܺ� ��Ʈ��ũ ������� �ʰ� �׽�Ʈ ����)
        var endpoint = NetworkEndpoint.LoopbackIpv4;
        endpoint.Port = ushort.Parse(port.AsSpan());
        m_Connection = m_Driver.Connect(endpoint);
        isClientCreated = true;
    }

    public uint RecievedUint(DataStreamReader stream)
    {
        uint value = stream.ReadUInt();
        Debug.Log("Got the value = " + value + " back from the server");
        return value;
    }

    public void SendPacket(int type, string data)
    {
        Packet packet = new Packet(type, data);

        m_Driver.BeginSend(m_Connection, out var writer);
        writer.WriteBytes(packet.TotalData.ToNativeArray(Allocator.Persistent));
        m_Driver.EndSend(writer);
    }

    public void RecievePacket(DataStreamReader stream)
    {
        NativeArray<byte> data = new NativeArray<byte>();
        stream.ReadBytes(data);

        int type = ((data[7] << 24) + (data[6] << 16) + (data[5] << 8) + (data[4]));
        string strData = Encoding.UTF8.GetString(data.Slice(8, data.Length - 1).ToArray());
        print(strData);
    }


    public void SendUint(uint num)
    {
        uint value = num;
        m_Driver.BeginSend(m_Connection, out var writer);
        writer.WriteUInt(value);
        m_Driver.EndSend(writer);
    }

    public void Disconnet()
    {
        Done = true;
        m_Connection.Disconnect(m_Driver);
        m_Connection = default(NetworkConnection);
    }
}


