using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buildings/Defense Tower")]
public class DefenseTowerData : BuildingData
{
    public CombatStats combat;

    [Header("Ammo (V2)")]
    [Tooltip("Resource id consumed per shot. Example: res_ammo_basic")]
    public string ammoResourceId = "res_ammo_basic";

    [Min(0)]
    [Tooltip("How much ammo is consumed per shot. 0 = free (debug).")]
    public int ammoPerShot = 1;
}