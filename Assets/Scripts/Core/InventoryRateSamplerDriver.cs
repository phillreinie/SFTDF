using UnityEngine;

public class InventoryRateSamplerDriver : MonoBehaviour
{
    [Tooltip("Window for displayed average rate.")]
    public float windowSeconds = 10f;

    [Tooltip("How often to sample inventory amounts.")]
    public float sampleIntervalSeconds = 1f;

    private void Awake()
    {
        // Create service if not created yet (safe)
        if (GameServices.InventoryRates == null)
            GameServices.InventoryRates = new InventoryRateSampler(windowSeconds, sampleIntervalSeconds);
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;
        GameServices.InventoryRates?.Tick();
    }
}