using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceRowUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text amountText;
    public TMP_Text rateText;
    float _smoothedRate;

    public void Bind(Sprite sprite, string displayName)
    {
        if (icon != null) icon.sprite = sprite;
        if (nameText != null) nameText.text = displayName;
    }

    public void SetValues(int amount, float netPerSecond)
    {
        if (amountText != null) amountText.text = amount.ToString();

        // Smooth rate display slightly
        _smoothedRate = Mathf.Lerp(_smoothedRate, netPerSecond, 0.25f);

        if (rateText != null)
        {
            string sign = _smoothedRate > 0.0001f ? "+" : "";
            rateText.text = $"{sign}{_smoothedRate:0.0}/s";
        }
    }
}