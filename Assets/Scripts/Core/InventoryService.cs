using System;
using System.Collections.Generic;

public class InventoryService
{
    private readonly Dictionary<string, int> _amounts = new();

    public event Action OnChanged;

    public int Get(string resourceId) =>
        (resourceId != null && _amounts.TryGetValue(resourceId, out var v)) ? v : 0;

    public void Add(string resourceId, int amount)
    {
        if (string.IsNullOrWhiteSpace(resourceId) || amount <= 0) return;
        _amounts[resourceId] = Get(resourceId) + amount;

        TrackDelta(resourceId, amount);


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