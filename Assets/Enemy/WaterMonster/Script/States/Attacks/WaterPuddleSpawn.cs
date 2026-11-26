using System.Collections;
using UnityEngine;
using WaterMonster.Phase2;

public class WaterPuddleSpawn : IAttackStrategy
{
    public float Cooldown => 6f;
    public string AnimationName => WaterMonsterPatterns.PuddleSpawn;

    public void ExecuteAttack(BossController boss)
    {
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, AnimationName);
        WaterMonsterPatterns.StartPattern(boss, SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(0.5f);
        var system = WaterMonsterPuddleSystem.Instance;
        if (system != null) system.SpawnPuddles(system.SpawnPerPattern);
    }
}