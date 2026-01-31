using UnityEngine;

[RequireComponent(typeof(BuildingRuntime))]
public class BuildingRadiusPresenter : MonoBehaviour
{
    [Header("Ring")]
    public RadiusRingRenderer ring;

    [Header("Show Rules (V1)")]
    public bool showWhenHovered = true;
    public bool showWhenSelectedForBuild = true; // optional hook (see below)

    [Header("Hover")]
    public float hoverMaxDistanceWorld = 0.75f; // mouse must be close to building center

    private BuildingRuntime _rt;
    private Camera _cam;

    private void Awake()
    {
        _rt = GetComponent<BuildingRuntime>();
        _cam = Camera.main;

        if (ring == null) ring = GetComponentInChildren<RadiusRingRenderer>(true);
        if (ring != null) ring.SetVisible(false);
    }

    private void Start()
    {
        ApplyRadiusFromData();
    }

    private void Update()
    {
        if (ring == null || _rt == null || _rt.data == null) return;

        bool visible = false;

        // V1 hover (works in both modes if you want)
        if (showWhenHovered && _cam != null)
        {
            Vector3 mw = _cam.ScreenToWorldPoint(Input.mousePosition);
            mw.z = 0f;
            float d = Vector2.Distance(mw, transform.position);
            if (d <= hoverMaxDistanceWorld) visible = true;
        }

        ring.SetVisible(visible);
    }

    public void ApplyRadiusFromData()
    {
        if (ring == null || _rt == null || _rt.data == null) return;

        float radiusTiles = GetRadiusTiles(_rt.data);
        if (radiusTiles <= 0.01f)
        {
            ring.SetVisible(false);
            return;
        }

        ring.SetRadiusTiles(radiusTiles);
    }

    private float GetRadiusTiles(BuildingData data)
    {
        if (data == null) return 0f;

        // Storage
        if (data is StorageData sd) return sd.linkRadiusTiles;

        // Power
        if (data is PowerData pd) return pd.powerRadiusTiles;

        // Tower
        if (data is DefenseTowerData td) return td.combat.rangeTiles;

        // Fallback
        return data.radiusTiles;
    }
}
