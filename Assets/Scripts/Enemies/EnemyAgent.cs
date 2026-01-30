using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAgent : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float retargetInterval = 0.5f;

    [Header("Targeting")]
    public float aggroRadius = 25f;
    public LayerMask targetMask; // Player + Building + Core etc.

    [Header("Fallback Target (Core)")]
    public Transform coreTransform; // assign in prefab or set by spawner

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;     // set to Obstacles
    public float avoidDistance = 2.0f;
    public float avoidStrength = 6f;
    public float sideProbeAngle = 50f;
    public float sideProbeDistance = 1.8f;
    public float probeRadius = 0.3f;

    [Header("Wander Noise")]
    public float wanderStrength = 0.15f;
    public float wanderChangeInterval = 0.8f;

    [Header("Wander Target")]
    public bool wanderWhenNoTarget = true;
    public float wanderPickRadius = 8f;        // around current position
    public float wanderArriveDistance = 0.6f;  // when considered reached
    public float wanderRepathTime = 2.0f;      // pick new if too slow / stuck
    public float wanderSpeedMultiplier = 0.65f;

    [Header("Aim / Rotation (optional)")]
    public RotateTowardsTarget2D rotator;
    public bool rotateTowardsTarget = true;
    public bool rotateTowardsMoveWhenNoTarget = true;
    public float moveAimLookAhead = 2f; // how far in front to aim when wandering

    private Transform _aimHelper;

    private Vector2 _wanderTargetPos;
    private bool _hasWanderTarget;
    private float _wanderRepathTimer;

    [Header("Unstuck")]
    public float stuckSpeedThreshold = 0.25f;
    public float stuckTimeToTrigger = 0.35f;
    public float unstuckDuration = 0.9f;
    public float unstuckTurnBias = 2.5f;

    [Header("Detour Memory")]
    public float detourCommitTime = 0.6f;

    [Header("Blocked Retarget")]
    public float blockedTimeToRetarget = 1.0f;

    private Rigidbody2D _rb;
    private float _retargetTimer;

    private Vector2 _wanderDir;
    private float _wanderTimer;

    private float _stuckTimer;
    private float _unstuckTimer;
    private int _unstuckSide;

    private float _detourTimer;
    private Vector2 _detourDir;

    private float _blockedTimer;

    public ITargetable CurrentTarget { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (rotator == null)
            rotator = GetComponentInChildren<RotateTowardsTarget2D>();

        if (rotator != null)
        {
            var go = new GameObject("AimHelper");
            go.transform.SetParent(transform, false);
            _aimHelper = go.transform;
        }

        _retargetTimer = Random.Range(0f, retargetInterval); // desync groups

        _wanderDir = Random.insideUnitCircle.normalized;
        _wanderTimer = Random.Range(0f, wanderChangeInterval);
        _unstuckSide = Random.value < 0.5f ? -1 : 1;
    }

    private void Update()
    {
        _retargetTimer -= Time.deltaTime;
        if (_retargetTimer <= 0f)
        {
            _retargetTimer = retargetInterval;
            CurrentTarget = FindBestTarget();

            if (CurrentTarget != null && CurrentTarget.IsAlive)
                _hasWanderTarget = false; // stop wandering
        }
    }

    private void FixedUpdate()
    {
        if (!RunStateGate.IsPlaying()) { _rb.linearVelocity = Vector2.zero; return; }

        // Update wander target if we currently have NO combat target
        bool hasCombatTarget = (CurrentTarget != null && CurrentTarget.IsAlive);

        if (!hasCombatTarget && wanderWhenNoTarget)
        {
            if (!_hasWanderTarget) PickNewWanderTarget();

            // reached
            float d = Vector2.Distance(_rb.position, _wanderTargetPos);
            if (d <= wanderArriveDistance) PickNewWanderTarget();

            // timeout repath
            _wanderRepathTimer -= Time.fixedDeltaTime;
            if (_wanderRepathTimer <= 0f) PickNewWanderTarget();
        }

        // Choose intent direction
        Vector2 seekDir;

        if (hasCombatTarget)
        {
            Vector2 toTarget = (Vector2)CurrentTarget.TargetTransform.position - _rb.position;
            seekDir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector2.zero;
        }
        else
        {
            if (!wanderWhenNoTarget)
            {
                _rb.linearVelocity = Vector2.zero;
                UpdateAim(hasCombatTarget);
                return;
            }

            Vector2 toWander = _wanderTargetPos - _rb.position;
            seekDir = toWander.sqrMagnitude > 0.0001f ? toWander.normalized : Vector2.zero;
        }

        // Stuck detection (works for both chase + wander)
        float speed = _rb.linearVelocity.magnitude;
        if (speed < stuckSpeedThreshold) _stuckTimer += Time.fixedDeltaTime;
        else _stuckTimer = 0f;

        if (_stuckTimer >= stuckTimeToTrigger && _unstuckTimer <= 0f)
        {
            _unstuckTimer = unstuckDuration;
            _unstuckSide = Random.value < 0.5f ? -1 : 1;
            _stuckTimer = 0f;
            _detourTimer = 0f; // force new detour
        }

        if (_unstuckTimer > 0f)
            _unstuckTimer -= Time.fixedDeltaTime;

        float bias = (_unstuckTimer > 0f) ? unstuckTurnBias : 1f;

        float speedMult = hasCombatTarget ? 1f : wanderSpeedMultiplier;

        // Detour memory: if committed, follow it
        if (_detourTimer > 0f)
        {
            _detourTimer -= Time.fixedDeltaTime;
            _rb.linearVelocity = _detourDir.normalized * (moveSpeed * speedMult);
            UpdateAim(hasCombatTarget);
            return;
        }

        // Small wander noise ONLY while chasing (optional).
        if (hasCombatTarget)
        {
            _wanderTimer -= Time.fixedDeltaTime;
            if (_wanderTimer <= 0f)
            {
                _wanderTimer = wanderChangeInterval;
                _wanderDir = Random.insideUnitCircle.normalized;
            }
        }

        Vector2 desired = seekDir;

        if (hasCombatTarget)
            desired += _wanderDir * wanderStrength; // tiny life while chasing

        // If blocked ahead, pick detour and commit
        if (IsBlockedAhead(seekDir))
        {
            _blockedTimer += Time.fixedDeltaTime;

            _detourDir = ChooseDetour(seekDir, _unstuckSide);
            _detourTimer = detourCommitTime;
            _rb.linearVelocity = _detourDir.normalized * (moveSpeed * speedMult);

            // Only retarget-drop when chasing something (prevents wander jitter)
            if (hasCombatTarget && _blockedTimer >= blockedTimeToRetarget)
            {
                CurrentTarget = null; // go back to wander
                _blockedTimer = 0f;
                _hasWanderTarget = false; // pick fresh wander point
            }

            UpdateAim(hasCombatTarget);
            return;
        }
        else
        {
            _blockedTimer = 0f;
        }

        // Avoidance push
        Vector2 forward = desired.sqrMagnitude > 0.0001f ? desired.normalized : seekDir;
        Vector2 avoid = ComputeAvoidance(forward);
        desired += avoid * avoidStrength * bias;

        if (desired.sqrMagnitude < 0.0001f)
        {
            _rb.linearVelocity = Vector2.zero;
            UpdateAim(hasCombatTarget);
            return;
        }

        _rb.linearVelocity = desired.normalized * (moveSpeed * speedMult);

        UpdateAim(hasCombatTarget);
    }

    private void UpdateAim(bool hasCombatTarget)
    {
        if (!rotateTowardsTarget || rotator == null) return;

        if (hasCombatTarget && CurrentTarget != null && CurrentTarget.IsAlive)
        {
            rotator.SetTarget(CurrentTarget.TargetTransform);
            return;
        }

        if (rotateTowardsMoveWhenNoTarget && _aimHelper != null)
        {
            Vector2 v = _rb.linearVelocity;
            if (v.sqrMagnitude > 0.01f)
            {
                _aimHelper.position = transform.position + (Vector3)(v.normalized * moveAimLookAhead);
                rotator.SetTarget(_aimHelper);
            }
            else
            {
                rotator.ClearTarget();
            }
        }
        else
        {
            rotator.ClearTarget();
        }
    }

    private ITargetable FindBestTarget()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, aggroRadius, targetMask);

        ITargetable best = null;
        int bestPriority = int.MaxValue;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            var t = col.GetComponentInParent<ITargetable>();
            if (t == null || !t.IsAlive) continue;

            int p = Priority(t.TargetType);
            float d = Vector2.Distance(transform.position, t.TargetTransform.position);

            if (p < bestPriority || (p == bestPriority && d < bestDist))
            {
                best = t;
                bestPriority = p;
                bestDist = d;
            }
        }

        return best;
    }

    private int Priority(TargetType t)
    {
        return t switch
        {
            TargetType.Defense => 0,
            TargetType.Building => 1,
            TargetType.Player => 2,
            TargetType.Core => 3,
            _ => 99
        };
    }

    private bool IsBlockedAhead(Vector2 forward)
    {
        if (forward.sqrMagnitude < 0.0001f) return false;
        var hit = Physics2D.CircleCast(_rb.position, probeRadius, forward, avoidDistance, obstacleMask);
        return hit.collider != null;
    }

    private Vector2 ComputeAvoidance(Vector2 forward)
    {
        if (forward.sqrMagnitude < 0.0001f) return Vector2.zero;
        var hit = Physics2D.CircleCast(_rb.position, probeRadius, forward, avoidDistance, obstacleMask);
        if (hit.collider == null) return Vector2.zero;
        return hit.normal;
    }

    private Vector2 ChooseDetour(Vector2 forward, int preferredSide)
    {
        if (forward.sqrMagnitude < 0.0001f) return Vector2.zero;

        Vector2 leftDir = Quaternion.Euler(0, 0, sideProbeAngle) * forward;
        Vector2 rightDir = Quaternion.Euler(0, 0, -sideProbeAngle) * forward;

        bool leftBlocked = Physics2D.CircleCast(_rb.position, probeRadius, leftDir, sideProbeDistance, obstacleMask).collider != null;
        bool rightBlocked = Physics2D.CircleCast(_rb.position, probeRadius, rightDir, sideProbeDistance, obstacleMask).collider != null;

        if (!leftBlocked && rightBlocked) return leftDir;
        if (!rightBlocked && leftBlocked) return rightDir;

        if (!leftBlocked && !rightBlocked)
            return preferredSide < 0 ? leftDir : rightDir;

        return preferredSide < 0 ? new Vector2(-forward.y, forward.x) : new Vector2(forward.y, -forward.x);
    }

    private void PickNewWanderTarget()
    {
        Vector2 center = _rb.position;
        Vector2 dir = Random.insideUnitCircle.normalized;
        float dist = Random.Range(wanderPickRadius * 0.4f, wanderPickRadius);

        _wanderTargetPos = center + dir * dist;
        _hasWanderTarget = true;
        _wanderRepathTimer = wanderRepathTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
    }
}
