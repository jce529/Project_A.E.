using System.Collections;
using UnityEngine;

public class WaterPrisonAttack : IAttackStrategy
{
    public virtual float Cooldown => 5f;
    public string AnimationName => "Attack_Prison";

    public virtual void ExecuteAttack(BossController boss)
    {
        if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);
        boss.StartCoroutine(PrisonSequence(boss));
    }

    protected IEnumerator FirePrisonShots(BossController boss)
    {
        if (!(boss is WaterMonsterController wmc)) yield break;
        if (wmc.PrisonProjectilePrefab == null) yield break;

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
