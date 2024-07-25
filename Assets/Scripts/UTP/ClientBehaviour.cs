using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using System.IO;

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
    public void CreateClient()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        //loopback ipv4 = 자기 자신 지칭 ip 127.0.0.1 (로컬에서 서버 설정시 유용, 외부 네트워크 사용하지 않고 테스트 가능)
        var endpoint = NetworkEndpoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
        isClientCreated = true;
    }

    public uint RecievedUint(DataStreamReader stream)
    {
        uint value = stream.ReadUInt();
        Debug.Log("Got the value = " + value + " back from the server");
        return value;
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
