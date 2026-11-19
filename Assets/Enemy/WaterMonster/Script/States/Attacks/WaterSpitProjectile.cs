// Phase 1 — WaterSpitProjectile (straight-line projectile)
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class WaterSpitProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private float lifetime = 5f;

    public Vector3 Direction { get; set; } = Vector3.right;

    private void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // REQ-WM-X-01 — Player layer only
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Player")) == 0) return;

        var playerStats = other.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
