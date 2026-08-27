using UnityEngine;

public class SpiritExhaustion : IAttackStrategy
{
    public float Cooldown => 2.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        boss.StopMove();
    }
}
