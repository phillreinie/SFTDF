public static class GameServices
{
    public static GridManager Grid { get; set; }
    public static BuildingManager Buildings { get; set; }
    public static InventoryService Inventory { get; set; }
    public static ResourceDatabase Resources { get; set; }
    
    public static EconomyRateTracker Rates { get; set; }
    
    public static PlayerStatsService PlayerStats { get; set; }
    

}