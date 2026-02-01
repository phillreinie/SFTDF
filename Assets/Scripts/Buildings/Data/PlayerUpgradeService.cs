// NEW: V2-3
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeService
{
    public event Action OnChanged;

    private readonly Dictionary<string, int> _levelsById = new(StringComparer.Ordinal);

    // Derived player modifiers (simple additive)
    public float MoveSpeedBonus { get; private set; }
    public float DamageBonus { get; private set; }
    public float FireRateBonus { get; private set; }
    public float RadiusBonus { get; private set; }

    public int GetLevel(PlayerUpgradeDef def)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.id)) return 0;
        return _levelsById.TryGetValue(def.id, out var lv) ? lv : 0;
    }

    public List<ResourceAmount> GetCostForNextLevel(PlayerUpgradeDef def)
    {
        var list = new List<ResourceAmount>();
        if (def == null) return list;

        int current = GetLevel(def);
        if (current >= def.maxLevel) return list;

        float mult = Mathf.Pow(def.costMultiplierPerLevel, current);

        for (int i = 0; i < def.baseCost.Count; i++)
        {
            var c = def.baseCost[i];
            if (string.IsNullOrWhiteSpace(c.resourceId) || c.amount <= 0) continue;

            int amt = Mathf.Max(1, Mathf.RoundToInt(c.amount * mult));
            list.Add(new ResourceAmount { resourceId = c.resourceId, amount = amt });
        }

        return list;
    }

    public bool CanBuyNext(PlayerUpgradeDef def)
    {
        if (def == null) return false;
        int current = GetLevel(def);
        if (current >= def.maxLevel) return false;

        var inv = GameServices.Inventory;
        if (inv == null) return false;

        var cost = GetCostForNextLevel(def);
        return inv.CanAfford(cost);
    }

    public bool TryBuyNext(PlayerUpgradeDef def)
    {
        if (def == null) return false;

        int current = GetLevel(def);
        if (current >= def.maxLevel) return false;

        var inv = GameServices.Inventory;
        if (inv == null) return false;

        var cost = GetCostForNextLevel(def);
        if (!inv.TrySpend(cost)) return false;

        // apply
        _levelsById[def.id] = current + 1;
        RecomputeModifiers();

        OnChanged?.Invoke();
        return true;
    }

    private void RecomputeModifiers()
    {
        MoveSpeedBonus = 0f;
        DamageBonus = 0f;
        FireRateBonus = 0f;
        RadiusBonus = 0f;

        // We need access to all upgrade defs to sum them.
        // We keep it simple: CoreUpgradePanel passes the list into SetKnownDefs once at startup.
        if (_knownDefs == null) return;

        for (int i = 0; i < _knownDefs.Count; i++)
        {
            var def = _knownDefs[i];
            if (def == null) continue;

            int lv = GetLevel(def);
            float total = lv * def.valuePerLevel;

            switch (def.type)
            {
                case PlayerUpgradeType.MoveSpeed: MoveSpeedBonus += total; break;
                case PlayerUpgradeType.Damage: DamageBonus += total; break;
                case PlayerUpgradeType.FireRate: FireRateBonus += total; break;
                case PlayerUpgradeType.Radius: RadiusBonus += total; break;
            }
            Debug.Log("total: " + total+ ", "+ def.type);
        }
    }

    private List<PlayerUpgradeDef> _knownDefs;

    public void SetKnownDefs(List<PlayerUpgradeDef> defs)
    {
        _knownDefs = defs;
        RecomputeModifiers();
    }
}
