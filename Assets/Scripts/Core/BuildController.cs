using System.Collections.Generic;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [Header("Selected Building SO")]
    public BuildingData selectedBuilding;

    [Header("Ghost Prefab (BuildingRuntime)")]
    public BuildingRuntime ghostPrefab;

    [Header("Ghost Tint")]
    public Color canPlaceColor = new Color(0.2f, 1f, 0.2f, 0.65f);
    public Color cannotPlaceColor = new Color(1f, 0.2f, 0.2f, 0.65f);

    private bool _buildMode;
    private BuildingRuntime _ghost;
    private SpriteRenderer _ghostSR;
    private RadiusRingRenderer _ghostRing;
    
    private Camera _cam;
    private readonly List<BuildingHighlight> _activeHighlights = new();


    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        if (ghostPrefab != null)
        {
            _ghost = Instantiate(ghostPrefab);
            _ghostSR = _ghost.GetComponentInChildren<SpriteRenderer>();
            _ghostRing = _ghost.GetComponentInChildren<RadiusRingRenderer>(true);
            _ghost.gameObject.SetActive(false);
        }
    }

    public void SetBuildMode(bool enabled)
    {
        _buildMode = enabled;

        // Build mode without selection: hide ghost
        UpdateGhostVisibility();
    }

    public void ClearSelection()
    {
        selectedBuilding = null;
        BuildSelectionEvents.OnSelectedChanged?.Invoke(null);
        UpdateGhostVisibility();
    }

    private void Update()
    {
        if (!_buildMode) return;
        if (selectedBuilding == null || _ghost == null) { UpdateGhostVisibility(); return; }

        UpdateGhostVisibility();

        var mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        var grid = GameServices.Grid;
        var cell = grid.WorldToGrid(mouseWorld);

        _ghost.transform.position = grid.GridToWorldCenter(cell);

        bool canPlace =
            grid.CanPlace(cell, selectedBuilding.footprint, selectedBuilding.requiredGroundId) &&
            GameServices.Inventory.CanAfford(selectedBuilding.buildCost);

        // Ghost sprite: match selected building runtime sprite if possible
        ApplyGhostSprite(selectedBuilding);
        ApplyGhostRadius(selectedBuilding);
        UpdatePlacementHighlights(cell);


        // Tint feedback
        if (_ghostSR != null)
            _ghostSR.color = canPlace ? canPlaceColor : cannotPlaceColor;

        // LMB: place
        if (Input.GetMouseButtonDown(0))
        {
            if (TryPlace(cell))
            {
                // After placing, clear selection + return to combat
                ClearSelection();
                ClearHighlights();

                GameModeManager.Instance?.SetMode(GameMode.Combat);
            }
            else
            {
                AudioService.Instance?.Play(SfxEvent.BuildDeny);
            }
        }

        // RMB: delete if building present; else cancel build
        if (Input.GetMouseButtonDown(1))
        {
            var occ = grid.GetOccupantAt(cell);
            if (occ != null)
            {
                TryRemove(cell);
                AudioService.Instance?.Play(SfxEvent.BuildDelete);
            }
            else
            {
                
                // cancel placement
                ClearSelection();
                GameModeManager.Instance?.SetMode(GameMode.Combat);
                ClearHighlights();
            }
        }
    }

    private void UpdateGhostVisibility()
    {
        if (_ghost == null) return;

        bool visible = _buildMode && selectedBuilding != null;
        if (_ghost.gameObject.activeSelf != visible)
            _ghost.gameObject.SetActive(visible);
    }

    private void ApplyGhostSprite(BuildingData data)
    {
        if (_ghostSR == null || data == null) return;

        // Prefer explicit ghost sprite
        if (data.ghostSpriteOverride != null)
        {
            _ghostSR.sprite = data.ghostSpriteOverride;
            return;
        }

        // Else try pull from prefab
        if (data.runtimePrefab != null)
        {
            var sr = data.runtimePrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) _ghostSR.sprite = sr.sprite;
        }
    }

    private bool TryPlace(Vector2Int origin)
    {
        var grid = GameServices.Grid;

        if (!grid.CanPlace(origin, selectedBuilding.footprint, selectedBuilding.requiredGroundId)) return false;
        if (!GameServices.Inventory.TrySpend(selectedBuilding.buildCost)) return false;

        var worldPos = grid.GridToWorldCenter(origin);
        var b = GameServices.Buildings.Spawn(selectedBuilding, origin, worldPos);
        if (b == null) return false;

        grid.Place(b, origin, selectedBuilding.footprint);
        BuildingEvents.RaisePlaced(b);
        return true;
    }

    private void TryRemove(Vector2Int cell)
    {
        var grid = GameServices.Grid;
        var occ = grid.GetOccupantAt(cell);
        if (occ == null) return;
        if (occ.GetComponentInChildren<CoreMarker>() != null) return;


        var footprint = occ.data.footprint;
        var origin = occ.originCell;

        grid.Remove(occ, origin, footprint);
        GameServices.Buildings.Despawn(occ);
        BuildingEvents.RaiseRemoved(occ);
    }
    public void SetSelected(BuildingData b)
    {
        selectedBuilding = b;
        BuildSelectionEvents.OnSelectedChanged?.Invoke(selectedBuilding);
        UpdateGhostVisibility();
    }
    
    private void ApplyGhostRadius(BuildingData data)
    {
        if (_ghostRing == null || data == null) return;

        float r = 0f;
        if (data is StorageData sd) r = sd.linkRadiusTiles;
        else if (data is PowerData pd) r = pd.powerRadiusTiles;
        else if (data is DefenseTowerData td) r = td.combat.rangeTiles;
        else r = data.radiusTiles;

        if (r <= 0.01f)
        {
            _ghostRing.SetVisible(false);
            return;
        }

        _ghostRing.SetVisible(true);
        _ghostRing.SetRadiusTiles(r);
    }
    
    private void ClearHighlights()
    {
        for (int i = 0; i < _activeHighlights.Count; i++)
            _activeHighlights[i]?.Set(false);

        _activeHighlights.Clear();
    }
    
    private void UpdatePlacementHighlights(Vector2Int originCell)
    {
        ClearHighlights();
        Debug.Log("[BuildController] calling UpdatePlacementHighlights");

        if (selectedBuilding == null) return;

        float radiusTiles = 0f;
        bool affectsPower = false;
        bool affectsStorage = false;

        if (selectedBuilding is PowerData pd)
        {
            radiusTiles = pd.powerRadiusTiles;
            affectsPower = true;
        }
        else if (selectedBuilding is StorageData sd)
        {
            radiusTiles = sd.linkRadiusTiles;
            affectsStorage = true;
        }
        else
        {
            return;
        }

        if (radiusTiles <= 0f) return;

        float radiusWorld = radiusTiles * GameServices.Grid.cellSize;
        Vector3 center = GameServices.Grid.GridToWorldCenter(originCell);

        int candidates = 0;
        int lit = 0;

        var buildings = GameServices.Buildings.All;
        for (int i = 0; i < buildings.Count; i++)
        {
            var b = buildings[i];
            if (b == null || b.data == null) continue;

            // Filter
            if (affectsPower && b.data.powerDraw <= 0) continue;
            if (affectsStorage && b.data.category != BuildingCategory.Producer) continue;

            candidates++;

            float d = Vector2.Distance(center, b.transform.position);
            if (d > radiusWorld) continue;

            var hl = b.GetComponentInChildren<BuildingHighlight>(true);
            if (hl == null) continue;

            hl.Set(true);
            _activeHighlights.Add(hl);
            lit++;
        }

        Debug.Log($"[Highlights] buildings={buildings.Count}, candidates={candidates}, lit={lit}");

    }



}
