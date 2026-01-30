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

    private BuildingRuntime _runtime;
    private float _cooldown;

    private void Awake()
    {
        _runtime = GetComponent<BuildingRuntime>();
        if (rotator == null) rotator = GetComponentInChildren<RotateTowardsTarget2D>();
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;
        if (_runtime == null || _runtime.data == null) return;

        // Must be a tower
        if (_runtime.data.category != BuildingCategory.DefenseTower) return;

        // Power gating
        if (_runtime.data.powerDraw > 0 && !_runtime.isPowered)
        {
            // still aim idle if you want, or do nothing:
            ApplyAim(null);
            return;
        }

        // Read combat stats from SO
        if (_runtime.data is not DefenseTowerData td)
            return;

        _cooldown -= Time.deltaTime;
        if (_cooldown > 0f)
        {
            // keep aiming at last target / idle
            ApplyAim(null);
            return;
        }

        var target = FindNearestEnemy(td.combat.rangeTiles);

        // Aim first (so the turret "tracks" even if on cooldown edge)
        ApplyAim(target);

        if (target == null) return;

        FireAt(target, td.combat.damage, td.combat.rangeTiles);
        _cooldown = 1f / Mathf.Max(0.01f, td.combat.shotsPerSecond);
    }

    private void ApplyAim(Transform enemyTargetOrNull)
    {
        if (!rotateAlways || rotator == null) return;

        // If we have an enemy, aim it. Otherwise aim idle.
        if (enemyTargetOrNull != null)
        {
            rotator.SetTarget(enemyTargetOrNull);
        }
        else if (idleAim != null)
        {
            rotator.SetTarget(idleAim);
        }
        else
        {
            // No idle provided: keep last rotation (rotator.freezeWhenNoTarget handles this)
            rotator.ClearTarget();
        }
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
