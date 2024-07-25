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
            //Unity C# Job System을 써서 ScheduleUpdate가 있
            m_Driver.ScheduleUpdate().Complete();

            //새 연결 전, 오래된 연결 제거
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

            //데이터의 흐름(데이터 순차적으로 읽거나 쓰는데 사용, 일화용, 내부 반복 -> 코드 간결)
            DataStreamReader stream;
            //최신 연결 목록을 가지고 마지막 업데이트 업데이트 이후 이벤트 쿼리(DB에 정보 요청) 가능
            for(int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                    continue;

                //해결안된 이벤트가 있다면 PopEventForConnection 불러낸다
                NetworkEvent.Type cmd;
                while((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    //그 이벤트가 Data 이벤트면
                    if(cmd == NetworkEvent.Type.Data)
                    {
                        RecievePacket(stream);

/*                        uint recievedNum = RecievedUintData(stream);
                        recievedNum += 2;

                        SendUint(recievedNum, m_Connections[i]);*/

/*
                        //stream에서 uint받아서 받았다는 것을 표시
                        uint number = stream.ReadUInt();
                        Debug.Log("Got " + number + " from the Client adding + 2 to it");

                        number += 2;
                        //데이터를 보내려면 DataStreamWriter 필요, 이는 BeginSend를 호출해 얻는다
                        m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                        writer.WriteUInt(number);
                        //전송 완료!
                        m_Driver.EndSend(writer);*/
                    }
                    //연결 해제 상황 처리
                    else if(cmd == NetworkEvent.Type.Disconnect)
                    {
                        //연결 해제 메시지를 받으면 해당 연결을 default(NetworkConnection)으로 재설정
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
        //서버 주소 할당 bind 성공시 0 실패시 -1
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port " + port);
        else
            m_Driver.Listen();

        //16개의 오래 지속되는 메모리 할당
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        isServerCreated = true;
    }


    public void SendUint(uint num, NetworkConnection network)
    {
        //데이터를 보내려면 DataStreamWriter 필요, 이는 BeginSend를 호출해 얻는다
        m_Driver.BeginSend(NetworkPipeline.Null, network, out var writer);
        writer.WriteUInt(num);

        //writer가 다 byte로 저장된다.
        print(writer.Length);

        //writer.WriteBytes();

        //전송 완료!
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
        //stream에서 uint받아서 받았다는 것을 표시
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

