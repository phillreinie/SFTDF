using UnityEngine;

public class PlayerRespawnHandler : MonoBehaviour
{
    public Transform respawnPoint;
    public float respawnDelay = 1.5f;

    private Health _health;
    private bool _respawning;

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health != null)
            _health.OnDeath += OnDeath;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= OnDeath;
    }

    private void OnDeath(Health h)
    {
        if (_respawning) return;
        _respawning = true;

        // Spawn a new player instance (simple V1)
        // NOTE: This requires the player prefab to exist in scene as the template.
        // We’ll do this by reloading a prefab reference later; for now, simplest is:
        Invoke(nameof(RespawnInScene), respawnDelay);
    }

    private void RespawnInScene()
    {
        // Since this component is destroyed with the player, this won't run.
        // So for Milestone 5, we keep death as "player destroyed" and skip respawn.
        // Respawn will be implemented properly in Milestone 9 with a GameManager.
    }
}