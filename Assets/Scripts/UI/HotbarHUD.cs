using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarHUD : MonoBehaviour
{
    [Header("Refs")]
    public BuildHotbar hotbar;
    public BuildController buildController;

    [Header("UI")]
    public Transform slotParent;
    public HotbarSlotUI slotPrefab;

    private readonly List<HotbarSlotUI> _slots = new();

    private void Start()
    {
        Rebuild();

        if (hotbar != null)
            hotbar.OnHotbarChanged += Rebuild;

        BuildSelectionEvents.OnSelectedChanged += OnSelectedChanged;
    }

    private void OnDestroy()
    {
        if (hotbar != null)
            hotbar.OnHotbarChanged -= Rebuild;

        BuildSelectionEvents.OnSelectedChanged -= OnSelectedChanged;
    }


    public void Rebuild()
    {
        if (hotbar == null || slotParent == null || slotPrefab == null) return;

        // clear
        for (int i = 0; i < _slots.Count; i++)
            if (_slots[i] != null) Destroy(_slots[i].gameObject);

        _slots.Clear();

        var pageList = hotbar.GetBuildingsOnCurrentPage();
        int count = Mathf.Min(pageList.Count, hotbar.slotsPerPage);

        for (int i = 0; i < count; i++)
        {
            var b = pageList[i];

            if (b == null) continue;

            var slot = Instantiate(slotPrefab, slotParent);
            slot.Bind(i, b, this);
            _slots.Add(slot);
        }
        
        RefreshSelectionVisuals();
    }

    public void SelectIndex(int index)
    {
        if (hotbar == null || buildController == null) return;
        if (index < 0 || index >= hotbar.buildings.Count) return;

        var b = hotbar.buildings[index];
        if (b == null) return;

        buildController.SetSelected(b);
        GameModeManager.Instance?.SetMode(GameMode.Build);
        
        hotbar.SelectPageSlot(index);
        RefreshSelectionVisuals();
    }

    private void OnSelectedChanged(BuildingData _)
    {
        RefreshSelectionVisuals();
    }

    private void RefreshSelectionVisuals()
    {
        var selected = buildController != null ? buildController.selectedBuilding : null;

        for (int i = 0; i < _slots.Count; i++)
        {
            var s = _slots[i];
            if (s == null) continue;
            s.SetSelected(selected != null && s.data == selected);
        }
    }
}
