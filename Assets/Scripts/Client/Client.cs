using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Contains client side scene data (such as gameobject) for each player.
/// </summary>
public class PlayerSceneData
{
    public int ID;
    public GameObject GameObject;
    public Vector3 Direction;
}

public class Client:MonoBehaviour
{
    NetworkDriver _driver;
    NetworkConnection _connection;
    [SerializeField] CTConfig _configData;
    private ClientData _clientData = new ClientData();
    private ServerData _latestServerData = new ServerData();
    List<PlayerSceneData> _players = new List<PlayerSceneData>();
    float _timerCounter;
    const string ID_KEY = "ID";
    const string LAST_PACKET_TIME = "LastPacketTime";
    void Start()
    {
        _driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        _connection = _driver.Connect(endpoint);
    }

    void OnDestroy()
    {
        _driver.Dispose();
    }

    void Update()
    {
        _driver.ScheduleUpdate().Complete();

        if (!_connection.IsCreated)
        {
            return;
        }

        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                if (PlayerPrefs.HasKey(ID_KEY))
                {
                    _clientData.PlayerID = PlayerPrefs.GetInt(ID_KEY);
                    // Send existing PlayerID so server knows it’s the same player
                    _driver.BeginSend(_connection, out var writer);
                    writer.WriteInt(_clientData.PlayerID);
                    _driver.EndSend(writer);
                }
                else
                {
                    _driver.BeginSend(_connection, out var writer);
                    writer.WriteInt(-1);
                    _driver.EndSend(writer);
                }

            }
            else if (cmd == NetworkEvent.Type.Data)
            {

                if (stream.Length == 4)
                {
                    _clientData.PlayerID = stream.ReadInt();
                    PlayerPrefs.SetInt(ID_KEY, _clientData.PlayerID);
                }
                else
                {
                    _latestServerData.ReadFromStream(ref stream);
                }

                foreach (PlayerServerData p in _latestServerData.PlayerDataList)
                {
                    PlayerSceneData playerSceneData = _players.FirstOrDefault(x => x.ID == p.PlayerID);

                    if (playerSceneData == null)
                    {
                        playerSceneData = new PlayerSceneData();
                        playerSceneData.GameObject = GameObject.Instantiate(_configData.PlayerPrefab,p.PlayerPosition,Quaternion.identity);
                        playerSceneData.ID = p.PlayerID;
                        _players.Add(playerSceneData);
                    }

                    playerSceneData.Direction = (p.PlayerPosition - playerSceneData.GameObject.transform.position);
                }
                PlayerPrefs.SetInt(LAST_PACKET_TIME, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
            if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected. Attempting to reconnect...");
                int lastPacketTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (PlayerPrefs.HasKey(LAST_PACKET_TIME))
                    lastPacketTime = PlayerPrefs.GetInt(LAST_PACKET_TIME);
                _connection = default;
                if (lastPacketTime + _configData.ReconnectTimeout * 1000 > (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
                    _connection = _driver.Connect(endpoint);
                }
                else
                {
                    CleanupClient();
                }
            }

        }

        _clientData.InputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (_timerCounter + _configData.ClientUpdateInterval < Time.time)
        {
            _driver.BeginSend(_connection, out var writer);
            _clientData.WriteToStream(ref writer);
            _driver.EndSend(writer);
            _timerCounter = Time.time;
        }

        foreach (PlayerSceneData p in _players)
        {
            p.GameObject.transform.Translate(p.Direction * Time.deltaTime);

        }
    }
    void CleanupClient()
    {
        // Dispose the driver and reset connection
        if (_driver.IsCreated)
            _driver.Dispose();

        _connection = default;
        PlayerPrefs.DeleteKey(ID_KEY);
        PlayerPrefs.DeleteKey(LAST_PACKET_TIME);
        Application.Quit();
    }
}

