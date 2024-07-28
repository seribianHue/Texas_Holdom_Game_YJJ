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

                SendReq(0, nickName);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                switch (stream.ReadUInt())
                {
                    case 0:
                        {
                            //int type = (int)stream.ReadUInt();
                            string data = stream.ReadFixedString128().ToString();
                            string pos = data.Substring(0, 1);
                            string nickname = data.Substring(1);

                            myPos = int.Parse(pos);

                            if(nickname != this.nickName)
                            {
                                print("New Player at " + pos + ", " + nickname);
                            }
                            break;
                        }
                    case 2:
                        {

                            break;
                        }
                    case 3:
                        {
                            GameManager.Instance.SetMyInfoClient(nickName,
                                (int)stream.ReadUInt(), (int)stream.ReadUInt(),  //card1
                                (int)stream.ReadUInt(), (int)stream.ReadUInt()); //card2
                            break;
                        }
                }
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
