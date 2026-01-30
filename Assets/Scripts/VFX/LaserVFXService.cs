using UnityEngine;

public enum LaserChannel
{
    Player,
    Tower
}

public class LaserVFXService : MonoBehaviour
{
    public static LaserVFXService Instance { get; private set; }

    [Header("Player Laser")]
    public LaserVFXPool playerPool;
    public LaserStyle playerStyle;

    [Header("Tower Laser")]
    public LaserVFXPool towerPool;
    public LaserStyle towerStyle;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Fire(LaserChannel channel, Vector3 start, Vector3 end)
    {
        if (!gameObject.activeInHierarchy) return;

        LaserVFXPool pool = channel == LaserChannel.Player ? playerPool : towerPool;
        LaserStyle style = channel == LaserChannel.Player ? playerStyle : towerStyle;

        if (pool == null) return;

        var vfx = pool.Get();
        vfx.Play(start, end, style);
    }
}