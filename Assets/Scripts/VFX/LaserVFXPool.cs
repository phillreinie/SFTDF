using System.Collections.Generic;
using UnityEngine;

public class LaserVFXPool : MonoBehaviour
{
    public LaserShotVFX prefab;
    public int prewarm = 20;

    private readonly Queue<LaserShotVFX> _pool = new();

    private void Awake()
    {
        for (int i = 0; i < prewarm; i++)
            _pool.Enqueue(Create());
    }

    private LaserShotVFX Create()
    {
        var v = Instantiate(prefab, transform);
        v.Init(this);
        v.gameObject.SetActive(false);
        return v;
    }

    public LaserShotVFX Get()
    {
        // If empty, grow automatically (no “run out” ever)
        var v = _pool.Count > 0 ? _pool.Dequeue() : Create();
        return v;
    }

    public void Recycle(LaserShotVFX v)
    {
        if (v == null) return;

        v.gameObject.SetActive(false);
        _pool.Enqueue(v);
    }
}