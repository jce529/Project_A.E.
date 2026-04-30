using UnityEngine;

// Stubs for Spirit Boss attack strategies to ensure compilation of SpiritCombatState.
// These will be fully implemented in Plan 05-02.

public class SpiritCharge : IAttackStrategy
{
    public float Cooldown => 3.0f;
    public string AnimationName => "Charge";
    public void ExecuteAttack(BossController boss) { Debug.Log("[Stub] SpiritCharge executed"); }
}

public class SpiritProjectileAttack : IAttackStrategy
{
    public float Cooldown => 2.5f;
    public string AnimationName => "Attack_Ranged";
    public void ExecuteAttack(BossController boss) { Debug.Log("[Stub] SpiritProjectileAttack executed"); }
}

public class SpiritRepel : IAttackStrategy
{
    public float Cooldown => 4.0f;
    public string AnimationName => "Repel";
    public void ExecuteAttack(BossController boss) { Debug.Log("[Stub] SpiritRepel executed"); }
}
