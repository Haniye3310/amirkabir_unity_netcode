using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    NetworkDriver m_Driver;
    NativeList<NetworkConnection> m_Connections;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 7777.");
            return;
        }
        m_Driver.Listen();
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
        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections.
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        // Accept new connections.
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default)
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection.");
        }

        for (int i = 0; i < m_Connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    ClientData clientData = new ClientData();
                    clientData.FromByteArray(DataConverter.StreamDataToByteList(stream));

                    Debug.Log($"SERVER_SIDE_RECEIVED:\nPlayerId:{clientData.PlayerID} | InputDirection:{clientData.InputDirection}");

                    ServerData serverData = new ServerData();
                    serverData.PlayerID = clientData.PlayerID;
                    serverData.PlayerPosition = new Vector3(100, 150, 100);
                    
                    m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                    writer.WriteBytes(serverData.ToByteArray());
                    m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    m_Connections[i] = default;
                    break;
                }

            }
        }
    }
}
