using System.Collections;
using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TutorialGroggyState - 그로기 상태
    //
    // [연출 순서]
    //   Phase 1: CoreTransform이 CoreNormalY → CoreGroggyY로 서서히 낙하
    //   Phase 2: GroggyDuration 동안 바닥에 머무름 (플레이어 공격 기회)
    //   Phase 3: CoreTransform이 CoreGroggyY → CoreNormalY로 서서히 복귀
    //   Phase 4: TutorialIdleState로 전환
    //
    // 코어의 피격 콜라이더는 그로기 중에도 활성화 상태 → 플레이어가 타격 가능
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialGroggyState : IBossState
    {
        public void Enter(BossController boss)
        {
            var tb = boss as TutorialBossController;
            if (tb == null) return;

            tb.SetGroggyFlag(true); // IsGroggy = true → 중복 진입 차단
            boss.StopMove();
            boss.Anim?.SetTrigger("GroggyTrigger");
            Debug.Log("[TutorialBoss] ──→ Groggy 상태! 코어 낙하 시작");

            // 그로기 전체 시퀀스(낙하 → 대기 → 복귀)를 코루틴으로 실행
            boss.StartCoroutine(GroggySequence(tb));
        }

        public void Execute(BossController boss)
        {
            // 모든 로직은 GroggySequence 코루틴 안에서 처리
            // 이 상태에서 보스는 공격하지 않고 플레이어에게 피격 기회를 줌
        }

        public void Exit(BossController boss)
        {
            var tb = boss as TutorialBossController;
            tb?.SetGroggyFlag(false); // 그로기 종료 시 플래그 해제
        }

        /// <summary>
        /// 그로기 전체 연출 코루틴.
        /// MoveCoreToY()를 두 번 호출하여 낙하 → 대기 → 복귀 순서로 처리.
        /// </summary>
        private IEnumerator GroggySequence(TutorialBossController tb)
        {
            // 그로기 진입 시점의 현재 Y를 기준으로 상대 이동
            float normalY = tb.CoreTransform.position.y;

            // ── Phase 1: 코어 낙하 (현재 위치에서 CoreGroggyY만큼 아래로) ─
            Debug.Log($"[TutorialBoss Groggy] 코어 낙하: {normalY} → {normalY + tb.CoreGroggyY}");
            yield return tb.StartCoroutine(tb.MoveCoreToY(normalY + tb.CoreGroggyY, tb.CoreMoveDuration));

            // ── Phase 2: 바닥 대기 (플레이어가 코어를 공격하는 시간) ─────
            Debug.Log($"[TutorialBoss Groggy] 코어 바닥 유지 ({tb.GroggyDuration}초) - 공격하세요!");
            yield return new WaitForSeconds(tb.GroggyDuration);

            // ── Phase 3: 일어나기 애니메이션 + 코어 복귀 동시 시작 ──────
            tb.Anim?.SetTrigger("GroggyExitTrigger");
            Debug.Log($"[TutorialBoss Groggy] 코어 복귀: {normalY + tb.CoreGroggyY} → {normalY}");
            yield return tb.StartCoroutine(tb.MoveCoreToY(normalY, tb.CoreMoveDuration));

            // ── Phase 4: 애니메이션이 코어 이동보다 길 경우 마무리 대기 ──
            if (tb.GroggyExitDuration > 0f)
            {
                Debug.Log($"[TutorialBoss Groggy] 일어나기 마무리 대기 ({tb.GroggyExitDuration}초)");
                yield return new WaitForSeconds(tb.GroggyExitDuration);
            }

            // ── Phase 5: Idle 상태로 복귀 ────────────────────────────────
            Debug.Log("[TutorialBoss Groggy] 그로기 종료 → Idle");
            tb.ChangeState(new TutorialIdleState());
        }
    }
}
