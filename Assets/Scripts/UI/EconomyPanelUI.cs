using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EconomyPanelUI : MonoBehaviour
{
    [Header("Data")]
    public ResourceDatabase resourceDatabase;

    [Header("UI")]
    public Transform resourceListRoot;
    public ResourceRowUI resourceRowPrefab;

    public TMP_Text headerText;
    public TMP_Text powerText;
    public TMP_Text producerText;

    [Header("Update Rate")]
    public float refreshInterval = 0.25f;

    private float _accum;
    private readonly List<ResourceRowUI> _rows = new();

    private void Start()
    {
        BuildRows();
        RefreshUI();
    }

    private void Update()
    {
        _accum += Time.deltaTime;
        if (_accum < refreshInterval) return;
        _accum = 0f;

        RefreshUI();
    }

    private void BuildRows()
    {
        for (int i = 0; i < _rows.Count; i++)
            if (_rows[i] != null) Destroy(_rows[i].gameObject);
        _rows.Clear();

        if (resourceDatabase == null || resourceListRoot == null || resourceRowPrefab == null) return;

        resourceDatabase.BuildIndex();

        foreach (var res in resourceDatabase.resources)
        {
            if (res == null) continue;

            var row = Instantiate(resourceRowPrefab, resourceListRoot);
            row.Bind(res.icon, res.displayName);
            _rows.Add(row);
        }
    }

    private void RefreshUI()
    {
        if (headerText != null) headerText.text = "ECONOMY";

        if (resourceDatabase != null)
        {
            for (int i = 0; i < resourceDatabase.resources.Count && i < _rows.Count; i++)
            {
                var res = resourceDatabase.resources[i];
                var row = _rows[i];
                if (res == null || row == null) continue;

                int amount = GameServices.Inventory != null ? GameServices.Inventory.Get(res.id) : 0;
                float rate = GameServices.InventoryRates != null ? GameServices.InventoryRates.GetRatePerSecond(res.id) : 0f;


                // NEW: cap
                int cap = int.MaxValue;
                if (GameServices.Inventory != null)
                    cap = GameServices.Inventory.GetCap(res.id);

                row.SetValues(amount, cap, rate);
            }
        }

        RefreshPowerSummary();
        RefreshProducerSummary();
    }

    private void RefreshPowerSummary()
    {
        int needPower = 0;
        int powered = 0;

        var bm = GameServices.Buildings;
        if (bm != null)
        {
            var all = bm.All;
            for (int i = 0; i < all.Count; i++)
            {
                var b = all[i];
                if (b == null || b.data == null) continue;

                if (b.data.powerDraw > 0)
                {
                    needPower++;
                    if (b.isPowered) powered++;
                }
            }
        }

        int unpowered = needPower - powered;
        if (powerText != null)
            powerText.text = $"POWER: Powered {powered}/{needPower}  (Unpowered {unpowered})";
    }

    private void RefreshProducerSummary()
    {
        int running = 0;
        int starved = 0;
        int inactive = 0;
        int blocked = 0; // OPTIONAL: show blocked too (caps cause this now)

        var bm = GameServices.Buildings;
        if (bm != null)
        {
            var all = bm.All;
            for (int i = 0; i < all.Count; i++)
            {
                var b = all[i];
                if (b == null || b.data == null) continue;
                if (b.data.category != BuildingCategory.Producer) continue;

                switch (b.productionState)
                {
                    case ProductionState.Running: running++; break;
                    case ProductionState.Starved: starved++; break;
                    case ProductionState.Blocked: blocked++; break;
                    default: inactive++; break;
                }
            }
        }

        if (producerText != null)
            producerText.text = $"PRODUCERS: Running {running}  Starved {starved}  Blocked {blocked}  Inactive {inactive}";
    }
}
