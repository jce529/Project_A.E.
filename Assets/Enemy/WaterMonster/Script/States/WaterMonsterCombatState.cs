// Phase 1+3 — WaterMonsterCombatState
// Phase 1: ShouldTransitionToGroggy override (MaxWater=0 support)
// Phase 8: 수작업 pool-random 을 CombatState 범용 헬퍼(PatternCandidate + SelectWeightedPattern)로 교체
// Phase 29: 기획 문서 페이즈 구조 — 누적 후보 풀(D-02), 1·2페이즈 단일 사용 / 3페이즈 겹침(D-03),
//           문서 밖 패턴·장판 스폰·구 텔레포트 가드 제거(D-04), 페이즈 배율은 컨트롤러 값 사용(D-01)

using System.Collections.Generic;
using UnityEngine;
using WaterMonster.Phase2;

public class WaterMonsterCombatState : CombatState
{
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

    // 점프 착지 패턴의 "플레이어 근접" 판정 거리.
    private const float MeleeRange = 3.0f;

    private bool _noCandidateLogged;
    private WaterMonsterController _wmc;

    // Phase 29 (D-03): 이번 프레임에 SelectAttackStrategy 가 패턴을 골랐는지 — 배율을 선택 시 1회만 적용한다.
    private bool _selectedThisFrame;

    public override void Enter(BossController boss)
    {
        _wmc = boss as WaterMonsterController;
        base.Enter(boss);
        _noCandidateLogged = false;
    }

    public override void Execute(BossController boss)
    {
        // 페이즈 전환 대기 중 — 컨트롤러가 실행 중 패턴 종료 후 그로기로 전환한다.
        if (_wmc != null && _wmc.HasPendingPhaseTransition) return;

        // D-03: 1·2페이즈는 단일 사용 — 앞 패턴이 끝나기 전에는 판단하지 않는다.
        //       base.Execute 를 건너뛰므로 _decisionTimer 는 패턴 종료 후부터 감소한다.
        if (_wmc != null && _wmc.CurrentPhase < 3 && _wmc.IsPatternRunning) return;

        _selectedThisFrame = false;
        base.Execute(boss);

        // base.Execute 가 선택 직후 _decisionTimer 를 attack.Cooldown 으로 설정한 프레임에만 배율을 곱한다.
        // (매 프레임 곱하면 대기 시간이 0 으로 수렴한다.)
        if (_selectedThisFrame)
            _decisionTimer *= GetPatternCooldownMultiplier();
    }

    // Phase 29: 패턴 이력을 보스가 보유 — WavePush 45초 잠금과 직전 패턴 감쇠가
    //           Chase/그로기/웅덩이 이동을 거쳐 CombatState 에 재진입해도 유지된다.
    protected override PatternHistory AcquireHistory(BossController boss)
    {
        if (boss is WaterMonsterController wmc)
            return wmc.PatternHistory;
        return base.AcquireHistory(boss);
    }

    // D-03a: 전체 판단 대기(_decisionTimer)와 패턴별 개별 쿨다운(CombatState._patternReadyAt)에
    //        동일한 배율을 적용한다. 배율 값은 컨트롤러(D-01: 1·2페이즈 1, 3페이즈, 30% 강화)가 결정한다.
    protected override float GetPatternCooldownMultiplier()
        => _wmc != null ? _wmc.PatternCooldownMultiplier : 1f;

    protected override bool ShouldTransitionToGroggy(BossController boss)
    {
        return false;
    }

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is WaterMonsterController wmc))
            return base.SelectAttackStrategy(boss, dist);

        // 페이즈 전환 그로기 대기 중에는 새 패턴을 고르지 않는다.
        if (wmc.HasPendingPhaseTransition)
            return null;

        // OD-7: 3페이즈 동시 실행 상한.
        if (wmc.CurrentPhase >= 3 && wmc.ActivePatternCount >= wmc.Phase3MaxConcurrentPatterns)
            return null;

        // D-06c: 페이즈는 전투 도중 HP 임계치로 전환되므로 후보 목록을
        //        Enter() 에 캐싱하지 않고 매 판단마다 새로 구성한다.
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
        _selectedThisFrame = true;
        return attack;
    }

    // D-02: 페이즈별 후보는 누적된다 — 1페이즈 4종, 2페이즈 + 감옥→전체 공격·웅덩이 3종, 3페이즈 + 색깔 감옥·파도 밀기.
    // D-04: WaterMeleeSwipe / WaterRangedSpit 은 기획 문서 밖 패턴이므로 후보에 넣지 않는다.
    // OD-7: 3페이즈 겹침 중(실행 중 패턴 있음)에는 몸이 움직이는 패턴(점프 착지, 웅덩이 이동)을 제외한다.
    private List<PatternCandidate> BuildCandidates(WaterMonsterController wmc)
    {
        bool bodyFree = !wmc.IsPatternRunning;

        var candidates = new List<PatternCandidate>
        {
            new PatternCandidate(() => new WaterGeyser(), PatternWeight),
            new PatternCandidate(() => new WaterBurst(), PatternWeight),
            new PatternCandidate(() => new WaterPrisonAttack(), PatternWeight),
        };

        if (bodyFree)
            candidates.Add(new PatternCandidate(() => new WaterJumpLand(), PatternWeight, maxDistance: MeleeRange));

        if (wmc.CurrentPhase >= 2)
        {
            candidates.Add(new PatternCandidate(() => new WaterPrisonMapAoe(), PatternWeight));

            var puddles = WaterMonsterPuddleSystem.Instance;
            if (puddles != null && puddles.IsActive)
            {
                if (puddles.CanSpawnMore)
                    candidates.Add(new PatternCandidate(() => new WaterPuddleSpawn(), PatternWeight));

                if (puddles.GetAgedPuddles().Count > 0 && puddles.Explosion != null && !puddles.Explosion.IsExploding)
                    candidates.Add(new PatternCandidate(() => new WaterPuddleExplode(), PatternWeight));

                if (bodyFree && wmc.CanTeleport()
                    && puddles.TrySelectMove(wmc.transform.position, wmc.Target.position, out _, out _))
                    candidates.Add(new PatternCandidate(() => new WaterPuddleMove(), PatternWeight));
            }
        }

        if (wmc.CurrentPhase >= 3)
        {
            candidates.Add(new PatternCandidate(() => new WaterColorPrison(), PatternWeight));
            candidates.Add(new PatternCandidate(() => new WaterWavePush(), PatternWeight, cooldownOverride: WaveAttackCooldown));
        }

        return candidates;
    }
}
