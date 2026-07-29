using System.Collections.Generic;
using UnityEngine;

public class CombatState : IBossState
{
    protected float _decisionTimer;

    // 현재 공격 중인 전략을 관리하기 위한 필드
    private IAttackStrategy _currentAttack;
    private bool _isAttacking; // 공격 중인지 여부

    private float _attackWaitTimer; // 애니메이션 타임아웃용 타이머
    private const float MaxAttackDuration = 0.5f; // 최대 대기 시간 (애니메이션 없을 때 대비)

    // ─── Phase 7 (D-01): 범용 패턴 후보 평가 헬퍼 ───────────────────────────
    // 파생 State 는 후보 목록만 선언하면 되고, 판단(거리 선택적 조건 + 쿨다운 +
    // 연속사용금지 + 가중치 랜덤)은 이 베이스 클래스가 담당한다.
    protected class PatternCandidate
    {
        public readonly System.Func<IAttackStrategy> Factory;
        public readonly System.Type StrategyType;
        public readonly float Weight;
        public readonly float? MinDistance; // null = 하한 없음 (D-02b: 거리 조건은 선택적)
        public readonly float? MaxDistance; // null = 상한 없음 (D-02b: 거리 조건은 선택적)

        // Phase 8 (D-04a): _patternReadyAt 에 기록할 재사용 잠금 시간을 IAttackStrategy.Cooldown
        // 과 분리한다. null = strategy.Cooldown 사용(기존 동작).
        public readonly float? CooldownOverride;

        public PatternCandidate(System.Func<IAttackStrategy> factory, float weight,
                                float? minDistance = null, float? maxDistance = null,
                                float? cooldownOverride = null)
        {
            Factory = factory;
            Weight = weight;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
            CooldownOverride = cooldownOverride;
            // D-06a: 게이팅 키를 슬롯 인덱스가 아닌 "전략 타입"으로 1회 확정한다.
            StrategyType = factory().GetType();
        }
    }

    // 직전에 실행한 패턴 타입 — D-05a 연속사용금지 판정 및 D-04 체인 트리거에 사용된다.
    protected System.Type LastUsedPatternType { get; private set; }

    // 패턴별 쿨다운 만료 시각. Execute() 에 early-return 분기가 있어 매 프레임 감산 방식은
    // 조용히 누락될 수 있으므로, Time.time 절대 시각 비교를 사용한다.
    private readonly Dictionary<System.Type, float> _patternReadyAt = new Dictionary<System.Type, float>();

    // Phase 8 (D-03a): _patternReadyAt 에 기록할 쿨다운에 곱할 배율.
    // 기본 1f = 배율 없음(기존 동작). 광폭화처럼 쿨다운을 단축하는 보스가 override 한다.
    protected virtual float GetPatternCooldownMultiplier() => 1f;

    public virtual void Enter(BossController boss)
    {
        boss.StopMove();
        _decisionTimer = 0;
        _isAttacking = false;
        _currentAttack = null;
        _attackWaitTimer = 0;

        // Phase 7: 패턴 추적값 초기화 (기존 SpiritCombatState._patternIndex = 0 과 동일한 세션 단위 리셋)
        LastUsedPatternType = null;
        _patternReadyAt.Clear();
    }

    public virtual void Execute(BossController boss)
    {
        // 1. [공격중] 공격 중이라면, 애니메이션이 끝났는지 확인
        if (_isAttacking && _currentAttack != null)
        {
            _attackWaitTimer += Time.deltaTime;

            // 애니메이션이 끝났거나, 너무 오래(0.5초) 기다렸다면 공격 종료
            if (boss.CheckAnimationState(_currentAttack.AnimationName) || _attackWaitTimer >= MaxAttackDuration)
            {
                if (_attackWaitTimer >= MaxAttackDuration)
                {
                    Debug.LogWarning($"[CombatState] 애니메이션 타임아웃으로 공격 강제 종료: {_currentAttack.AnimationName}");
                }
                else
                {
                    Debug.Log($"[CombatState] 공격 종료: {_currentAttack.AnimationName}");
                }

                _isAttacking = false;
                _currentAttack = null;
                _attackWaitTimer = 0;
            }
            else
            {
                return;
            }
        }

        // 2. 쿨타임 체크 (공격 종료 후 쿨타임 동안 대기)
        _decisionTimer -= Time.deltaTime;
        if (_decisionTimer > 0) return;

        // 3. 상태 체크 (베리어가 없으면 그로기)
        if (ShouldTransitionToGroggy(boss))
        {
            boss.ChangeState(new GroggyState());
            return;
        }

        // 4. 거리 체크
        float dist = Vector2.Distance(boss.transform.position, boss.Target.position);
        if (dist > boss.AttackRange + 1.0f)
        {
            if (!boss.TargetFound)
            {
                Debug.Log($"[CombatState] 타겟이 인식 범위 밖(거리: {dist:F1}). IdleState로 전환.");
                boss.ChangeState(new IdleState());
            }
            else
            {
                Debug.Log($"[CombatState] 타겟이 사거리 밖임(거리: {dist:F1}). ChaseState로 전환.");
                boss.ChangeState(new ChaseState());
            }
            return;
        }

        // 5. 새로운 공격 전략 선택
        IAttackStrategy attack = SelectAttackStrategy(boss, dist);

        if (attack != null)
        {
            Debug.Log($"[CombatState] 새로운 공격 시작: {attack.GetType().Name}");
            _currentAttack = attack;
            _isAttacking = true;
            _attackWaitTimer = 0; // 타이머 초기화

            attack.ExecuteAttack(boss);
            _decisionTimer = attack.Cooldown;
        }
    }

    protected virtual bool ShouldTransitionToGroggy(BossController boss)
    {
        return !boss.Stats.IsBarrierActive;
    }

    protected virtual IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (dist > 8f) return new RangedPokeAttack();
        if (boss.CanUseHeavyAttack) return new HeavyAttack();
        return new LightAttack();
    }

    // D-01/D-03: 조건(거리 선택적 + 연속금지 + 쿨다운)을 통과한 후보 중 가중치 랜덤으로 1개 선택.
    // 통과 후보가 없으면 null — 기존 계약대로 Execute() 가 다음 프레임에 재시도한다.
    protected IAttackStrategy SelectWeightedPattern(float dist, IReadOnlyList<PatternCandidate> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;

        List<PatternCandidate> eligible = new List<PatternCandidate>();
        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            PatternCandidate c = candidates[i];

            if (c.MinDistance.HasValue && dist < c.MinDistance.Value) continue; // 거리 하한 (D-02b)
            if (c.MaxDistance.HasValue && dist > c.MaxDistance.Value) continue; // 거리 상한 (D-02b)
            if (LastUsedPatternType == c.StrategyType) continue;                // 연속사용금지 (D-05a)
            if (_patternReadyAt.TryGetValue(c.StrategyType, out float readyAt) && Time.time < readyAt) continue; // 쿨다운

            eligible.Add(c);
            totalWeight += c.Weight;
        }

        if (eligible.Count == 0 || totalWeight <= 0f) return null;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < eligible.Count; i++)
        {
            cumulative += eligible[i].Weight;
            if (roll <= cumulative) return CommitSelection(eligible[i].Factory(), eligible[i].CooldownOverride);
        }

        // 부동소수 경계 보정: 루프에서 못 골랐어도 null 로 새지 않도록 마지막 후보를 선택한다.
        return CommitSelection(eligible[eligible.Count - 1].Factory(), eligible[eligible.Count - 1].CooldownOverride);
    }

    // D-04: 강제 체인용 — 후보 평가/거리/쿨다운/연속사용금지를 전부 우회하고 지정 전략을 확정한다.
    protected IAttackStrategy ForceSelectPattern(IAttackStrategy strategy)
    {
        if (strategy == null) return null;
        return CommitSelection(strategy);
    }

    // 선택 확정 처리 — 연속사용금지 기준값과 해당 패턴의 쿨다운 만료 시각을 갱신한다.
    // Phase 8: cooldownOverride 가 있으면 strategy.Cooldown 대신 그 값을 쓰고(D-04a),
    //          GetPatternCooldownMultiplier() 배율을 곱한다(D-03a).
    private IAttackStrategy CommitSelection(IAttackStrategy strategy, float? cooldownOverride = null)
    {
        System.Type t = strategy.GetType();
        LastUsedPatternType = t;
        float baseCooldown = cooldownOverride ?? strategy.Cooldown;
        _patternReadyAt[t] = Time.time + baseCooldown * GetPatternCooldownMultiplier();
        return strategy;
    }

    public virtual void Exit(BossController boss)
    {
        _isAttacking = false;
        _currentAttack = null;
    }
}
