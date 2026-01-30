using UnityEngine;

[RequireComponent(typeof(Health))]
public class HitFlash : MonoBehaviour
{
    [Header("Renderer")]
    public SpriteRenderer targetRenderer; // auto-find if null

    [Header("Flash")]
    public float flashDuration = 0.08f;

    [Header("Colors")]
    public Color damageColor = Color.white;
    public Color healColor = new Color(0.2f, 1f, 0.2f, 1f);

    public bool flashOnHeal = true;

    private Health _health;
    private Color _originalColor;
    private float _timer;
    private bool _flashing;

    private void Awake()
    {
        _health = GetComponent<Health>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            _originalColor = targetRenderer.color;
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnHealthChanged -= OnHealthChanged;

        ResetColor();
    }

    private void Update()
    {
        if (!_flashing) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _flashing = false;
            ResetColor();
        }
    }

    private void OnHealthChanged(Health h, float delta)
    {
        if (targetRenderer == null) return;

        if (delta < 0f)
        {
            Flash(damageColor);
        }
        else if (delta > 0f && flashOnHeal)
        {
            Flash(healColor);
        }
    }

    private void Flash(Color c)
    {
        targetRenderer.color = c;
        _timer = flashDuration;
        _flashing = true;
    }

    private void ResetColor()
    {
        if (targetRenderer != null)
            targetRenderer.color = _originalColor;
    }
}