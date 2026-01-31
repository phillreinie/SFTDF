using UnityEngine;

public class BuildingHighlight : MonoBehaviour
{
    [SerializeField] private GameObject highlightGO;

    private void Reset()
    {
        // convenience: if not set, assume this GO is the highlight itself
        if (highlightGO == null) highlightGO = gameObject;
    }

    public void Set(bool on)
    {
        if (highlightGO == null) return;
        if (highlightGO.activeSelf != on)
            highlightGO.SetActive(on);
    }
}