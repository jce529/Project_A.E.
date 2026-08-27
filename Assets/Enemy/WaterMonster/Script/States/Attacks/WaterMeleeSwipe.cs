// Phase 1 — WaterMeleeSwipe (melee IAttackStrategy)
using UnityEngine;

public class WaterMeleeSwipe : IAttackStrategy
{
    public float Cooldown => 1.4f;
    public string AnimationName => "Attack_Melee";

    private const float HpCostPercent = 0.03f;
    private const float HitRadius = 1.2f;
    private const float HitForward = 1.5f;
    private const float MeleeDamage = 10f;

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);

        if (boss.Stats is WaterMonsterStats wms)
        {
            wms.SpendHpCost(wms.MaxHealth * HpCostPercent);
        }

        Vector3 origin = boss.transform.position + boss.transform.right * HitForward;
        var hits = Physics2D.OverlapCircleAll(origin, HitRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            var playerStats = hit.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(MeleeDamage);
            }
        }
    }
}
