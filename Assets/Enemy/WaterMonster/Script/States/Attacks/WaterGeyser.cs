using System.Collections;
using UnityEngine;

public class WaterGeyser : IAttackStrategy
{
    public float Cooldown => 3f;
    public string AnimationName => "Attack_Geyser";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);
        boss.StartCoroutine(GeyserSequence(boss));
    }

    private IEnumerator GeyserSequence(BossController boss)
    {
        Vector3 strikePos = boss.Target.position;
        yield return new WaitForSeconds(0.7f);

        if (boss is WaterMonsterController wmc && wmc.GeyserEffectPrefab != null)
            Object.Instantiate(wmc.GeyserEffectPrefab, strikePos, Quaternion.identity);
    }
}
