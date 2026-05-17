using UnityEngine;
using System.Collections;

public class SpiritStealth : IAttackStrategy
{
    public float Cooldown => 2.0f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss) => boss.StartCoroutine(StealthRoutine(boss));

    // SpiritController에서 직접 호출할 수 있도록 public으로 변경 및 이름 매칭
    public IEnumerator StealthRoutine(BossController boss)
    {
        if (!(boss is SpiritController spirit)) yield break;

        // 1단계: 모든 Collider2D 비활성화 (피격 불가)
        var colliders = spirit.GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders) c.enabled = false;
        
        // 목적지 확정 (플레이어 주변 랜덤)
        Vector2 destination = spirit.transform.position;
        if (spirit.Target != null)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(spirit.MinTeleportRadius, spirit.MaxTeleportRadius);
            destination = (Vector2)spirit.Target.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        spirit.StopMove();

        // 2단계: 은신 대기
        yield return new WaitForSeconds(spirit.StealthDuration);

        // 3단계: 텔레포트
        spirit.transform.position = destination;
        Debug.Log($"[SpiritStealth] 텔레포트 완료: {destination}");

        // 4단계: 콜라이더 재활성화
        foreach (var c in colliders) if (c != null) c.enabled = true;
    }
}