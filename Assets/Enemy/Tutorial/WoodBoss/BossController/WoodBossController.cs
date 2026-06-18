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

    [Header("클리어 UI")]
    [SerializeField] private GameObject clearPanel;

    private WoodBossStatsSystem _woodStats;

    protected override void Awake()
    {
        _hp = GetComponent<HP>();
        base.Awake();
        _woodStats = GetComponent<WoodBossStatsSystem>();

        // ���� ���� ���� ���� ������������������������������������������������������������������������������������
        // �浹/������ �¾Ƶ� ���� �и��ų� ȸ������ �ʵ��� ����
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // �ܺ� ������ ���� ����
        }
        // ������������������������������������������������������������������������������������������������������������������������
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
        Debug.Log("���� ���");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Debug.Log("��� ����...");

        // Anim.SetTrigger("Die");

        yield return new WaitForSeconds(2.5f);

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
            GameStateManager.Instance?.SetState(GameStateManager.GameState.Paused);
        }
    }
}