using System.Collections;
using UnityEngine;

public class PlayerAttack : PlayerInputHandler
{
    private int playerLayer;
    private int enemyLayer;
    private int enemyAttackLayer;
    PlayerController controller;

    [Header("공격 기본 설정")]
    public float damage = 10f;
    public GameObject attackBox;
    public float attackDistance = 1.5f;
    public float attackDuration = 0.2f;

    public float attackCooldown = 0.3f;
    private bool canAttack = true;

    [Header("공격 콤보 설정")]
    public int comboStep = 0;
    public float comboResetTime = 0.8f;     // 콤보 유효 시간
    private float lastAttackTime;
    public float combo3DamageMultiplier = 1.5f;
    public float combo3Cooldown = 0.6f;     // 3타 전용 긴 쿨타임 (기존 attackCooldown보다 길게 설정)

    private SpriteRenderer spriteRenderer;

    [Header("Water Boost")]
    public bool WaterBoostImprov = false;
    public bool isWaterBoostActive = false;
    public float attackBuffMultiplier = 1.25f;
    public float attackSpeedMultiplier = 1.15f;
    public float splashDamageMultiplier = 0.5f; // 원본 데미지의 50% 추가 데미지
    public float splashKnockbackForce = 5.0f;   // 주변 적 넉백 힘
    public float splashRadius = 2.0f;

    private float waterBoostDrainTimer = 0f;

    [Header("Water Leap")]
    public bool WaterLeapImprov = false;
    public float leapSpeed = 20f;
    public float leapDuration = 0.18f;
    public float improvMultiplier = 2f;
    public int leapWaterCost = 1;
    public float leapCooldown = 1.2f;

    private bool canLeap = true;
    private Rigidbody2D rb;
    private int defaultLayer;

    [Header("Q : Obstacle Clear")]
    public string obstacleTag = "Obstacle";
    public float qCooldown = 0.5f;

    private bool canUseQ = true;

    [Header("기타 컴포넌트")]
    public WaterController waterController;
    public PlayerStats playerStats;

    protected override void OnBasicAttack()
    {
        // =====================================================================
        // [버전 A] 마우스 방향 조준형 (현재 주석 처리됨)
        // =====================================================================
        /*
        if (!canAttack) return;

        GameObject atkBox = null;
        Vector2 direction = Vector2.right;

        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos3D.x, mousePos3D.y);
        Vector2 playerPos2D = new Vector2(transform.position.x, transform.position.y);

        direction = (mousePos2D - playerPos2D).normalized;
        Vector2 spawnPos = playerPos2D + direction * attackDistance;

        atkBox = Instantiate(attackBox, spawnPos, Quaternion.identity);
        atkBox.transform.right = direction; 
        */

        // =====================================================================
        // [버전 B] 바라보는 방향 고정형 (현재 활성화됨)
        // =====================================================================
        if (!canAttack) return;
        if (spriteRenderer == null) return;

        // 1. 콤보 시간 체크 (너무 늦게 누르면 1타부터)
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        comboStep++;
        lastAttackTime = Time.time;

        // 2. 공격 방향 및 위치 계산
        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 playerPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 spawnPos = playerPos2D + direction * attackDistance;

        // 3. 공격 박스 생성
        GameObject atkBox = Instantiate(attackBox, spawnPos, Quaternion.identity);
        if (direction == Vector2.left) atkBox.transform.rotation = Quaternion.Euler(0, 180, 0);
        PlayerAttackDamager damager = atkBox.GetComponent<PlayerAttackDamager>();
        if (damager != null) damager.playerAttack = this;

        // 4. 변수 초기화 (기본값)
        float currentDamage = damage;
        float finalCooldown = attackCooldown; // 기본 쿨타임 (0.3f)

        // 5. 3타 특수 로직
        if (comboStep == 3)
        {
            currentDamage *= combo3DamageMultiplier;
            finalCooldown = combo3Cooldown; // 3타 후에는 더 긴 쿨타임 적용

            Debug.Log("막타, 쿨다운");
            comboStep = 0; // 콤보 초기화
        }

        // 6. Water Boost(버프) 적용
        float finalDuration = attackDuration;
        if (isWaterBoostActive)
        {
            finalDuration /= attackSpeedMultiplier;
            finalCooldown /= attackSpeedMultiplier;

            if (WaterBoostImprov)
            {
                ApplySplashEffect(spawnPos); // 공격 박스 생성 위치를 중심으로 스플래시
            }
        }

        // 7. 실행
        StartCoroutine(DisableAfterTime(atkBox, finalDuration));
        StartCoroutine(AttackCooldownRoutine(finalCooldown));
    }

    protected override void OnSkillE()
    {
        if (!canLeap || waterController == null) return;
        if (!WaterLeapImprov && !controller.IsGrounded()) return;

        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < leapWaterCost)
            return;
        waterController.UseBottle(leapWaterCost);
        StartCoroutine(WaterLeap());
    }

    protected override void OnSkillR()
    {
        if (waterController == null) return;
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < 1)
            return;
        isWaterBoostActive = !isWaterBoostActive;
        waterBoostDrainTimer = 0f;
    }

    protected override void OnHeal()
    {
        if (waterController == null || playerStats == null) return;

        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < 1)
            return;

        waterController.UseBottle(1);
        playerStats.Heal(10);
    }

    protected override void OnSkillQ()
    {
        if (!canUseQ) return;

        StartCoroutine(Q_ClearObstacle());
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");

        if (playerLayer < 0) Debug.LogError("'Player' 레이어가 없습니다. Project Settings > Tags and Layers에서 추가하세요.");
        if (enemyLayer < 0) Debug.LogError("'Enemy' 레이어가 없습니다. Project Settings > Tags and Layers에서 추가하세요.");
        if (enemyAttackLayer < 0) Debug.LogError("'EnemyAttack' 레이어가 없습니다. Project Settings > Tags and Layers에서 추가하세요.");

        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();

        if (isWaterBoostActive)
        {
            waterBoostDrainTimer += Time.deltaTime;

            if (waterBoostDrainTimer >= 1f)
            {
                if (waterController.waterCounter() + waterController.corruptedwaterCounter() >= 1)
                {
                    waterController.UseBottle(1);
                    waterBoostDrainTimer = 0f;
                }
                else
                {
                    isWaterBoostActive = false;
                }
            }
        }
    }

    IEnumerator WaterLeap()
    {
        canLeap = false;

        float dirX = spriteRenderer.flipX ? -1f : 1f;

        if (playerLayer >= 0 && enemyLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        if (playerLayer >= 0 && enemyAttackLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, enemyAttackLayer, true);

        if (controller != null)
        {
            controller.movementLocked = true;
        }

        float currentLeapSpeed = WaterLeapImprov ? leapSpeed * improvMultiplier : leapSpeed;
        float currentLeapDuration = WaterLeapImprov ? leapDuration * improvMultiplier : leapDuration;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float elapsed = 0f;
        while (elapsed < leapDuration)
        {
            rb.linearVelocity = new Vector2(dirX * leapSpeed, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravity;

        if (playerLayer >= 0 && enemyLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        if (playerLayer >= 0 && enemyAttackLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, enemyAttackLayer, false);

        if (controller != null)
        {
            controller.movementLocked = false;
        }

        yield return new WaitForSeconds(leapCooldown);
        canLeap = true;
    }

    IEnumerator Q_ClearObstacle()
    {
        canUseQ = false;

        Vector2 center = transform.position;
        float radius = attackDistance * 1.5f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        bool isSuccess = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(obstacleTag))
            {
                Destroy(hit.gameObject);
            }

            if (hit.CompareTag("Enemy"))
            {
                Charge enemyCharge = hit.GetComponent<Charge>();
                if (enemyCharge != null)
                {
                    enemyCharge.CancelCharge();
                    isSuccess = true;
                }
            }
        }
        if (isSuccess)
        {
            yield return StartCoroutine(HitStop(0.15f)); // 0.15초간 정지
        }

        yield return new WaitForSeconds(qCooldown);
        canUseQ = true;
        Debug.Log("Q 준비완료!");
    }

    IEnumerator DisableAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        if (obj != null)
        {
            Destroy(obj);
        }
    }

    private IEnumerator AttackCooldownRoutine(float cooldown)
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (isWaterBoostActive && WaterBoostImprov)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }

    private void OnDisable()
    {
        if (playerLayer >= 0 && enemyLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        if (playerLayer >= 0 && enemyAttackLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, enemyAttackLayer, false);

        if (controller != null)
        {
            controller.movementLocked = false;
        }
    }

    IEnumerator HitStop(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0f; // 시간 정지

        // Time.timeScale이 0일 때는 WaitForSeconds가 작동하지 않으므로 
        // 현실 시간 기준인 WaitForSecondsRealtime을 사용해야 합니다.
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalScale; // 시간 복구
    }

    private void ApplySplashEffect(Vector2 position)
    {
        // 공격 위치 주변의 적들을 탐색
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, splashRadius);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // "Enemy" 태그를 가진 적만 대상
            if (enemyCollider.CompareTag("Enemy"))
            {
                // 1. 추가 데미지 입히기 (Enemy 스크립트가 TakeDamage 메서드를 가지고 있다고 가정)
                // float extraDamage = damage * splashDamageMultiplier;
                // enemyCollider.GetComponent<Enemy>()?.TakeDamage(extraDamage);

                // 2. 주변 적 넉백 (Rigidbody2D가 있는 경우)
                Rigidbody2D enemyRb = enemyCollider.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDir = (Vector2)enemyCollider.transform.position - position;
                    enemyRb.AddForce(knockbackDir.normalized * splashKnockbackForce, ForceMode2D.Impulse);
                }

                Debug.Log(enemyCollider.name + "에게 스플래시 효과 적용!");
            }
        }
    }
}