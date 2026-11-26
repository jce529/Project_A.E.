using System.Collections;
using UnityEngine;

public enum BossState
{
    Chase,
    Attacking,
    Stunned
}

public class BossManager : MonoBehaviour
{
    [Header("Scripts & Components")]
    public Chase chaseScript;
    public BossSmash bossSmash;
    public BossCharge bossCharge;
    public BossVine bossVine;
    public BossHorn bossHorn;

    [Header("Target")]
    public Transform player;

    [Header("UI & Effects")]
    public GameObject lockOnMark; // 💡 인스펙터에서 자식으로 넣은 록온 마크 할당

    [Header("State")]
    public BossState currentState = BossState.Chase;

    private float smashTimer = 0f;
    private float chargeTimer = 0f;
    private float vineTimer = 0f;
    private float hornTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 💡 게임 시작 시 록온 마크가 켜져있다면 꺼줍니다.
        if (lockOnMark != null) lockOnMark.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        UpdateCooldowns();

        switch (currentState)
        {
            case BossState.Chase:
                chaseScript.enabled = true;
                CheckPatterns();
                break;

            case BossState.Attacking:
                chaseScript.enabled = false;
                break;

            case BossState.Stunned:
                chaseScript.enabled = false;
                break;
        }
    }

    private void UpdateCooldowns()
    {
        if (smashTimer > 0) smashTimer -= Time.deltaTime;
        if (chargeTimer > 0) chargeTimer -= Time.deltaTime;
        if (hornTimer > 0) hornTimer -= Time.deltaTime;

        if (currentState != BossState.Stunned)
        {
            if (vineTimer > 0) vineTimer -= Time.deltaTime;

            if (vineTimer <= 0 && Vector2.Distance(transform.position, player.position) <= bossVine.detectionRadius)
            {
                vineTimer = bossVine.attackInterval;
                bossVine.TriggerVine(player.position);
            }
        }
    }

    private void CheckPatterns()
    {
        if (currentState != BossState.Chase) return;
        float distance = Vector2.Distance(transform.position, player.position);

        PlayerController playerCtrl = player.GetComponent<PlayerController>();
        bool isPlayerOnPillar = (playerCtrl != null && playerCtrl.isLadder);

        if (distance <= bossHorn.hornRange && hornTimer <= 0f && !isPlayerOnPillar)
        {
            hornTimer = bossHorn.hornCooldown;
            StartCoroutine(ExecutePattern(bossHorn.HornRoutine()));
            return;
        }

        if (distance > bossHorn.hornRange && distance <= bossSmash.smashRange && smashTimer <= 0f && !isPlayerOnPillar)
        {
            smashTimer = bossSmash.smashCooldown;
            StartCoroutine(ExecutePattern(bossSmash.SmashRoutine()));
            return;
        }

        if (isPlayerOnPillar && distance <= bossCharge.chargeRange && chargeTimer <= 0f)
        {
            chargeTimer = bossCharge.chargeCooldown;
            StartCoroutine(ExecutePattern(bossCharge.ChargeRoutine(player)));
            return;
        }

        if (!isPlayerOnPillar && distance > bossSmash.smashRange && vineTimer <= 0f)
        {

        }
    }

    private IEnumerator ExecutePattern(IEnumerator patternRoutine)
    {
        currentState = BossState.Attacking;

        if (chaseScript != null) chaseScript.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return StartCoroutine(patternRoutine);

        if (currentState != BossState.Stunned)
        {
            currentState = BossState.Chase;
            if (chaseScript != null) chaseScript.enabled = true;
        }
    }

    public void EnterStun(float stunDuration)
    {
        StartCoroutine(StunRoutine(stunDuration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        currentState = BossState.Stunned;
        chaseScript.enabled = false;

        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // 1. 그로기 진입
        if (lockOnMark != null) lockOnMark.SetActive(true);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.gray;

        Debug.Log("보스 그로기: 약점 노출!");
        yield return new WaitForSeconds(duration);

        // 2. 그로기 종료 
        if (lockOnMark != null) lockOnMark.SetActive(false);
        if (sr != null) sr.color = Color.white;

        Debug.Log("보스 그로기 종료, 추적 재개");

        chargeTimer = 3.0f;
        smashTimer = 1.5f;

        currentState = BossState.Chase;
    }
}