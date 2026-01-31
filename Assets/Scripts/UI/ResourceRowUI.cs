using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceRowUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text amountText;
    public TMP_Text rateText;

    [Header("Cap Warning Colors")]
    [Range(0.5f, 0.99f)]
    public float warnThreshold = 0.90f; // 90% full

    public Color normalAmountColor = Color.white;
    public Color warnAmountColor = new Color(1f, 0.85f, 0.25f, 1f); // yellow
    public Color fullAmountColor = new Color(1f, 0.35f, 0.35f, 1f); // red
    public Color positiveRate = new Color(1f, 0.35f, 0.35f, 1f); // red

    float _smoothedRate;

    public void Bind(Sprite sprite, string displayName)
    {
        if (icon != null) icon.sprite = sprite;
        if (nameText != null) nameText.text = displayName;

        // Ensure default color on bind
        if (amountText != null) amountText.color = normalAmountColor;
    }

    public void SetValues(int amount, int cap, float netPerSecond)
    {
        // Amount / cap display
        if (amountText != null)
        {
            if (cap <= 0 || cap == int.MaxValue)
                amountText.text = amount.ToString();
            else
                amountText.text = $"{amount}/{cap}";

            // NEW: tint when near/full
            ApplyCapTint(amount, cap);
        }

        // Smooth rate display slightly
        _smoothedRate = Mathf.Lerp(_smoothedRate, netPerSecond, 0.25f);

        if (rateText != null)
        {
            string sign = _smoothedRate > 0.0001f ? "+" : "";
            rateText.text = $"{sign}{_smoothedRate:0.0}/s";
            if (rateText != null)
            {
                if (_smoothedRate > 0.0001f) rateText.color = positiveRate;
                else if (_smoothedRate < -0.0001f) rateText.color = fullAmountColor;
                else rateText.color = warnAmountColor;
            }
        }
    }

    // Backward compatible overload (if anything still uses it)
    public void SetValues(int amount, float netPerSecond)
    {
        SetValues(amount, int.MaxValue, netPerSecond);
    }

    private void ApplyCapTint(int amount, int cap)
    {
        if (amountText == null) return;

        // If uncapped, keep normal color
        if (cap <= 0 || cap == int.MaxValue)
        {
            amountText.color = normalAmountColor;
            return;
        }

        if (cap == 0)
        {
            amountText.color = fullAmountColor;
            return;
        }

        float t = amount / (float)cap;

        if (t >= 1f - 0.0001f) amountText.color = fullAmountColor;
        else if (t >= warnThreshold) amountText.color = warnAmountColor;
        else amountText.color = normalAmountColor;
    }
}
