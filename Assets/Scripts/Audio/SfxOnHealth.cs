using UnityEngine;

[RequireComponent(typeof(Health))]
public class SfxOnHealth : MonoBehaviour
{
    public SfxEvent onHit = SfxEvent.HitEnemy;
    public SfxEvent onDeath = SfxEvent.EnemyDeath;

    private Health _h;

    private void Awake() => _h = GetComponent<Health>();

    private void OnEnable()
    {
        _h.OnHealthChanged += OnChanged;
        _h.OnDeath += OnDied;
    }

    private void OnDisable()
    {
        _h.OnHealthChanged -= OnChanged;
        _h.OnDeath -= OnDied;
    }

    private void OnChanged(Health h, float delta)
    {
        if (delta < 0f)
            AudioService.Instance?.Play(onHit);
    }

    private void OnDied(Health _)
    {
        AudioService.Instance?.Play(onDeath);
    }
}