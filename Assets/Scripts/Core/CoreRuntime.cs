using UnityEngine;

[RequireComponent(typeof(Health))]
public class CoreRuntime : MonoBehaviour
{
    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _health.OnDeath += OnCoreDeath;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= OnCoreDeath;
    }

    private void OnCoreDeath(Health _)
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.TriggerGameOver();
    }
}