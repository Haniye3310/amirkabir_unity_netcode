using UnityEngine;

[CreateAssetMenu(fileName = "SRConfig", menuName = "ScriptableObjects/SRConfig")]
public class SRConfig : ScriptableObject
{
    [SerializeField] GameObject _playerPrefab;
    public GameObject PlayerPrefab => _playerPrefab;

    [SerializeField, Range(0.01f, 1)] float _serverUpdateInterval = 0.05f;
    public float ServerUpdateInterval => _serverUpdateInterval;
    [SerializeField] int _reconnectTimeout = 60;
    public int ReconnectTimeout => _reconnectTimeout;
}
