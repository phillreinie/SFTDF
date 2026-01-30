using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostRowUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text amountText;

    public void Bind(Sprite sprite, int amount)
    {
        if (icon != null) icon.sprite = sprite;
        if (amountText != null) amountText.text = amount.ToString();
    }
}