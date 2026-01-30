using System;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon")]
    public float range = 10f;
    public float damage = 10f;
    public float fireRate = 8f;

    [Header("Targeting")]
    public LayerMask hitMask;

    [Header("Debug")]
    public bool drawDebugRay = true;

    private float _cooldown;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        _cooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && _cooldown <= 0f)
        {
            Fire();
            _cooldown = 1f / fireRate;
        }
    }

    private void Fire()
    {

        if (cam == null) return;
        AudioService.Instance?.Play(SfxEvent.PlayerShoot);


        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 origin = transform.position;
        Vector2 dir = ((Vector2)mouseWorld - origin).normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, range, hitMask);



        if (drawDebugRay)
        {
            Vector2 end = hit.collider != null ? hit.point : origin + dir * range;
            LaserVFXService.Instance?.Fire(LaserChannel.Player, origin, end);


        }

        if (hit.collider == null) return;

        // Try GetComponent on collider, then on rigidbody root (common setup)
        if (!hit.collider.TryGetComponent<IDamageable>(out var dmg))
        {
            if (hit.rigidbody != null)
                dmg = hit.rigidbody.GetComponent<IDamageable>();
        }

        dmg?.TakeDamage(damage);
    }
}