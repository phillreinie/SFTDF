using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildHotbar : MonoBehaviour
{
    public BuildController buildController;

    [Header("Hotbar Buildings (order matters)")]
    public List<BuildingData> buildings = new();

    [Header("Settings")]
    [Tooltip("Maximum number of hotbar keys supported (1–9).")]
    public int maxKeys = 9;

    // NEW: notify UI / other systems
    public event Action OnHotbarChanged;

    private void Update()
    {
        if (buildController == null) return;

        int count = Mathf.Min(buildings.Count, maxKeys);

        for (int i = 0; i < count; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;

            if (Input.GetKeyDown(key))
            {
                AudioService.Instance?.Play(SfxEvent.UnlockBuilding);
                var b = buildings[i];
                if (b == null) continue;

                buildController.SetSelected(b);
                GameModeManager.Instance?.SetMode(GameMode.Build);
            }
        }
    }

    public bool IsFull() => buildings != null && buildings.Count >= maxKeys;

    public bool AddBuilding(BuildingData b)
    {
        if (b == null) return false;
        if (buildings == null) buildings = new List<BuildingData>();

        if (buildings.Contains(b)) return false;
        if (buildings.Count >= maxKeys) return false;

        buildings.Add(b);
        OnHotbarChanged?.Invoke();
        return true;
    }
}