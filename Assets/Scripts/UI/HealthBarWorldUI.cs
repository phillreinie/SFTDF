using UnityEngine;
using UnityEngine.UI;

public class HealthBarWorldUI : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;

    [Header("Colors")]
    public Color damagedColor = new Color(1f, 0.2f, 0.2f, 1f); // red
    public Color healingColor = new Color(0.2f, 1f, 0.2f, 1f); // green

    [Header("Follow")]
    public Transform followTarget;
    public Vector3 worldOffset = new Vector3(0f, 0.6f, 0f);

    private Health _health;

    public void Bind(Health health, Transform anchor, Vector3 offset)
    {
        _health = health;
        followTarget = anchor != null ? anchor : health.transform;
        worldOffset = offset;

        // subscribe
        _health.OnHealthChanged += HandleHealthChanged;
        _health.OnDeath += HandleDeath;

        // initial paint
        HandleHealthChanged(_health, 0f);
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= HandleHealthChanged;
            _health.OnDeath -= HandleDeath;
        }
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.position = followTarget.position + worldOffset;
        transform.rotation = Quaternion.identity; // keep upright in 2D
    }

    private void HandleHealthChanged(Health h, float delta)
    {
        if (h == null || h.maxHP <= 0f) return;

        float t = Mathf.Clamp01(h.currentHP / h.maxHP);
        if (fillImage != null) fillImage.fillAmount = t;

        bool full = h.currentHP >= h.maxHP - 0.0001f;
        gameObject.SetActive(!full);
        if (full) return;

        // delta < 0 => damage (red), delta > 0 => heal (green)
        if (fillImage != null)
            fillImage.color = (delta > 0f) ? healingColor : damagedColor;
    }

    private void HandleDeath(Health h)
    {
        Destroy(gameObject);
    }
}