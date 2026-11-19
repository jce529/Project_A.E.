using UnityEngine;
using System.Collections;

public class SpiritFarProjectile : IAttackStrategy
{
    public float Cooldown => 2.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        boss.StartCoroutine(FarProjectileRoutine(boss));
    }

    private IEnumerator FarProjectileRoutine(BossController boss)
    {
        if (!(boss is SpiritController spirit)) yield break;
        if (boss.Target == null) yield break;

        // 돌진 텔레포트 거리(MaxTeleportRadius)의 2배 위치로 순간이동
        Vector2 playerPos = boss.Target.position;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = spirit.MaxTeleportRadius * 2f;
        Vector2 teleportPos = playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

        boss.transform.position = teleportPos;
        boss.StopMove();

        // 조준 대기
        yield return new WaitForSeconds(0.4f);

        new SpiritProjectileAttack().ExecuteAttack(boss);
    }
}
