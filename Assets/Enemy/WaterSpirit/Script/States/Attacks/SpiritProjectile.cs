using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SpiritProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _lifetime = 4f;

    private float _damage;
    private LayerMask _targetLayer;
    private bool _hasHit = false;
    private bool _armed = false;

    public void Init(Vector2 direction, float damage, LayerMask targetLayer)
    {
        _damage = damage;
        _targetLayer = targetLayer;

        Vector2 dir = direction.normalized;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        GetComponent<Rigidbody2D>().linearVelocity = dir * _speed;
        Destroy(gameObject, _lifetime);
        StartCoroutine(ArmDelay());
    }

    // 스폰 직후 보스 콜라이더와 즉시 충돌하는 것을 방지
    private IEnumerator ArmDelay()
    {
        yield return new WaitForSeconds(0.15f);
        _armed = true;
    }

    private void OnTriggerEnter2D(Collider2D other) => HandleHit(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => HandleHit(collision.gameObject);

    private void HandleHit(GameObject other)
    {
        if (!_armed || _hasHit) return;
        if (((1 << other.layer) & _targetLayer) == 0) return;

        _hasHit = true;
        other.GetComponentInParent<HP>()?.TakeDamage(_damage);
        Destroy(gameObject);
    }
}