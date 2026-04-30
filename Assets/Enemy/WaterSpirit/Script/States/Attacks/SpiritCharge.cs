using UnityEngine;
using System.Collections;

public class SpiritCharge : IAttackStrategy
{
    public float Cooldown => 3.0f;
    public string AnimationName => ""; // 로직 우선 구현, 애니메이션은 후순위

    public void ExecuteAttack(BossController boss)
    {
        boss.StartCoroutine(ChargeRoutine(boss));
    }

    private IEnumerator ChargeRoutine(BossController boss)
    {
        if (!(boss is SpiritController spirit)) yield break;
        if (boss.Target == null) yield break;

        // 1단계: Windup (기항 대기)
        Debug.Log("[SpiritCharge] 돌진 기항 시작");
        boss.StopMove();
        yield return new WaitForSeconds(spirit.ChargeWindup);

        // 2단계: 목표점 계산 (현재 플레이어 위치 + 진행 방향으로 OvershotDistance 만큼 더 이동)
        Vector2 bossPos = boss.transform.position;
        Vector2 playerPos = boss.Target.position;
        Vector2 dir = (playerPos - bossPos).normalized;
        Vector2 targetPos = playerPos + dir * spirit.OvershotDistance;

        Debug.Log($"[SpiritCharge] 돌진 시작! 목표점: {targetPos}");

        // 3단계: 돌진 이동
        spirit.SetCharging(true); // 데미지 판정 활성화

        float timeout = 2.0f; // 무한 루프 방지용 타임아웃
        float elapsedTime = 0f;

        while (Vector2.Distance(boss.transform.position, targetPos) > 0.3f && elapsedTime < timeout)
        {
            Vector2 currentPos = boss.transform.position;
            Vector2 moveDir = (targetPos - currentPos).normalized;
            
            spirit.SetVelocity(moveDir * spirit.ChargeSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 4단계: 정지 및 초기화
        boss.StopMove();
        spirit.SetCharging(false);
        Debug.Log("[SpiritCharge] 돌진 종료");
    }
}
