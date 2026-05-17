using UnityEngine;

public class SpiritExhaustion : IAttackStrategy
{
    public float Cooldown => 2.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        boss.StopMove();
        Debug.Log("[SpiritExhaustion] 기진맥진 - 잠시 정지 (취약 상태)");
    }
}
