using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SpiritProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _lifetime = 4f;

    private float _damage = 12f;
    private Vector2 _direction = Vector2.right;
    private bool _initialized = false;

    public void Init(Vector2 direction, float damage)
    {
        _direction = direction.normalized;
        _damage = damage;
        _initialized = true;

        // 방향에 맞춰 회전 (선택 사항 - 스프라이트가 오른쪽을 바라보고 있다고 가정)
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (_initialized)
        {
            rb.linearVelocity = _direction * _speed;
        }

        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player 레이어 체크
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Player")) == 0) return;

        var ps = other.GetComponentInParent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(_damage);
            Debug.Log($"[SpiritProjectile] Hit Player! Damage: {_damage}");
        }

        Destroy(gameObject);
    }
}
