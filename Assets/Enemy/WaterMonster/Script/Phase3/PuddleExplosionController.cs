using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterMonster.Phase2;

public class PuddleExplosionController : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float warningDuration = 2f;   // D-03: Inspector tuning
    [SerializeField] private float explosionRadius = 2.5f;  // AoE radius per puddle
    [SerializeField] private float explosionDamage = 50f;   // Fatal damage

    [Header("Warning VFX")]
    [SerializeField] private Color warningColor = Color.red; // Puddle color during warning

    private Coroutine _explosionRoutine;
    private bool _isExploding = false; // Pitfall 1: Prevent duplicate explosions

    private void OnEnable()
    {
        if (PuddleStackManager.Instance != null)
            PuddleStackManager.Instance.OnThresholdReached += OnThresholdReached;
    }

    private void OnDisable()
    {
        if (PuddleStackManager.Instance != null)
            PuddleStackManager.Instance.OnThresholdReached -= OnThresholdReached;
    }

    private void OnThresholdReached()
    {
        if (_isExploding) return; // Prevent concurrent explosions
        if (_explosionRoutine != null) StopCoroutine(_explosionRoutine);
        _explosionRoutine = StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        _isExploding = true;

        // D-02: Warning effect — change Indestructible puddles' color to red
        var puddles = PuddleStackManager.Instance.IndestructiblePuddles;
        var snapshot = new List<WaterPuddle>(puddles); // Copy to avoid modification during iteration

        foreach (var puddle in snapshot)
        {
            if (puddle != null && puddle.gameObject.activeSelf)
            {
                var sr = puddle.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = warningColor;
            }
        }

        // D-02, D-03: Warning delay (default 2s)
        yield return new WaitForSeconds(warningDuration);

        // D-01: Simultaneous explosion — AoE at each Indestructible puddle position
        foreach (var puddle in snapshot)
        {
            if (puddle != null && puddle.gameObject.activeSelf)
            {
                ApplyExplosionDamage(puddle.transform.position);
            }
        }

        // D-05: Bulk Pool Return + count reset after explosion
        PuddleStackManager.Instance.ReturnAllIndestructibleToPool();

        _isExploding = false;
        _explosionRoutine = null;
    }

    // D-04: Radius-based OverlapCircleAll, damage only Player layer (REQ-WM-X-01)
    private void ApplyExplosionDamage(Vector3 puddlePosition)
    {
        var hits = Physics2D.OverlapCircleAll(puddlePosition, explosionRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            var playerStats = hit.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(explosionDamage);
            }
        }
    }
}
