// Phase 1 — WaterRangedSpit (ranged IAttackStrategy)
using UnityEngine;

public class WaterRangedSpit : IAttackStrategy
{
    public float Cooldown => 2.0f;
    public string AnimationName => "Attack_Ranged";

    private const float HpCostPercent = 0.05f;
    private const float SpawnForward = 1.0f;

    public void ExecuteAttack(BossController boss)
    {
        Debug.Log($"[WaterMonster] 원거리 공격 실행 (Attack_Ranged)");
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);

        if (boss.Stats is WaterMonsterStats wms)
        {
            wms.SpendHpCost(wms.MaxHealth * HpCostPercent);
        }

        Vector3 spawnPos = boss.transform.position + boss.transform.right * SpawnForward;
        CombatSpawner.SpawnProjectile(spawnPos, boss.transform.right);
    }
}
