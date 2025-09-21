using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    NetworkDriver m_Driver;
    NativeList<NetworkConnection> m_Connections;

    [SerializeField] SRConfig _configData;
    ServerData _serverData =  new ServerData();
    ClientData _clientData = new ClientData();
    float _timerCounter;
    GameObject _gameObject;

    void Start()
    {
        _timerCounter = Time.time;
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
                    _clientData.FromByteArray(DataConverter.StreamDataToByteList(stream));
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    m_Connections[i] = default;
                    break;
                }

            }

            
        }
        if (_timerCounter + _configData.ServerUpdateInterval < Time.time)
        {
            if (m_Connections.Length > 0)
            {
                m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[0], out var writer);
                writer.WriteBytes(_serverData.ToByteArray());
                m_Driver.EndSend(writer);
            }
            _timerCounter = Time.time;
        }
        if(_gameObject == null&& _clientData.PlayerID != -1)
        {
            _gameObject = GameObject.Instantiate(_configData.PlayerPrefab, new Vector3(0f, 2, 0f), Quaternion.identity);
            _serverData.PlayerID = 1;
        }
        else
        {
            Vector3 moveDirection = new Vector3(_clientData.InputDirection.x, 0, _clientData.InputDirection.y);
            _gameObject.transform.Translate(moveDirection * Time.deltaTime);
            _serverData.PlayerPosition = _gameObject.transform.position;
        }
        
    }
}
