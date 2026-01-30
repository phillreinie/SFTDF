using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buildings/Storage")]
public class StorageData : BuildingData
{
    [Header("Storage")]
    public float linkRadiusTiles = 10f;
    public bool flushToGlobalInstantly = true;
}