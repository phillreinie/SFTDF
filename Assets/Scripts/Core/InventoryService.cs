// CHANGED: V2-0
using System;
using System.Collections.Generic;

public class InventoryService
{
    private readonly Dictionary<string, int> _amounts = new();

    public event Action OnChanged;

    public int Get(string resourceId) =>
        (resourceId != null && _amounts.TryGetValue(resourceId, out var v)) ? v : 0;

    // NEW: allow other systems to force UI refresh when caps change
    public void NotifyChanged() => OnChanged?.Invoke();

    // NEW: total cap = base cap (ResourceData) + storage bonus (CapacityService)
    public int GetCap(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            return int.MaxValue;

        int baseCap = int.MaxValue;

        var db = GameServices.Resources;
        if (db != null)
        {
            var r = db.Get(resourceId);
            if (r != null)
                baseCap = r.baseCap;
        }

        // If resource not in DB, treat as uncapped (safe fallback)
        if (baseCap == int.MaxValue)
            return int.MaxValue;

        int bonus = GameServices.Capacity != null ? GameServices.Capacity.GetExtraCap(resourceId) : 0;

        long total = (long)baseCap + bonus;
        if (total > int.MaxValue) return int.MaxValue;
        return (int)total;
    }

    public int GetFreeSpace(string resourceId)
    {
        int cap = GetCap(resourceId);
        if (cap == int.MaxValue) return int.MaxValue;

        int have = Get(resourceId);
        return Math.Max(0, cap - have);
    }

    public bool CanAdd(string resourceId, int amount)
    {
        if (string.IsNullOrWhiteSpace(resourceId) || amount <= 0) return true;

        int cap = GetCap(resourceId);
        if (cap == int.MaxValue) return true;

        return Get(resourceId) + amount <= cap;
    }

    public void Add(string resourceId, int amount)
    {
        if (string.IsNullOrWhiteSpace(resourceId) || amount <= 0) return;

        int current = Get(resourceId);
        int cap = GetCap(resourceId);

        int next = current + amount;

        // Clamp to cap if capped
        if (cap != int.MaxValue)
            next = Math.Min(next, cap);

        int appliedDelta = next - current;
        if (appliedDelta <= 0)
        {
            // Nothing changed (already full)
            OnChanged?.Invoke();
            return;
        }

        _amounts[resourceId] = next;

        TrackDelta(resourceId, appliedDelta);
        OnChanged?.Invoke();
    }

    public bool CanAfford(List<ResourceAmount> cost)
    {
        if (cost == null) return true;
        for (int i = 0; i < cost.Count; i++)
        {
            var c = cost[i];
            if (Get(c.resourceId) < c.amount) return false;
        }
        return true;
    }

    public bool TrySpend(List<ResourceAmount> cost)
    {
        if (!CanAfford(cost)) return false;

        for (int i = 0; i < cost.Count; i++)
        {
            var c = cost[i];
            _amounts[c.resourceId] = Get(c.resourceId) - c.amount;
            TrackDelta(c.resourceId, -c.amount);
        }

        OnChanged?.Invoke();
        return true;
    }

    public bool TryConsume(string resourceId, int amount)
    {
        if (string.IsNullOrWhiteSpace(resourceId) || amount <= 0) return true;

        int have = Get(resourceId);
        if (have < amount) return false;

        _amounts[resourceId] = have - amount;
        TrackDelta(resourceId, -amount);

        OnChanged?.Invoke();
        return true;
    }

    private void TrackDelta(string resourceId, int delta)
    {
        if (GameServices.Rates == null) return;
        GameServices.Rates.AddDeltaInt(resourceId, delta);
    }

    public IReadOnlyDictionary<string, int> GetAll() => _amounts; // for HUD later
}
