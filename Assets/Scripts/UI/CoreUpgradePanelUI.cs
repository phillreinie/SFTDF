// NEW: V2-3
using System.Collections.Generic;
using UnityEngine;

public class CoreUpgradePanelUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;                 // panel root (enable/disable)
    public Transform listRoot;              // content parent
    public UpgradeButtonUI buttonPrefab;    // your button prefab

    [Header("Upgrades")]
    public List<PlayerUpgradeDef> upgrades = new();

    private readonly List<UpgradeButtonUI> _spawned = new();
    
    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    private void OnEnable()
    {
        // ensure service knows defs (so bonuses compute)
        GameServices.PlayerUpgrades?.SetKnownDefs(upgrades);

        if (GameServices.PlayerUpgrades != null)
            GameServices.PlayerUpgrades.OnChanged += Refresh;

        if (GameServices.Inventory != null)
            GameServices.Inventory.OnChanged += Refresh;
    }

    private void OnDisable()
    {
        if (GameServices.PlayerUpgrades != null)
            GameServices.PlayerUpgrades.OnChanged -= Refresh;

        if (GameServices.Inventory != null)
            GameServices.Inventory.OnChanged -= Refresh;
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    private void Refresh()
    {
        if (root != null && !root.activeSelf) return;
        RebuildButtons();
    }

    private void RebuildButtons()
    {
        ClearButtons();

        if (buttonPrefab == null || listRoot == null) return;
        if (GameServices.PlayerUpgrades == null) return;

        for (int i = 0; i < upgrades.Count; i++)
        {
            var def = upgrades[i];
            if (def == null) continue;

            int level = GameServices.PlayerUpgrades.GetLevel(def);
            var cost = GameServices.PlayerUpgrades.GetCostForNextLevel(def);
            bool canBuy = GameServices.PlayerUpgrades.CanBuyNext(def);

            var b = Object.Instantiate(buttonPrefab, listRoot);
            b.Bind(def, level, def.maxLevel, cost, canBuy, OnUpgradeClicked);
            _spawned.Add(b);
        }
    }

    private void ClearButtons()
    {
        for (int i = 0; i < _spawned.Count; i++)
            if (_spawned[i] != null) Object.Destroy(_spawned[i].gameObject);
        _spawned.Clear();
    }

    private void OnUpgradeClicked(PlayerUpgradeDef def)
    {
        if (def == null) return;

        bool ok = GameServices.PlayerUpgrades.TryBuyNext(def);
        if (!ok)
        {
            // reuse any deny sound you already have
            AudioService.Instance?.Play(SfxEvent.BuildDeny);
        }

        Refresh();
    }


}
