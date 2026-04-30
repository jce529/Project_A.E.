using UnityEngine;

public class SpiritCombatState : CombatState
{
    public override void Enter(BossController boss)
    {
        base.Enter(boss);
        Debug.Log("[SpiritCombatState] 정령 보스 전투 상태 진입");
    }

    protected override bool ShouldTransitionToGroggy(BossController boss)
    {
        // D-01d: Spirit boss does not have a groggy state in Phase 1
        return false;
    }

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is SpiritController spirit)) return null;

        // D-03a: Priority based on distance
        // 1. Close range -> Repel
        if (dist <= spirit.RepelRange)
        {
            return new SpiritRepel();
        }

        // 2. Mid range -> Charge
        if (dist <= spirit.ChargeRange)
        {
            return new SpiritCharge();
        }

        // 3. Long range -> Projectile
        return new SpiritProjectileAttack();
    }
}
