// Phase 1+3 — WaterMonsterCombatState
// Phase 1: ShouldTransitionToGroggy override (MaxWater=0 support)
// Phase 3: Add teleport condition to SelectAttackStrategy (D-12, D-13)
// Phase 8: 수작업 pool-random 을 CombatState 범용 헬퍼(PatternCandidate + SelectWeightedPattern)로 교체

using System.Collections.Generic;
using UnityEngine;
using WaterMonster.Phase2;

public class WaterMonsterCombatState : CombatState
{
    [SerializeField] private float _enrageCooldownMultiplier = 0.5f;
    private bool _isEnraged = false;

    // D-02a: 모든 패턴 균등 가중치 — 기존 uniform pool-random 과 동일한 체감을 유지한다.
    //        이번 페이즈에서 패턴 간 밸런싱(차등 가중치)은 하지 않는다.
    private const float PatternWeight = 1.0f;

    // D-01a~c: 직전 사용 패턴은 완전배제(Phase 7 D-05)가 아니라 가중치 0.5배로 감쇠시킨다.
    //          감쇠는 직전 1개 패턴에만 적용되며 누적되지 않는다.
    private const float LastUsedWeightDecay = 0.5f;

    // D-04a: WaterWavePush 특수 재사용 잠금(45초). WaterWavePush.Cooldown(3f) 과는 별개 값이므로
    //        PatternCandidate 의 cooldownOverride 로 전달한다. 이 값을 strategy.Cooldown 에
    //        의존시키면 잠금이 3초로 조용히 줄어든다.
    private const float WaveAttackCooldown = 45f;

    // D-06a: 근접(<= 3.0) / 원거리(> 3.0) 분기 경계값.
    private const float MeleeRange = 3.0f;

    private bool _noCandidateLogged;

    public void SetEnraged(bool value) { _isEnraged = value; }

    public override void Enter(BossController boss)
    {
        base.Enter(boss);
        // 광폭화 트리거가 다른 State에서 발생했을 수 있으므로 Controller에서 상태 읽기
        if (boss is WaterMonsterController wmc)
            _isEnraged = wmc.IsEnraged;
        _noCandidateLogged = false;
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

    // D-03a: 광폭화 시 전체 판단 대기(_decisionTimer) 뿐 아니라 패턴별 개별 쿨다운
    //        (CombatState._patternReadyAt)에도 동일한 배율을 적용한다.
    // D-04b: 이에 따라 WaveAttackCooldown 45초도 광폭화 중에는 22.5초로 함께 단축된다.
    protected override float GetPatternCooldownMultiplier()
        => _isEnraged ? _enrageCooldownMultiplier : 1f;

    protected override bool ShouldTransitionToGroggy(BossController boss)
    {
        return false;
    }

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is WaterMonsterController wmc))
            return base.SelectAttackStrategy(boss, dist);

        // D-05a: 장판 스폰 / 텔레포트 전환은 "공격 패턴 후보"가 아니라 상태 전환 트리거다.
        //        후보 목록 구성보다 앞선 최상단 early-return 으로 기존 흐름 그대로 유지한다.
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

        // D-06c: 페이즈(IsPhase2/IsPhase3)는 전투 도중 HP 임계치로 전환되므로 후보 목록을
        //        Enter() 에 캐싱하지 않고 매 판단마다 새로 구성한다.
        //        (SpiritCombatState 의 Enter()-1회-캐싱 방식을 복사하면 페이즈 전환 후에도
        //         이전 페이즈의 프리즌 패턴이 계속 후보에 남는다.)
        //        마이그레이션 이전 코드도 매 호출마다 pool 을 새로 만들었으므로 할당 프로파일 동일.
        List<PatternCandidate> candidates = BuildCandidates(wmc);

        // D-01d: 감쇠 배율을 전달한다. 0f 를 넘기면 WaterSpirit 과 같은 완전배제가 되므로 주의.
        IAttackStrategy attack = SelectWeightedPattern(dist, candidates, LastUsedWeightDecay);

        if (attack == null)
        {
            // 전 후보가 쿨다운/거리로 막힌 일시적 상황. 매 프레임 재호출되므로 1회만 로그.
            if (!_noCandidateLogged)
            {
                _noCandidateLogged = true;
            }
            return null;
        }

        _noCandidateLogged = false;
        return attack;
    }

    // D-06a: 근접 dist <= 3.0 → WaterMeleeSwipe / WaterJumpLand, 원거리 dist > 3.0 → WaterRangedSpit.
    //        헬퍼의 MinDistance/MaxDistance 는 양끝 포함 판정이므로 dist 가 정확히 3.0 인 순간에만
    //        양쪽이 동시에 후보가 된다 — 실측 확률상 무시 가능한 경계 차이로 수용한다(RESEARCH Pitfall 5).
    // D-06b: 프리즌 변형 3종은 현재 페이즈에 해당하는 1개만 후보에 오른다 (상호 배타, 기존 동작과 동일).
    private List<PatternCandidate> BuildCandidates(WaterMonsterController wmc)
    {
        var candidates = new List<PatternCandidate>
        {
            new PatternCandidate(() => new WaterGeyser(), PatternWeight),
            new PatternCandidate(() => new WaterWavePush(), PatternWeight, cooldownOverride: WaveAttackCooldown),
            new PatternCandidate(() => new WaterMeleeSwipe(), PatternWeight, maxDistance: MeleeRange),
            new PatternCandidate(() => new WaterJumpLand(), PatternWeight, maxDistance: MeleeRange),
            new PatternCandidate(() => new WaterRangedSpit(), PatternWeight, minDistance: MeleeRange),
        };

        if (wmc.IsPhase3)
            candidates.Add(new PatternCandidate(() => new WaterColorPrison(), PatternWeight));      // 페이즈 3: 패턴 7
        else if (wmc.IsPhase2)
            candidates.Add(new PatternCandidate(() => new WaterPrisonMapAoe(), PatternWeight));     // 페이즈 2: 패턴 5
        else
            candidates.Add(new PatternCandidate(() => new WaterPrisonAttack(), PatternWeight));     // 페이즈 1: 패턴 4

        return candidates;
    }
}
