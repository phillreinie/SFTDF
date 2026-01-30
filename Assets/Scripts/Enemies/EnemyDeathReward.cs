using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeathReward : MonoBehaviour
{
    public int worth = 1;

    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        _health.OnDeath += OnDied;
    }

    private void OnDisable()
    {
        _health.OnDeath -= OnDied;
    }

    private void OnDied(Health h)
    {
        if (GameServices.PlayerStats != null)
            GameServices.PlayerStats.AddSouls(worth);
    }
}