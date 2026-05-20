using UnityEngine;

namespace TutorialBoss
{
    // ════════════════════════════════════════════════════════════════════════
    // TutorialAttackState - 공격 상태
    //
    // [패턴 순서]
    //   짝수 번째 공격: TentacleSwipeStrategy (하단 휘두르기)
    //   홀수 번째 공격: TentaclePierceStrategy (거대 AoE → 완료 후 그로기 예약)
    //
    // [그로기 전환 조건]
    //   PendingGroggy == true 이고 busyTimer <= 0 인 시점 → GroggyState로 전환
    //   → 공격 모션 중 강제 전환을 막아 자연스러운 흐름 보장
    // ════════════════════════════════════════════════════════════════════════
    public class TutorialAttackState : IBossState
    {
        // 공격 쿨다운 타이머 (strategy.Cooldown으로 초기화)
        private float _attackTimer;

        // 현재 공격 모션 잠금 타이머 (모션 중 다른 패턴 발동 방지)
        private float _busyTimer;

        // 진입 후 첫 공격까지의 대기 시간 (연출용 여유)
        private const float EnterDelay = 1.5f;

        // 공격 횟수 카운터 (짝/홀로 패턴 교대 결정)
        private int _attackCount = 0;

        // 스파이크 독립 타이머 (다른 패턴과 무관하게 5초마다 발동)
        private float _spikeTimer;
        private const float SpikeInterval = 5f;
        private TutorialRootSpikeStrategy _spikeStrategy;

        public void Enter(BossController boss)
        {
            _attackTimer = EnterDelay; // 진입 직후 바로 공격하지 않도록 딜레이
            _busyTimer   = 0f;
            _spikeTimer  = SpikeInterval;

            var tb = boss as TutorialBossController;
            if (tb != null)
                _spikeStrategy = new TutorialRootSpikeStrategy(tb.WarningPrefab, tb.RootSpikePrefab);

            boss.StopMove();
            boss.Anim?.SetTrigger("AttackTrigger");
            Debug.Log("[TutorialBoss] ──→ Attack 상태");
        }

        public void Execute(BossController boss)
        {
            var tb = boss as TutorialBossController;
            if (tb == null) return;

            // ── 그로기 예약 감지 (현재 공격 모션이 끝났을 때만 전환) ──────
            // busyTimer가 0이 되어 공격이 완전히 끝난 뒤에 그로기로 전환
            // → 공격 모션 도중 갑작스런 상태 전환으로 인한 연출 끊김 방지
            if (tb.PendingGroggy && _busyTimer <= 0f)
            {
                tb.PendingGroggy = false;
                boss.ChangeState(new TutorialGroggyState());
                return;
            }

            // 플레이어가 시야 밖으로 나가면 Idle로 귀환
            if (!boss.TargetFound)
            {
                boss.ChangeState(new TutorialIdleState());
                return;
            }

            // 타이머 감소 (매 프레임)
            if (_busyTimer   > 0f) _busyTimer   -= Time.deltaTime;
            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

            // 스파이크 독립 타이머 (busyTimer와 무관하게 항상 진행)
            _spikeTimer -= Time.deltaTime;
            if (_spikeTimer <= 0f)
            {
                _spikeTimer = SpikeInterval;
                _spikeStrategy?.Execute(boss);
            }

            // 두 타이머 중 하나라도 남아있으면 대기
            if (_busyTimer > 0f || _attackTimer > 0f) return;

            // ── 다음 공격 패턴 실행 ──────────────────────────────────────
            ExecuteNextAttack(boss, tb);
        }

        public void Exit(BossController boss) { boss.StopMove(); }

        /// <summary>
        /// 공격 카운터(짝/홀)에 따라 패턴을 교대 실행한다.
        ///   짝수: TentacleSwipe (하단 바닥 훑기 - 점프로 회피 가능)
        ///   홀수: TentaclePierce (전방위 AoE - 좌우 끝으로만 회피 가능)
        ///         → 완료 후 그로기 자동 예약
        /// </summary>
        private void ExecuteNextAttack(BossController boss, TutorialBossController tb)
        {
            IAttackStrategy strategy;
            float expectedDuration; // 이 공격이 끝나는 데 걸리는 예상 시간 (busyTimer에 설정)

            if (_attackCount % 2 == 0)
            {
                // 짝수 번째: 촉수 휘두르기 (하단 스윕)
                // expectedDuration = 경고(1.0s) + 스윕 이동(2.0s) + 후딜(0.5s)
                strategy         = new TentacleSwipeStrategy(tb.WarningPrefab, tb.SwipePrefab);
                expectedDuration = 3.5f;
            }
            else
            {
                // 홀수 번째: 촉수 찌르기 (거대 AoE)
                // expectedDuration = 경고(0.8s) + 즉발(0.1s) + 후딜(1.4s) + 여유
                // → 코루틴 끝에서 PendingGroggy = true가 설정됨
                strategy         = new TentaclePierceStrategy(tb.WarningPrefab);
                expectedDuration = 2.8f;
            }

            Debug.Log($"[TutorialBoss] 공격 #{_attackCount}: {strategy.GetType().Name}");

            strategy.ExecuteAttack(boss);
            _attackCount++;
            _attackTimer = strategy.Cooldown;   // 다음 공격까지 쿨다운
            _busyTimer   = expectedDuration;    // 이 공격 모션 동안 잠금
        }
    }
}
