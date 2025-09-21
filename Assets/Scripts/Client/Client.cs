using Unity.Networking.Transport;
using UnityEngine;

public class Client:MonoBehaviour
{
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;
    GameObject _gameObject;
    [SerializeField] CTConfig _configData;
    private ClientData _clientData = new ClientData();
    private ServerData _latestServerData = new ServerData();

    float _timerCounter;
    private float _dataSyncingInterval = 0.05f;
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
            }
            else if (cmd == NetworkEvent.Type.Data)
            {

                _latestServerData.FromByteArray(DataConverter.StreamDataToByteList(stream));
            }
        }

        if (_timerCounter + _configData.ClientUpdateInterval < Time.time)
        {
            m_Driver.BeginSend(m_Connection, out var writer);
            writer.WriteBytes(_clientData.ToByteArray());
            m_Driver.EndSend(writer);
            _timerCounter = Time.time;
        }
        _clientData.InputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (_gameObject == null)
        {
            _gameObject = GameObject.Instantiate(_configData.PlayerPrefab);
        }
        if (_gameObject)
        {
            _gameObject.transform.position = _latestServerData.PlayerPosition;
        }
    }
}

