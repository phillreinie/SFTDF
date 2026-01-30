using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buildings/Power")]
public class PowerData : BuildingData
{
    [Header("Power")]
    public float powerRadiusTiles = 10f;
    public int powerOutputCapacity = 0; // 0 = infinite/simple
}