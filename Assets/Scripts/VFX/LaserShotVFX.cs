using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LaserShotVFX : MonoBehaviour
{
    public LineRenderer line;
    public Light2D flashLight;

    [Header("Timing")]
    public float beamLifetime = 0.06f;
    public float lightLifetime = 0.09f;

    private float _beamT;
    private float _lightT;

    // Pool back-reference
    private LaserVFXPool _pool;

    private void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (flashLight == null) flashLight = GetComponentInChildren<Light2D>();
    }

    public void Init(LaserVFXPool pool)
    {
        _pool = pool;
    }

    public void Play(Vector3 start, Vector3 end, LaserStyle style)
    {
        ApplyStyle(style);
        _beamT = beamLifetime;
        _lightT = lightLifetime;

        if (line != null)
        {
            line.enabled = true;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        if (flashLight != null)
        {
            flashLight.enabled = true;
            flashLight.transform.position = end;
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        _beamT -= Time.deltaTime;
        _lightT -= Time.deltaTime;

        if (_beamT <= 0f && line != null)
            line.enabled = false;

        if (_lightT <= 0f && flashLight != null)
            flashLight.enabled = false;

        if (_beamT <= 0f && _lightT <= 0f)
        {
            // Return to pool (don’t Destroy)
            if (_pool != null) _pool.Recycle(this);
            else gameObject.SetActive(false);
        }
    }
    
    public void ApplyStyle(LaserStyle style)
    {
        if (style == null) return;

        beamLifetime = style.beamLifetime;
        lightLifetime = style.lightLifetime;

        if (line != null) line.widthMultiplier = style.width;

        if (flashLight != null)
        {
            flashLight.intensity = style.lightIntensity;
            flashLight.pointLightOuterRadius = style.lightOuterRadius;
        }
    }
}