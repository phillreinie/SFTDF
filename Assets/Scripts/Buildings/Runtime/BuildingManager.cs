using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public Transform buildingsRoot;

    private readonly List<BuildingRuntime> _all = new();
    public IReadOnlyList<BuildingRuntime> All => _all;

    public BuildingRuntime Spawn(BuildingData data, Vector2Int origin, Vector3 worldPos)
    {
        if (data == null || data.runtimePrefab == null)
        {
            Debug.LogError($"BuildingData '{data?.name}' missing runtimePrefab.");
            return null;
        }

        var inst = Instantiate(data.runtimePrefab, worldPos, Quaternion.identity, buildingsRoot);
        inst.Init(data, origin, worldPos);

        _all.Add(inst);
        return inst;
    }

    public void Despawn(BuildingRuntime b)
    {
        if (b == null) return;
        _all.Remove(b);
        Destroy(b.gameObject);
    }
}