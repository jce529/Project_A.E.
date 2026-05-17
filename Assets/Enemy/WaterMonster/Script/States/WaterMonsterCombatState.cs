// Phase 1+3 — WaterMonsterCombatState
// Phase 1: ShouldTransitionToGroggy override (MaxWater=0 support)
// Phase 3: Add teleport condition to SelectAttackStrategy (D-12, D-13)

using System.Collections.Generic;
using UnityEngine;
using WaterMonster.Phase2;

public class WaterMonsterCombatState : CombatState
{
    [SerializeField] private float _enrageCooldownMultiplier = 0.5f;
    private bool _isEnraged = false;

    private float _lastWaveTime = -999f;
    private const float WaveAttackCooldown = 45f;

    public void SetEnraged(bool value) { _isEnraged = value; }

    public override void Enter(BossController boss)
    {
        base.Enter(boss);
        // 광폭화 트리거가 다른 State에서 발생했을 수 있으므로 Controller에서 상태 읽기
        if (boss is WaterMonsterController wmc)
            _isEnraged = wmc.IsEnraged;
    }

    public override void Execute(BossController boss)
    {
        base.Execute(boss);
        // 광폭화 시 쿨다운 배율 적용
        // base.Execute가 _decisionTimer = attack.Cooldown 으로 설정한 직후
        // 배율을 곱해 실질적 대기 시간 감소
        if (_isEnraged)
        {
            _decisionTimer *= _enrageCooldownMultiplier;
        }
    }

    protected override bool ShouldTransitionToGroggy(BossController boss)
    {
        return false;
    }

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is WaterMonsterController wmc))
            return base.SelectAttackStrategy(boss, dist);

        if (_isEnraged && wmc.CanSpawnZone())
        {
            wmc.SpawnRandomZone();
            wmc.RecordZoneTime();
            _decisionTimer = 1.0f;
            return null;
        }

        if (PuddleStackManager.Instance != null
            && PuddleStackManager.Instance.IndestructibleCount >= 2
            && wmc.CanTeleport())
        {
            boss.ChangeState(new WaterTeleportState());
            return null;
        }

        // 모든 페이즈 공통: 패턴 1, 2, 8
        var pool = new List<IAttackStrategy>
        {
            new WaterGeyser(),
        };

        if (Time.time - _lastWaveTime >= WaveAttackCooldown)
            pool.Add(new WaterWavePush());

        if (dist <= 3.0f)
        {
            pool.Add(new WaterMeleeSwipe());
            pool.Add(new WaterJumpLand());
        }
        else
        {
            pool.Add(new WaterRangedSpit());
        }

        // 페이즈별 전용 패턴
        if (wmc.IsPhase3)
            pool.Add(new WaterColorPrison());       // 페이즈 3: 패턴 7
        else if (wmc.IsPhase2)
            pool.Add(new WaterPrisonMapAoe());      // 페이즈 2: 패턴 5
        else
            pool.Add(new WaterPrisonAttack());      // 페이즈 1: 패턴 4

        IAttackStrategy selected = pool[Random.Range(0, pool.Count)];
        if (selected is WaterWavePush)
            _lastWaveTime = Time.time;
        return selected;
    }
}
