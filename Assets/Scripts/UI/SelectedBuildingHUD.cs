using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedBuildingHUD : MonoBehaviour
{
    [Header("UI (basic)")]
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text countText;
    public TMP_Text modeText;

    [Header("UI (cost)")]
    public Transform costRoot;
    public CostRowUI costRowPrefab;

    [Header("UI (stats)")]
    public Transform statsRoot;
    public StatRowUI statRowPrefab;

    [Header("Inspect world building under mouse (grid-based)")]
    public bool inspectUnderMouse = true;
    public float inspectRefreshInterval = 0.10f;

    private readonly List<CostRowUI> _costRows = new();
    private readonly List<StatRowUI> _statRows = new();

    private BuildingData _current;
    private Camera _cam;

    private float _inspectTimer;
    private BuildingRuntime _inspected; // building under mouse

    private void OnEnable()
    {
        _cam = Camera.main;

        BuildSelectionEvents.OnSelectedChanged += OnSelectedChanged;
        BuildSelectionEvents.OnModeChanged += RefreshMode;

        if (GameServices.Inventory != null)
            GameServices.Inventory.OnChanged += RefreshCount;

        // initial state
        OnSelectedChanged(_current);
        RefreshMode(GameModeManager.Instance != null ? GameModeManager.Instance.Mode : GameMode.Combat);
    }

    private void OnDisable()
    {
        BuildSelectionEvents.OnSelectedChanged -= OnSelectedChanged;
        BuildSelectionEvents.OnModeChanged -= RefreshMode;

        if (GameServices.Inventory != null)
            GameServices.Inventory.OnChanged -= RefreshCount;
    }

    private void Update()
    {
        if (!inspectUnderMouse) return;
        if (_cam == null) return;
        if (GameServices.Grid == null) return;

        _inspectTimer -= Time.deltaTime;
        if (_inspectTimer > 0f) return;
        _inspectTimer = inspectRefreshInterval;

        UpdateWorldInspect();
    }

    // ---------------- Selection ----------------

    private void OnSelectedChanged(BuildingData b)
    {
        _current = b;

        if (b == null)
        {
            if (icon != null) icon.enabled = false;
            if (nameText != null) nameText.text = "";
            if (countText != null) countText.text = "";
            ClearCost();
            ClearStats();
            return;
        }

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = b.icon;
        }

        if (nameText != null)
            nameText.text = b.displayName;

        RefreshCount();
        RebuildCost(b);
        RebuildStats(b);
    }

    private void RefreshMode(GameMode mode)
    {
        if (modeText != null)
            modeText.text = mode == GameMode.Build ? "BUILD" : "COMBAT";
    }

    private void RefreshCount()
    {
        if (_current == null || countText == null) return;

        int canBuild = ComputePlaceableCount(_current);
        countText.text = $"Can build: {canBuild}";
    }

    private int ComputePlaceableCount(BuildingData b)
    {
        if (b == null || b.buildCost == null || b.buildCost.Count == 0) return 999;

        int min = int.MaxValue;

        for (int i = 0; i < b.buildCost.Count; i++)
        {
            var cost = b.buildCost[i];
            if (string.IsNullOrEmpty(cost.resourceId) || cost.amount <= 0) continue;

            int have = GameServices.Inventory.Get(cost.resourceId);
            int possible = have / cost.amount;

            if (possible < min) min = possible;
        }

        return min == int.MaxValue ? 0 : min;
    }

    // ---------------- COST ----------------

    private void ClearCost()
    {
        for (int i = 0; i < _costRows.Count; i++)
            if (_costRows[i] != null) Destroy(_costRows[i].gameObject);
        _costRows.Clear();
    }

    private void RebuildCost(BuildingData b)
    {
        ClearCost();
        if (b == null || b.buildCost == null) return;
        if (costRoot == null || costRowPrefab == null) return;

        var db = GameServices.Resources;
        if (db != null) db.BuildIndex();

        for (int i = 0; i < b.buildCost.Count; i++)
        {
            var c = b.buildCost[i];
            if (string.IsNullOrEmpty(c.resourceId) || c.amount <= 0) continue;

            Sprite spr = null;
            if (db != null)
            {
                var r = db.Get(c.resourceId);
                if (r != null) spr = r.icon;
            }

            var row = Instantiate(costRowPrefab, costRoot);
            row.Bind(spr, c.amount);
            _costRows.Add(row);
        }
    }

    // ---------------- STATS ----------------

    private void ClearStats()
    {
        for (int i = 0; i < _statRows.Count; i++)
            if (_statRows[i] != null) Destroy(_statRows[i].gameObject);
        _statRows.Clear();
    }

    private void AddStat(string label, string value)
    {
        if (statsRoot == null || statRowPrefab == null) return;
        var row = Instantiate(statRowPrefab, statsRoot);
        row.Bind(label, value);
        _statRows.Add(row);
    }

    private void RebuildStats(BuildingData b)
    {
        ClearStats();
        if (b == null) return;

        // --- Data (SO) ---
        if (b.powerDraw > 0)
            AddStat("Power draw", $"{b.powerDraw}");

        if (b.requiredGroundId != 0)
            AddStat("Placement", $"Requires node {b.requiredGroundId}");

        // Producer (recipe-based)
        if (b is ProducerData pd)
        {
            AddStat("Cycles", $"{pd.cyclesPerSecond:0.##}/s");
            AddStat("Storage", pd.requiresStorageLink ? "Required" : "Not required");

            var db = GameServices.Resources;
            if (db != null) db.BuildIndex();

            // Inputs
            if (pd.recipe != null && pd.recipe.inputs != null && pd.recipe.inputs.Count > 0)
            {
                for (int i = 0; i < pd.recipe.inputs.Count; i++)
                {
                    var ra = pd.recipe.inputs[i];
                    if (string.IsNullOrEmpty(ra.resourceId) || ra.amount <= 0) continue;

                    string n = ResolveName(ra.resourceId, db);
                    float perSec = ra.amount * pd.cyclesPerSecond;

                    AddStat("Intake", $"{n}  -{perSec:0.##}/s");
                }
            }
            else AddStat("Intake", "None");

            // Outputs
            if (pd.recipe != null && pd.recipe.outputs != null && pd.recipe.outputs.Count > 0)
            {
                for (int i = 0; i < pd.recipe.outputs.Count; i++)
                {
                    var ra = pd.recipe.outputs[i];
                    if (string.IsNullOrEmpty(ra.resourceId) || ra.amount <= 0) continue;

                    string n = ResolveName(ra.resourceId, db);
                    float perSec = ra.amount * pd.cyclesPerSecond;

                    AddStat("Output", $"{n}  +{perSec:0.##}/s");
                }
            }
            else AddStat("Output", "None");
        }

        // --- Runtime (world under mouse) ---
        // We append runtime state info here when we have a hovered building.
        AppendRuntimeState();
    }

    private void AppendRuntimeState()
    {
        if (_inspected == null) return;

        AddStat("State", _inspected.productionState.ToString());
        AddStat("Powered", _inspected.isPowered ? "Yes" : "No");
        AddStat("Storage link", _inspected.isStorageLinked ? "Yes" : "No");
    }

    // ---------------- World Inspect (GRID) ----------------

    private void UpdateWorldInspect()
    {
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        var grid = GameServices.Grid;
        var cell = grid.WorldToGrid(mouseWorld);

        var occ = grid.GetOccupantAt(cell); // returns BuildingRuntime (in your project)
        if (occ == _inspected) return;

        _inspected = occ;

        // Rebuild stats so runtime lines update
        if (_current != null)
            RebuildStats(_current);
        else
        {
            // If nothing selected, still show runtime-only info (optional)
            ClearStats();
            AppendRuntimeState();
        }
    }

    private string ResolveName(string resourceId, ResourceDatabase db)
    {
        if (db == null) return resourceId;
        var r = db.Get(resourceId);
        return r != null ? r.displayName : resourceId;
    }
}
