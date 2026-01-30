using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public BuildingCategory category;
    public Tier tier;
    public Sprite icon;

    [Header("Grid")]
    public Vector2Int footprint = Vector2Int.one;

    [Header("Durability")]
    public int maxHP = 500;

    [Header("Power")]
    public int powerDraw = 0;

    [Header("Utility Radius (if applicable)")]
    public float radiusTiles = 0f;

    [Header("Internal Stacks")]
    public List<StackDef> inputStacks = new();
    public List<StackDef> outputStacks = new();

    [Header("Build Cost (global inventory)")]
    public List<ResourceAmount> buildCost = new();

    [Header("Self-Repair")]
    public float repairDelaySeconds = 20f;
    [Range(0f, 0.25f)] public float repairPercentPerSecond = 0.02f;

    [Header("Prefab (Option A)")]
    public BuildingRuntime runtimePrefab;
    
    [Header("Placement Constraints")]
    public int requiredGroundId = 0;
    
    public Sprite ghostSpriteOverride;

}