using System.Collections.Generic;
using UnityEngine;

public class BuildingUnlockManager : MonoBehaviour
{
    [Header("Target Hotbar")]
    public BuildHotbar hotbar;

    [Header("Timing")]
    [Tooltip("Unlock interval in minutes.")]
    public float unlockEveryMinutes = 3f;

    [Header("Tier Pools (ordered)")]
    public List<BuildingPoolData> tierPools = new();

    [Header("Tier Progression")]
    [Tooltip("Every N unlocks, advance to next tier pool (if available).")]
    public int unlocksPerTier = 3;

    [Header("Start Unlocks")]
    [Tooltip("If true, consider buildings already in hotbar as unlocked (recommended).")]
    public bool seedUnlockedFromHotbar = true;

    private float _timer;
    private int _unlockCountTotal;

    private readonly HashSet<BuildingData> _unlocked = new();

    private void Start()
    {
        _timer = unlockEveryMinutes * 60f;

        if (seedUnlockedFromHotbar && hotbar != null && hotbar.buildings != null)
        {
            for (int i = 0; i < hotbar.buildings.Count; i++)
            {
                var b = hotbar.buildings[i];
                if (b != null) _unlocked.Add(b);
            }
        }
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;
        if (hotbar == null) return;
        if (tierPools == null || tierPools.Count == 0) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        _timer = unlockEveryMinutes * 60f;
        TryUnlockOne();
    }

    private void TryUnlockOne()
    {
        int tierIndex = Mathf.Min(tierPools.Count - 1, _unlockCountTotal / Mathf.Max(1, unlocksPerTier));

        // Find first tier (from current upwards) that still has an available candidate
        List<BuildingData> available = null;

        for (int t = tierIndex; t < tierPools.Count; t++)
        {
            var pool = tierPools[t];
            if (pool == null || pool.candidates == null || pool.candidates.Count == 0) continue;

            available = BuildAvailable(pool);
            if (available.Count > 0) break;
        }

        if (available == null || available.Count == 0) return;

        var picked = available[Random.Range(0, available.Count)];
        _unlocked.Add(picked);
        _unlockCountTotal++;

        hotbar.AddBuilding(picked);

        Debug.Log($"[Unlock] Unlocked {picked.displayName} (Tier {picked.tier})");
    }

    private List<BuildingData> BuildAvailable(BuildingPoolData pool)
    {
        var list = new List<BuildingData>();
        for (int i = 0; i < pool.candidates.Count; i++)
        {
            var b = pool.candidates[i];
            if (b == null) continue;
            if (_unlocked.Contains(b)) continue;
            list.Add(b);
        }
        return list;
    }
}
