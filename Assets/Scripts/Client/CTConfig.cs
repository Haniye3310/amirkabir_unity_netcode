using UnityEngine;

[CreateAssetMenu(fileName = "CTConfig", menuName = "ScriptableObjects/CTConfig")]
public class CTConfig : ScriptableObject
{
    [SerializeField] GameObject _playerPrefab;
    public GameObject PlayerPrefab => _playerPrefab;
    [SerializeField, Range(0.01f, 1)] float _clientUpdateInterval = 0.05f;
    public float ClientUpdateInterval => _clientUpdateInterval;
    [SerializeField] int _reconnectTimeout = 60;
    public int ReconnectTimeout => _reconnectTimeout;
}
