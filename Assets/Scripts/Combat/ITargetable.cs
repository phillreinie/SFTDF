using UnityEngine;

public interface ITargetable
{
    TargetType TargetType { get; }
    Transform TargetTransform { get; }
    bool IsAlive { get; }
}