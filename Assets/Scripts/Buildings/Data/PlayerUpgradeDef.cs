// NEW: V2-3
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrades/Player Upgrade")]
public class PlayerUpgradeDef : ScriptableObject
{
    [Header("Identity")]
    public string id = "upg_player_movespeed";
    public string displayName = "Move Speed";
    [TextArea] public string description = "Move faster.";

    [Header("Upgrade")]
    public PlayerUpgradeType type = PlayerUpgradeType.MoveSpeed;
    [Min(1)] public int maxLevel = 10;

    [Tooltip("Flat value added per level (ex: +0.25 move speed per level).")]
    public float valuePerLevel = 0.25f;

    [Header("Cost")]
    public List<ResourceAmount> baseCost = new(); // uses your struct
    [Min(1f)] public float costMultiplierPerLevel = 1.35f;
}