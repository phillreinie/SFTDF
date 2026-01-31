// CHANGED: V2-0
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public GridManager gridManager;
    public BuildingManager buildingManager;

    [Header("Databases")]
    public ResourceDatabase resourceDatabase;

    [Header("Starting Inventory")]
    public int startingScrap = 200;
    public string scrapResourceId = "res_scrap";

    private void Awake()
    {
        GameServices.Grid = gridManager;
        GameServices.Buildings = buildingManager;

        GameServices.Resources = resourceDatabase;
        if (GameServices.Resources != null)
            GameServices.Resources.BuildIndex();

        GameServices.Inventory = new InventoryService();
        GameServices.Inventory.Add(scrapResourceId, startingScrap);

        GameServices.Rates = new EconomyRateTracker(10f);

        // NEW: Capacity service (must exist before production ticks clamp to caps)
        GameServices.Capacity = new ResourceCapacityService();

        // Optional: ensure UI refresh hooks when capacity changes
        GameServices.Capacity.OnCapacityChanged += () =>
        {
            // Amounts may not change, but caps do — we still want UI to update if it listens to inventory changes.
            GameServices.Inventory?.NotifyChanged();
        };

        gridManager.Init();
    }
}