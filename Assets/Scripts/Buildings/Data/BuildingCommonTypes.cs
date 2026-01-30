using System;
using UnityEngine;

public enum Tier { T1 = 1, T2, T3, T4 }

public enum BuildingCategory
{
    Producer,
    Storage,
    Power,
    DefenseTower,
    DefenseWall,
    UnitProducer,
    Core
}

[Serializable]
public struct ResourceAmount
{
    public string resourceId;  // e.g. "res_scrap"
    public int amount;
}

[Serializable]
public struct StackDef
{
    public string resourceId;  // single-type stacks in V1
    public int capacity;
}

[Serializable]
public struct CombatStats
{
    public float rangeTiles;
    public float damage;
    public float shotsPerSecond;
}