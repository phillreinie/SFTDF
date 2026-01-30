using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class GroundNodeDef
{
    public int groundId;         // 1=Iron, 2=Copper, 3=Stone...
    public string displayName;
    public TileBase tile;
    [Range(0f, 1f)] public float rarityWeight = 1f; // lower = rarer
}

[CreateAssetMenu(menuName = "Game/World/Ground Node Database")]
public class GroundNodeDatabase : ScriptableObject
{
    public List<GroundNodeDef> nodes = new();

    public GroundNodeDef Get(int id)
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].groundId == id) return nodes[i];
        return null;
    }
}