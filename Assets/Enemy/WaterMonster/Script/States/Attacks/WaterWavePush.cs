using System.Collections;
using UnityEngine;

public class WaterWavePush : IAttackStrategy
{
    public float Cooldown => 3f;
    public string AnimationName => WaterMonsterPatterns.Wave;

    public void ExecuteAttack(BossController boss)
    {
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, AnimationName);
        WaterMonsterPatterns.StartPattern(boss, WavePushSequence(boss));
    }

    private IEnumerator WavePushSequence(BossController boss)
    {
        if (!(boss is WaterMonsterController wmc)) yield break;
        if (wmc.WavePrefab == null || wmc.BattleAreaBounds == null) yield break;

        Bounds b = wmc.BattleAreaBounds.bounds;
        float centerY = b.center.y;

        Vector2 firstPos  = new Vector2(b.min.x, centerY);
        Vector2 firstDir  = Vector2.right;
        Vector2 secondPos = new Vector2(b.max.x, centerY);
        Vector2 secondDir = Vector2.left;

        const float firstSpeed = 6f;

        var firstGo = Object.Instantiate(wmc.WavePrefab, firstPos, Quaternion.identity);
        var firstWave = firstGo.GetComponent<WaterWaveProjectile>();
        if (firstWave != null)
        {
            firstWave.Direction = firstDir;
            firstWave.Speed = firstSpeed;
        }

        // 첫 번째 파동이 맵을 완전히 통과한 뒤 두 번째 출발
        yield return new WaitForSeconds((b.max.x - b.min.x) / firstSpeed);

        var secondGo = Object.Instantiate(wmc.WavePrefab, secondPos, Quaternion.identity);
        var secondWave = secondGo.GetComponent<WaterWaveProjectile>();
        if (secondWave != null)
        {
            secondWave.Direction = secondDir;
            secondWave.Speed = 9f;
        }
    }
}
