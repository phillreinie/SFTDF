using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement (Base)")]
    public float moveSpeed = 6f;

    private Rigidbody2D _rb;
    private Vector2 _move;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _move.x = Input.GetAxisRaw("Horizontal");
        _move.y = Input.GetAxisRaw("Vertical");
        _move = _move.normalized;
    }

    private void FixedUpdate()
    {
        float finalMoveSpeed = GetFinalMoveSpeed();
        _rb.linearVelocity = _move * finalMoveSpeed;
    }

    private float GetFinalMoveSpeed()
    {
        float bonus = GameServices.PlayerUpgrades != null ? GameServices.PlayerUpgrades.MoveSpeedBonus : 0f;
        return Mathf.Max(0.1f, moveSpeed + bonus);
    }
}