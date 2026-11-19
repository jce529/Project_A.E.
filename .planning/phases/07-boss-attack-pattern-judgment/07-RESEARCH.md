# Phase 7: 보스 공격 패턴 판단 로직 리팩토링 - Research

**Researched:** 2026-07-27
**Domain:** Unity 6 (6000.3.10f1) C# gameplay state-machine refactor — enemy AI pattern selection
**Confidence:** HIGH (all findings grounded in direct reading of current project source; no external library research required)

## Summary

This phase is a pure internal refactor of C# gameplay logic inside an existing Unity 6 project. No new packages, no Context7/WebSearch research applies — the "stack" here is the project's own `IBossState` / `IAttackStrategy` architecture. The work is: (1) add a generic, reusable "weighted-candidate pattern selector" to `CombatState` (the shared base class already used by `WaterMonsterCombatState` and `SpiritCombatState`/`Stage2CombatState`), and (2) replace `SpiritCombatState`'s fixed round-robin array with a declarative list of pattern candidates that the new helper evaluates.

I read the full current source of `CombatState.cs`, `SpiritCombatState.cs`, `Stage2CombatState.cs`, `WaterMonsterCombatState.cs` (existing pool-random reference), `TutorialBossController.cs`/`TutorialAttackState.cs` (cooldown/no-repeat reference), `BossController.cs`, `GroggyState.cs`, `IAttackStrategy.cs`, `SpiritController.cs`, and all five `Attacks/*.cs` strategy files named in CONTEXT.md. Key confirmed facts and one important correction to CONTEXT.md's canonical_refs are below.

**Primary recommendation:** Add a `protected` nested `PatternCandidate` data class + a `protected IAttackStrategy SelectWeightedPattern(...)` helper method directly on `CombatState`, backed by two new `protected` fields (`Type _lastUsedPatternType`, `Dictionary<Type,float> _patternReadyTime` using `Time.time` timestamps — not per-frame decrementing counters, to avoid coupling with the existing early-return branches in `Execute()`). `SpiritCombatState` becomes a thin data layer: it declares its candidate list once, special-cases the Exhaustion→WakeRepel forced chain by reading the same `_lastUsedPatternType` the helper already tracks, and otherwise delegates to the helper. `Stage2CombatState` requires **zero changes** — verified below.

## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01 판단 로직 아키텍처:** `CombatState`에 범용 우선순위 기반 패턴 후보 평가 헬퍼 추가 (거리 범위/쿨다운/연속사용금지/가중치를 가진 "패턴 후보" 목록 선언 구조). `SpiritCombatState`는 이 헬퍼 위에 자신의 패턴 후보 목록만 선언하는 얇은 데이터 레이어. 목적: 향후 WaterMonster 등 다른 보스도 재사용 가능하도록 일반적으로 설계 (WaterMonster 자체 마이그레이션은 이번 범위 아님).
- **D-02 순간이동형 패턴 발동 조건:** `SpiritCharge`, `SpiritFarProjectile`은 거리 조건 없이 쿨다운 + 직전 패턴과 다름(연속금지)만으로 판단. 헬퍼는 패턴별로 거리 조건을 선택적(optional)으로 걸 수 있어야 함 — 이 유연성은 헬퍼 설계 자체의 요구사항.
- **D-03 패턴 우선순위 — 가중치 기반 랜덤:** 고정 우선순위가 아니라 현재 조건(쿨다운 통과+연속금지 통과+거리 조건 있으면 통과)을 만족하는 후보들 중 가중치 기반 랜덤으로 하나 선택. 가중치 구체값은 플래너/구현 재량. WaterMonsterCombatState의 풀 기반 랜덤 선택 방식 참고 가능.
- **D-04 패턴 체인 — Exhaustion → WakeRepel 강제 연결:** `SpiritExhaustion` 실행 직후에는 판단 로직을 거치지 않고 무조건 `SpiritWakeRepel`을 다음 패턴으로 강제 실행. WakeRepel 실행이 끝나면 다시 일반 판단 풀로 복귀 (WakeRepel은 체인 밖에서도 독립 후보로 계속 존재). 체인 트리거 조건은 "직전 실행 패턴 == SpiritExhaustion"이며 일반 후보 평가보다 우선 처리.
- **D-05 연속 사용 금지 규칙:** TutorialBoss와 동일하게 직전 실행 패턴과 같은 패턴은 이번 판단에서 후보 제외. 5개 패턴(Charge/Exhaustion/WakeRepel/FarProjectile + 중복 슬롯) 모두 동일 규칙 적용. D-04 체인은 연속금지 규칙보다 우선한다(체인 강제 실행에는 연속금지 미적용).
- **D-06 회전 배열 0번/4번 중복 슬롯(`SpiritCharge` 중복):** 이번 페이즈에서 손대지 않음. 새 판단 로직에서는 `SpiritCharge`를 하나의 후보로만 등록(중복은 라운드로빈 배열 자체가 사라지므로 자연히 정리됨). "5. 순간이동+돌진" 주석은 실제 미구현 의도였다는 점만 기록, 새 패턴 생성 안 함.
- **D-07 Stage 2 헤비콤보 카운터 연동:** `Stage2CombatState`의 "일반 패턴 3회 후 헤비콤보 전환" 카운터 로직 그대로 유지. `SelectAttackStrategy`가 조건 불충족 시 `null`을 반환할 수 있다는 기존 계약은 동일 유지되므로, 새 판단 로직 도입 후에도 `_patternsExecuted` 카운터는 수정 없이 호환됨.
- **D-08 적용 범위:** WaterSpirit 보스(`SpiritCombatState`, `Stage2CombatState`)에만 적용. `WaterMonsterCombatState`는 건드리지 않음(다음 페이즈 후보로 별도 기록).

### Claude's Discretion

- 범용 헬퍼의 정확한 API 형태(클래스/구조체 설계, 후보 등록 방식).
- 각 패턴의 쿨다운/가중치 수치 — 기존 `IAttackStrategy.Cooldown` 값 유지 또는 조정 가능.
- 근접(WakeRepel)·원거리(기본 투사체) 패턴의 거리 임계값 — 기존 `RepelRange`/`ChargeRange`/`ProjectileRange` Inspector 값 재사용 여부.
  - **Research correction:** `ProjectileRange` field does **not** currently exist on `SpiritController` (see Environment/Code Findings below). Only `RepelRange` and `ChargeRange` exist today, and `ChargeRange` is currently unused dead code. Planner must decide: add a new field, or repurpose `ChargeRange` as the far-distance threshold for `SpiritFarProjectile`.

### Deferred Ideas (OUT OF SCOPE)

- `WaterMonsterCombatState`를 동일 범용 헬퍼로 마이그레이션 — 향후 페이즈 후보.
- 회전 배열 0/4번 중복 슬롯을 실제로 구분되는 새 변종 패턴("진짜 순간이동+돌진")으로 만드는 것 — 이번엔 보류.

## Project Constraints (from CLAUDE.md)

- **Think before coding:** No plan/code should be written before this research feeds `/gsd:plan-phase`; ambiguities must be surfaced to the user, not silently resolved.
- **Simplicity first / scope management:** Only touch `CombatState.cs`, `SpiritCombatState.cs` (and verify, not rewrite, `Stage2CombatState.cs`). Do not touch `WaterMonsterCombatState.cs` (explicitly deferred) and do not pre-build anything for a hypothetical future WaterMonster migration beyond making the helper reasonably generic per D-01c.
- **Surgical changes:** Do not "clean up" or reformat adjacent code (e.g., don't touch `RangedPokeAttack`/`HeavyAttack`/`LightAttack` default `SelectAttackStrategy` fallback in base `CombatState`, don't touch `IdleState`/`ChaseStates`/`GroggyState`). Only remove orphans created by *this* change (e.g., `_pattern`/`_patternIndex` fields become dead once replaced — these must be removed since the change itself orphans them). Do not delete the pre-existing unused `ChargeRange` field unless the plan actively repurposes it — otherwise just note it.
- **Traceability:** Every changed line must map to a specific decision (D-01 through D-08) in this phase's plan.
- **Verification discipline:** No automated test framework exists in `Assets/` (Unity Test Runner not set up for this project — see Environment section). Verification will be manual/logic-inspection based (Debug.Log output + play-mode observation), consistent with how Phases 5/6 were verified.
- **YOLO mode:** `.planning/config.json` does not currently set `"mode": "yolo"` (only `workflow.nyquist_validation: false` and `_auto_chain_active: false` are set) — standard confirmation flow applies unless the user has set yolo elsewhere.

## Current Code — Exact Signatures and Contracts

### `CombatState.cs` (`Assets/Enemy/NewBoss/Script/States/CombatState.cs`)

```csharp
public class CombatState : IBossState
{
    protected float _decisionTimer;
    private IAttackStrategy _currentAttack;
    private bool _isAttacking;
    private float _attackWaitTimer;
    private const float MaxAttackDuration = 0.5f;

    public virtual void Enter(BossController boss) { ... _decisionTimer = 0; _isAttacking = false; _currentAttack = null; ... }

    public virtual void Execute(BossController boss)
    {
        // 1. if attacking: wait for CheckAnimationState(...) or MaxAttackDuration timeout, then clear _isAttacking
        // 2. _decisionTimer -= Time.deltaTime; if (_decisionTimer > 0) return;
        // 3. groggy check -> ChangeState(new GroggyState())
        // 4. distance check -> ChangeState(new IdleState()) or ChangeState(new ChaseState())
        // 5. IAttackStrategy attack = SelectAttackStrategy(boss, dist);
        //    if (attack != null) { _currentAttack=attack; _isAttacking=true; attack.ExecuteAttack(boss); _decisionTimer = attack.Cooldown; }
    }

    protected virtual bool ShouldTransitionToGroggy(BossController boss) => !boss.Stats.IsBarrierActive;

    protected virtual IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (dist > 8f) return new RangedPokeAttack();
        if (boss.CanUseHeavyAttack) return new HeavyAttack();
        return new LightAttack();
    }

    public virtual void Exit(BossController boss) { _isAttacking = false; _currentAttack = null; }
}
```

**Contract confirmed:** `SelectAttackStrategy(BossController boss, float dist)` returns `IAttackStrategy` or `null`. Returning `null` means "no attack started this frame" — `Execute()` does nothing further, `_decisionTimer` and `_isAttacking` stay unchanged, and because `_decisionTimer` was already `<= 0` to reach this branch, **`SelectAttackStrategy` will be called again on the very next frame** (this is exactly how `Stage2CombatState` currently uses `null` to "hold" while waiting to trigger heavy combo — see below). This null-retry-every-frame behavior is a load-bearing existing contract; any new helper must preserve it.

`CombatState` is a **plain C# class, not a `MonoBehaviour`** — no Unity lifecycle (`OnEnable`/`OnDisable`) concerns for new fields added here; it's instantiated via `new CombatState()`/`new SpiritCombatState()` and swapped by `BossController.ChangeState()`, which calls `Exit()` then `Enter()` on the new instance. **New state persists only within one continuous combat session** — if the boss leaves `SpiritCombatState`/`Stage2CombatState` (distance-out → `IdleState`/`ChaseState`, or Stage2's groggy cycle → `GroggyState` → base `CombatState` → `SpiritController.Update()` intercept creates a **new** `Stage2CombatState()` instance), all new tracking fields (cooldown timestamps, last-used-pattern, chain flag) reset to defaults. This matches the *existing* behavior of `_patternIndex` resetting to 0 in `SpiritCombatState.Enter()` today, so it is not a regression — just something to confirm the plan accounts for (per-session state, not global/persistent).

### `SpiritCombatState.cs` (current — to be replaced)

```csharp
public class SpiritCombatState : CombatState
{
    private static readonly System.Func<IAttackStrategy>[] _pattern = new System.Func<IAttackStrategy>[] {
        () => new SpiritCharge(), () => new SpiritExhaustion(), () => new SpiritWakeRepel(),
        () => new SpiritFarProjectile(), () => new SpiritCharge(),
    };
    private int _patternIndex;

    public override void Enter(BossController boss) { base.Enter(boss); _patternIndex = 0; }
    protected override bool ShouldTransitionToGroggy(BossController boss) => false;
    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is SpiritController)) return null;
        IAttackStrategy attack = _pattern[_patternIndex]();
        _patternIndex = (_patternIndex + 1) % _pattern.Length;
        return attack;
    }
}
```

Notice `dist` is currently an **unused parameter** — the round-robin array ignores it entirely, confirming CONTEXT's framing that the original distance-based intent (Phase 5 D-03a) was lost. `ShouldTransitionToGroggy` is hard-locked to `false` for Stage 1 (Spirit boss has no barrier/groggy of its own — must stay unchanged).

### `Stage2CombatState.cs` (verify-only — D-07b compatibility check)

```csharp
public class Stage2CombatState : SpiritCombatState
{
    private int _patternsExecuted = 0;
    private const int PatternsBeforeHeavyCombo = 3;

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (_patternsExecuted >= PatternsBeforeHeavyCombo)
        {
            TriggerHeavyComboCycle(boss);
            return null; // holds; base Execute() will retry next frame
        }
        var strategy = base.SelectAttackStrategy(boss, dist); // calls into SpiritCombatState's (new) helper-based logic
        if (strategy != null) _patternsExecuted++;
        return strategy;
    }
}
```

**Confirmed: D-07b holds.** `Stage2CombatState` depends on exactly one thing from `SpiritCombatState.SelectAttackStrategy`: whether it returns `null` or non-`null`. It does not inspect *which* strategy came back, does not care about weights/distance/cooldowns, and calls `base.SelectAttackStrategy(boss, dist)` exactly once per invocation (before the 3-count threshold is hit). **No changes to `Stage2CombatState.cs` are required.** The only thing the plan must guarantee is that the new `SpiritCombatState.SelectAttackStrategy` keeps the same signature and null/non-null semantics (which it will, since the pool-exhausted case naturally returns `null` the same way the old round-robin never did — round-robin always returned non-null; the new helper *can* return null when the pool is empty, see Pitfalls). This is compatible but changes behavior subtly: previously Stage2's counter always incremented every call (round robin never fails); now it's possible (edge case) for `SelectAttackStrategy` to return `null` due to pool exhaustion even when not in the heavy-combo-hold state, which would just cause a same-frame retry next frame without incrementing the counter — harmless, but confirm the plan's pool design keeps at least one always-eligible (no-distance-condition) candidate to make this a near-zero-probability event (see below).

### `IAttackStrategy.cs`

```csharp
public interface IAttackStrategy
{
    void ExecuteAttack(BossController boss);
    float Cooldown { get; }
    string AnimationName { get; }
}
```

No existing per-instance cooldown-remaining tracking — `Cooldown` is a fixed duration value per strategy type (e.g. `SpiritCharge.Cooldown => 3.0f`), read once when the attack starts and copied into `CombatState._decisionTimer`. There is no built-in per-pattern "time since last used" tracking anywhere in the engine today — this must be added by the new helper.

### WaterSpirit attack strategies (`Assets/Enemy/WaterSpirit/Script/States/Attacks/`)

| Strategy | Cooldown | Behavior | Distance-relevant? |
|---|---|---|---|
| `SpiritCharge` | 3.0f | Teleports near player, windup, then charges through a fixed overshoot point | No (self-teleports; matches D-02a) |
| `SpiritExhaustion` | 2.5f | Synchronous (no coroutine) — just `boss.StopMove()`, no attack; sets up vulnerability window | Not specified in CONTEXT — see Open Questions |
| `SpiritWakeRepel` | 1.5f | Waits 0.4s, then calls `new SpiritRepel().ExecuteAttack(boss)` | Yes — logically "근접" (Repel only hits within `RepelRange`) |
| `SpiritFarProjectile` | 2.5f | Teleports to `2× MaxTeleportRadius` from player, waits 0.4s, fires `SpiritProjectileAttack` | Yes — logically "원거리" but *also* self-teleports first (see Open Question below) |
| `SpiritRepel` | 1.5f | Not itself a pool candidate — invoked internally by `SpiritWakeRepel` | N/A |

`SpiritController.cs` fields relevant to distance thresholds:

```csharp
public float RepelRange = 1.5f;
public float ChargeRange = 5.0f;   // currently UNUSED by any code (dead field, pre-existing)
// no "ProjectileRange" field exists
```

**Research finding / correction to CONTEXT.md:** The canonical_refs section says "기존 `RepelRange`/`ChargeRange`/`ProjectileRange` Inspector 값 재사용 여부는 플래너 재량" — but `ProjectileRange` does not exist in the current `SpiritController.cs`. Only `RepelRange` (1.5) and `ChargeRange` (5.0, currently unused/dead) exist. The plan must either (a) add a new `[SerializeField] float FarProjectileMinRange` field, or (b) repurpose the existing unused `ChargeRange` as the far-distance threshold for `SpiritFarProjectile` eligibility (recommended — avoids a redundant field, and D-08's discretion note already anticipates reusing existing Inspector values).

### Reference: `TutorialBossController.cs` / `TutorialAttackState.cs` (style reference only)

- Cooldown model: **per-pattern countdown floats** (`_tentacleStabCooldownTimer`, `_groundTentacleCooldownTimer`) decremented every frame in the *controller's* `Update()` (independent of Idle/Attack state swapping), because `IBossState` instances there are recreated every Idle↔Attack transition and can't hold persistent timers themselves.
- No-repeat: `public PatternType? LastUsedPattern { get; set; }` lives on the **controller** (survives state recreation), set once per attack in `TutorialAttackState.Enter()`.
- Selection: `SelectPattern()` is a simple **first-match priority list** (`if (CanUseTentacleStab()) return ...; if (CanUseGroundTentacle()) return ...;`), NOT weighted-random. WaterSpirit's D-03 explicitly wants weighted-random instead, so this reference informs *gating style* (cooldown + no-repeat + distance conditions inside boolean-returning checks) but not the final selection algorithm.
- **Important structural difference from WaterSpirit:** because `CombatState`/`SpiritCombatState` do NOT recreate the state object between Idle/Attack (there's no separate Attack state; `CombatState.Execute()` handles both deciding and waiting-for-animation internally), tracking fields can safely live directly on `CombatState`/`SpiritCombatState` as instance fields — there's no need to push them up to `BossController`/`SpiritController` the way TutorialBoss does. This is simpler and keeps the change contained to the State classes, consistent with D-01a/D-01b.

### Reference: `WaterMonsterCombatState.cs` (existing pool-random precedent)

```csharp
var pool = new List<IAttackStrategy> { new WaterGeyser() };
if (Time.time - _lastWaveTime >= WaveAttackCooldown) pool.Add(new WaterWavePush());
if (dist <= 3.0f) { pool.Add(new WaterMeleeSwipe()); pool.Add(new WaterJumpLand()); }
else { pool.Add(new WaterRangedSpit()); }
// ... phase-specific additions ...
IAttackStrategy selected = pool[Random.Range(0, pool.Count)];
```

Confirms two things already used elsewhere in this codebase: (1) `Time.time` timestamp comparison for a single ad-hoc cooldown (`_lastWaveTime`), rather than a decrementing counter — validates the recommended approach below; (2) `UnityEngine.Random.Range(...)` is the established RNG API in this project (not `System.Random`). This is **uniform** random over an eligible pool, not weighted — WaterSpirit's D-03 requires weighted, so the new helper is a superset of this pattern, not a copy.

## Architecture Patterns

### Recommended: `PatternCandidate` + weighted-pool helper on `CombatState`

```csharp
// CombatState.cs — new generic reusable helper (D-01)
using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : IBossState
{
    // ... existing fields unchanged ...

    // --- New: generic pattern-candidate helper (D-01) ---
    protected class PatternCandidate
    {
        public readonly Func<IAttackStrategy> Factory;
        public readonly float Weight;
        public readonly float? MinDistance; // null = no lower bound
        public readonly float? MaxDistance; // null = no upper bound

        public PatternCandidate(Func<IAttackStrategy> factory, float weight,
                                 float? minDistance = null, float? maxDistance = null)
        {
            Factory = factory;
            Weight = weight;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }
    }

    // Cooldown/no-repeat tracking — keyed by strategy Type, using Time.time timestamps
    // (NOT per-frame decrement) so nothing needs to run while Execute() early-returns.
    protected Type LastUsedPatternType { get; private set; }
    private readonly Dictionary<Type, float> _patternReadyAt = new Dictionary<Type, float>();

    protected IAttackStrategy SelectWeightedPattern(float dist, IReadOnlyList<PatternCandidate> candidates)
    {
        var eligible = new List<(PatternCandidate cand, IAttackStrategy instance)>();

        foreach (var c in candidates)
        {
            IAttackStrategy instance = c.Factory();
            Type t = instance.GetType();

            if (c.MinDistance.HasValue && dist < c.MinDistance.Value) continue;
            if (c.MaxDistance.HasValue && dist > c.MaxDistance.Value) continue;
            if (LastUsedPatternType == t) continue; // no-repeat (D-05)
            if (_patternReadyAt.TryGetValue(t, out float readyAt) && Time.time < readyAt) continue; // cooldown

            eligible.Add((c, instance));
        }

        if (eligible.Count == 0) return null; // pool exhausted this frame — Execute() retries next frame (existing contract)

        float totalWeight = 0f;
        foreach (var e in eligible) totalWeight += e.cand.Weight;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var e in eligible)
        {
            cumulative += e.cand.Weight;
            if (roll <= cumulative) return CommitSelection(e.instance);
        }
        return CommitSelection(eligible[eligible.Count - 1].instance); // float-rounding fallback
    }

    // Forced-chain support (D-04): bypasses pool/cooldown/no-repeat entirely
    protected IAttackStrategy ForceSelectPattern(IAttackStrategy strategy) => CommitSelection(strategy);

    private IAttackStrategy CommitSelection(IAttackStrategy strategy)
    {
        Type t = strategy.GetType();
        LastUsedPatternType = t;
        _patternReadyAt[t] = Time.time + strategy.Cooldown;
        return strategy;
    }

    // ... existing Enter/Execute/ShouldTransitionToGroggy/SelectAttackStrategy/Exit unchanged ...
}
```

```csharp
// SpiritCombatState.cs — thin data layer (D-01b)
public class SpiritCombatState : CombatState
{
    private List<PatternCandidate> _candidates;

    public override void Enter(BossController boss)
    {
        base.Enter(boss);
        _candidates ??= BuildCandidates(boss as SpiritController);
    }

    protected override bool ShouldTransitionToGroggy(BossController boss) => false;

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is SpiritController spirit)) return null;

        // D-04: forced chain — highest priority, bypasses pool/cooldown/no-repeat
        if (LastUsedPatternType == typeof(SpiritExhaustion))
            return ForceSelectPattern(new SpiritWakeRepel());

        return SelectWeightedPattern(dist, _candidates);
    }

    private List<PatternCandidate> BuildCandidates(SpiritController spirit)
    {
        return new List<PatternCandidate>
        {
            new PatternCandidate(() => new SpiritCharge(), weight: 1.0f),                                   // D-02a: no distance
            new PatternCandidate(() => new SpiritExhaustion(), weight: 0.6f),                                // no distance (see Open Questions)
            new PatternCandidate(() => new SpiritWakeRepel(), weight: 1.0f, maxDistance: spirit.RepelRange),  // 근접
            new PatternCandidate(() => new SpiritFarProjectile(), weight: 1.0f, minDistance: spirit.ChargeRange), // 원거리 (repurposed field)
        };
    }
}
```

Note the array literal `() => new SpiritCharge()` appearing twice in the old code collapses to a single candidate entry (D-06a) — this happens naturally since we're declaring one candidate per *strategy type*, not per rotation slot.

### Anti-Patterns to Avoid

- **Don't decrement per-pattern cooldowns every `Execute()` frame.** `CombatState.Execute()` early-returns in several branches (while attacking, while `_decisionTimer > 0`), so a "tick down every frame" counter would silently stall or double-tick depending on which branch runs. Use `Time.time` absolute timestamps instead (as `WaterMonsterCombatState._lastWaveTime` already does) — comparison-based, immune to being skipped.
- **Don't key cooldown/no-repeat tracking by candidate *slot index*.** The old bug this phase fixes was exactly that (`_patternIndex` ignoring what the candidate actually was). Key by `strategy.GetType()` instead, so `SpiritCharge` appearing once (D-06a) works correctly and future candidate list edits don't silently break gating.
- **Don't let `Stage2CombatState` special-case the new helper.** It must keep calling `base.SelectAttackStrategy(boss, dist)` and reading only null/non-null, exactly as today (D-07b). Any temptation to have Stage2 reach into candidate lists directly would violate the thin-layer/generic-helper design (D-01b).
- **Don't use `System.Random`.** This codebase exclusively uses `UnityEngine.Random` (confirmed in `WaterMonsterCombatState`, `SpiritCharge`, `SpiritFarProjectile`, `Stage2CombatState`'s clone offsets). Mixing RNG sources adds inconsistency with no benefit (no seeding/determinism requirement exists in this project).

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---|---|---|---|
| Weighted random selection | A custom sorting/bucketing scheme | Cumulative-weight roulette (`Random.Range(0, totalWeight)` + running sum) | Standard, ~10 lines, already the simplest correct approach; no library needed for this scale (3–5 candidates) |
| Per-pattern cooldown timers | New coroutines or per-frame decrement fields | `Time.time` timestamp comparison (`Time.time < readyAt`) | Already proven in this exact codebase (`WaterMonsterCombatState._lastWaveTime`); avoids frame-skip bugs from `Execute()`'s early returns |

**Key insight:** This is a small, self-contained gameplay algorithm (4–5 candidates, evaluated a few times per combat cycle) — there is no case here for a third-party weighted-random package or state-machine framework. Hand-rolling the ~40-line helper described above *is* the right-sized solution; the risk to avoid is re-introducing the *old* bug class (index-based bookkeeping instead of type-based).

## Runtime State Inventory

> This phase's title includes "리팩토링" (refactoring), so this section is included for completeness, per the trigger rule. However, the canonical question ("what runtime systems still have old string/state cached, stored, or registered after the code change?") does not apply here — this is a pure in-memory algorithm change with no renamed identifiers, no persisted data, and no external services.

| Category | Items Found | Action Required |
|---|---|---|
| Stored data | None — no database/save-file references any pattern name or index. `_patternIndex` is a transient in-memory field, never serialized. | None |
| Live service config | None — no external services (n8n, Datadog, etc.) involved; this is client-side Unity game logic only. | None |
| OS-registered state | None. | None |
| Secrets/env vars | None. | None |
| Build artifacts | None — no renamed assembly/namespace/package; class names (`CombatState`, `SpiritCombatState`, `Stage2CombatState`) are unchanged, only their internal method bodies change. | None |

## Common Pitfalls

### Pitfall 1: Pool exhaustion causing null-return spin
**What goes wrong:** If, in some distance band, all candidates are simultaneously on cooldown or excluded by no-repeat, `SelectWeightedPattern` returns `null`, and per the existing contract `Execute()` will call `SelectAttackStrategy` again on the very next frame (potentially every frame) until something becomes eligible.
**Why it happens:** `CombatState.Execute()`'s null-handling was designed for `Stage2CombatState`'s deliberate "wait one/more frames for heavy combo" use case, not for silent pool starvation.
**How to avoid:** Ensure at least one candidate has no distance condition at all times (both `SpiritCharge` and `SpiritExhaustion` qualify under the recommended design above, per D-02a treatment), so the pool is realistically never fully empty except for brief multi-cooldown coincidences.
**Warning signs:** Repeated `Debug.Log` spam with no attack executing for more than a frame or two; boss appears to freeze mid-combat.

### Pitfall 2: `_decisionTimer` is the *only* mutual-exclusion guard for coroutine-based attacks
**What goes wrong:** All `AnimationName` values in WaterSpirit's strategies are `""` (empty — animation not implemented per project scope). `BossController.CheckAnimationState("")` will almost always return `true` immediately (the state name won't match `""`, so the "not in transition" fallback fires), meaning `_isAttacking` clears far sooner than the actual coroutine (e.g. `SpiritCharge`'s ~2.5s teleport+windup+dash sequence) finishes. The *only* thing currently preventing the boss from starting a second attack while `SpiritCharge`'s coroutine is still running is `_decisionTimer = attack.Cooldown` (set to the *just-started* attack's own cooldown, which happens to be ≥ its coroutine duration for all 4 WaterSpirit patterns today).
**Why it happens:** Pre-existing design (not introduced by this phase), inherited from `CombatState`'s original tutorial-derived timing model.
**How to avoid:** When exercising the "cooldown/weight are Claude's discretion" freedom, do not lower any `Cooldown` value below that pattern's actual coroutine execution time, or overlapping coroutine executions could occur (e.g., a second attack teleporting the boss mid-charge). This is out of scope to fix, but the plan should not make it worse.
**Warning signs:** Boss visually "teleports mid-animation" or two attack coroutines appear to run simultaneously in Play mode.

### Pitfall 3: Float-rounding at the roulette boundary
**What goes wrong:** Cumulative-weight comparison (`roll <= cumulative`) can, in rare float-precision edge cases, fail to select any candidate before the loop ends if `roll` lands exactly on `totalWeight` due to rounding.
**Why it happens:** Standard floating-point roulette-wheel selection caveat.
**How to avoid:** Always include a fallback return of the last eligible candidate after the loop (shown in the Architecture Patterns code above) rather than allowing a silent `null` fall-through from the loop itself.
**Warning signs:** Extremely rare `null` returns from `SelectWeightedPattern` even when candidates are eligible.

### Pitfall 4: Per-session (not global) tracking state reset
**What goes wrong:** If a planner assumes `LastUsedPatternType`/cooldown timestamps persist across a full boss fight, they will be surprised that leaving `SpiritCombatState`/`Stage2CombatState` (e.g., player runs out of `AttackRange + 1.0f`, triggering `ChaseState`, then back into a **new** `SpiritCombatState` instance) resets all of it.
**Why it happens:** `BossController.ChangeState()` always fully replaces the state object; nothing is persisted on `SpiritController` itself.
**How to avoid:** This matches pre-existing behavior (`_patternIndex` already resets the same way today) — no fix needed, just don't design around an assumption of persistence across state-object boundaries.
**Warning signs:** None expected — this is consistent with current shipped behavior from Phases 5/6.

## Code Examples

See "Architecture Patterns" above for the full recommended `PatternCandidate`/`SelectWeightedPattern`/`ForceSelectPattern` implementation and the resulting `SpiritCombatState` rewrite. These are concrete, ready-to-adapt code shapes grounded in the actual current file contents (line-level signatures confirmed above), not generic pseudocode.

## State of the Art

| Old Approach | Current/Recommended Approach | When Changed | Impact |
|---|---|---|---|
| Fixed round-robin array (`_pattern[_patternIndex++ % length]`), no conditions | Weighted-random pool with per-candidate distance/cooldown/no-repeat gating | This phase (Phase 7) | Restores the distance-aware intent from Phase 5 D-03a (superseded/updated: Charge is now distance-free per D-02a) and adds variety via randomness instead of deterministic cycling |
| Slot-index-keyed dedup (`SpiritCharge` appears twice in the array to weight it) | Type-keyed weight field on a single candidate entry | This phase | Removes the accidental-duplication hack (D-06a); weight is now explicit and adjustable without editing array length |

**Deprecated/outdated:** The `_pattern`/`_patternIndex` fields in `SpiritCombatState` become dead code once replaced and must be removed (they are orphaned directly by this change, so per CLAUDE.md's dependency-cleanup rule they should be deleted, not just left in place).

## Open Questions

1. **Does `SpiritExhaustion` need a distance condition?**
   - What we know: CONTEXT.md D-02 only names `SpiritCharge` and `SpiritFarProjectile` as explicitly distance-free (they self-teleport). D-02b says distance conditions apply to "근접(WakeRepel→Repel)·원거리(기본 투사체류) 패턴" — Exhaustion isn't mentioned in either category. Exhaustion doesn't move the boss and isn't part of Phase 5's original 3-pattern distance triad (Repel/Charge/Projectile) — it appears to have been added during Phase 5/6 implementation as a "vulnerability window" utility pattern, undocumented in either phase's CONTEXT.md decisions.
   - What's unclear: Whether the user intends Exhaustion to have its own distance gate (e.g., only usable at close range to set up a "punish" moment) or should also be distance-free like the two teleport patterns.
   - Recommendation: Treat it as distance-free (cooldown + no-repeat only) by default, since it doesn't involve player positioning and its sole purpose is to set up the forced WakeRepel chain (D-04) regardless of where the player is standing. Flag this assumption for the user/planner to confirm or override during planning — MEDIUM confidence default, not a locked decision.

2. **Exact distance thresholds and field for `SpiritFarProjectile`'s far-range condition.**
   - What we know: `SpiritController` has `RepelRange` (1.5) and `ChargeRange` (5.0, currently dead/unused). No `ProjectileRange` field exists despite CONTEXT.md's canonical_refs implying it does.
   - What's unclear: Whether to repurpose `ChargeRange` (recommended — avoids adding a redundant Inspector field, matches "existing values reuse is discretion" framing) or add a new dedicated field.
   - Recommendation: Repurpose `ChargeRange` as the far-distance minimum threshold for `SpiritFarProjectile` (i.e., `minDistance: spirit.ChargeRange`). This is Claude's Discretion per CONTEXT.md D-08's threshold note — surfaced here so the planner makes it an explicit, documented choice rather than an implicit one.

3. **Weight values for D-03's weighted-random pool.**
   - What we know: CONTEXT.md explicitly defers exact weights to planner/implementation discretion (D-03b).
   - What's unclear: Whether Exhaustion (which forcibly chains into WakeRepel) should be weighted lower than the other three to avoid dominating pattern variety, since each Exhaustion pick effectively "spends" two pattern slots (itself + the forced WakeRepel that follows).
   - Recommendation: Give `SpiritExhaustion` a somewhat lower weight (e.g., ~0.5–0.6 relative to 1.0 for the other three) so the forced two-pattern chain doesn't crowd out variety too often. This is a balancing suggestion only (explicitly out of this phase's scope to finalize per CONTEXT.md's "패턴 수치 밸런싱 최종값 확정" exclusion) — the plan should make weights easily tunable (e.g., named constants or Inspector-exposed floats) rather than hardcoded magic numbers, so post-playtesting balancing doesn't require touching selection logic again.

## Environment Availability

No external dependencies are introduced by this phase — it is a pure C#/Unity 6 in-engine code change using only classes already present in the project (`UnityEngine.Random`, `System.Collections.Generic.Dictionary`, `System.Type`). Unity Editor version confirmed via `ProjectSettings/ProjectVersion.txt`: **6000.3.10f1** (Unity 6.3), consistent with the Unity 6 API usage already present in the codebase (`linearVelocity`, `FindObjectsByType`).

No automated test framework (Unity Test Framework / NUnit assembly) is set up under `Assets/` in this project — verification for this phase will be manual/Play-mode + log-inspection based, consistent with how Phases 5 and 6 were verified. `.planning/config.json` has `workflow.nyquist_validation: false`, so the Validation Architecture section is intentionally omitted from this document.

## Sources

### Primary (HIGH confidence — direct source reading, this project)
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — full current implementation
- `Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs` — full current implementation
- `Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs` — full current implementation
- `Assets/Enemy/WaterSpirit/Script/SpiritController.cs` — fields, Update() intercept logic
- `Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs` — interface contract
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/{SpiritCharge,SpiritExhaustion,SpiritFarProjectile,SpiritWakeRepel,SpiritRepel}.cs` — all 5 strategy implementations
- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs` and `State/TutorialAttackState.cs` — reference cooldown/no-repeat style
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — reference pool-random precedent
- `Assets/Enemy/NewBoss/Script/BossController.cs`, `States/GroggyState.cs` — lifecycle/state-swap confirmation
- `ProjectSettings/ProjectVersion.txt` — Unity 6000.3.10f1 confirmed
- `.planning/phases/07-boss-attack-pattern-judgment/07-CONTEXT.md`, `.planning/phases/05-.../05-CONTEXT.md`, `.planning/phases/06-.../06-CONTEXT.md`, `.planning/REQUIREMENTS.md`, `.planning/STATE.md`, `.planning/config.json`

### Secondary / Tertiary
None — no WebSearch/Context7/external research was needed or performed; this phase is entirely internal-codebase research.

## Metadata

**Confidence breakdown:**
- Standard Stack (N/A — internal refactor, no external libs): HIGH — confirmed no new dependencies needed
- Architecture: HIGH — recommended design is directly grounded in verified current signatures and an existing in-codebase precedent (`WaterMonsterCombatState`)
- Pitfalls: MEDIUM-HIGH — Pitfall 2 (animation-timeout/coroutine overlap) is inferred from reading `CheckAnimationState`/`AnimationName` behavior rather than observed at runtime; flagged accordingly. All others are directly verified from source.

**Research date:** 2026-07-27
**Valid until:** Stable — no external dependencies to go stale; valid until `CombatState.cs`/`SpiritCombatState.cs`/`Stage2CombatState.cs` are next modified by another phase.
