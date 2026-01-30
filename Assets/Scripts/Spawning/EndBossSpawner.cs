using UnityEngine;

[RequireComponent(typeof(Health))]
public class EndBossSpawner : MonoBehaviour
{
    [Header("Spawner Prefab To Spawn")]
    public SpawnerRuntime spawnerPrefab;

    [Header("Wiring")]
    public SpawnerRegistry registry;
    public WaveDirector director;

    [Header("Spawn Rules")]
    public float spawnSpawnerEverySeconds = 45f;
    public int maxSpawnedSpawners = 8;
    public int spawnRingRadiusTiles = 25;
    public int placementAttempts = 12;

    private float _timer;
    private int _spawnedCount;
    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _health.OnDeath += _ => { /* marker handles win elsewhere */ };
        _timer = spawnSpawnerEverySeconds;
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;
        if (spawnerPrefab == null) return;
        if (_spawnedCount >= maxSpawnedSpawners) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        _timer = spawnSpawnerEverySeconds;
        TrySpawnSpawner();
    }

    private void TrySpawnSpawner()
    {
        var grid = GameServices.Grid;
        if (grid == null) return;

        Vector2Int origin = grid.WorldToGrid(transform.position);

        for (int i = 0; i < placementAttempts; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector2Int cell = origin + new Vector2Int(
                Mathf.RoundToInt(dir.x * spawnRingRadiusTiles),
                Mathf.RoundToInt(dir.y * spawnRingRadiusTiles)
            );

            // jitter
            cell += new Vector2Int(Random.Range(-2, 3), Random.Range(-2, 3));

            if (!grid.InBounds(cell)) continue;

            // spawner footprint assumed 1x1 for V1
            if (!grid.CanPlace(cell, Vector2Int.one, 0)) continue;

            Vector3 world = grid.GridToWorldCenter(cell);
            var s = Instantiate(spawnerPrefab, world, Quaternion.identity);

            // wire it
            s.registry = registry;
            s.director = director;

            // ensure it uses current director interval (but keeps its own enemyPrefab)
            if (director != null)
                s.spawnInterval = Mathf.Max(s.spawnInterval, director.minSpawnInterval);

            _spawnedCount++;
            return;
        }

        // If it can’t place, we just skip this cycle (V1 rule)
    }
}
