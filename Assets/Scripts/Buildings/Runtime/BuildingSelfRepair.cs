using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(BuildingRuntime))]
public class BuildingSelfRepair : MonoBehaviour
{
    private Health _health;
    private BuildingRuntime _runtime;

    private float _timeSinceDamage;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _runtime = GetComponent<BuildingRuntime>();

        _health.OnDamaged += OnDamaged;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(Health h, float amount)
    {
        _timeSinceDamage = 0f;
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;
        if (_runtime == null || _runtime.data == null) return;

        // Only repair if below max
        if (_health.currentHP >= _health.maxHP) return;

        _timeSinceDamage += Time.deltaTime;

        float delay = _runtime.data.repairDelaySeconds;
        if (_timeSinceDamage < delay) return;

        float pctPerSec = _runtime.data.repairPercentPerSecond;
        if (pctPerSec <= 0f) return;

        float healPerSec = _health.maxHP * pctPerSec;
        _health.Heal(healPerSec * Time.deltaTime);
    }
}