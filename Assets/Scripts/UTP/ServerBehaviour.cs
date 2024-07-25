using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

public class ServerBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;


    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4;
        endpoint.Port = 9000;
        //���� �ּ� �Ҵ� bind ������ 0 ���н� -1
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        //16���� ���� ���ӵǴ� �޸� �Ҵ�
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
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
                    //stream���� uint�޾Ƽ� �޾Ҵٴ� ���� ǥ��
                    uint number = stream.ReadUInt();
                    Debug.Log("Got " + number + " from the Client adding + 2 to it");

                    number += 2;

                    //�����͸� �������� DataStreamWriter �ʿ�, �̴� BeginSend�� ȣ���� ��´�
                    m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                    writer.WriteUInt(number);
                    //���� �Ϸ�!
                    m_Driver.EndSend(writer);
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

