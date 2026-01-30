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
    private Camera _cam;

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

}
