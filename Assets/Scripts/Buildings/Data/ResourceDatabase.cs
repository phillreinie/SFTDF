using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Resources/Resource Database")]
public class ResourceDatabase : ScriptableObject
{
    public List<ResourceData> resources = new();

    [NonSerialized] private Dictionary<string, ResourceData> _map;

    public void BuildIndex()
    {
        _map = new Dictionary<string, ResourceData>(StringComparer.Ordinal);

        foreach (var r in resources)
        {
            if (r == null) continue;
            if (string.IsNullOrWhiteSpace(r.id)) continue;

            _map[r.id] = r;
        }
    }

    public ResourceData Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        if (_map == null || _map.Count == 0)
            BuildIndex();

        return _map.TryGetValue(id, out var r) ? r : null;
    }
}