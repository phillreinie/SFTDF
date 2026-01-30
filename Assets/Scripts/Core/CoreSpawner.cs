using UnityEngine;

public class CoreSpawner : MonoBehaviour
{
    public BuildingData coreBuildingData;

    private void Start()
    {
        var grid = GameServices.Grid;
        if (grid == null || coreBuildingData == null) return;

        Vector2Int center = grid.GetCenterCell();
        // Make sure it’s placeable; if blocked, search nearby ring
        if (!grid.CanPlace(center, coreBuildingData.footprint, coreBuildingData.requiredGroundId))
        {
            if (!TryFindNearbyPlaceable(center, 10, out center))
            {
                Debug.LogError("[CoreSpawner] Could not find placeable cell for core.");
                return;
            }
        }

        var worldPos = grid.GridToWorldCenter(center);
        var b = GameServices.Buildings.Spawn(coreBuildingData, center, worldPos);
        if (b == null) return;

        grid.Place(b, center, coreBuildingData.footprint);

        // Ensure it has CoreMarker (either on prefab or add)
        if (b.GetComponentInChildren<CoreMarker>() == null && b.GetComponent<CoreMarker>() == null)
            b.gameObject.AddComponent<CoreMarker>();
    }

    private bool TryFindNearbyPlaceable(Vector2Int start, int maxRadius, out Vector2Int found)
    {
        var grid = GameServices.Grid;
        for (int r = 1; r <= maxRadius; r++)
        {
            for (int x = -r; x <= r; x++)
            for (int y = -r; y <= r; y++)
            {
                if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue; // ring only
                var c = new Vector2Int(start.x + x, start.y + y);
                if (!grid.InBounds(c)) continue;

                if (grid.CanPlace(c, coreBuildingData.footprint, coreBuildingData.requiredGroundId))
                {
                    found = c;
                    return true;
                }
            }
        }

        found = start;
        return false;
    }
}