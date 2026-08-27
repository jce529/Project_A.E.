using UnityEngine;

public class HeavyAttack : IAttackStrategy
{
    public float Cooldown => 5.0f;
    public string AnimationName => "Attack_Heavy";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger("Attack_Heavy");
        boss.StartHeavyAttackCooldown(Cooldown);

        // [변경] 강공 물 소모 (10%)
        if (boss.Stats != null)
        {
            boss.Stats.ConsumeWater(boss.Stats.MaxWater * 0.10f);
        }

    }
}