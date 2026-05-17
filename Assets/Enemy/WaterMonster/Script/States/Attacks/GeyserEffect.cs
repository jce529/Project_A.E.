using System.Collections;
using UnityEngine;

public class GeyserEffect : MonoBehaviour
{
    [SerializeField] private float _warningDuration = 0.3f;
    [SerializeField] private float _activeDuration = 1.5f;
    [SerializeField] private float _damage = 20f;
    [SerializeField] private float _damageRadius = 1.5f;

    private void Start()
    {
        StartCoroutine(Lifecycle());
    }

    private IEnumerator Lifecycle()
    {
        yield return new WaitForSeconds(_warningDuration);

        var hits = Physics2D.OverlapCircleAll(transform.position, _damageRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
            hit.GetComponentInParent<PlayerStats>()?.TakeDamage(_damage);

        yield return new WaitForSeconds(_activeDuration);
        Destroy(gameObject);
    }
}
