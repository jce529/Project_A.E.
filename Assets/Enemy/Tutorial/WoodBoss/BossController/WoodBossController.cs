using System;
using System.Collections;
using UnityEngine;
using WoodBoss;

public class WoodBossController : BossController
{
    [Header("References")]
    private HP _hp;

    [Header("Wood Boss Specifics")]
    public GameObject RootSpikePrefab;
    public GameObject WarningPrefab;
    public GameObject SweepPrefab;

    private WoodBossStatsSystem _woodStats;

    protected override void Awake()
    {
        _hp = GetComponent<HP>();
        base.Awake();
        _woodStats = GetComponent<WoodBossStatsSystem>();

        // ── 보스 물리 고정 ──────────────────────────────────────────
        // 충돌/공격을 맞아도 절대 밀리거나 회전하지 않도록 설정
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // 외부 물리력 완전 무시
        }
        // ────────────────────────────────────────────────────────────
    }

    private void Start()
    {
        if (_hp != null)
        {
            _hp.ManualDeath = true;
            _hp.OnDeath += HandleDeath;
        }

        ChangeState(new WoodBoss.IdleState());
    }

    private void HandleDeath()
    {
        StopAllCoroutines();
        Debug.Log("보스 사망");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Debug.Log("사망 연출...");

        // Anim.SetTrigger("Die");

        yield return new WaitForSeconds(2.5f);

        if (UIManager.Instance != null && UIManager.Instance.ClearPanel != null)
        {
            Debug.Log("UI 전환");
            UIManager.Instance.PushPanel(UIManager.Instance.ClearPanel);
        }
    }
}