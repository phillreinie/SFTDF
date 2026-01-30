using UnityEngine;

public class ProductionStateVisualizer : MonoBehaviour
{
    public BuildingRuntime runtime;
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (runtime == null) runtime = GetComponent<BuildingRuntime>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (runtime == null || spriteRenderer == null) return;

        // No custom colors locked long-term — this is debug-only.
        // For now we use subtle tints.
        if (runtime.productionState == ProductionState.Running)
            spriteRenderer.color = Color.white;
        else if (runtime.productionState == ProductionState.Starved)
            spriteRenderer.color = new Color(1f, 1f, 0.6f);
        else // Inactive / Blocked
            spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f);
    }
}