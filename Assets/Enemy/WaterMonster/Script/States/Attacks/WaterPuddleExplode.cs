using System.Collections;
using UnityEngine;
using WaterMonster.Phase2;

public class WaterPuddleExplode : IAttackStrategy
{
    public float Cooldown => 4f;
    public string AnimationName => WaterMonsterPatterns.PuddleExplode;

    public void ExecuteAttack(BossController boss)
    {
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, AnimationName);
        WaterMonsterPatterns.StartPattern(boss, ExplodeSequence());
    }

    private IEnumerator ExplodeSequence()
    {
        var system = WaterMonsterPuddleSystem.Instance;
        if (system == null || system.Explosion == null || system.Explosion.IsExploding) yield break;

        var aged = system.GetAgedPuddles();
        if (aged.Count == 0) yield break;

        var explosion = system.Explosion;
        int generation = explosion.StartExplosion(aged);
        if (generation == -1) yield break;

        while (explosion.IsRunning(generation)) yield return null;
    }
}