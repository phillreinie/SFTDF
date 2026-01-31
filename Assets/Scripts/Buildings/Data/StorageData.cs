// CHANGED: V2-0
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buildings/Storage")]
public class StorageData : BuildingData
{
    [Header("Storage")]
    public float linkRadiusTiles = 10f;
    public bool flushToGlobalInstantly = true;

    [Header("V2-0: Global Capacity")]
    [Tooltip("Flat bonus capacity added to ALL resources per placed Storage building.")]
    public int capacityBonusAllResources = 200;
}