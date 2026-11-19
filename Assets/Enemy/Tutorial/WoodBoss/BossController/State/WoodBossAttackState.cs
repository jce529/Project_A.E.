using UnityEngine;

namespace WoodBoss
{
    public class WoodBossAttackState : IBossState
    {
        // 일반 공격 타이머
        private float _attackTimer;   // 공격 쿨다운
        private float _busyTimer;     // 공격 모션 중 행동 잠금

        // 가시 공격 독립 타이머
        private float _spikeTimer;
        private const float SpikeInterval = 3.0f;

        // 진입 후 첫 공격까지 대기 시간
        private const float EnterDelay = 1.0f;

        // 거리 기준
        private const float CloseRange = 5.0f;

        public void Enter(BossController boss)
        {
            _attackTimer = EnterDelay;
            _busyTimer = 0f;
            _spikeTimer = SpikeInterval;

            boss.StopMove();
        }

        public void Execute(BossController boss)
        {
            if (!boss.TargetFound)
            {
                boss.ChangeState(new IdleState());
                return;
            }

            // 타이머 갱신
            if (_busyTimer > 0f) _busyTimer -= Time.deltaTime;
            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;
            if (_spikeTimer > 0f) _spikeTimer -= Time.deltaTime;

            // ── RootSpike: 다른 공격과 독립적으로 발동 ──
            if (_spikeTimer <= 0f)
            {
                var spike = new RootSpikeStrategy();
                spike.ExecuteAttack(boss);
                _spikeTimer = SpikeInterval;

                Debug.Log($"[Boss] Attack: RootSpike (independent)");
            }

            // ── 일반 공격: busyTimer / attackTimer 게이트 ──
            if (_busyTimer > 0f) return;
            if (_attackTimer > 0f) return;

            DecideNormalAttack(boss);
        }

        public void Exit(BossController boss)
        {
            boss.StopMove();
        }

        /// <summary>
        /// 거리 기반 일반 공격 선택 (VineSwing / FloorSweep)
        /// </summary>
        private void DecideNormalAttack(BossController boss)
        {
            float dist = Vector2.Distance(boss.transform.position, boss.Target.position);

            // 시야 밖이면 Idle로 전환
            if (dist > boss.VisionRange)
            {
                boss.ChangeState(new IdleState());
                return;
            }

            IAttackStrategy strategy;
            float expectedDuration;

            if (dist <= boss.AttackRange)
            {
                strategy = new VineSwingStrategy();
                expectedDuration = 0.8f;   // 차징(0.6) + 여유(0.2)
            }
            else
            {
                strategy = new FloorSweepStrategy();
                expectedDuration = 3.0f;   // 준비(1.2) + 이동(1.5) + 후딜(0.3)
            }

            Debug.Log($"[Boss] Attack: {strategy.GetType().Name}, dist={dist:F1}");

            strategy.ExecuteAttack(boss);
            _attackTimer = strategy.Cooldown;
            _busyTimer = expectedDuration;
        }
    }
}