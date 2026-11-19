using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WaterPrisonProjectile : MonoBehaviour
{
    [SerializeField] float speed = 7f;
    [SerializeField] float lifetime = 5f;

    public Vector3 Direction { get; set; } = Vector3.right;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (pc.GetComponent<PrisonDebuff>() == null)
            pc.gameObject.AddComponent<PrisonDebuff>();

        Destroy(gameObject);
    }
}
