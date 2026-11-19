using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterMonster.Phase2;

public class PuddleExplosionController : MonoBehaviour
{
    [Header("Explosion Conditions")]
    [SerializeField] private bool enableIndestructibleExplosion = true;
    [Tooltip("파괴 불가 웅덩이가 이 수에 도달하면 폭발")]
    [SerializeField] private int indestructibleThreshold = 5;
    [SerializeField] private bool enableTotalExplosion = true;
    [Tooltip("전체 활성 웅덩이 수가 이 수에 도달하면 일제 폭발")]
    [SerializeField] private int totalThreshold = 8;

    [Header("Explosion Settings")]
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionDamage = 50f;

    [Header("Warning VFX")]
    [SerializeField] private Color warningColor = Color.red;

    private Coroutine _explosionRoutine;
    private bool _isExploding = false;
    private PuddleStackManager _stackSource;
    private PuddlePool _poolSource;

    private void Start()
    {
        if (PuddleStackManager.Instance != null)
            PuddleStackManager.Instance.ExplosionThreshold = indestructibleThreshold;
        if (PuddlePool.Instance != null)
            PuddlePool.Instance.TotalExplosionThreshold = totalThreshold;

        // OnEnable이 Awake 직후 실행돼 Instance가 없었을 수 있으므로 여기서 재구독
        Subscribe();
    }

    private void OnEnable() => Subscribe();

    private void Subscribe()
    {
        if (enableIndestructibleExplosion && _stackSource == null && PuddleStackManager.Instance != null)
        {
            _stackSource = PuddleStackManager.Instance;
            _stackSource.OnThresholdReached += OnIndestructibleThresholdReached;
        }
        if (enableTotalExplosion && _poolSource == null && PuddlePool.Instance != null)
        {
            _poolSource = PuddlePool.Instance;
            _poolSource.OnTotalThresholdReached += OnTotalThresholdReached;
        }
    }

    private void OnDisable()
    {
        if (_stackSource != null) _stackSource.OnThresholdReached -= OnIndestructibleThresholdReached;
        if (_poolSource != null) _poolSource.OnTotalThresholdReached -= OnTotalThresholdReached;
        _stackSource = null;
        _poolSource = null;
        if (_explosionRoutine != null) StopCoroutine(_explosionRoutine);
        _explosionRoutine = null;
        _isExploding = false;
    }

    private void OnIndestructibleThresholdReached()
    {
        if (_isExploding) return;
        if (_explosionRoutine != null) StopCoroutine(_explosionRoutine);
        var snapshot = new List<WaterPuddle>(PuddleStackManager.Instance.IndestructiblePuddles);
        _explosionRoutine = StartCoroutine(ExplosionSequence(
            snapshot,
            () => PuddleStackManager.Instance.ReturnAllIndestructibleToPool()));
    }

    private void OnTotalThresholdReached()
    {
        if (_isExploding) return;
        if (_explosionRoutine != null) StopCoroutine(_explosionRoutine);
        _explosionRoutine = StartCoroutine(ExplosionSequence(
            PuddlePool.Instance.GetAllActive(),
            () => PuddlePool.Instance.ReturnAll()));
    }

    private IEnumerator ExplosionSequence(List<WaterPuddle> snapshot, Action afterExplosion)
    {
        _isExploding = true;

        foreach (var puddle in snapshot)
        {
            if (puddle != null && puddle.gameObject.activeSelf)
            {
                var sr = puddle.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = warningColor;
            }
        }

        yield return new WaitForSeconds(warningDuration);

        foreach (var puddle in snapshot)
        {
            if (puddle != null && puddle.gameObject.activeSelf)
                ApplyExplosionDamage(puddle.transform.position);
        }

        afterExplosion?.Invoke();

        _isExploding = false;
        _explosionRoutine = null;
    }

    private void ApplyExplosionDamage(Vector3 puddlePosition)
    {
        var hits = Physics2D.OverlapCircleAll(puddlePosition, explosionRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            var playerStats = hit.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
                playerStats.TakeDamage(explosionDamage);
        }
    }
}
