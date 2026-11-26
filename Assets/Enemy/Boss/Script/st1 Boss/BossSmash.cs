using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossSmash : MonoBehaviour
{
    [Header("Settings")]
    public float smashRange = 7.0f;
    public float smashCooldown = 5.0f;
    public float windupTime = 0.8f;
    public float recoverTime = 1.0f;
    public float smashRadius = 3.0f;    // 중심부 즉사/강한 타격 범위 (필요시 사용)
    public LayerMask playerLayer;

    [Header("Visuals & FX")]
    public GameObject expandingShockwavePrefab;  // 🌟 원형으로 커지는 프리팹
    public Transform shockwaveSpawnPoint;
    public Color windupColor = new Color(1f, 0.5f, 0f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    public IEnumerator SmashRoutine()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // 준비 동작 (주황색)
        if (spriteRenderer != null) spriteRenderer.color = windupColor;
        yield return new WaitForSeconds(windupTime);

        // 타격 순간 (원래 색 복구)
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        // 1. 중앙 즉발 타격 (보스와 아예 겹쳐있을 때 맞는 판정)
        Collider2D hit = Physics2D.OverlapCircle(transform.position, smashRadius, playerLayer);
        if (hit != null) Debug.Log("대지 강타 즉발 타격 적중!");

        // 2. 🌟 커지는 원형 충격파 1개만 생성 (좌우 방향 설정 제거)[cite: 4]
        if (expandingShockwavePrefab != null && shockwaveSpawnPoint != null)
        {
            Instantiate(expandingShockwavePrefab, shockwaveSpawnPoint.position, Quaternion.identity);
        }

        // 후딜레이
        yield return new WaitForSeconds(recoverTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, smashRadius);
    }
}