using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("Regen (optional)")]
    public bool regenEnabled = false;
    public float regenDelaySeconds = 20f;
    [Range(0f, 0.25f)]
    public float regenPercentPerSecond = 0.02f; // 2% of max HP per second

    [Header("World Healthbar (optional)")]
    public HealthBarWorldUI healthBarPrefab;
    public Transform healthBarAnchor;
    public Vector3 healthBarOffset = new Vector3(0f, 0.6f, 0f);

    // Existing events (keep compatibility)
    public System.Action<Health> OnDeath;
    public System.Action<Health, float> OnDamaged;

    // New unified change event: delta > 0 heal, delta < 0 damage
    public System.Action<Health, float> OnHealthChanged;

    private float _regenDelayTimer;
    
    public static System.Action<Health> OnAnyDeath;

    private void Awake()
    {
        currentHP = maxHP;

        if (healthBarPrefab != null)
        {
            var bar = Instantiate(healthBarPrefab);
            bar.Bind(this, healthBarAnchor, healthBarOffset);
        }

        // initial broadcast (delta 0)
        OnHealthChanged?.Invoke(this, 0f);
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;

        if (!regenEnabled) return;
        if (currentHP <= 0f) return;
        if (currentHP >= maxHP) return;

        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= Time.deltaTime;
            return;
        }

        float healPerSecond = maxHP * regenPercentPerSecond;
        float amount = healPerSecond * Time.deltaTime;
        Heal(amount);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        if (currentHP <= 0f) return;

        currentHP -= amount;

        // Start / refresh regen delay on any damage
        if (regenEnabled)
            _regenDelayTimer = regenDelaySeconds;

        OnDamaged?.Invoke(this, amount);
        OnHealthChanged?.Invoke(this, -amount);

        if (currentHP <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        if (currentHP <= 0f) return;

        float before = currentHP;
        currentHP = Mathf.Min(maxHP, currentHP + amount);

        float delta = currentHP - before;
        if (delta > 0f)
            OnHealthChanged?.Invoke(this, delta);
    }

    private void Die()
    {
        OnAnyDeath?.Invoke(this);
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}

