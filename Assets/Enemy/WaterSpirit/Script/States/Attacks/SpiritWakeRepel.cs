using UnityEngine;
using System.Collections;

public class SpiritWakeRepel : IAttackStrategy
{
    public float Cooldown => 1.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        boss.StartCoroutine(WakeRepelRoutine(boss));
    }

    private IEnumerator WakeRepelRoutine(BossController boss)
    {
        // 깨어나는 딜레이 (취약 종료 직전 짧은 예고)
        yield return new WaitForSeconds(0.4f);
        new SpiritRepel().ExecuteAttack(boss);
    }
}
