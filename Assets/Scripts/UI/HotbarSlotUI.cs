using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [HideInInspector] public BuildingData data;

    [Header("UI")]
    public Image background;
    public Image icon;
    public TMP_Text keyText;

    [Header("Highlight")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.25f);
    public Color selectedColor = new Color(1f, 1f, 1f, 0.65f);

    private int _index;
    private HotbarHUD _hud;
    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
    }

    public void Bind(int index, BuildingData building, HotbarHUD hud)
    {
        _index = index;
        data = building;
        _hud = hud;

        if (icon != null) icon.sprite = building.icon != null ? building.icon : null;
        if (keyText != null) keyText.text = (index + 1).ToString();

        if (_btn != null)
        {
            _btn.onClick.RemoveAllListeners();
            _btn.onClick.AddListener(() => _hud.SelectIndex(_index));
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? selectedColor : normalColor;
    }
}