using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RadiusRingRenderer : MonoBehaviour
{
    [Header("Render")]
    public int segments = 64;
    public float lineWidth = 0.05f;
    public bool useWorldSpace = false;

    [Header("Refs")]
    public Transform center; // if null, uses this transform
    public GridManager grid; // optional, auto from GameServices

    private LineRenderer _lr;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.loop = true;
        _lr.positionCount = segments;
        _lr.startWidth = lineWidth;
        _lr.endWidth = lineWidth;
        _lr.useWorldSpace = useWorldSpace;

        if (center == null) center = transform;
        if (grid == null) grid = GameServices.Grid;
    }

    public void SetVisible(bool visible)
    {
        if (_lr != null) _lr.enabled = visible;
    }

    public void SetRadiusTiles(float radiusTiles)
    {
        if (grid == null) grid = GameServices.Grid;
        float cell = (grid != null) ? grid.cellSize : 1f;

        SetRadiusWorld(radiusTiles * cell);
    }

    public void SetRadiusWorld(float radiusWorld)
    {
        if (_lr == null) return;

        float step = (Mathf.PI * 2f) / segments;

        for (int i = 0; i < segments; i++)
        {
            float a = step * i;
            float x = Mathf.Cos(a) * radiusWorld;
            float y = Mathf.Sin(a) * radiusWorld;

            // local ring around center
            Vector3 p = new Vector3(x, y, 0f);

            if (useWorldSpace)
                _lr.SetPosition(i, center.position + p);
            else
                _lr.SetPosition(i, p);
        }
    }
}