using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;


    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        //loopback ipv4 = 자기 자신 지칭 ip 127.0.0.1 (로컬에서 서버 설정시 유용, 외부 네트워크 사용하지 않고 테스트 가능)
        var endpoint = NetworkEndpoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
    }


    private void OnDestroy()
    {
        m_Driver.Dispose();
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

                uint value = 1;
                m_Driver.BeginSend(m_Connection, out var writer);
                writer.WriteUInt(value);
                m_Driver.EndSend(writer);
            }
            else if(cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log("Got the value = " + value + " back from the server");
                Done = true;
                m_Connection.Disconnect(m_Driver);
                m_Connection = default(NetworkConnection);
            }
            else if(cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnect form server");
                m_Connection = default(NetworkConnection);
            }
        }

    }

}
