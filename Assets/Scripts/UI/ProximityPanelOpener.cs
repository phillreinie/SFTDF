using UnityEngine;

public class ProximityPanelOpener : MonoBehaviour
{
    [Header("Target Panel")]
    public CoreUpgradePanelUI panel;

    [Header("Player")]
    public Transform player;

    [Header("Distance")]
    [Tooltip("Distance in WORLD units. If you want tiles, enable useTiles.")]
    public float openDistance = 3f;

    [Tooltip("If true, openDistance is interpreted as tiles.")]
    public bool useTiles = true;

    [Header("Performance")]
    [Tooltip("How often we check distance (seconds). 0.1 = 10x/sec, feels instant).")]
    public float checkInterval = 0.10f;

    private float _t;

    private void Awake()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;
        if (panel == null || player == null) return;

        _t -= Time.deltaTime;
        if (_t > 0f) return;
        _t = Mathf.Max(0.02f, checkInterval);

        float distWorld = Vector2.Distance(player.position, transform.position);

        float threshold = openDistance;
        if (useTiles && GameServices.Grid != null)
            threshold = openDistance * GameServices.Grid.cellSize;

        bool inRange = distWorld <= threshold;

        if (inRange)
        {
            if (!panel.IsOpen)
                panel.Show();
        }
        else
        {
            if (panel.IsOpen)
                panel.Hide();
        }
    }
}