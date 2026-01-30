using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
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
        _rb.linearVelocity = _move * moveSpeed;
    }
}