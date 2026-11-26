using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossCharge : MonoBehaviour
{
    [Header("Settings")]
    public float chargeRange = 10f;
    public float chargeCooldown = 5f;
    public float telegraphTime = 1.2f;
    public float compressTime = 0.5f;
    public float chargeSpeed = 15f;
    public float maxChargeTime = 2f;
    public float stunDuration = 4.0f;

    public Color telegraphColor = Color.red;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private BossManager manager;
    private bool isCharging = false; // 충돌 판정을 위해 돌진 중인지 기록

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        manager = GetComponent<BossManager>(); // 매니저 가져오기
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // Manager가 방향 결정을 위해 player Transform을 받아 코루틴 실행
    public IEnumerator ChargeRoutine(Transform player)
    {
        float dirX = (player.position.x > transform.position.x) ? 1f : -1f;
        Vector2 direction = new Vector2(dirX, 0);

        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
        yield return new WaitForSeconds(telegraphTime + compressTime);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        isCharging = true;
        float startTime = Time.time;

        while (Time.time - startTime < maxChargeTime && isCharging)
        {
            rb.linearVelocity = new Vector2(direction.x * chargeSpeed, rb.linearVelocity.y);
            yield return null;
        }

        // 도중에 기둥에 박지 않고 자연스럽게 끝났을 경우
        if (isCharging)
        {
            isCharging = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            yield return new WaitForSeconds(1.0f); // 후딜레이
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Pillar"))
        {
            isCharging = false; // 돌진 정지
            if (spriteRenderer != null) spriteRenderer.color = Color.gray; // 임시 기절 색상

            manager.EnterStun(stunDuration); // 그로기 진입
        }
    }
}