using UnityEngine;

[CreateAssetMenu(menuName = "Game/VFX/Laser Style")]
public class LaserStyle : ScriptableObject
{
    [Header("Beam")]
    public float width = 0.08f;
    public float beamLifetime = 0.06f;

    [Header("Light Flash")]
    public float lightLifetime = 0.09f;
    public float lightIntensity = 1.2f;
    public float lightOuterRadius = 1.5f;
}