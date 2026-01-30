using UnityEngine;

[RequireComponent(typeof(Health))]
public class SpawnerRuntime : MonoBehaviour
{
    [Header("Spawner")]
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public int maxAliveFromThisSpawner = 12;
    public float spawnRadius = 0.6f;

    [Header("Wiring")]
    public SpawnerRegistry registry;
    public WaveDirector director;
    
    [Header("Enemy Multipliers Per Wave (Percent)")]
    public float hpPercentPerWave = 0.05f;      // +5% each wave
    public float damagePercentPerWave = 0.03f;  // +3%
    public float speedPercentPerWave = 0.02f;   // +2%
    public float aggroPercentPerWave = 0.05f;   // +5%
    
    


    private Health _health;
    private float _timer;
    private int _aliveCount;
    private bool _isDead;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _health.OnDeath += OnKilled;
    }

    private void OnEnable()
    {
        if (registry != null) registry.Register(this);
    }

    private void OnDisable()
    {
        if (registry != null) registry.Unregister(this);
    }

    private void OnKilled(Health _)
    {
        if (_isDead) return;
        _isDead = true;
    }

    private void Update()
    {
        if (_isDead) return;
        if (enemyPrefab == null) return;

        if (_aliveCount >= maxAliveFromThisSpawner) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        _timer = spawnInterval;
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + (Vector3)offset;

        var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        _aliveCount++;

        if (director != null)
        {
            float hpMult = director.GetHPPercentMult();
            float dmgMult = director.GetDamagePercentMult();
            float spdMult = director.GetSpeedPercentMult();
            float aggroMult = director.GetAggroPercentMult();

            var h = go.GetComponent<Health>();
            if (h != null)
            {
                h.maxHP *= hpMult;
                h.currentHP = h.maxHP;
            }

            var melee = go.GetComponent<EnemyMeleeAttack>();
            if (melee != null)
                melee.damage *= dmgMult;

            var agent = go.GetComponent<EnemyAgent>();
            if (agent != null)
            {
                agent.moveSpeed *= spdMult;
                agent.aggroRadius *= aggroMult;
            }
        }

        var eh = go.GetComponent<Health>();
        if (eh != null)
            eh.OnDeath += _ => { _aliveCount = Mathf.Max(0, _aliveCount - 1); };
    }

    public void Configure(float interval, WaveDirector waveDirector, SpawnerRegistry spawnerRegistry)
    {
        spawnInterval = Mathf.Max(0.1f, interval);
        director = waveDirector;
        registry = spawnerRegistry;
    }


}
