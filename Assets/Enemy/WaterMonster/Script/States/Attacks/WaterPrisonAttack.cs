using System.Collections;
using UnityEngine;

public class WaterPrisonAttack : IAttackStrategy
{
    protected const float CondenseTime = 0.6f;
    public virtual float Cooldown => 5f;
    public virtual string AnimationName => WaterMonsterPatterns.Prison;

    public virtual void ExecuteAttack(BossController boss)
    {
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, AnimationName);
        WaterMonsterPatterns.StartPattern(boss, PrisonSequence(boss));
    }

    protected IEnumerator FirePrisonShots(BossController boss)
    {
        if (!(boss is WaterMonsterController wmc)) yield break;
        if (wmc.PrisonProjectilePrefab == null) yield break;

        yield return new WaitForSeconds(CondenseTime);

        for (int i = 0; i < 2; i++)
        {
            Vector3 dir = (boss.Target.position - boss.transform.position).normalized;
            Vector3 spawnPos = boss.transform.position + dir * 0.8f;
            var go = Object.Instantiate(wmc.PrisonProjectilePrefab, spawnPos, Quaternion.identity);
            var proj = go.GetComponent<WaterPrisonProjectile>();
            if (proj != null) proj.Direction = dir;

            if (i < 1) yield return new WaitForSeconds(0.6f);
        }
    }

    private IEnumerator PrisonSequence(BossController boss)
    {
        yield return boss.StartCoroutine(FirePrisonShots(boss));
    }
}
