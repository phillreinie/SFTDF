using UnityEngine;

public class BuildingRuntime : MonoBehaviour
{
    public BuildingData data;
    [HideInInspector] public Vector2Int originCell;

    [Header("Link State")]
    public bool isPowered;
    public bool isStorageLinked;

    [Header("Production Runtime")]
    public float productionCycleAccum;
    
    [Header("Production State")]
    public ProductionState productionState;


    public void Init(BuildingData d, Vector2Int cell, Vector3 worldPos)
    {
        data = d;
        originCell = cell;
        transform.position = worldPos;
        name = $"Building_{d.displayName}_{cell.x}_{cell.y}";
    }
}