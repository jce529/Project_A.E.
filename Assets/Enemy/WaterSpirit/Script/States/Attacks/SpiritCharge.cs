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

        // 1단계: 플레이어 주변으로 텔레포트 (D-05b 패턴 활용)
        Vector2 playerPos = boss.Target.position;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(spirit.MinTeleportRadius, spirit.MaxTeleportRadius);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        Vector2 teleportPos = playerPos + offset;

        boss.transform.position = teleportPos;

        // 목적지 다시 계산 (텔레포트 후 위치 기준)
        Vector2 bossPos = boss.transform.position;
        Vector2 dir = (playerPos - bossPos).normalized;
        if (dir == Vector2.zero) dir = boss.transform.right; // 겹쳤을 때 안전 처리
        
        Vector2 targetPos = playerPos + dir * spirit.OvershotDistance;

        // 2단계: Windup (기항 대기)
        boss.StopMove();
        yield return new WaitForSeconds(spirit.ChargeWindup);

        // 3단계: 돌진 시작
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
    }
}
