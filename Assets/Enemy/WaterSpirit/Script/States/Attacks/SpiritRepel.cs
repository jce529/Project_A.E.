using UnityEngine;

public class SpiritRepel : IAttackStrategy
{
    public float Cooldown => 1.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        if (!(boss is SpiritController spirit)) return;

        Debug.Log("[SpiritRepel] 튕겨내기 공격 실행");

        Vector2 origin = boss.transform.position;
        
        // RepelRange 내의 Player 레이어 객체 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, spirit.RepelRange, LayerMask.GetMask("Player"));

        foreach (var hit in hits)
        {
            // PlayerStats와 PlayerController는 보통 부모 오브젝트에 위치함
            var playerStats = hit.GetComponentInParent<PlayerStats>();
            var playerCtrl = hit.GetComponentInParent<PlayerController>();

            // 넉백 방향: 보스에서 플레이어 쪽으로 밀어내는 방향
            Vector2 knockDir = ((Vector2)hit.transform.position - origin).normalized;
            if (knockDir == Vector2.zero) knockDir = Vector2.up; // 겹쳐있을 경우 위로 튕김

            if (playerStats != null)
            {
                playerStats.TakeDamage(spirit.RepelDamage);
                Debug.Log($"[SpiritRepel] Hit Player! Damage: {spirit.RepelDamage}");
            }

            if (playerCtrl != null)
            {
                playerCtrl.ApplyKnockback(knockDir, spirit.RepelForce);
                Debug.Log($"[SpiritRepel] Apply Knockback! Force: {spirit.RepelForce}");
            }
        }
    }
}
