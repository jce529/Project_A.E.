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
            // HP와 PlayerController는 보통 부모 오브젝트에 위치함
            var hp = hit.GetComponentInParent<HP>();
            var playerCtrl = hit.GetComponentInParent<PlayerController>();

            // 넉백 방향: 보스에서 플레이어 쪽으로 밀어내는 방향
            Vector2 knockDir = ((Vector2)hit.transform.position - origin).normalized;
            if (knockDir == Vector2.zero) knockDir = Vector2.up; // 겹쳐있을 경우 위로 튕김

            if (hp != null)
            {
                hp.TakeDamage(spirit.RepelDamage);
                Debug.Log($"[SpiritRepel] Hit Player! Target: {hit.name}, Damage: {spirit.RepelDamage}");
            }
            else
            {
                Debug.LogWarning($"[SpiritRepel] Detected object on Player layer ({hit.name}), but no HP component found in parent!");
            }

            if (playerCtrl != null)
            {
                playerCtrl.ApplyKnockback(knockDir, spirit.RepelForce);
                Debug.Log($"[SpiritRepel] Apply Knockback! Force: {spirit.RepelForce}");
            }
        }
    }
}
