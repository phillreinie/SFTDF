using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildHotbar : MonoBehaviour
{
    public BuildController buildController;

    [Header("All unlocked buildings (big list)")]
    public List<BuildingData> buildings = new();

    [Header("Hotbar Settings")]
    [Range(1, 12)]
    public int slotsPerPage = 9;

    [Header("Input")]
    public bool enableNumberKeys = true;
    public bool enableScrollSelection = true;
    public bool altScrollPages = true;

    public int CurrentPage { get; private set; }
    public int CurrentIndexGlobal { get; private set; } = -1;

    public event Action OnHotbarChanged; // list/page changes
    public event Action<int> OnPageChanged; // page index

    private void Update()
    {
        if (buildController == null) return;

        // Number keys select within page
        if (enableNumberKeys)
            HandleNumberKeys();

        if (enableScrollSelection)
            HandleScroll();
    }

    private void HandleNumberKeys()
    {
        int countOnPage = GetCountOnPage(CurrentPage);

        for (int i = 0; i < countOnPage; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (!Input.GetKeyDown(key)) continue;

            SelectPageSlot(i);
        }
    }

    private void HandleScroll()
    {
        float s = Input.mouseScrollDelta.y;
        if (Mathf.Abs(s) < 0.001f) return;

        bool alt = altScrollPages && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

        if (alt)
        {
            int dir = s > 0 ? -1 : 1;  // scroll up -> previous page (feel free to invert)
            SetPage(CurrentPage + dir);
            return;
        }

        // Normal scroll = move selection by 1 global slot
        int step = s > 0 ? -1 : 1; // scroll up -> previous (feel free to invert)
        StepSelection(step);
    }

    public void StepSelection(int step)
    {
        int total = buildings != null ? buildings.Count : 0;
        if (total <= 0) return;

        // If nothing selected yet, start at 0
        if (CurrentIndexGlobal < 0) CurrentIndexGlobal = 0;

        CurrentIndexGlobal = Mod(CurrentIndexGlobal + step, total);

        // Update page to match selection
        int newPage = CurrentIndexGlobal / slotsPerPage;
        if (newPage != CurrentPage)
            SetPage(newPage, silentSelection: true);

        // Select it
        var b = buildings[CurrentIndexGlobal];
        if (b != null)
        {
            buildController.SetSelected(b);
            GameModeManager.Instance?.SetMode(GameMode.Build);
        }

        OnHotbarChanged?.Invoke();
    }

    public void SelectPageSlot(int indexOnPage)
    {
        int global = CurrentPage * slotsPerPage + indexOnPage;
        if (global < 0 || global >= buildings.Count) return;

        CurrentIndexGlobal = global;

        var b = buildings[global];
        if (b == null) return;

        buildController.SetSelected(b);
        GameModeManager.Instance?.SetMode(GameMode.Build);

        OnHotbarChanged?.Invoke();
    }

    public void SetPage(int page, bool silentSelection = false)
    {
        int totalPages = GetTotalPages();
        if (totalPages <= 0) { CurrentPage = 0; return; }

        page = Mathf.Clamp(page, 0, totalPages - 1);
        if (page == CurrentPage) return;

        CurrentPage = page;
        OnPageChanged?.Invoke(CurrentPage);

        // Keep selection if it belongs to the new page; otherwise snap to first slot on page
        if (!silentSelection)
        {
            int start = CurrentPage * slotsPerPage;
            if (buildings.Count > start)
            {
                CurrentIndexGlobal = start;
                var b = buildings[start];
                if (b != null)
                {
                    buildController.SetSelected(b);
                    GameModeManager.Instance?.SetMode(GameMode.Build);
                }
            }
        }

        OnHotbarChanged?.Invoke();
    }

    public int GetTotalPages()
    {
        int total = buildings != null ? buildings.Count : 0;
        if (total == 0) return 0;
        return Mathf.CeilToInt(total / (float)slotsPerPage);
    }

    public int GetCountOnPage(int page)
    {
        if (buildings == null) return 0;
        int start = page * slotsPerPage;
        if (start >= buildings.Count) return 0;
        return Mathf.Min(slotsPerPage, buildings.Count - start);
    }

    public List<BuildingData> GetBuildingsOnCurrentPage()
    {
        var list = new List<BuildingData>();
        int count = GetCountOnPage(CurrentPage);
        int start = CurrentPage * slotsPerPage;

        for (int i = 0; i < count; i++)
            list.Add(buildings[start + i]);

        return list;
    }

    private int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }

    // Call this when you unlock a building
    public void AddBuilding(BuildingData b)
    {
        if (b == null) return;
        if (buildings.Contains(b)) return;

        buildings.Add(b);
        OnHotbarChanged?.Invoke();
    }
}
