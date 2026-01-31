// NEW: V2-0
// Computes global extra resource capacity from placed Storage buildings.
// Flat rule (locked for now): each Storage adds +X capacity to ALL resources.

using System;
using UnityEngine;

public class ResourceCapacityService
{
    public event Action OnCapacityChanged;

    // Flat bonus applied to ALL resources
    public int ExtraCapAllResources { get; private set; }

    public ResourceCapacityService()
    {
        // Rebuild when buildings change
        BuildingEvents.Placed += OnBuildingChanged;
        BuildingEvents.Removed += OnBuildingChanged;

        RebuildFromWorld();
    }

    public void Dispose()
    {
        BuildingEvents.Placed -= OnBuildingChanged;
        BuildingEvents.Removed -= OnBuildingChanged;
    }

    private void OnBuildingChanged(BuildingRuntime _)
    {
        int before = ExtraCapAllResources;
        RebuildFromWorld();

        if (before != ExtraCapAllResources)
            OnCapacityChanged?.Invoke();
    }

    public void RebuildFromWorld()
    {
        ExtraCapAllResources = 0;

        var bm = GameServices.Buildings;
        if (bm == null) return;

        var all = bm.All;
        for (int i = 0; i < all.Count; i++)
        {
            var b = all[i];
            if (b == null || b.data == null) continue;

            if (b.data.category != BuildingCategory.Storage) continue;
            if (b.data is not StorageData sd) continue;

            if (sd.capacityBonusAllResources > 0)
                ExtraCapAllResources += sd.capacityBonusAllResources;
        }
    }

    public int GetExtraCap(string resourceId)
    {
        // Flat rule: same bonus for every resource
        return ExtraCapAllResources;
    }
}