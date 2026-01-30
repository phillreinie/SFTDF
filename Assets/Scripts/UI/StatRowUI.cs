using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    public TMP_Text labelText;
    public TMP_Text valueText;

    public void Bind(string label, string value)
    {
        if (labelText != null) labelText.text = label;
        if (valueText != null) valueText.text = value;
    }
}