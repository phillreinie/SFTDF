using UnityEngine;

[RequireComponent(typeof(EnemyAgent))]
public class EnemyMeleeAttack : MonoBehaviour
{
    public float attackRange = 0.8f;
    public float damage = 10f;
    public float attacksPerSecond = 1f;

    [Header("Line of Sight")]
    public LayerMask obstacleMask;   // set to Obstacles
    public float losRadius = 0.1f;   // small radius to match your collider feel

    private EnemyAgent _agent;
    private float _cooldown;

    private void Awake()
    {
        _agent = GetComponent<EnemyAgent>();
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;

        _cooldown -= Time.deltaTime;

        var target = _agent.CurrentTarget;
        if (target == null || !target.IsAlive) return;

        Vector2 from = transform.position;
        Vector2 to = target.TargetTransform.position;

        float dist = Vector2.Distance(from, to);
        if (dist > attackRange) return;

        // LOS / no obstacle between us and target
        Vector2 dir = (to - from);
        if (dir.sqrMagnitude > 0.0001f)
        {
            dir.Normalize();
            // CircleCast is more forgiving than Raycast for corners
            var hit = Physics2D.CircleCast(from, losRadius, dir, dist, obstacleMask);
            if (hit.collider != null)
                return; // blocked => don't attack
        }

        if (_cooldown > 0f) return;

        var dmg = target.TargetTransform.GetComponentInParent<IDamageable>();
        dmg?.TakeDamage(damage);

        _cooldown = 1f / Mathf.Max(0.01f, attacksPerSecond);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}