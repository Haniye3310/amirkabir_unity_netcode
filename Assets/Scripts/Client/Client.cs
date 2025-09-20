using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client:MonoBehaviour
{
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;

    void Start()
    {
        m_Driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        m_Connection = m_Driver.Connect(endpoint);
    }

    void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }

        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                ClientData clientData = new ClientData();
                clientData.PlayerID = 1;
                clientData.InputDirection = new Vector2(10, 10);
                Debug.Log("We are now connected to the server.");
                m_Driver.BeginSend(m_Connection, out var writer);
                writer.WriteBytes(clientData.ToByteArray());
                m_Driver.EndSend(writer);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                ServerData serverData = new ServerData();
                serverData.FromByteArray(DataConverter.StreamDataToByteList(stream));
                Debug.Log($"CLIENT_SIDE_RECEIVED:\nPlayerID:{serverData.PlayerID} | PlayerPosition:{serverData.PlayerPosition}");

                m_Connection.Disconnect(m_Driver);
                m_Connection = default;
            }
        }
    }
}

