using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unlocks/Building Pool")]
public class BuildingPoolData : ScriptableObject
{
    public Tier tier = Tier.T1;
    public List<BuildingData> candidates = new();
}