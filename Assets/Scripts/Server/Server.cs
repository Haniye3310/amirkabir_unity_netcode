using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Server : MonoBehaviour
{
    NetworkDriver _driver;

    private ServerData _serverData = new ServerData();
    private List<PlayerData> _playerDataList;
    private List<NetworkConnection> _unverifiedConnections = new List<NetworkConnection>();
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
            if (!_playerDataList[i].NetworkConnection.IsCreated &&
                _playerDataList[i].DisconnectionTime + _configData.ReconnectTimeout * 1000 > (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                GameObject.Destroy(_playerDataList[i].GameObject);
                _playerDataList.RemoveAtSwapBack(i);
                
                i--;
            }
        }
        // Clean up unverified connections.
        for (int i = 0; i < _unverifiedConnections.Count; i++)
        {
            if (!_unverifiedConnections[i].IsCreated)
            {
                _unverifiedConnections.RemoveAtSwapBack(i);
                i--;
            }
        }
        // Accept new connections.
        {
            NetworkConnection c;
            while ((c = _driver.Accept()) != default)
            {
                _unverifiedConnections.Add(c);
                Debug.Log("Accepted a connection.");
            }
        }

        for (int i = 0; i < _unverifiedConnections.Count; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            NetworkConnection c = _unverifiedConnections[i];
            while ((cmd = _driver.PopEventForConnection(_unverifiedConnections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    if (stream.Length == 4)
                    {
                        int id = stream.ReadInt();
                        bool exits = (id != -1) && (_playerDataList.Any(p => p.ClientData.PlayerID == id));

                        if (!exits) //it means this is a new player
                        {
                            PlayerData playerData = new PlayerData();
                            playerData.NetworkConnection = c;
                            _playerDataList.Add(playerData);
                            _playerIdIndex++;
                            playerData.ClientData.PlayerID = _playerIdIndex;
                            playerData.GameObject = GameObject.Instantiate(_configData.PlayerPrefab, new Vector3(0f, 2, 0f), Quaternion.identity);
                            PlayerServerData playerServerData = new PlayerServerData();
                            playerServerData.PlayerID = playerData.ClientData.PlayerID;
                            _serverData.PlayerDataList.Add(playerServerData);

                            _driver.BeginSend(NetworkPipeline.Null, c, out var writer);
                            writer.WriteInt(_playerIdIndex);
                            _driver.EndSend(writer);
                        }
                        else
                        {
                            PlayerData p = _playerDataList.First(p => p.ClientData.PlayerID == id);
                            p.NetworkConnection = _unverifiedConnections[i];
                        }
                        _unverifiedConnections[i] = default;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    _unverifiedConnections[i] = default;
                }
            }

        }


        for (int i = 0; i < _playerDataList.Count; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            if (_playerDataList[i].NetworkConnection.IsCreated)
            {
                while ((cmd = _driver.PopEventForConnection(_playerDataList[i].NetworkConnection, out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        _playerDataList[i].ClientData.ReadFromStream(ref stream);
                        _playerDataList[i].DisconnectionTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected from the server.");
                        _playerDataList[i].NetworkConnection = default;
                        break;
                    }
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
                PlayerData playerData = _playerDataList[i];

                Vector3 moveDirection = new Vector3(
                    _playerDataList[i].ClientData.InputDirection.x,
                    0,
                    _playerDataList[i].ClientData.InputDirection.y
                    );
                playerData.GameObject.transform.Translate(moveDirection * Time.deltaTime);


                PlayerServerData playerServerData = _serverData.PlayerDataList.First(p => p.PlayerID == i);
                playerServerData.PlayerPosition = _playerDataList[i].GameObject.transform.position;


            }
        }
    }
}
