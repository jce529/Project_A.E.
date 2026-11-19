using System.Collections.Generic;
using UnityEngine;

// Phase 7 (D-01b): 판단 로직은 CombatState 의 범용 헬퍼가 담당하고,
// 이 클래스는 정령 보스의 패턴 후보 목록만 선언하는 얇은 데이터 레이어다.
public class SpiritCombatState : CombatState
{
    // D-03b: 가중치 튜닝 상수 (밸런싱 시 선택 로직을 건드리지 않고 이 값만 조정)
    private const float ChargeWeight = 1.0f;
    private const float ExhaustionWeight = 0.6f;   // D-04 체인으로 2패턴 분량을 소비하므로 낮게 설정
    private const float WakeRepelWeight = 1.0f;
    private const float FarProjectileWeight = 1.0f;

    private List<PatternCandidate> _candidates;
    private bool _noCandidateLogged;

    public override void Enter(BossController boss)
    {
        base.Enter(boss);
        _candidates = (boss is SpiritController spirit) ? BuildCandidates(spirit) : null;
        _noCandidateLogged = false;
    }

    protected override bool ShouldTransitionToGroggy(BossController boss) => false;

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is SpiritController)) return null;
        if (_candidates == null) return null;

        // D-04: 직전 실행 패턴이 SpiritExhaustion 이면 후보 평가를 건너뛰고
        //       SpiritWakeRepel 을 무조건 실행한다 (거리/쿨다운/연속금지 전부 우회 — D-04c > D-05b).
        //       WakeRepel 실행 후에는 LastUsedPatternType 이 갱신되어 일반 판단 풀로 복귀한다(D-04b).
        if (LastUsedPatternType == typeof(SpiritExhaustion))
        {
            _noCandidateLogged = false;
            return ForceSelectPattern(new SpiritWakeRepel());
        }

        IAttackStrategy attack = SelectWeightedPattern(dist, _candidates);

        if (attack == null)
        {
            // 전 후보가 쿨다운/연속금지/거리로 막힌 일시적 상황. 매 프레임 재호출되므로 1회만 로그.
            if (!_noCandidateLogged)
            {
                _noCandidateLogged = true;
            }
            return null;
        }

        _noCandidateLogged = false;
        return attack;
    }

    // D-02a: SpiritCharge / SpiritFarProjectile 은 공격 전 스스로 텔레포트하므로 거리 조건 없음.
    // SpiritExhaustion 도 StopMove 만 수행하고 플레이어 위치를 참조하지 않으므로 거리 조건 없음.
    // 근접형 SpiritWakeRepel(→SpiritRepel) 만 RepelRange 상한을 건다 (D-02b).
    // D-06a: SpiritCharge 는 정확히 후보 1개로만 등록한다 (구 배열의 0/4번 중복 슬롯 제거).
    private List<PatternCandidate> BuildCandidates(SpiritController spirit)
    {
        return new List<PatternCandidate>
        {
            new PatternCandidate(() => new SpiritCharge(), ChargeWeight),
            new PatternCandidate(() => new SpiritExhaustion(), ExhaustionWeight),
            new PatternCandidate(() => new SpiritWakeRepel(), WakeRepelWeight, maxDistance: spirit.RepelRange),
            new PatternCandidate(() => new SpiritFarProjectile(), FarProjectileWeight),
        };
    }
}
