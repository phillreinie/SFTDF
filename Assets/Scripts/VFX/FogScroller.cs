using UnityEngine;

public class FogMover : MonoBehaviour
{
    [Header("Speed")]
    public float minSpeed = 0.01f;
    public float maxSpeed = 0.08f;

    [Header("Direction")]
    [Tooltip("If false, fog only moves in positive directions (useful for subtle drift).")]
    public bool allowNegative = true;

    [Header("Direction Change Timer")]
    public float minChangeTime = 4f;
    public float maxChangeTime = 10f;

    private Vector2 _currentVelocity;
    private float _timer;

    private void Start()
    {
        PickNewMovement();
    }

    private void Update()
    {
        if (!RunStateGate.IsPlaying()) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            PickNewMovement();
        }

        transform.position += (Vector3)(_currentVelocity * Time.deltaTime);
    }

    private void PickNewMovement()
    {
        float speed = Random.Range(minSpeed, maxSpeed);

        Vector2 dir = Random.insideUnitCircle.normalized;

        if (!allowNegative)
        {
            dir.x = Mathf.Abs(dir.x);
            dir.y = Mathf.Abs(dir.y);
        }

        _currentVelocity = dir * speed;
        _timer = Random.Range(minChangeTime, maxChangeTime);
    }
}