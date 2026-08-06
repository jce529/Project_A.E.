using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TutorialIdleState - 대기 상태
    //
    // 모든 공격은 이 상태를 거쳐야만 다시 실행될 수 있다 (§1 ⑧, §5).
    // 진입 후 PostAttackIdleTime(기본 2초) 동안 대기한 뒤에만 SelectPattern()으로
    // 다음 패턴 탐색을 시작한다. 조건을 만족하는 패턴이 없으면 Idle을 유지하고
    // 다음 프레임에 다시 시도한다.
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialIdleState : IBossState
    {
        private float _idleTimer;

        public void Enter(BossController boss)
        {
            var tb = boss as TutorialBossController;
            _idleTimer = tb != null ? tb.PostAttackIdleTime : 2f;

            boss.StopMove();
            boss.Anim?.SetTrigger("IdleTrigger");
            Debug.Log("[TutorialBoss] ──→ Idle 상태");
        }

        public void Execute(BossController boss)
        {
            var tb = boss as TutorialBossController;
            if (tb == null || !boss.TargetFound) return;

            if (_idleTimer > 0f)
            {
                _idleTimer -= Time.deltaTime;
                return;
            }

            PatternType? pattern = tb.SelectPattern();
            if (pattern.HasValue)
                boss.ChangeState(new TutorialAttackState(pattern.Value));
            // candidate가 없으면 Idle 유지, 다음 프레임에 재시도
        }

        public void Exit(BossController boss) { }
    }
}
