using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [Header("References")]
    public SpawnerRegistry registry;
    public GameObject enemyPrefab;

    [Header("Wave Timing (overlapping)")]
    public float waveInterval = 20f;
    public float waveIntervalRamp = -0.25f;
    public float minWaveInterval = 8f;

    [Header("Spawner Scaling")]
    public float startSpawnInterval = 3.5f;
    public float spawnIntervalRamp = -0.15f;
    public float minSpawnInterval = 0.8f;

    [Header("Enemy Scaling")]
    public float enemyHPStartMultiplier = 1f;
    public float enemyHPMultiplierPerWave = 0.05f;
    
    [Header("Enemy Multipliers Per Wave (Percent)")]
    public float hpPercentPerWave = 0.05f;      // +5% each wave
    public float damagePercentPerWave = 0.03f;  // +3%
    public float speedPercentPerWave = 0.02f;   // +2%
    public float aggroPercentPerWave = 0.05f;   // +5%


    private float _waveTimer;
    private int _waveIndex;

    private void Start()
    {
        _waveTimer = waveInterval;
        ApplyToAllSpawners();
    }

    private void Update()
    {
        _waveTimer -= Time.deltaTime;
        if (_waveTimer > 0f) return;

        StartNextWave();
    }

    private void StartNextWave()
    {
        _waveIndex++;

        waveInterval = Mathf.Max(minWaveInterval, waveInterval + waveIntervalRamp);
        _waveTimer = waveInterval;

        ApplyToAllSpawners();
    }

    private void ApplyToAllSpawners()
    {
        if (registry == null || enemyPrefab == null) return;

        float newInterval = Mathf.Max(minSpawnInterval, startSpawnInterval + spawnIntervalRamp * _waveIndex);

        var spawners = registry.Spawners;
        for (int i = 0; i < spawners.Count; i++)
        {
            var s = spawners[i];
            if (s == null) continue;

            s.Configure(newInterval, this, registry);
        }
    }

    public float GetEnemyHPMultiplier()
    {
        return enemyHPStartMultiplier + enemyHPMultiplierPerWave * _waveIndex;
    }
    public float GetHPPercentMult() => 1f + hpPercentPerWave * _waveIndex;
    public float GetDamagePercentMult() => 1f + damagePercentPerWave * _waveIndex;
    public float GetSpeedPercentMult() => 1f + speedPercentPerWave * _waveIndex;
    public float GetAggroPercentMult() => 1f + aggroPercentPerWave * _waveIndex;

}