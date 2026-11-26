using System.Collections;
using UnityEngine;

public class WaterGeyser : IAttackStrategy
{
    private const float WarningTime = 0.7f;
    public float Cooldown => 3f;
    public string AnimationName => WaterMonsterPatterns.Geyser;

    public void ExecuteAttack(BossController boss)
    {
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, AnimationName);
        WaterMonsterPatterns.StartPattern(boss, GeyserSequence(boss));
    }

    private IEnumerator GeyserSequence(BossController boss)
    {
        Vector3 strikePos = boss.Target.position;
        if (boss is WaterMonsterController wmc && wmc.GeyserEffectPrefab != null)
        {
            var go = Object.Instantiate(wmc.GeyserEffectPrefab, strikePos, Quaternion.identity);
            var effect = go.GetComponent<GeyserEffect>();
            if (effect != null)
            {
                effect.Configure(WarningTime * wmc.EnhanceWindupMul, wmc.EnhanceDamageMul, wmc.EnhanceRadiusMul);
                yield return new WaitForSeconds(effect.HitDelay);
            }
        }
    }
}
