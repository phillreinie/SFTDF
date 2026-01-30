using System.Collections.Generic;
using UnityEngine;

public class LinkSystem : MonoBehaviour
{
    private GridManager Grid => GameServices.Grid;
    private BuildingManager Buildings => GameServices.Buildings;

    private void OnEnable()
    {
        Debug.Log("[LinkSystem] OnEnable subscribe");
        BuildingEvents.Placed += OnBuildingChanged;
        BuildingEvents.Removed += OnBuildingChanged;
    }

    private void OnDisable()
    {
        BuildingEvents.Placed -= OnBuildingChanged;
        BuildingEvents.Removed -= OnBuildingChanged;
    }

    private void Start()
    {
        Debug.Log("[LinkSystem] Start -> RefreshAll");
        RefreshAll();
    }

    private void OnBuildingChanged(BuildingRuntime b)
    {
        Debug.Log($"[LinkSystem] BuildingChanged: {b?.name} -> RefreshAll()");
        RefreshAll();
    }

    public void RefreshAll()
    {
        if (Grid == null || Buildings == null) return;

        var list = Buildings.All;
        for (int i = 0; i < list.Count; i++)
            RefreshOne(list[i], list);
    }

    private void RefreshOne(BuildingRuntime b, IReadOnlyList<BuildingRuntime> all)
    {
        if (b == null || b.data == null) return;

        bool powered = IsCoveredByPower(b, all);
        bool linked = IsCoveredByStorage(b, all);

        // Temp debug (remove later)
        if (b.isPowered != powered || b.isStorageLinked != linked)
            Debug.Log($"[LinkSystem] {b.name} cat={b.data.category} -> P:{powered} S:{linked}");

        b.isPowered = powered;
        b.isStorageLinked = linked;
    }

    private bool IsCoveredByPower(BuildingRuntime b, IReadOnlyList<BuildingRuntime> all)
    {
        Vector3 p = b.transform.position;

        for (int i = 0; i < all.Count; i++)
        {
            var other = all[i];
            if (other == null || other == b || other.data == null) continue;
            if (other.data.category != BuildingCategory.Power) continue;

            float radiusTiles = other.data.radiusTiles;
            if (other.data is PowerData pd) radiusTiles = pd.powerRadiusTiles;

            // If both are 0, this power building can never power anyone
            if (radiusTiles <= 0f) continue;

            float rWorld = radiusTiles * Grid.cellSize;
            if (Vector2.Distance(p, other.transform.position) <= rWorld)
                return true;
        }
        return false;
    }

    private bool IsCoveredByStorage(BuildingRuntime b, IReadOnlyList<BuildingRuntime> all)
    {
        Vector3 p = b.transform.position;

        for (int i = 0; i < all.Count; i++)
        {
            var other = all[i];
            if (other == null || other == b || other.data == null) continue;
            if (other.data.category != BuildingCategory.Storage) continue;

            float radiusTiles = other.data.radiusTiles;
            if (other.data is StorageData sd) radiusTiles = sd.linkRadiusTiles;

            if (radiusTiles <= 0f) continue;

            float rWorld = radiusTiles * Grid.cellSize;
            if (Vector2.Distance(p, other.transform.position) <= rWorld)
                return true;
        }
        return false;
    }
}
