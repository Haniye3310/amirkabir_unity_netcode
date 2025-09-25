using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Server : MonoBehaviour
{
    NetworkDriver _driver;

    private ServerData _serverData = new ServerData();
    private List<PlayerData> _playerDataList;

    [SerializeField] SRConfig _configData;
    float _timerCounter;
    int _playerIdIndex = -1;

    void Start()
    {
        _timerCounter = Time.time;
        _driver = NetworkDriver.Create();
        _playerDataList = new List<PlayerData>(16);

        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
        if (_driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 7777.");
            return;
        }
        _driver.Listen();
    }


    void OnDestroy()
    {
        if (_driver.IsCreated)
        {
            _driver.Dispose();
        }
    }

    void Update()
    {
        _driver.ScheduleUpdate().Complete();

        // Clean up connections.
        for (int i = 0; i < _playerDataList.Count; i++)
        {
            if (!_playerDataList[i].NetworkConnection.IsCreated)
            {
                _playerDataList.RemoveAtSwapBack(i);
                i--;
            }
        }

        // Accept new connections.
        NetworkConnection c;
        while ((c = _driver.Accept()) != default)
        {
            PlayerData connectionData = new PlayerData();
            connectionData.NetworkConnection = c;
            _playerIdIndex++;
            connectionData.ClientData.PlayerID = _playerIdIndex;
            _playerDataList.Add(connectionData);
            connectionData.GameObject = GameObject.Instantiate(_configData.PlayerPrefab, new Vector3(0f, 2, 0f), Quaternion.identity);
            PlayerServerData playerData = new PlayerServerData();
            playerData.PlayerID = connectionData.ClientData.PlayerID;
            _serverData.PlayerDataList.Add(playerData);

            _driver.BeginSend(NetworkPipeline.Null,c, out var writer);
            writer.WriteInt(_playerIdIndex);
            _driver.EndSend(writer);

            Debug.Log("Accepted a connection.");
        }

        for (int i = 0; i < _playerDataList.Count; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = _driver.PopEventForConnection(_playerDataList[i].NetworkConnection, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    _playerDataList[i].ClientData.ReadFromStream(ref stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from the server.");
                    _playerDataList[i] = default;
                    break;
                }
            }
        }
        if (_timerCounter + _configData.ServerUpdateInterval < Time.time)
        {
            for (int i = 0; i < _playerDataList.Count; i++)
            {
                _driver.BeginSend(NetworkPipeline.Null, _playerDataList[i].NetworkConnection, out var writer);
                _serverData.WriteToStream(ref writer);
                _driver.EndSend(writer);
            }
            _timerCounter = Time.time;
        }

        //game loop
        {
            for (int i = 0; i < _playerDataList.Count; i++)
            {

                Vector3 moveDirection = new Vector3
                    (_playerDataList[i].ClientData.InputDirection.x, 0, _playerDataList[i].ClientData.InputDirection.y);
                PlayerServerData playerData = _serverData.PlayerDataList.First(p => p.PlayerID == i);
                _playerDataList[i].GameObject.transform.Translate(moveDirection * Time.deltaTime);
                playerData.PlayerPosition = _playerDataList[i].GameObject.transform.position;

            }
        }
    }
}
