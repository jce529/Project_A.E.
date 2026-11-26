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

        spirit.PlayFx(SpiritFx.Flash);
        boss.transform.position = teleportPos;
        spirit.PlayFx(SpiritFx.Flash);
        FaceTarget(boss, playerPos);

        // 목적지 다시 계산 (텔레포트 후 위치 기준)
        Vector2 bossPos = boss.transform.position;
        Vector2 dir = (playerPos - bossPos).normalized;
        if (dir == Vector2.zero) dir = boss.transform.right; // 겹쳤을 때 안전 처리
        
        Vector2 targetPos = playerPos + dir * spirit.OvershotDistance;

        // 2단계: Windup (기항 대기)
        boss.StopMove();
        spirit.PlayAnim("Charge");
        var windupFx = spirit.PlayFx(SpiritFx.Windup, null, 0f, 1f, true, boss.transform);
        // 기 모으는 동안 플레이어 쪽을 계속 바라본다
        for (float t = 0f; t < spirit.ChargeWindup; t += Time.deltaTime)
        {
            if (boss.Target != null) FaceTarget(boss, boss.Target.position);
            yield return null;
        }
        if (windupFx != null) Object.Destroy(windupFx);
        FaceTarget(boss, targetPos);

        // 3단계: 돌진 시작
        spirit.SetCharging(true); // 데미지 판정 활성화
        // 돌진 잔상: 스프라이트의 머리가 왼쪽이므로 진행 반대 방향으로 꼬리가 오도록 180° 회전
        float trailRot = Mathf.Atan2(targetPos.y - boss.transform.position.y, targetPos.x - boss.transform.position.x) * Mathf.Rad2Deg + 180f;
        var trailFx = spirit.PlayFx(SpiritFx.Charge, null, trailRot, 1f, true, boss.transform);

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
        if (trailFx != null) Object.Destroy(trailFx);
    }

    // 스피릿 스프라이트는 머리가 왼쪽을 향하므로(scale.x > 0 = 왼쪽을 봄) 타깃이 오른쪽이면 x 스케일을 음수로 둔다.
    // BossController.LookAtTarget 은 private 이고 반대 기준이라 쓰지 않는다.
    private static void FaceTarget(BossController boss, Vector2 point)
    {
        Vector3 s = boss.transform.localScale;
        float x = Mathf.Abs(s.x);
        if (point.x > boss.transform.position.x) boss.transform.localScale = new Vector3(-x, s.y, s.z);
        else if (point.x < boss.transform.position.x) boss.transform.localScale = new Vector3(x, s.y, s.z);
    }
}
