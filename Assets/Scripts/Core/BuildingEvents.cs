using System;

public static class BuildingEvents
{
    public static event Action<BuildingRuntime> Placed;
    public static event Action<BuildingRuntime> Removed;

    public static void RaisePlaced(BuildingRuntime b)
    {
        UnityEngine.Debug.Log($"[BuildingEvents] Placed: {b?.name}");
        Placed?.Invoke(b);
    }

    public static void RaiseRemoved(BuildingRuntime b)
    {
        UnityEngine.Debug.Log($"[BuildingEvents] Removed: {b?.name}");
        Removed?.Invoke(b);
    }

}