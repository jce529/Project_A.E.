# Phase 8: WaterMonster 보스를 CombatState 기반 패턴 판단 로직으로 마이그레이션 - Research

**Researched:** 2026-07-27
**Domain:** Unity C# 상태머신 리팩토링 (내부 코드베이스, 외부 패키지 없음)
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Phase Boundary:** `WaterMonsterCombatState.SelectAttackStrategy`의 "일반 공격 패턴 풀" 선택 로직(`WaterGeyser`/`WaterWavePush`/`WaterMeleeSwipe`/`WaterJumpLand`/`WaterRangedSpit`/페이즈별 프리즌 변형)을 `CombatState`(Phase 7에서 추가된 `PatternCandidate` + `SelectWeightedPattern`) 헬퍼 기반으로 교체한다. 이 페이즈는 또한 CombatState 공통 헬퍼에 새로운 범용 옵션(직전 1개 패턴 가중치 감쇠)을 추가한다 — WaterSpirit도 향후 선택적으로 쓸 수 있지만, 이번 페이즈에서 WaterSpirit 코드를 되돌리지는 않는다.

**범위 외:**
- 장판 스폰(Enrage 상태의 `CanSpawnZone`/`SpawnRandomZone`)과 텔레포트 상태 전환(`WaterTeleportState` 진입) 분기 — `SelectAttackStrategy` 최상단의 사전 가드(early return, `null` 반환 + 직접 `ChangeState`)로 그대로 유지한다. 이들은 "공격 패턴 후보"가 아니라 상태 전환 트리거이므로 후보 풀에 포함하지 않는다.
- WaterSpirit(`SpiritCombatState`)의 기존 완전배제 연속금지(Phase 7 D-05) 방식 변경 — 그대로 유지, 되돌리지 않는다.
- 그로기(Groggy) 시스템 — `WaterMonsterCombatState.ShouldTransitionToGroggy()`는 항상 `false`를 반환하며 이번 페이즈에서 변경하지 않는다.
- 애니메이션·시각 이펙트 연동 (v3.0+)

**D-01: 연속 사용 처리 — 완전배제 대신 가중치 감쇠**
- D-01a: WaterMonster에는 Phase 7(D-05, 완전배제)과 다른 방식을 새로 도입한다: 직전에 실행한 패턴은 완전히 제외되지 않고, 가중치만 감쇠된 채로 여전히 후보에 남는다.
- D-01b: 감쇠는 정확히 "직전 1개 패턴"에만 적용된다 (2턴 전 이상 패턴은 감쇠 없이 원래 가중치). 지수 누적 감쇠 아님.
- D-01c: 감쇠 비율은 0.5배(절반)로 고정한다.
- D-01d: 이 감쇠 메커니즘은 `CombatState` 공통 헬퍼에 범용 옵션으로 추가한다. WaterSpirit은 기존 완전배제(D-05) 방식을 그대로 유지하며 두 메커니즘이 헬퍼 안에 공존한다.

**D-02: 패턴 기본 가중치**
- D-02a: 각 패턴의 기본 가중치는 균등(예: 전부 1.0)하게 유지한다 — 기존 uniform pool-random과 동일한 체감을 목표로 한다. D-01의 감쇠만이 유일한 차등 요소.

**D-03: 광폭화(Enrage)가 개별 패턴 쿨다운에도 적용**
- D-03a: 현재 광폭화는 전체 판단 대기시간(`_decisionTimer`)만 0.5배 단축한다. 새로 도입되는 패턴별 개별 쿨다운(Phase 7의 `_patternReadyAt` 메커니즘)에도 동일한 배율(0.5배)이 적용되어야 한다.
- D-03b: 정확한 구현 지점은 플래너 재량.

**D-04: WaterWavePush 특수 쿨다운 유지**
- D-04a: `WaterWavePush`의 45초 특수 재사용 쿨다운은 이번 마이그레이션에서도 그대로 유지한다 — 밸런스를 바꾸지 않는다.
- D-04b: D-03에 따라 광폭화 중에는 이 45초도 동일하게 0.5배(약 22.5초)로 단축된다.

**D-05: 사전 가드(장판 스폰/텔레포트) 범위 경계**
- D-05a: `CanSpawnZone`+`SpawnRandomZone`(광폭화 장판 스폰)과 `PuddleStackManager` 기반 텔레포트 전환 분기는 새 후보 풀 헬퍼 밖에서, `SelectAttackStrategy` 최상단의 조건부 early-return으로 그대로 유지한다. 코드 흐름·조건·쿨다운 값 전부 변경하지 않는다.

**D-06: 페이즈별(Phase1/2/3) 프리즌 변형과 거리 기반 근접/원거리 분기**
- D-06a: 근접(`dist <= 3.0`) → `WaterMeleeSwipe`/`WaterJumpLand`, 원거리 → `WaterRangedSpit` 분기는 헬퍼의 `MinDistance`/`MaxDistance` 조건부 후보로 재구성한다.
- D-06b: 페이즈별 프리즌 패턴(`WaterPrisonAttack`/`WaterPrisonMapAoe`/`WaterColorPrison`)은 현재 페이즈(`wmc.IsPhase2`/`IsPhase3`)에 해당하는 것 하나만 후보에 오르도록 유지한다 (상호 배타적, 기존 동작과 동일).
- D-06c: WaterSpirit처럼 `Enter()`에서 후보 목록을 한 번만 구성할 수 없다 — WaterMonster는 전투 세션 도중 페이즈가 바뀌므로, 후보 목록(혹은 최소한 페이즈 의존 후보)은 페이즈 변화를 반영해 최신 상태로 판단되어야 한다. 정확한 갱신 시점/방식은 플래너 재량.

### Claude's Discretion
- D-01d 감쇠 메커니즘의 정확한 API 형태 (`PatternCandidate` 확장 vs `SelectWeightedPattern` 오버로드 vs 별도 메서드)
- D-06c 페이즈 변화에 따른 후보 목록 재구성 시점/방식
- D-03b 광폭화 배율을 개별 쿨다운에 적용하는 정확한 구현 지점
- `WaterGeyser` 등 나머지 패턴의 정확한 가중치 수치(균등 유지 원칙 하에서의 구체값)

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope.
</user_constraints>

## Summary

이 페이즈는 순수 내부 C#/Unity 리팩토링이다. 외부 패키지나 신규 라이브러리는 전혀 관련이 없다 — 핵심 작업은 Phase 7에서 만들어진 `CombatState` 범용 헬퍼(`PatternCandidate`/`SelectWeightedPattern`/`ForceSelectPattern`/`CommitSelection`)를 확장하고, `WaterMonsterCombatState.SelectAttackStrategy`의 수작업 `List<IAttackStrategy>` 풀-랜덤 로직을 그 헬퍼 기반 후보 선언으로 교체하는 것이다.

조사 결과 세 가지 설계 공백(gap)이 발견되었으며, 이들은 플래너가 반드시 다뤄야 한다:

1. **쿨다운 값의 이중 의미 충돌** — `CommitSelection`은 `strategy.Cooldown`(IAttackStrategy 자체 값)을 그대로 `_patternReadyAt` 만료 시각 계산에 사용한다. 그런데 `WaterWavePush.Cooldown => 3f`이며, 현재의 "45초 특수 재사용 쿨다운"은 이 값과 무관하게 별도 필드(`_lastWaveTime`/`WaveAttackCooldown`)로 수작업 관리되고 있다. 헬퍼를 그대로 쓰면 45초 요구사항(D-04)이 깨진다 — `PatternCandidate`/`CommitSelection`에 쿨다운 오버라이드를 추가해야 한다.
2. **광폭화 배율 훅 부재** — `_patternReadyAt`은 `CombatState`의 `private` 필드이고, `CommitSelection`도 `private`이며 배율 파라미터가 없다. D-03이 요구하는 "개별 쿨다운에도 0.5배 적용"을 만족하려면 `CombatState`에 새로운 확장 지점(가상 메서드 또는 파라미터)이 필요하다.
3. **완전배제(WaterSpirit) vs 가중치 감쇠(WaterMonster) 공존** — 현재 `SelectWeightedPattern`은 `if (LastUsedPatternType == c.StrategyType) continue;`로 하드코딩된 완전배제만 지원한다. 감쇠 옵션을 기본값 `false`(기존 동작 유지)인 선택적 파라미터로 추가하면 WaterSpirit 호출부(`SpiritCombatState.cs`)를 한 글자도 건드리지 않고 공존시킬 수 있다.

반면 D-06c(페이즈 전환 중 후보 갱신)는 생각보다 단순하다 — 현재 `WaterMonsterCombatState.SelectAttackStrategy`는 이미 매 호출마다 `pool` 리스트를 새로 만들며 그 안에서 `wmc.IsPhase2`/`IsPhase3`를 실시간으로 읽는다(캐싱 없음). 즉 "문제"는 SpiritCombatState의 `Enter()`-1회-캐싱 패턴을 그대로 베끼지만 않으면 자동으로 해결된다. 새 헬퍼 기반 후보 목록도 `Enter()`가 아니라 `SelectAttackStrategy` 호출 시점마다(또는 최소한 페이즈 의존 후보만) 새로 구성하면 기존과 동일한 반응성을 얻는다 — 이는 GC 할당 패턴 측면에서도 기존 코드(매 호출마다 `List<IAttackStrategy>` 신규 할당)와 동등한 수준이라 회귀가 아니다.

**Primary recommendation:** `CombatState`에 (a) `PatternCandidate`에 선택적 `cooldownOverride` 필드, (b) `SelectWeightedPattern`에 선택적 `decayLastUsed`(또는 유사) 파라미터(기본값 false), (c) 쿨다운 배율을 위한 `protected virtual float GetPatternCooldownMultiplier() => 1f;` 가상 메서드를 추가하는 방식으로 헬퍼를 확장하고, `WaterMonsterCombatState`는 `SelectAttackStrategy` 매 호출마다 (사전 가드 통과 후) 후보 리스트를 새로 구성해 `SelectWeightedPattern`을 호출하도록 재작성한다.

<phase_requirements>
## Phase Requirements

No requirement IDs have been mapped to this phase yet (REQUIREMENTS.md traceability table only lists Phase 5–7; this phase was added ad-hoc per ROADMAP evolution as Phase 7's deferred D-08b follow-up). The phase's acceptance criteria are entirely defined by CONTEXT.md's decisions (D-01 ~ D-06) above — the planner should treat each D-xx as the requirement to satisfy, since no formal REQ-ID exists.
</phase_requirements>

## Standard Stack

Not applicable — this phase introduces no new third-party libraries or packages. All work reuses existing internal project types.

### Internal APIs to Reuse (equivalent of "Core")
| Type/Member | Location | Purpose | Current Access Level |
|---|---|---|---|
| `CombatState.PatternCandidate` | `Assets/Enemy/NewBoss/Script/States/CombatState.cs` | 후보 패턴 선언 데이터 구조 (Factory, StrategyType, Weight, MinDistance, MaxDistance) | `protected class` (nested) |
| `CombatState.SelectWeightedPattern(float dist, IReadOnlyList<PatternCandidate>)` | 위 파일 | 거리/연속금지/쿨다운 필터링 후 가중치 룰렛 선택 | `protected` |
| `CombatState.ForceSelectPattern(IAttackStrategy)` | 위 파일 | 후보평가 전부 우회, 지정 전략 강제 확정 | `protected` |
| `CombatState.CommitSelection(IAttackStrategy)` | 위 파일 | `LastUsedPatternType` 갱신 + `_patternReadyAt[type] = Time.time + strategy.Cooldown` | `private` — 확장 필요 |
| `CombatState.LastUsedPatternType` | 위 파일 | 직전 실행 패턴 타입 | `protected get; private set;` |
| `CombatState._patternReadyAt` | 위 파일 | `Dictionary<Type, float>` — 타입별 쿨다운 만료 절대시각(`Time.time` 기준) | `private` |
| `IAttackStrategy.Cooldown` | `Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs` | 각 전략 자체 쿨다운(초) — `_decisionTimer` 산정과 `_patternReadyAt` 산정 두 곳에 재사용됨 | `interface` |

**Installation:** N/A — no packages to install.

## Architecture Patterns

### Current `CombatState` Helper Structure (Phase 7, verified 2026-07-27)

```csharp
// Source: Assets/Enemy/NewBoss/Script/States/CombatState.cs (lines 18-193)
protected class PatternCandidate
{
    public readonly System.Func<IAttackStrategy> Factory;
    public readonly System.Type StrategyType;
    public readonly float Weight;
    public readonly float? MinDistance;
    public readonly float? MaxDistance;

    public PatternCandidate(System.Func<IAttackStrategy> factory, float weight,
                            float? minDistance = null, float? maxDistance = null)
    {
        Factory = factory;
        Weight = weight;
        MinDistance = minDistance;
        MaxDistance = maxDistance;
        // NOTE: calls factory() immediately, once, just to read .GetType() —
        // allocates one throwaway IAttackStrategy instance per candidate construction.
        StrategyType = factory().GetType();
    }
}

protected System.Type LastUsedPatternType { get; private set; }
private readonly Dictionary<System.Type, float> _patternReadyAt = new Dictionary<System.Type, float>();

protected IAttackStrategy SelectWeightedPattern(float dist, IReadOnlyList<PatternCandidate> candidates)
{
    // filters: MinDistance, MaxDistance, LastUsedPatternType == StrategyType (FULL EXCLUSION),
    // _patternReadyAt cooldown-not-elapsed
    // then weighted roulette over eligible list, calls CommitSelection(eligible[i].Factory())
}

protected IAttackStrategy ForceSelectPattern(IAttackStrategy strategy)
{
    // bypasses all filters, straight to CommitSelection(strategy)
}

private IAttackStrategy CommitSelection(IAttackStrategy strategy)
{
    System.Type t = strategy.GetType();
    LastUsedPatternType = t;
    _patternReadyAt[t] = Time.time + strategy.Cooldown;  // <-- uses strategy.Cooldown directly, no override hook
    return strategy;
}
```

Key facts a planner needs:
- `SelectWeightedPattern`'s exclusion check (`if (LastUsedPatternType == c.StrategyType) continue;`) is unconditional today — there is no parameter to switch it to "decay instead of exclude." Adding decay requires either a new optional parameter on this method, a new overload, or a field on `PatternCandidate` marking it "decayable." A parameter with a safe default (`false` = current full-exclusion behavior) is the minimal-diff option since `SpiritCombatState`'s existing call site (`SelectWeightedPattern(dist, _candidates)`, `SpiritCombatState.cs:42`) would continue compiling unchanged.
- `_patternReadyAt` is written only from `CommitSelection`, using `strategy.Cooldown` (the `IAttackStrategy` interface value) with no way to substitute a different duration. This is the crux of the D-04 (45s WaterWavePush) gap — see Common Pitfalls.
- `_patternReadyAt`/`CommitSelection`/`LastUsedPatternType` are ordinary **instance** fields — each `CombatState` subclass instance (one per boss state-machine instance) has its own copy. No shared/static state risk between `SpiritCombatState` and `WaterMonsterCombatState` instances.

### Current `WaterMonsterCombatState.SelectAttackStrategy` Structure (migration target)

```csharp
// Source: Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs (lines 44-96)
protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
{
    if (!(boss is WaterMonsterController wmc))
        return base.SelectAttackStrategy(boss, dist);

    // Pre-guard 1: Enrage zone-spawn (OUT OF SCOPE — keep exactly as-is)
    if (_isEnraged && wmc.CanSpawnZone()) { ...; _decisionTimer = 1.0f; return null; }

    // Pre-guard 2: teleport transition (OUT OF SCOPE — keep exactly as-is)
    if (PuddleStackManager.Instance != null && ... && wmc.CanTeleport())
    { boss.ChangeState(new WaterTeleportState()); return null; }

    // Candidate pool — built FRESH every call (no caching), phase read live via wmc.IsPhase2/IsPhase3
    var pool = new List<IAttackStrategy> { new WaterGeyser() };
    if (Time.time - _lastWaveTime >= WaveAttackCooldown) pool.Add(new WaterWavePush());  // manual 45s gate
    if (dist <= 3.0f) { pool.Add(new WaterMeleeSwipe()); pool.Add(new WaterJumpLand()); }
    else pool.Add(new WaterRangedSpit());
    if (wmc.IsPhase3) pool.Add(new WaterColorPrison());
    else if (wmc.IsPhase2) pool.Add(new WaterPrisonMapAoe());
    else pool.Add(new WaterPrisonAttack());

    IAttackStrategy selected = pool[Random.Range(0, pool.Count)];
    if (selected is WaterWavePush) _lastWaveTime = Time.time;   // manual reuse-lock bookkeeping
    return selected;
}
```

Also relevant — `Execute()` override:
```csharp
// Source: WaterMonsterCombatState.cs lines 27-37
public override void Execute(BossController boss)
{
    base.Execute(boss);   // may set _decisionTimer = attack.Cooldown, AFTER calling SelectAttackStrategy internally
    if (_isEnraged) _decisionTimer *= _enrageCooldownMultiplier;   // existing D-03 precedent for _decisionTimer only
}
```
Important ordering fact: `base.Execute()` internally calls `SelectAttackStrategy` (step 5) **before** it sets `_decisionTimer = attack.Cooldown` (step 5 tail). So `_isEnraged` is already known and stable at the moment `SelectAttackStrategy` (and therefore any new `_patternReadyAt` commit) runs during the same `Execute()` call — there is no ordering hazard in reading `_isEnraged` from inside `SelectAttackStrategy` or a `CommitSelection` hook triggered from within it.

### Reference Pattern — `SpiritCombatState` (Phase 7, already on the helper)

```csharp
// Source: Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs
public override void Enter(BossController boss)
{
    base.Enter(boss);
    _candidates = (boss is SpiritController spirit) ? BuildCandidates(spirit) : null;  // ONE-TIME build
}

protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
{
    if (LastUsedPatternType == typeof(SpiritExhaustion))
        return ForceSelectPattern(new SpiritWakeRepel());  // forced chain, bypasses SelectWeightedPattern entirely

    return SelectWeightedPattern(dist, _candidates);
}
```
**Why this exact pattern (`Enter()`-once caching) will NOT work unmodified for WaterMonster:** WaterSpirit's candidate list has no phase-dependent members — every candidate is valid for the entire combat session, so building it once in `Enter()` is safe. WaterMonster's prison-variant candidate (`WaterPrisonAttack`/`WaterPrisonMapAoe`/`WaterColorPrison`) depends on `wmc.IsPhase2`/`IsPhase3`, which flip mid-session (HP-threshold triggered via `WaterMonsterController.CheckPhase2Trigger`/`CheckPhase3Trigger`, wired to `WaterStats.OnDamageTaken`). If WaterMonster copied the `Enter()`-once pattern, a phase transition mid-fight would leave the candidate list showing a stale phase's prison pattern for the rest of combat. **Recommendation:** build the candidate list (or at minimum, re-evaluate/replace the single phase-dependent candidate) inside `SelectAttackStrategy` on every call — this exactly matches what the current pre-migration code already does (fresh `pool` list per call), so it is a zero-regression, zero-new-GC-profile choice, not a novel performance tradeoff.

### Recommended Extension Shape for `CombatState` (addresses D-01d, D-03b)

Not mandated by CONTEXT.md (both are "Claude's Discretion"), but the following shape satisfies all constraints found during research with minimal call-site disruption to `SpiritCombatState`:

```csharp
// PatternCandidate: add an optional cooldown override so committed _patternReadyAt
// duration can differ from IAttackStrategy.Cooldown (needed for WaterWavePush's 45s).
public PatternCandidate(System.Func<IAttackStrategy> factory, float weight,
                        float? minDistance = null, float? maxDistance = null,
                        float? cooldownOverride = null) { ... }

// SelectWeightedPattern: add an optional decay flag. Default false preserves
// SpiritCombatState's existing full-exclusion behavior with ZERO changes to its call site.
protected IAttackStrategy SelectWeightedPattern(float dist, IReadOnlyList<PatternCandidate> candidates,
                                                bool decayLastUsed = false)
{
    // when decayLastUsed == false: unchanged (today's `continue` full-exclusion)
    // when decayLastUsed == true: candidate matching LastUsedPatternType is NOT skipped;
    //   its effective weight for the roulette (and totalWeight sum) is multiplied by 0.5f instead.
}

// Cooldown multiplier hook: virtual, defaults to 1f (no behavior change for any existing
// subclass that doesn't override it). WaterMonsterCombatState overrides to return
// _enrageCooldownMultiplier when _isEnraged.
protected virtual float GetPatternCooldownMultiplier() => 1f;

// CommitSelection: use override cooldown if present, else strategy.Cooldown; apply multiplier.
private IAttackStrategy CommitSelection(IAttackStrategy strategy, float? cooldownOverride = null)
{
    System.Type t = strategy.GetType();
    LastUsedPatternType = t;
    float baseCooldown = cooldownOverride ?? strategy.Cooldown;
    _patternReadyAt[t] = Time.time + baseCooldown * GetPatternCooldownMultiplier();
    return strategy;
}
```

This is one viable shape, not a locked design — the planner has full discretion per D-01d/D-03b to choose a different API surface (e.g., a separate overload instead of optional params, or a field on `PatternCandidate` instead of a method parameter for decay). The key **constraints** this shape must satisfy, regardless of exact API chosen:
1. `SpiritCombatState.cs` must require zero code changes (existing full-exclusion behavior preserved by default).
2. WaterWavePush must be able to commit a `_patternReadyAt` duration of 45s (22.5s enraged) that is independent of its own `Cooldown => 3f`.
3. The enrage multiplier must reach `_patternReadyAt` writes, not just `_decisionTimer`.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Weighted random selection among eligible patterns | A new custom roulette-wheel loop in `WaterMonsterCombatState` | `CombatState.SelectWeightedPattern` (already implements cumulative-weight roulette with float-precision boundary correction) | Exact purpose of Phase 7's helper; duplicating it defeats D-08b's stated goal of "다른 보스도 재사용" |
| Per-pattern reuse cooldown tracking | A second parallel `Dictionary<Type,float>` or per-pattern `float _lastXTime` fields in `WaterMonsterCombatState` (mirroring the current `_lastWaveTime` approach for every pattern) | `CombatState._patternReadyAt` (extended with an override value for WaterWavePush) | Keeping `_lastWaveTime`-style manual tracking for WaterWavePush while other patterns use the new helper's `_patternReadyAt` creates two divergent cooldown systems in the same class — a maintenance/consistency risk and dead-code risk once `_patternReadyAt` exists (CLAUDE.md §3 orphan-cleanup obligation applies to `_lastWaveTime`/`WaveAttackCooldown` once migrated) |
| Consecutive-repeat prevention with partial weight reduction | A bespoke "decay map" tracked separately from `LastUsedPatternType` | Extend `SelectWeightedPattern` itself (single source of truth for both full-exclusion and decay, since both need `LastUsedPatternType`) | Avoids two boss classes tracking "last pattern" through different mechanisms while relying on the same base field |

**Key insight:** Every piece of judgment logic this phase needs (weighted random, cooldown gating, distance gating, consecutive-use handling) already has a partial implementation in `CombatState`. The work is 90% "extend the existing helper's parameter surface" and 10% "wire `WaterMonsterCombatState` to declare candidates instead of building a `List<IAttackStrategy>` by hand."

## Common Pitfalls

### Pitfall 1: `IAttackStrategy.Cooldown` is silently overloaded for two different meanings
**What goes wrong:** If `WaterWavePush` is added to the new candidate pool without a cooldown override, `CommitSelection` will lock it out for only 3 seconds (its `Cooldown` property value used for the post-attack `_decisionTimer`), not 45 seconds — breaking D-04 without any compile error or obvious symptom (it will just fire far more often than intended).
**Why it happens:** `strategy.Cooldown` currently serves double duty: (1) sets `_decisionTimer` after any attack executes (`base.Execute`), and (2) — once the helper is adopted — would be the sole source for `_patternReadyAt`. `WaterWavePush`'s "true" reuse-lockout (45s) has always been a value distinct from its `Cooldown => 3f`, tracked manually via `_lastWaveTime`/`WaveAttackCooldown` outside `IAttackStrategy` entirely.
**How to avoid:** Give the candidate for `WaterWavePush` an explicit cooldown override (45f, or a named constant) that `CommitSelection` uses instead of `strategy.Cooldown` when present. Confirm this override, not `strategy.Cooldown`, is what feeds `_patternReadyAt`.
**Warning signs:** Play-testing shows WaterWavePush firing much more frequently than the old build; or a grep for `WaveAttackCooldown`/`45f` turns up nothing in the new code (silently dropped instead of migrated).

### Pitfall 2: Enrage multiplier only reaches `_decisionTimer`, not `_patternReadyAt`
**What goes wrong:** Without a new hook, all pattern cooldowns (including the 45s special-case) will NOT speed up under Enrage, violating D-03/D-04b, even though the overall `_decisionTimer` still does (existing behavior, unchanged).
**Why it happens:** `_patternReadyAt` writes are buried inside the `private CommitSelection`, called from `SelectWeightedPattern`/`ForceSelectPattern` inside the base class — `WaterMonsterCombatState`'s existing `_enrageCooldownMultiplier *= _decisionTimer` line in `Execute()` has no reach into that private write path.
**How to avoid:** Add an extension point in `CombatState` (virtual method, protected field, or parameter threaded from `SelectAttackStrategy` call sites) that `WaterMonsterCombatState` can use to scale the cooldown duration written to `_patternReadyAt` at commit time.
**Warning signs:** Grep for `_enrageCooldownMultiplier` usages after migration — if it only appears once (in the existing `Execute()` override) and never near the new candidate-selection code, D-03 is unmet.

### Pitfall 3: Copying `SpiritCombatState`'s `Enter()`-once candidate caching verbatim
**What goes wrong:** Prison-variant candidate would freeze at whatever phase was active when `CombatState.Enter()` last ran (combat-state re-entry, e.g., after `WaterTeleportState` returns to `WaterMonsterCombatState`) or never update at all if `Enter()` only runs once per fight — a Phase 2→3 mid-fight transition would keep offering `WaterPrisonMapAoe` instead of switching to `WaterColorPrison`.
**Why it happens:** `SpiritCombatState` is the only extant reference implementation of the helper, and its `Enter()`-once approach is natural to copy without checking whether it depends on session-invariant candidates (it does — WaterSpirit has no mid-fight phase changes).
**How to avoid:** Build (or at least refresh the phase-dependent slot of) the candidate list inside `SelectAttackStrategy`, called every judgment tick — matching the pre-migration code's behavior of constructing `pool` fresh every call.
**Warning signs:** A `_candidates` field assigned only in `Enter()`, with no rebuild logic tied to `IsPhase2`/`IsPhase3`.

### Pitfall 4: Confusing "no candidates found nothing" (helper returns `null`) with the pre-guard `null` returns
**What goes wrong:** Both `SelectWeightedPattern` returning `null` (all candidates gated by cooldown/distance/exclusion) and the existing zone-spawn/teleport pre-guards returning `null` cause `Execute()`'s step 5 to simply skip starting an attack this frame and retry next frame (existing, unchanged contract per `07-CONTEXT.md` D-07b). This is expected/benign, but if the pre-guards are accidentally moved inside the candidate-building logic (instead of staying as early returns before it, per D-05a), the semantics of "state-transition trigger" vs "temporarily no eligible attack" become conflated.
**Why it happens:** Both paths look identical from the caller's perspective (`SelectAttackStrategy` returning `null`).
**How to avoid:** Keep the two pre-guards (`CanSpawnZone`/`CanTeleport`) as literal early-`return null`/`ChangeState` statements at the very top of `SelectAttackStrategy`, exactly as today, entirely before any `PatternCandidate` list construction — do not express them as `PatternCandidate` entries with some kind of "always-excluded" trick.
**Warning signs:** Zone-spawn or teleport logic appearing inside a `BuildCandidates`-style method, or a `PatternCandidate` whose factory calls `wmc.SpawnRandomZone()`.

### Pitfall 5: Distance boundary semantics differ subtly between `<=`/`>` and `MinDistance`/`MaxDistance`
**What goes wrong:** Old code: `dist <= 3.0f` → melee/jump pool; `else` (i.e. `dist > 3.0f`) → ranged pool — perfectly mutually exclusive at `dist == 3.0`. The helper's filters are `MaxDistance`: exclude if `dist > MaxDistance` (so `dist == 3.0` passes when `MaxDistance = 3.0`) and `MinDistance`: exclude if `dist < MinDistance` (so `dist == 3.0` also passes when `MinDistance = 3.0`). Using `maxDistance: 3.0f` for melee/jump and `minDistance: 3.0f` for ranged makes both branches simultaneously eligible at the exact float value `dist == 3.0`, unlike the original strict either/or.
**Why it happens:** `<=`/`else` and `MinDistance`/`MaxDistance` inclusive-on-both-ends semantics are not identical at the boundary.
**How to avoid:** This is a negligible-probability edge case (an exact float distance of precisely 3.0) and not something CONTEXT.md flags as a hard requirement to preserve bit-for-bit — call it out to the planner as an accepted minor behavior nuance, not a blocker, unless the planner wants to nudge one bound by an epsilon for exact parity.
**Warning signs:** N/A — flagged for awareness only, not expected to be observable in practice.

### Pitfall 6: Per-pattern cooldowns are a NEW behavior for the non-WavePush patterns, not a pure refactor
**What goes wrong/context:** Prior to this migration, only `WaterWavePush` had any reuse-lockout; `WaterGeyser`, `WaterMeleeSwipe`, `WaterJumpLand`, `WaterRangedSpit`, and the prison variants could all repeat immediately (`Random.Range` over the full pool every tick, no cooldown gating, no anti-repeat). Adopting `CommitSelection`'s default behavior gives every pattern an automatic `_patternReadyAt` lockout equal to its own `Cooldown` (e.g., `WaterMeleeSwipe` locked 1.4s, `WaterGeyser` locked 3s, `WaterColorPrison` locked 10s, etc.), which is a genuine behavior change beyond what D-01/D-02 explicitly asked for (they only mandate decay-based anti-repeat for the *immediately previous* pattern, not a standing self-cooldown for every pattern).
**Why it happens:** This is simply how `SelectWeightedPattern`/`CommitSelection` already work — it is inherent to reusing the Phase 7 helper as-is, not something WaterMonster-specific.
**How to avoid / note for planner:** This is not a bug to fix — it mirrors exactly what happened when `SpiritCombatState` adopted the same helper in Phase 7 (every Spirit pattern gained an implicit self-cooldown it didn't have as a hard array-rotation constraint before). Treat as established precedent and an accepted side effect, but the planner should record it explicitly (e.g., in the plan's decisions) so it isn't mistaken for scope creep during review — D-02a's "동일한 체감 목표" is about *weight uniformity*, not about eliminating self-cooldowns entirely.

## Code Examples

See "Architecture Patterns" above for verified current-state snippets (`CombatState.cs`, `WaterMonsterCombatState.cs`, `SpiritCombatState.cs`) — all sourced directly from the files listed in `<files_to_read>`, not from training-data assumptions about Unity/C# patterns in general.

## Runtime State Inventory

Not applicable. This phase is an internal C# logic refactor (no renaming, no persisted data, no external service configuration, no OS-level registration, no secrets). Nothing in this migration touches stored data, live external service config, OS-registered state, secrets/env vars, or build artifacts — it is a pure in-memory algorithm swap inside two `.cs` files evaluated fresh every play session. Section omitted per the "omit entirely for greenfield/non-rename phases" rule.

## Open Questions

1. **Should the per-pattern self-cooldown side effect (Pitfall 6) be scoped down to only the previously-cooldowned pattern (WaterWavePush), or accepted for all patterns as an emergent property of adopting the helper?**
   - What we know: CONTEXT.md D-02a says weights stay uniform "to feel like the old uniform pool-random," but doesn't explicitly address self-cooldowns for non-WavePush patterns. Phase 7 precedent (SpiritCombatState) shows every migrated pattern gaining a self-cooldown was accepted there without objection.
   - What's unclear: Whether the user considered this specific consequence when approving D-02a for WaterMonster, since WaterMonster's pre-migration pool truly had zero gating except WavePush (unlike WaterSpirit's array rotation which already implied *some* spacing).
   - Recommendation: Proceed with the Phase 7 precedent (accept it as inherent to helper adoption) but have the plan explicitly document this decision so it's visible for user review before implementation, rather than silently emerging as an implementation detail.

2. **Exact API shape for the decay parameter and cooldown-override/multiplier hook (D-01d, D-03b) — multiple valid shapes exist.**
   - What we know: The constraints (zero changes to `SpiritCombatState.cs`, override cooldown independent of `strategy.Cooldown`, multiplier reaching `_patternReadyAt`) are clear from research.
   - What's unclear: Whether the planner prefers optional-parameter overloads (shown above), a separate new method name (e.g. `SelectWeightedPatternWithDecay`), or fields directly on `PatternCandidate` (e.g. `DecayOnRepeat: bool`, `CooldownOverride: float?`) declared per-candidate instead of per-call.
   - Recommendation: A per-candidate field on `PatternCandidate` (rather than a per-call flag on `SelectWeightedPattern`) is slightly more flexible for future bosses that might want to mix decay-candidates and exclude-candidates within the same call, but either shape satisfies all decisions. Planner's discretion per D-01d.

3. **Exact rebuild strategy for D-06c — full list rebuild every `SelectAttackStrategy` call vs. static+dynamic split.**
   - What we know: Rebuilding the full candidate list every call matches the pre-migration GC/CPU profile (already done today) and guarantees correctness with minimal code.
   - What's unclear: Whether a micro-optimization (cache the 4 phase-invariant candidates once in `Enter()`, and only reconstruct/replace the 1 phase-dependent prison candidate on each call, or on a phase-transition event) is worth the added complexity.
   - Recommendation: Default to full rebuild every call (simplest, matches existing behavior, avoids a stale-cache class of bugs) unless the plan surfaces a concrete performance concern — this is a boss `SelectAttackStrategy` call gated behind `_decisionTimer <= 0`, not a per-frame hot path in the profiling sense that pre-migration code wasn't already paying.

## Sources

### Primary (HIGH confidence — direct file reads, this repository)
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — full read, current helper implementation
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — full read, migration target
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — full read, `IsPhase2`/`IsPhase3`/`IsEnraged`/`CanSpawnZone`/`CanTeleport` definitions and trigger wiring
- `Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs` — full read, reference implementation
- `Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs` — full read, `Cooldown` contract
- `Assets/Enemy/WaterMonster/Script/States/Attacks/*.cs` (grep) — verified `Cooldown` values: WaterGeyser=3f, WaterWavePush=3f (own Cooldown, distinct from the manual 45s reuse-lock), WaterMeleeSwipe=1.4f, WaterJumpLand=4f, WaterRangedSpit=2.0f, WaterPrisonAttack=5f, WaterPrisonMapAoe=8f, WaterColorPrison=10f
- `.planning/phases/08-watermonster-combatstate/08-CONTEXT.md` — locked decisions (D-01~D-06)
- `.planning/phases/07-boss-attack-pattern-judgment/07-CONTEXT.md` — Phase 7 decisions, D-08b deferred item
- `.planning/phases/07-boss-attack-pattern-judgment/07-01-SUMMARY.md` — Phase 7 implementation decisions (Time.time absolute comparison, type-key tracking rationale)
- `.planning/REQUIREMENTS.md`, `.planning/STATE.md` — project/milestone context
- `.planning/config.json` — confirmed `workflow.nyquist_validation: false` (Validation Architecture section correctly omitted below)

### Secondary / Tertiary
None used — this phase required no external web research; all unknowns were resolvable from direct source inspection.

## Metadata

**Confidence breakdown:**
- Standard Stack (internal APIs): HIGH — read directly from source, current as of this session
- Architecture Patterns: HIGH — verified against actual `CombatState.cs`/`WaterMonsterCombatState.cs`/`SpiritCombatState.cs` contents, not training-data assumptions
- Pitfalls: HIGH for Pitfalls 1–4 (directly derived from reading `CommitSelection`/`_patternReadyAt` source and comparing to `WaterMonsterCombatState`'s existing `_lastWaveTime` mechanism); MEDIUM for Pitfall 5 (float-boundary edge case reasoning, extremely low real-world impact) and Pitfall 6 (behavioral judgment call, flagged as Open Question 1 for user/planner visibility)

**Research date:** 2026-07-27
**Valid until:** Stable — this is a closed, internal-only codebase snapshot; valid until `CombatState.cs`/`WaterMonsterCombatState.cs`/`SpiritCombatState.cs` are next modified by another phase or agent.

## Environment Availability

Skipped — no external dependencies (tools, services, runtimes, CLIs) are introduced or required by this phase. It is a pure in-repo C#/Unity code change consumed entirely within the existing Unity project.

## Validation Architecture

Skipped — `.planning/config.json` sets `workflow.nyquist_validation: false`. Per Phase 7 precedent (`07-02-PLAN.md`), Play-mode verification for this phase is deferred to a post-migration checklist (`Check.md`-style, to be authored alongside `Assets/Enemy/WaterSpirit/Check.md` and `Assets/Enemy/Tutorial/TutorialBoss/Check.md`) rather than automated tests — no existing automated test framework was found for boss AI logic in this project (Unity PlayMode/EditMode test infra not detected under `Assets/`).
