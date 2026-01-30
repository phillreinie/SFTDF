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

    private float _timer;
    private int _unlockCountTotal;

    private readonly HashSet<BuildingData> _unlocked = new();

    private void Start()
    {
        _timer = unlockEveryMinutes * 60f;
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
        if (hotbar.IsFull())
            return; // V1: just stop adding when full

        int tierIndex = Mathf.Min(tierPools.Count - 1, _unlockCountTotal / Mathf.Max(1, unlocksPerTier));
        var pool = tierPools[tierIndex];
        if (pool == null || pool.candidates == null || pool.candidates.Count == 0) return;

        // Build list of available candidates (not yet unlocked)
        var available = new List<BuildingData>();
        for (int i = 0; i < pool.candidates.Count; i++)
        {
            var b = pool.candidates[i];
            if (b == null) continue;
            if (_unlocked.Contains(b)) continue;
            available.Add(b);
        }

        // If current tier exhausted, try higher tiers (optional fallback)
        int fallbackIndex = tierIndex;
        while (available.Count == 0 && fallbackIndex < tierPools.Count - 1)
        {
            fallbackIndex++;
            pool = tierPools[fallbackIndex];
            if (pool == null) continue;

            for (int i = 0; i < pool.candidates.Count; i++)
            {
                var b = pool.candidates[i];
                if (b == null) continue;
                if (_unlocked.Contains(b)) continue;
                available.Add(b);
            }
        }

        if (available.Count == 0) return;

        var picked = available[Random.Range(0, available.Count)];
        _unlocked.Add(picked);
        _unlockCountTotal++;

        hotbar.AddBuilding(picked);

        Debug.Log($"[Unlock] Unlocked {picked.displayName} (Tier {picked.tier})");
    }
}
