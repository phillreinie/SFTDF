using System.Collections.Generic;
using UnityEngine;

public class SpawnerRegistry : MonoBehaviour
{
    private readonly List<SpawnerRuntime> _spawners = new();
    public IReadOnlyList<SpawnerRuntime> Spawners => _spawners;

    public void Register(SpawnerRuntime s)
    {
        if (s == null) return;
        if (_spawners.Contains(s)) return;
        _spawners.Add(s);
    }

    public void Unregister(SpawnerRuntime s)
    {
        if (s == null) return;
        _spawners.Remove(s);
    }
}