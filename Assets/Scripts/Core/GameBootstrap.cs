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
        

        gridManager.Init();
    }
}