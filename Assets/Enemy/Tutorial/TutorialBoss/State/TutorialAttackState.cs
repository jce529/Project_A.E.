using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TutorialAttackState - 공격 상태
    //
    // TutorialIdleState에서 SelectPattern()으로 이미 결정된 패턴 하나만 실행한다.
    // 패턴 실행 중에는 다른 판단을 하지 않으며(§1 ③), busyTimer가 0이 되어
    // 애니메이션/판정이 끝난 뒤에만 그로기 여부를 검사하고 Idle 또는 Groggy로 전환한다.
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialAttackState : IBossState
    {
        private readonly PatternType _pattern;

        // 이 패턴이 Attack 상태를 점유하는 시간 (전조+판정+후딜 예상 시간)
        private float _busyTimer;

        public TutorialAttackState(PatternType pattern)
        {
            _pattern = pattern;
        }

        public void Enter(BossController boss)
        {
            var tb = boss as TutorialBossController;
            if (tb == null) return;

            tb.LastUsedPattern = _pattern;      // 연속 사용 금지 판정용 (§5)
            tb.SavePlayerPositionSnapshot();    // 공격 시작 직전 1회만 저장 (§6)
            tb.StartPatternCooldown(_pattern);

            boss.StopMove();
            boss.Anim?.SetTrigger("AttackTrigger");

            switch (_pattern)
            {
                case PatternType.TentacleStab:
                    new TentaclePierceStrategy(tb.WarningPrefab).ExecuteAttack(boss);
                    _busyTimer = tb.TentacleStabActiveDuration;
                    break;

                case PatternType.GroundTentacle:
                    new TutorialRootSpikeStrategy(tb.WarningPrefab, tb.RootSpikePrefab).Execute(boss);
                    _busyTimer = tb.GroundTentacleActiveDuration;
                    break;
            }

            Debug.Log($"[TutorialBoss] ──→ Attack 상태 ({_pattern})");
        }

        public void Execute(BossController boss)
        {
            if (_busyTimer > 0f)
            {
                _busyTimer -= Time.deltaTime;
                return; // 공격 모션 중에는 다른 판단을 하지 않는다 (§1 ③)
            }

            var tb = boss as TutorialBossController;
            if (tb == null) return;

            // 현재 패턴을 끝까지 실행한 뒤에만 그로기 진입 여부를 검사 (§2, 강제 캔슬 없음)
            if (tb.ShouldEnterGroggy())
            {
                boss.ChangeState(new TutorialGroggyState());
                return;
            }

            boss.ChangeState(new TutorialIdleState());
        }

        public void Exit(BossController boss) { boss.StopMove(); }
    }
}
