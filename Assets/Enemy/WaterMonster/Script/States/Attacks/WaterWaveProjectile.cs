using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WaterWaveProjectile : MonoBehaviour
{
    [SerializeField] float damage = 15f;
    [SerializeField] float lifetime = 8f;

    public Vector2 Direction { get; set; }
    public float Speed { get; set; } = 6f;

    private Rigidbody2D rb;
    private bool _hasHit;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Direction.normalized * Speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        _hasHit = true;

        var ps = other.GetComponentInParent<PlayerStats>();
        if (ps != null) ps.TakeDamage(damage);

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null) pc.ApplyKnockback(Direction, 8f);
    }
}
