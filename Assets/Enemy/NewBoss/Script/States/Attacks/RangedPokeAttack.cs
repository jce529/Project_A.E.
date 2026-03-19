using UnityEngine;

public class RangedPokeAttack : IAttackStrategy
{
    public float Cooldown => 2.0f;
    public string AnimationName => "Attack_Ranged";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger("Attack_Ranged");

        // [변경] 여기서 직접 물을 소모합니다 (1%)
        if (boss.Stats != null)
        {
            boss.Stats.ConsumeWater(boss.Stats.MaxWater * 0.01f);
        }

        Debug.Log("보스: 원거리 견제 발사! (물 1% 소모)");
    }
}