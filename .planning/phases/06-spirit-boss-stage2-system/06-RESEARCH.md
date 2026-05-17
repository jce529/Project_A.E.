# Phase 06: Spirit Boss Stage 2 System - Research

**Researched:** 2026-04-30
**Domain:** Unity Boss AI / State Machine / Clone Management
**Confidence:** HIGH

## Summary

This phase implements the second stage of the Water Spirit boss, triggered at 50% HP. The system introduces a cyclical combat structure involving dummy clones, normal patterns, a heavy "Stealth + Charge" combo, and a groggy recovery phase. Key technical challenges include state interception to handle Stage 2 recovery after groggy, synchronizing behavior across multiple clones, and implementing uniform circular distribution for teleportation.

**Primary recommendation:** Use a specialized `Stage2CombatState` that manages a sub-phase cycle and commands a list of `SpiritController` dummies. Re-intercept the `CombatState` in `SpiritController.Update` to ensure the boss returns to Stage 2 after recovering from the base `GroggyState`.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01: HP 50% 체크 위치**: `SpiritStats.TakeDamage` 내부에서 체크 및 최초 1회 발동 가드 사용.
- **D-02: 스테이지 2 상태 관리**: `Stage2CombatState : SpiritCombatState` 신규 클래스 작성.
- **D-03: 스테이지 2 진입 즉시 실행**: `Stage2CombatState.Enter()`에서 `DummyPrefab` 2개 생성 및 순간 무적 처리.
- **D-04: 은신 패턴 구조**: Collider off -> 대기 -> 텔레포트 -> Collider on 순서로 진행.
- **D-05: 은신 텔레포트 목적지**: 플레이어 중심 고리 범위(`MinTeleportRadius`, `MaxTeleportRadius`) 내 랜덤 위치.
- **D-06: 분신 생성 및 스폰**: `SpiritController`의 `DummyPrefab` 필드 활용.
- **D-07: 분신 데미지 분기**: `SpiritStats.IsDummy == true` 일 때 데미지 0 처리.
- **D-08: 분신 소멸 조건**: 헤비콤보 완료 후 그로기 전환 시 분신 전체 `Destroy`.
- **D-09: 분신 동기화**: `Stage2CombatState`가 분신 리스트를 보유하고 `TriggerHeavyCombo()` 명령 전달.
- **D-10: 스테이지 2 일반 패턴 선택**: `SpiritCombatState`의 거리 기반 로직 재사용.

### the agent's Discretion
- 패턴 카운터 구조 (일반 단계 최대 N회 추적 방식)
- 그로기 해제 후 Stage2CombatState 재진입 시 구체적 인터셉트 구현
- 헤비콤보 단계 진입 조건 (카운터 >= N)
- 분신 스폰 정확한 위치 (보스 주변 반경 등)
- 순간 무적의 구체적 구현 (콜라이더 off vs IsInvincible 플래그)
- 그로기 복구 시간 (`GroggyState._recoveryTime`)

### Deferred Ideas (OUT OF SCOPE)
- 애니메이션·이펙트 연동 (v3.0+)
- 은신 시각 효과 (투명화 등) (v3.0+)
- 스테이지 전환 연출 (컷씬, 화면 효과) (v3.0+)
- 분신과 진짜 보스 시각 구별 방법 (v3.0+)
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| S2-01 | HP 50% trigger | `SpiritStats.TakeDamage` override and `SpiritController` callback pattern confirmed. |
| S2-02 | Stage 2 cycle | Implementation via `Stage2CombatState` sub-phases or pattern counter. |
| S2-03 | Stealth mechanism | Collider2D toggle and polar sampling for ring-area teleportation verified. |
| S2-04 | Dummies | Synchronized commanding via `TriggerHeavyCombo()` confirmed. |
| S2-05 | Dummy Damage | `IsDummy` flag in `SpiritStats` to bypass health reduction. |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Unity Engine | 6000.3.10f1 | Game Engine | Project standard (Unity 6). |
| C# | 9.0+ | Scripting | Standard for Unity development. |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|--------------|
| Unity Physics 2D | Built-in | Collision and Velocity | `_rb.linearVelocity` (Unity 6 property). |

## Architecture Patterns

### Pattern 1: State Interception (Post-Groggy Recovery)
**What:** Overriding `Update()` in `SpiritController` to check if the current state is the base `CombatState` and swapping it with `Stage2CombatState` if the boss is in Phase 2.
**When to use:** To integrate with the base `GroggyState` which transitions back to a generic `CombatState` upon recovery.

### Pattern 2: Polar Sampling for Ring Area
**What:** Using polar coordinates to pick a point within an annulus (ring) instead of repeated re-sampling with `insideUnitCircle`.
**Example:**
```csharp
float angle = Random.Range(0f, Mathf.PI * 2f);
float r = Mathf.Sqrt(Random.Range(minR * minR, maxR * maxR));
Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * r;
```

### Pattern 3: Commander-Clone Sync
**What:** The "Real" boss state machine (`Stage2CombatState`) holds a reference list to dummy `SpiritController` instances and broadcasts state change commands.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Uniform distribution in a ring | `while` loop with `insideUnitCircle` | Polar coordinates with `sqrt` sampling | More efficient and guarantees uniform density. |
| Dummy damage suppression | Custom HP subtraction logic in every skill | `IsDummy` flag in `TakeDamage` | Centralized and cleaner architecture. |

## Common Pitfalls

### Pitfall 1: Missing HP Trigger Guard
**What goes wrong:** Stage 2 trigger fires multiple times every frame while HP is below 50%.
**How to avoid:** Use a `bool _stage2Triggered` flag in `SpiritStats` or `SpiritController`.

### Pitfall 2: Dummy Cleanup Leak
**What goes wrong:** Dummies are not destroyed when the boss is defeated or when the scene reloads.
**How to avoid:** Explicitly call `Destroy` on all dummies in the clone list when transitioning to Groggy or on boss death.

### Pitfall 3: Collider State Sync
**What goes wrong:** Turning collider off for stealth might prevent the boss from detecting the ground or player triggers if not handled carefully.
**How to avoid:** Only disable the specific `Collider2D` used for taking damage or player collision, ensuring movement/grounding logic isn't broken.

## Code Examples

### Annulus (Ring) Sampling
```csharp
// Source: Mathematical standard for uniform distribution in a ring
public Vector3 GetRandomPositionInRing(Vector3 center, float minR, float maxR)
{
    float angle = Random.Range(0f, Mathf.PI * 2f);
    // sqrt is required to maintain uniform density across the area
    float r = Mathf.Sqrt(Random.Range(minR * minR, maxR * maxR));
    return center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * r;
}
```

### Stage 2 State Interception
```csharp
// In SpiritController.cs
protected override void Update()
{
    base.Update();
    if (CurrentState != null && CurrentState.GetType() == typeof(CombatState))
    {
        if (_isStage2)
            ChangeState(new Stage2CombatState());
        else
            ChangeState(new SpiritCombatState());
    }
}
```

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Unity 6 | Core Runtime | ✓ | 6000.3.10f1 | — |
| Rigidbody2D | Movement | ✓ | Built-in | — |

## Sources

### Primary (HIGH confidence)
- `Assets/Enemy/NewBoss/Script/BossController.cs` - Base state machine logic.
- `Assets/Enemy/NewBoss/Script/States/GroggyState.cs` - Recovery behavior.
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs` - Current HP and Damage logic.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Unity 6 verified.
- Architecture: HIGH - Pattern already established in Phase 5.
- Pitfalls: HIGH - Common Unity AI/Cloning issues.

**Research date:** 2026-04-30
**Valid until:** 2026-05-30
