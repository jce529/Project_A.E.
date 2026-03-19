using UnityEngine;

public class LightAttack : IAttackStrategy
{
    public float Cooldown => 1.5f;
    public string AnimationName => "Attack_Light";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger("Attack_Light");

        // [변경] 평타 물 소모 (5%)
        if (boss.Stats != null)
        {
            boss.Stats.ConsumeWater(boss.Stats.MaxWater * 0.05f);
        }

        Debug.Log("보스: 평타 베기! (물 5% 소모)");
    }
}