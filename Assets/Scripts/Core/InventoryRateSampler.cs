using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryRateSampler
{
    private struct Snapshot
    {
        public float time;
        public Dictionary<string, int> amounts;
    }

    public float WindowSeconds { get; private set; } = 10f;
    public float SampleIntervalSeconds { get; private set; } = 1f;

    private readonly Queue<Snapshot> _snapshots = new();
    private readonly Dictionary<string, float> _rates = new();

    public IReadOnlyDictionary<string, float> Rates => _rates;

    public InventoryRateSampler(float windowSeconds = 10f, float sampleIntervalSeconds = 1f)
    {
        WindowSeconds = Mathf.Max(1f, windowSeconds);
        SampleIntervalSeconds = Mathf.Clamp(sampleIntervalSeconds, 0.2f, windowSeconds);
    }

    public void Tick()
    {
        if (GameServices.Inventory == null || GameServices.Resources == null) return;

        float now = Time.time;

        // If we just sampled recently, skip
        if (_snapshots.Count > 0)
        {
            float lastTime = GetLastSnapshotTime();
            if (now - lastTime < SampleIntervalSeconds)
                return;
        }

        TakeSnapshot(now);
        PruneOld(now);
        RecomputeRates(now);
    }

    public float GetRatePerSecond(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId)) return 0f;
        return _rates.TryGetValue(resourceId, out var r) ? r : 0f;
    }

    private float GetLastSnapshotTime()
    {
        // Queue doesn't expose last; track by iterating (small count).
        float t = 0f;
        foreach (var s in _snapshots) t = s.time;
        return t;
    }

    private void TakeSnapshot(float now)
    {
        var dict = new Dictionary<string, int>(StringComparer.Ordinal);

        // Sample only resources in the DB (keeps it predictable / ordered)
        var db = GameServices.Resources;
        db.BuildIndex();

        foreach (var r in db.resources)
        {
            if (r == null || string.IsNullOrWhiteSpace(r.id)) continue;
            dict[r.id] = GameServices.Inventory.Get(r.id);
        }

        _snapshots.Enqueue(new Snapshot { time = now, amounts = dict });
    }

    private void PruneOld(float now)
    {
        while (_snapshots.Count > 0 && (now - _snapshots.Peek().time) > WindowSeconds)
            _snapshots.Dequeue();

        // Ensure we always have at least 2 snapshots to compute meaningful rates
        while (_snapshots.Count > 2)
        {
            // Keep earliest within window for better span stability
            // (No action; this is just a guard for very small windows)
            break;
        }
    }

    private void RecomputeRates(float now)
    {
        _rates.Clear();
        if (_snapshots.Count < 2) return;

        var oldest = _snapshots.Peek();
        Snapshot newest = default;
        foreach (var s in _snapshots) newest = s;

        float span = Mathf.Max(0.1f, newest.time - oldest.time);

        // Compute for all ids present in oldest snapshot
        foreach (var kv in oldest.amounts)
        {
            string id = kv.Key;

            int a0 = kv.Value;
            int a1 = newest.amounts.TryGetValue(id, out var v) ? v : a0;

            _rates[id] = (a1 - a0) / span;
        }
    }
}
