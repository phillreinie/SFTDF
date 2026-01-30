using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buildings/Producer")]
public class ProducerData : BuildingData
{
    [Header("Production")]
    [Tooltip("How many recipe cycles per second are attempted (before gating).")]
    public float cyclesPerSecond = 1f;

    [Tooltip("Inputs are consumed from global inventory; outputs are added to global inventory.")]
    public RecipeDef recipe = new();

    [Tooltip("If true, producer runs only when covered by at least one Storage.")]
    public bool requiresStorageLink = true;
}