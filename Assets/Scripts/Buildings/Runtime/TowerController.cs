using UnityEngine;

[RequireComponent(typeof(BuildingRuntime))]
public class TowerController : MonoBehaviour
{
    [Header("Targeting")]
    public LayerMask enemyMask;

    [Header("Aim / Rotation (optional)")]
    public RotateTowardsTarget2D rotator;
    public Transform idleAim; // assign in prefab (e.g. points downward)
    public bool rotateAlways = true;

    [Header("Debug")]
    public bool drawDebugShot = true;

    // -------------------------
    // V2-1: Starved feedback
    // -------------------------
    [Header("Ammo Starved Feedback")]
    [Tooltip("Optional. If null, will try GetComponentInChildren<SpriteRenderer>().")]
    public SpriteRenderer flashRenderer;

    [Tooltip("Flash color when ammo is missing.")]
    public Color starvedFlashColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Tooltip("How long the starved flash lasts.")]
    public float starvedFlashDuration = 0.08f;

    [Tooltip("Dry-fire SFX when ammo is missing (anti-spam).")]
    public bool playStarvedSfx = true;

    [Tooltip("Cooldown between dry-fire sounds.")]
    public float starvedSfxCooldown = 0.35f;

    // NOTE: Using an existing event you already reference elsewhere.
    // If you later add SfxEvent.TowerDryFire, just swap this default.
    public SfxEvent starvedSfxEvent = SfxEvent.BuildDeny;

    private BuildingRuntime _runtime;
    private float _cooldown;

    private float _starvedFlashT;
    private float _starvedSfxT;
    private Color _originalColor;
    private bool _hasOriginalColor;

    private void Awake()
    {
        _runtime = GetComponent<BuildingRuntime>();
        if (rotator == null) rotator = GetComponentInChildren<RotateTowardsTarget2D>();

        if (flashRenderer == null)
            flashRenderer = GetComponentInChildren<SpriteRenderer>();

        if (flashRenderer != null)
        {
            _originalColor = flashRenderer.color;
            _hasOriginalColor = true;
        }
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;
        if (_runtime == null || _runtime.data == null) return;

        TickStarvedFeedbackTimers();

        // Must be a tower
        if (_runtime.data.category != BuildingCategory.DefenseTower) return;

        // Power gating (V1 gate stays)
        if (_runtime.data.powerDraw > 0 && !_runtime.isPowered)
        {
            _runtime.productionState = ProductionState.Inactive;
            ApplyAim(null);
            return;
        }

        // Read combat stats from SO
        if (_runtime.data is not DefenseTowerData td)
            return;

        // Default "ready" state (may be overridden below)
        _runtime.productionState = ProductionState.Running;

        _cooldown -= Time.deltaTime;
        if (_cooldown > 0f)
        {
            ApplyAim(null);
            return;
        }

        var target = FindNearestEnemy(td.combat.rangeTiles);

        // Aim first (so turret tracks even if we don't shoot)
        ApplyAim(target);

        if (target == null) return;

        // -------------------------
        // V2-1: AMMO GATING
        // -------------------------
        int ammoCost = Mathf.Max(0, td.ammoPerShot);
        string ammoId = td.ammoResourceId;

        if (ammoCost > 0)
        {
            if (GameServices.Inventory == null || !GameServices.Inventory.TryConsume(ammoId, ammoCost))
            {
                // No ammo => no shot (readable + feedback)
                _runtime.productionState = ProductionState.Starved;
                TriggerStarvedFeedback();

                // tiny retry delay to avoid spamming checks
                _cooldown = 0.10f;
                return;
            }
        }

        // Fire
        FireAt(target, td.combat.damage, td.combat.rangeTiles);
        _cooldown = 1f / Mathf.Max(0.01f, td.combat.shotsPerSecond);
    }

    private void TickStarvedFeedbackTimers()
    {
        if (_starvedSfxT > 0f) _starvedSfxT -= Time.deltaTime;

        if (_starvedFlashT > 0f)
        {
            _starvedFlashT -= Time.deltaTime;
            if (_starvedFlashT <= 0f)
                ResetFlashColor();
        }
    }

    private void TriggerStarvedFeedback()
    {
        // Flash
        if (flashRenderer != null)
        {
            flashRenderer.color = starvedFlashColor;
            _starvedFlashT = Mathf.Max(0.01f, starvedFlashDuration);
        }

        // SFX (anti-spam)
        if (playStarvedSfx && _starvedSfxT <= 0f)
        {
            AudioService.Instance?.Play(starvedSfxEvent);
            _starvedSfxT = Mathf.Max(0.05f, starvedSfxCooldown);
        }
    }

    private void ResetFlashColor()
    {
        if (flashRenderer != null && _hasOriginalColor)
            flashRenderer.color = _originalColor;
    }

    private void ApplyAim(Transform enemyTargetOrNull)
    {
        if (!rotateAlways || rotator == null) return;

        if (enemyTargetOrNull != null)
            rotator.SetTarget(enemyTargetOrNull);
        else if (idleAim != null)
            rotator.SetTarget(idleAim);
        else
            rotator.ClearTarget();
    }

    private Transform FindNearestEnemy(float rangeTiles)
    {
        float rangeWorld = rangeTiles * GameServices.Grid.cellSize;

        var hits = Physics2D.OverlapCircleAll(transform.position, rangeWorld, enemyMask);
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            var h = col.GetComponentInParent<Health>();
            if (h == null || h.currentHP <= 0f) continue;

            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = col.transform;
            }
        }

        return best;
    }

    private void FireAt(Transform target, float damage, float rangeTiles)
    {
        Vector2 origin = transform.position;
        Vector2 dir = ((Vector2)target.position - origin).normalized;

        float rangeWorld = rangeTiles * GameServices.Grid.cellSize;

        var hit = Physics2D.Raycast(origin, dir, rangeWorld, enemyMask);

        if (drawDebugShot)
        {
            Vector2 end = hit.collider != null ? hit.point : origin + dir * rangeWorld;
            LaserVFXService.Instance?.Fire(LaserChannel.Tower, origin, end);
        }

        if (hit.collider == null) return;

        var dmg = hit.collider.GetComponentInParent<IDamageable>();
        dmg?.TakeDamage(damage);

        Debug.unityLogger.Log(hit.collider.gameObject.name + " damaged " + damage);
        AudioService.Instance?.Play(SfxEvent.TowerShoot);
    }

    private void OnDrawGizmosSelected()
    {
        float cell = GameServices.Grid != null ? GameServices.Grid.cellSize : 1f;

        var rt = GetComponent<BuildingRuntime>();
        if (rt != null && rt.data is DefenseTowerData td)
            Gizmos.DrawWireSphere(transform.position, td.combat.rangeTiles * cell);
    }
}
