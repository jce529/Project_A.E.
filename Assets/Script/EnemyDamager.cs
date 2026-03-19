using UnityEngine;

/// <summary>
/// Damager를 상속받아, 적이 몸으로 부딪혔을 때 데미지를 주는 기능을 구현한 클래스.
/// 이 스크립트는 적(Enemy) 게임 오브젝트에 부착합니다.
/// </summary>
public class EnemyDamager : Damager
{
    [Header("적 공격 설정")]
    [Tooltip("적이 플레이어와 충돌했을 때 입힐 데미지 양")]
    public float contactDamage = 15f;

    [Header("넉백 설정")]
    [Tooltip("플레이어가 밀려나는 힘의 크기")]
    public float knockbackForce = 10f;

    [Tooltip("넉백 방향이 수평으로만 적용될지 여부 (예: 2D 횡스크롤용)")]
    public bool horizontalOnly = true;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic; 
    }

    // ⭐ 실제 물리 충돌 발생 시 호출됨 (Trigger 아님)
    private new void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject target = collision.gameObject;

        // 지정된 데미지 가능한 레이어에 속해 있는지 확인
        if (((1 << target.layer) & damageableLayers) == 0)
            return;

        HP targetHP = target.GetComponent<HP>();
        if (targetHP != null)
        {
            ApplyDamageEffect(targetHP); // Damager 시스템의 일관성 유지
            ApplyKnockback(targetHP.transform);
        }
    }


    /// <summary>
    /// 부모 클래스(Damager)의 추상 메서드를 구현(override)합니다.
    /// Damager가 유효한 타겟(HP 컴포넌트가 있고, 지정된 레이어에 속한)을 찾으면 이 메서드를 호출해줍니다.
    /// </summary>
    /// <param name="targetHP">데미지를 받을 대상의 HP 컴포넌트 (이 경우 플레이어의 HP)</param>
    protected override void ApplyDamageEffect(HP targetHP)
    {
        // 대상에게 설정된 만큼의 접촉 데미지를 입힙니다.
        targetHP.TakeDamage(contactDamage);
    }

    private void ApplyKnockback(Transform target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return;

        // 방향 계산: 적 → 플레이어
        Vector2 knockbackDir = (target.position - transform.position).normalized;
        if (horizontalOnly) knockbackDir.y = 0;

        var controller = target.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplyKnockback(knockbackDir, knockbackForce);
            return; // PlayerController 쪽에서 AddForce 이미 처리하므로 여기서 끝
        }

        targetRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }
}