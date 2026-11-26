using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBurst : IAttackStrategy
{
    private const float AbsorbTime = 0.6f;
    private const float HoldTime = 0.4f;
    private const float BaseRadius = 4f;
    private const float BaseDamage = 25f;

    public float Cooldown => 5f;
    public string AnimationName => WaterMonsterPatterns.Burst;

    public void ExecuteAttack(BossController boss)
    {
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, AnimationName);
        WaterMonsterPatterns.StartPattern(boss, BurstSequence(boss));
    }

    private IEnumerator BurstSequence(BossController boss)
    {
        boss.StopMove();
        var wmc = boss as WaterMonsterController;
        yield return new WaitForSeconds(AbsorbTime * (wmc != null ? wmc.EnhanceWindupMul : 1f));
        yield return new WaitForSeconds(HoldTime);

        var hits = Physics2D.OverlapCircleAll(boss.transform.position,
            BaseRadius * (wmc != null ? wmc.EnhanceRadiusMul : 1f), LayerMask.GetMask("Player"));
        var damagedPlayers = new HashSet<PlayerStats>();
        foreach (var hit in hits)
        {
            var ps = hit.GetComponentInParent<PlayerStats>();
            if (ps != null && damagedPlayers.Add(ps))
                ps.TakeDamage(BaseDamage * (wmc != null ? wmc.EnhanceDamageMul : 1f));
        }
    }
}
