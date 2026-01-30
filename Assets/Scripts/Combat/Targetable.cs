using UnityEngine;

[RequireComponent(typeof(Health))]
public class Targetable : MonoBehaviour, ITargetable
{
    public TargetType targetType;

    private Health _health;

    public TargetType TargetType => targetType;
    public Transform TargetTransform => transform;
    public bool IsAlive => _health != null && _health.currentHP > 0f;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }
}