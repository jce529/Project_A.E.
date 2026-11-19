using System.Collections;
using UnityEngine;

public class WaterPrisonMapAoe : WaterPrisonAttack
{
    public override float Cooldown => 8f;

    public override void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);
        boss.StartCoroutine(PrisonMapAoeSequence(boss));
    }

    private IEnumerator PrisonMapAoeSequence(BossController boss)
    {
        yield return boss.StartCoroutine(FirePrisonShots(boss));
        yield return new WaitForSeconds(2.5f);

        var playerStats = PlayerStats.Instance;
        if (playerStats == null) yield break;

        var playerGo = playerStats.gameObject;
        if (playerGo.GetComponent<PrisonDebuff>() == null)
            playerStats.TakeDamage(60f);
    }
}
