// Phase 1+3 — WaterMonsterCombatState
// Phase 1: ShouldTransitionToGroggy override (MaxWater=0 support)
// Phase 3: Add teleport condition to SelectAttackStrategy (D-12, D-13)

using WaterMonster.Phase2;

public class WaterMonsterCombatState : CombatState
{
    protected override bool ShouldTransitionToGroggy(BossController boss)
    {
        return false;
    }

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        // D-13: Teleport condition — Indestructible count >= 2 and cooldown elapsed
        bool canTeleport = PuddleStackManager.Instance != null
            && PuddleStackManager.Instance.IndestructibleCount >= 2
            && boss is WaterMonsterController wmc
            && wmc.CanTeleport();

        if (canTeleport)
        {
            // D-12: Integrate teleport as an attack strategy candidate
            // Return null after ChangeState to stop the base Execute() from continuing current frame's attack
            boss.ChangeState(new WaterTeleportState());
            return null;
        }

        // D-08: Original patterns (dist <= 3.0f melee, else ranged)
        if (dist <= 3.0f)
        {
            return new WaterMeleeSwipe();
        }
        return new WaterRangedSpit();
    }
}
