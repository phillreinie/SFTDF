using UnityEngine;

public class RotateTowardsTarget2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Rotation")]
    public float turnSpeedDegPerSec = 720f;

    [Tooltip("Angle offset if your sprite doesn't face right by default")]
    public float angleOffsetDeg = 0f;

    [Tooltip("If true, keeps last rotation when target is null. If false, does nothing.")]
    public bool freezeWhenNoTarget = true;

    public void SetTarget(Transform t) => target = t;
    public void ClearTarget() => target = null;

    private void Update()
    {
        if (target == null)
        {
            if (freezeWhenNoTarget) return;
            return;
        }

        Vector2 dir = (Vector2)(target.position - transform.position);
        if (dir.sqrMagnitude < 0.0001f) return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffsetDeg;

        float current = transform.eulerAngles.z;
        float next = Mathf.MoveTowardsAngle(current, targetAngle, turnSpeedDegPerSec * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, next);
    }
}