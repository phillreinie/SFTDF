using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public GameObject playerPrefab;
    public Transform respawnPoint;
    public float respawnDelay = 1.5f;

    private GameObject _current;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SpawnPlayer();
    }

    public void RegisterPlayer(GameObject player, Health health)
    {
        _current = player;
        if (health != null)
            health.OnDeath += _ => OnPlayerDied();
    }

    private void OnPlayerDied()
    {
        Invoke(nameof(SpawnPlayer), respawnDelay);
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null || respawnPoint == null) return;
        var go = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        var health = go.GetComponent<Health>();
        RegisterPlayer(go, health);
    }
}