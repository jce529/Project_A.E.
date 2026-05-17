using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Charge : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private float chargeRange = 4f;
    [SerializeField] private float stopRange = 3.5f;
    [SerializeField] private float chargeCooldown = 2.5f;

    private Coroutine currentChargeRoutine; 

    [Header("Charge Timing")]
    [SerializeField] private float telegraphTime = 0.4f;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 0.35f;
    [SerializeField] private float recoverTime = 0.6f;

    [Header("Stun on Cancel")]
    [SerializeField] private float stunDuration = 3.0f;  

    [Header("Visual Telegraph")]
    [SerializeField] private Color telegraphColor = Color.cyan;

    private Rigidbody2D rb;
    private Transform player;
    private Chase chase;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private float cooldownTimer;
    private bool isCharging;
    private bool isStunned; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        chase = GetComponent<Chase>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isCharging || isStunned || player == null) return;

        cooldownTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        if (chase != null)
        {
            if (distance <= stopRange)
            {
                if (chase.enabled)
                {
                    chase.enabled = false;
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }
            else
            {
                if (!chase.enabled) chase.enabled = true;
            }
        }

        if (distance <= chargeRange && cooldownTimer <= 0f)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            currentChargeRoutine = StartCoroutine(ChargeRoutine(dir));
        }
    }

    IEnumerator ChargeRoutine(Vector2 direction)
    {
        isCharging = true;
        cooldownTimer = chargeCooldown;

        if (chase != null) chase.enabled = false;
        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
        yield return new WaitForSeconds(telegraphTime);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        float originalY = rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(direction.x * chargeSpeed, originalY);
        yield return new WaitForSeconds(chargeDuration);

        rb.linearVelocity = new Vector2(0, originalY);

        yield return new WaitForSeconds(recoverTime);
        isCharging = false;
    }

    public void CancelCharge()
    {
        if (!isCharging) return;
        if (currentChargeRoutine != null)
        {
            StopCoroutine(currentChargeRoutine);
            currentChargeRoutine = null;
        }
        StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;
        isCharging = false;
        cooldownTimer = chargeCooldown;

        if (chase != null) chase.enabled = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.gray;

        float elapsed = 0f;
        while (elapsed < stunDuration)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            elapsed += Time.deltaTime; // 흐른 시간 추가
            yield return null;         // 다음 프레임까지 대기
        }

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        isStunned = false;
        if (chase != null) chase.enabled = true;
    }
}