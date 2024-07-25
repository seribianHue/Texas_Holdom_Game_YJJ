using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

public class ServerBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;


    void Start()
    {
    }


    void OnDestroy()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }


    void Update()
    {
        if (isServerCreated)
        {
            //Unity C# Job System�� �Ἥ ScheduleUpdate�� ��
            m_Driver.ScheduleUpdate().Complete();

            //�� ���� ��, ������ ���� ����
            for(int i = 0; i < m_Connections.Length; i++)
            {
                if(!m_Connections[i].IsCreated)
                {
                    m_Connections.RemoveAtSwapBack(i);
                    --i;
                }
            }

            //accept new connections
            NetworkConnection c;
            while((c = m_Driver.Accept()) != default(NetworkConnection))
            {
                m_Connections.Add(c);
                Debug.Log("Accepted a connection");
            }

            //�������� �帧(������ ���������� �аų� ���µ� ���, ��ȭ��, ���� �ݺ� -> �ڵ� ����)
            DataStreamReader stream;
            //�ֽ� ���� ����� ������ ������ ������Ʈ ������Ʈ ���� �̺�Ʈ ����(DB�� ���� ��û) ����
            for(int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                    continue;

                //�ذ�ȵ� �̺�Ʈ�� �ִٸ� PopEventForConnection �ҷ�����
                NetworkEvent.Type cmd;
                while((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    //�� �̺�Ʈ�� Data �̺�Ʈ��
                    if(cmd == NetworkEvent.Type.Data)
                    {
                        RecievePacket(stream);

/*                        uint recievedNum = RecievedUintData(stream);
                        recievedNum += 2;

                        SendUint(recievedNum, m_Connections[i]);*/

/*
                        //stream���� uint�޾Ƽ� �޾Ҵٴ� ���� ǥ��
                        uint number = stream.ReadUInt();
                        Debug.Log("Got " + number + " from the Client adding + 2 to it");

                        number += 2;
                        //�����͸� �������� DataStreamWriter �ʿ�, �̴� BeginSend�� ȣ���� ��´�
                        m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                        writer.WriteUInt(number);
                        //���� �Ϸ�!
                        m_Driver.EndSend(writer);*/
                    }
                    //���� ���� ��Ȳ ó��
                    else if(cmd == NetworkEvent.Type.Disconnect)
                    {
                        //���� ���� �޽����� ������ �ش� ������ default(NetworkConnection)���� �缳��
                        Debug.Log("Client disconnected from server");
                        m_Connections[i] = default(NetworkConnection);
                    }
                }
            }
        }
    }

    public bool isServerCreated;
    public void CreatServer(string port)
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4;
        endpoint.Port = ushort.Parse(port.AsSpan());
        //���� �ּ� �Ҵ� bind ������ 0 ���н� -1
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port " + port);
        else
            m_Driver.Listen();

        //16���� ���� ���ӵǴ� �޸� �Ҵ�
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        isServerCreated = true;
    }


    public void SendUint(uint num, NetworkConnection network)
    {
        //�����͸� �������� DataStreamWriter �ʿ�, �̴� BeginSend�� ȣ���� ��´�
        m_Driver.BeginSend(NetworkPipeline.Null, network, out var writer);
        writer.WriteUInt(num);

        //writer�� �� byte�� ����ȴ�.
        print(writer.Length);

        //writer.WriteBytes();

        //���� �Ϸ�!
        m_Driver.EndSend(writer);
    }

    public void SendPacket(int type, string data, NetworkConnection network)
    {
        Packet packet = new Packet(type, data);

        m_Driver.BeginSend(NetworkPipeline.Null, network, out var writer);
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

    public uint RecievedUintData(DataStreamReader stream)
    {
        //stream���� uint�޾Ƽ� �޾Ҵٴ� ���� ǥ��
        uint number = stream.ReadUInt();
        Debug.Log("Got " + number + " from the Client adding + 2 to it");

        return number;
    }
}

public struct Packet
{
    int Length;
    int Type;
    string Data;
    public List<byte> TotalData;

    public Packet(int type, string data)
    {
        Type = type; 
        Data = data;
        Length = 1 + Data.Length;

        TotalData = new List<byte>();
        TotalData.AddRange(BitConverter.GetBytes(Length));
        TotalData.AddRange(BitConverter.GetBytes(Type));
        TotalData.AddRange(Encoding.UTF8.GetBytes(Data));

        NativeArray<byte> natd = TotalData.ToNativeArray<byte>(Allocator.Persistent);
    }
}

