using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : PlayerAttackBase
{
    [Header("공격 기본 설정")]
    public float damage = 10f;
    public GameObject attackBox;
    public float attackDistance = 1.5f;
    public float attackDuration = 0.2f;

    [Header("E 스킬 (섬광참) 설정")]
    public float teleportDistance = 3f;
    public GameObject slashHitboxPrefab;

    [Header("R 스킬 (파동참) 설정")]
    public float radius = 2.5f;
    public GameObject waveEffectPrefab;

    [Header("기타 컴포넌트")]
    public WaterController waterController;
    public PlayerStats playerStats;

    protected override void Start()
    {
        base.Start();
        if (waterController == null) waterController = GetComponent<WaterController>();
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
    }

    protected override void OnBasicAttack()
    {
        if (Camera.main == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mousePos - (Vector2)transform.position).normalized;
        Vector2 spawnPos = (Vector2)transform.position + direction * attackDistance;

        if (attackBox != null)
        {
            GameObject atkBox = Instantiate(attackBox, spawnPos, Quaternion.identity);
            atkBox.transform.right = direction;

            var damager = atkBox.GetComponent<PlayerAttackDamager>();
            if (damager != null)
            {
                damager.playerAttack = this;
                damager.SetOverrideType(DamageType.Normal);
            }

            StartCoroutine(DisableAfterTime(atkBox, attackDuration));
        }
    }

    protected override void OnSkillE()
    {
        if (waterController == null || slashHitboxPrefab == null || Camera.main == null) return;

        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < 1) return;

        waterController.UseBottle(1);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos.x > transform.position.x) ? Vector2.right : Vector2.left;

        Vector2 targetPos = new Vector2(transform.position.x + (direction.x * teleportDistance), transform.position.y);
        GameObject slash = Instantiate(slashHitboxPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        slash.transform.right = direction;

        var damager = slash.GetComponent<PlayerAttackDamager>();
        if (damager != null)
        {
            damager.playerAttack = this;
            damager.SetOverrideType(DamageType.Other);
        }

        Destroy(slash, 0.1f);
        transform.position = targetPos;
    }

    protected override void OnSkillR()
    {
        var ws = GetComponent<WaveSlice>();
        if (ws != null)
        {
            ws.waveSlice();
        }
        else
        {
            // 폴백 (컴포넌트 없을 시)
            if (waterController != null && waveEffectPrefab != null)
            {
                if (waterController.waterCounter() + waterController.corruptedwaterCounter() >= 2)
                {
                    waterController.UseBottle(2);
                    GameObject wave = Instantiate(waveEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(wave, 1.0f);
                }
            }
        }
    }

    protected override void OnHeal()
    {
        if (waterController == null || playerStats == null) return;
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < 1) return;

        waterController.UseBottle(1);
        playerStats.Heal(1);
    }

    private IEnumerator DisableAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        if (obj != null) Destroy(obj);
    }

    public void Attack() { }
    public void OnAttack() { }
    public void Hit() { }
    public void Footstep() { }
    public void OnExecute() { }
    public void AnimationEvent_Attack() { }
}
