using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using UnityEngine;
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

            }
            else if (cmd == NetworkEvent.Type.Data)
            {

                if (stream.Length == 4)
                {
                    _clientData.PlayerID = stream.ReadInt();                    
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
                        playerSceneData.GameObject = GameObject.Instantiate(_configData.PlayerPrefab);
                        playerSceneData.ID = p.PlayerID;
                        _players.Add(playerSceneData);
                    }

                    playerSceneData.Direction = (p.PlayerPosition - playerSceneData.GameObject.transform.position);
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
}

