using System.Collections.Generic;
using UnityEngine;

public class EconomyRateTracker
{
    private struct Sample
    {
        public float time;
        public int delta;
    }

    private readonly Dictionary<string, Queue<Sample>> _history = new();
    private readonly float _windowSeconds;

    public EconomyRateTracker(float windowSeconds = 10f)
    {
        _windowSeconds = Mathf.Max(1f, windowSeconds);
    }

    // API stays: AddDelta(resourceId, delta)
    public void AddDelta(string resourceId, float deltaPerSecond)
    {
        // We now interpret this as an instantaneous delta amount, not "per second".
        // Keep signature to avoid breaking old calls, but we’ll pass ints from Inventory.
        AddDeltaInt(resourceId, Mathf.RoundToInt(deltaPerSecond));
    }

    // Use this from Inventory (preferred)
    public void AddDeltaInt(string resourceId, int delta)
    {
        if (string.IsNullOrEmpty(resourceId) || delta == 0) return;

        if (!_history.TryGetValue(resourceId, out var q))
        {
            q = new Queue<Sample>();
            _history[resourceId] = q;
        }

        float now = Time.time;
        q.Enqueue(new Sample { time = now, delta = delta });
        Prune(q, now);
    }

public float GetNetPerSecond(string resourceId)
{
    if (!_history.TryGetValue(resourceId, out var q) || q.Count == 0) return 0f;

    float now = Time.time;
    Prune(q, now);
    if (q.Count == 0) return 0f;

    int sum = 0;
    foreach (var s in q) sum += s.delta;

    // Use the real time span covered by the samples (up to windowSeconds)
    float oldest = q.Peek().time;
    float span = Mathf.Clamp(now - oldest, 0.1f, _windowSeconds);

    return sum / span;
}


    public void Clear() => _history.Clear();

    private void Prune(Queue<Sample> q, float now)
    {
        while (q.Count > 0 && (now - q.Peek().time) > _windowSeconds)
            q.Dequeue();
    }
}