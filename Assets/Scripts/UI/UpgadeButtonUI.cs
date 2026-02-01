// NEW: V2-3
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text descText;
    public TMP_Text levelText;
    public TMP_Text costText;
    public Button button;

    private PlayerUpgradeDef _def;
    private System.Action<PlayerUpgradeDef> _onClick;

    public void Bind(PlayerUpgradeDef def, int level, int maxLevel, List<ResourceAmount> cost, bool canBuy, System.Action<PlayerUpgradeDef> onClick)
    {
        _def = def;
        _onClick = onClick;

        if (titleText != null) titleText.text = def != null ? def.displayName : "Upgrade";
        if (descText != null) descText.text = def != null ? def.description : "";

        if (levelText != null)
            levelText.text = $"Lv {level}/{maxLevel}";

        if (costText != null)
            costText.text = BuildCostString(cost);

        if (button != null)
        {
            button.interactable = canBuy && level < maxLevel;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onClick?.Invoke(_def));
        }
    }

    private string BuildCostString(List<ResourceAmount> cost)
    {
        if (cost == null || cost.Count == 0) return "MAX";

        // Simple: "Scrap x50, Iron x10"
        var db = GameServices.Resources;
        if (db != null) db.BuildIndex();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < cost.Count; i++)
        {
            var c = cost[i];
            if (string.IsNullOrWhiteSpace(c.resourceId) || c.amount <= 0) continue;

            string name = c.resourceId;
            if (db != null)
            {
                var r = db.Get(c.resourceId);
                if (r != null) name = r.displayName;
            }

            if (sb.Length > 0) sb.Append(", ");
            sb.Append(name).Append(" x").Append(c.amount);
        }

        return sb.Length == 0 ? "-" : sb.ToString();
    }
}
