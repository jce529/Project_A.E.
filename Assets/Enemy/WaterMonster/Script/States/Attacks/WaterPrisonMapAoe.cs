using System.Collections;
using UnityEngine;

public class WaterPrisonMapAoe : WaterPrisonAttack
{
    public override float Cooldown => 8f;
    public override string AnimationName => WaterMonsterPatterns.PrisonAoe;

    public override void ExecuteAttack(BossController boss)
    {
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, AnimationName);
        WaterMonsterPatterns.StartPattern(boss, PrisonMapAoeSequence(boss));
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
