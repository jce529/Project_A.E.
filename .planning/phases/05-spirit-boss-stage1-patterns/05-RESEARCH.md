# Phase 5: 보스 기반 엔티티 및 스테이지 1 공격 패턴 - Research

**Researched:** 2026-04-30
**Domain:** Unity 6 C# — Boss state machine extension, attack strategy pattern, 2D physics knockback
**Confidence:** HIGH (all findings verified against live codebase)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**D-01: HP·데미지 파이프라인**
- D-01a: `SpiritStats : BossStatsSystem` 을 신규 작성한다.
- D-01b: `protected override void TakeDamage(DamageInfo info)` 에서 배리어/물 로직을 완전히 제거하고, `_currentHealth -= info.amount` 로 직접 HP를 차감한다.
- D-01c: `IsBarrierActive` 를 사용하지 않는다. Spirit Boss는 배리어 없음.
- D-01d: `SpiritCombatState.ShouldTransitionToGroggy()` 는 `return false` 로 고정한다 (그로기 없음).

**D-02: 3종 패턴 거리 임계치**
- D-02a: `RepelRange`, `ChargeRange`, `ProjectileRange` 3개를 `[SerializeField]` 로 Inspector에 노출한다.
- D-02b: 기본값은 플래너(Claude) 재량으로 설정한다.

**D-03: 패턴 선택 우선순위 및 쿨다운**
- D-03a: 우선순위: 근접(RepelRange 이내) → 튕겨내기. 중거리(ChargeRange 이내) → 돌진. 그 외 → 투사체.
- D-03b: 공통 쿨다운 방식: 한 패턴 시전 후 `_decisionTimer` 에 해당 패턴의 Cooldown 값을 적용하며, 타이머가 0 이하일 때 다음 패턴 실행.
- D-03c: 각 패턴의 Cooldown 값은 Inspector 노출 또는 플래너 재량.

**D-04: 돌진 패턴 (S1-01)**
- D-04a: 2단계 구조 — ①준비(Windup): 보스 정지 후 ChargeWindup(초) 대기, ②돌진: 목표점으로 직선 이동.
- D-04b: 목표점 고정 시점: 준비 종료 시점의 플레이어 위치 + 진행 방향으로 OvershotDistance(유닛) 만큼 더 나간 좌표.
- D-04c: 플레이어가 피해도 고정된 목표점까지 이동 후 종료.
- D-04d: 돌진 중 Player 레이어와 충돌(OnTriggerEnter2D) 시 데미지 1회 적용.
- D-04e: ChargeWindup / ChargeSpeed / OvershotDistance 모두 `[SerializeField]` 로 Inspector 노출.

### Claude's Discretion
- 파일 위치: `Assets/Enemy/WaterSpirit/Script/` (Spirit 전용 폴더 신규 생성)
- `SpiritController` 의 Update() 에서 `CombatState` → `SpiritCombatState` 교체 인터셉트 방식 (WaterMonsterController 동일 패턴 참고)
- 사망 처리: `SpiritStats.Die()` override → `gameObject.SetActive(false)` 또는 DeadState 전환 (플래너 재량)
- S1-02 투사체 세부 동작 (속도·수명·데미지): Inspector 노출 + WaterSpitProjectile 패턴 참고
- S1-03 튕겨내기 세부 동작 (knockback force·데미지): Inspector 노출
- 수치 기본값 전부: 플래너 재량 (Inspector 조정 가능하게)

### Deferred Ideas (OUT OF SCOPE)
- 스테이지 2 전환 트리거 — Phase 6에서 구현
- 애니메이션·이펙트 — v3.0+
- 사망 처리 세부 방식 — 구현 방식(비활성화 vs DeadState)은 플래너 재량이나 Phase 5 범위임
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| CORE-01 | 물의 정령 보스가 독립 GameObject로 씬에 배치되고 플레이어를 감지하면 전투 상태로 진입한다 | IdleState→ChaseState→CombatState 흐름 확인. SpiritController : BossController 상속으로 구현. Update() 인터셉트로 SpiritCombatState 교체 필요. |
| CORE-02 | 보스는 HP 시스템을 가지며, 피격 시 HP가 감소한다 | SpiritStats : BossStatsSystem 신규 작성. TakeDamage override에서 배리어 로직 제거, `_currentHealth -= info.amount` 직접 차감. TakeDmg.cs가 아닌 EnemyHitBox → BossStatsSystem.TakeDamageInfo() 파이프라인 확인. |
| CORE-04 | HP가 0이 되면 보스가 사망 처리된다 | BossStatsSystem.TakeDamage()에 `if (_currentHealth <= 0) Die()` 호출 이미 존재. SpiritStats.Die() override → gameObject.SetActive(false). |
| S1-01 | 중거리 돌진 — 빠른 속도로 플레이어 방향 직선 돌진 후 쿨다운 | SpiritCharge : IAttackStrategy 신규 구현. Coroutine 기반 2단계(Windup → Dash). 목표점 계산식 확인: `playerPos + dir.normalized * (dist + OvershotDistance)`. OnTriggerEnter2D로 데미지 1회 적용. |
| S1-02 | 자동추적 투사체 — 발사 시 플레이어 위치를 향해 날아가며 히트 시 데미지 | SpiritProjectileAttack : IAttackStrategy + SpiritProjectile MonoBehaviour 신규 작성. WaterRangedSpit / WaterSpitProjectile 패턴 직접 재사용 가능. 발사 방향 = 발사 시점 플레이어 위치 기준 고정. |
| S1-03 | 거리유지 튕겨내기 — 플레이어 근접 시 knockback + 데미지 | SpiritRepel : IAttackStrategy. Physics2D.OverlapCircleAll("Player") + PlayerStats.TakeDamage() + PlayerController.ApplyKnockback(dir, force). ApplyKnockback API 확인 완료. |
</phase_requirements>

---

## Summary

Phase 5는 기존 BossController / BossStatsSystem / IAttackStrategy 상속 구조를 활용하여 SpiritController, SpiritStats, SpiritCombatState, 3종 IAttackStrategy 구현체를 새로 작성하는 작업이다. 모든 기반 클래스와 레퍼런스 구현체(WaterMonster 계열)가 코드베이스에 이미 존재하며, 구조적으로 검증된 패턴을 그대로 따른다. 신규 코드 작성 비율이 높지만 패턴은 명확하므로 설계 리스크는 낮다.

핵심 주의점은 두 가지다. 첫째, `ChaseState.Execute()`가 `new CombatState()`를 직접 instantiate하므로, `SpiritController.Update()`에서 `typeof(CombatState)` 타입 체크 후 `SpiritCombatState`로 교체하는 인터셉트 패턴이 필수다(WaterMonsterController에서 검증됨). 둘째, 돌진 패턴은 `IAttackStrategy.ExecuteAttack()`이 synchronous call이므로 Windup 대기를 `MonoBehaviour.StartCoroutine()`으로 처리해야 하며, boss 참조를 코루틴에 캡처해야 한다.

`BossController`의 `HandleDamageTaken` 이벤트 핸들러가 `Stats.IsBarrierActive`를 체크해 `CounterState`로 전환하는 로직이 있다. SpiritStats는 배리어가 없으므로 `IsBarrierActive`가 항상 false를 반환하여 CounterState 전환이 발생하지 않는다 — 이는 BossController 기본 로직과 자연스럽게 호환된다.

**Primary recommendation:** WaterMonsterController → WaterMonsterStats → WaterMonsterCombatState → WaterRangedSpit/WaterSpitProjectile 순서로 코드 구조를 참조하여 Spirit 계열 클래스를 작성한다. 새로운 설계 패턴은 필요 없다.

---

## Standard Stack

### Core
| Library/Class | Version | Purpose | Why Standard |
|---------------|---------|---------|--------------|
| BossController | project | 플레이어 감지·상태 전환·이동 기반 | 모든 보스의 공통 베이스, 검증됨 |
| BossStatsSystem | project | HP 시스템, TakeDamage 파이프라인, Die() | TakeDmg/EnemyHitBox와 자동 연동 |
| IBossState | project | 상태 인터페이스 (Enter/Execute/Exit) | 모든 상태의 계약 |
| CombatState | project | 쿨다운 기반 패턴 실행 루프 | _decisionTimer, _isAttacking 내장 |
| IAttackStrategy | project | 공격 패턴 인터페이스 (Cooldown, AnimationName, ExecuteAttack) | 3종 패턴 모두 이 인터페이스로 구현 |
| PlayerController.ApplyKnockback | project | knockback 적용 공개 API | 기존 isKnockedBack 플래그 처리 포함 |
| PlayerStats.TakeDamage | project | 플레이어 HP 감소 진입점 | HP 기반 클래스 상속, 기존 파이프라인 |

### Supporting
| Library/Class | Version | Purpose | When to Use |
|---------------|---------|---------|-------------|
| EnemyHitBox | project | 자식 HitBox → BossStatsSystem 브릿지 | 보스 피격 감지에 사용 |
| DamageInfo struct | project | 데미지 전달 데이터 (amount, element) | TakeDamage() 모든 호출에 사용 |
| Physics2D.OverlapCircleAll | Unity 6 | 범위 내 콜라이더 감지 (튕겨내기용) | S1-03 근접 감지에 사용 |
| LayerMask.GetMask("Player") | Unity 6 | Player 레이어 필터 | 모든 패턴의 플레이어 타겟팅 |
| Rigidbody2D.linearVelocity | Unity 6 | Unity 6 이동 API | velocity 대신 반드시 사용 |
| Coroutine (StartCoroutine) | Unity 6 | 비동기 Windup 대기 | S1-01 돌진 2단계 타이밍 처리 |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Coroutine for Windup | `_windupTimer` in Execute() | Timer 방식은 CombatState.Execute() 흐름과 충돌 없이 동작하나, 상태를 IAttackStrategy 내부에서 관리해야 해 복잡성 증가. Coroutine이 WaterMonster 패턴과 일관성 유지. |
| Resources.Load for Projectile | Direct prefab SerializeField | Resources.Load는 WaterRangedSpit에서 사용됨. Spirit의 경우 SpiritController에 `[SerializeField]` 프리팹 필드로 두고 boss에서 참조하는 것이 Inspector-friendly. |
| gameObject.SetActive(false) for Die() | DeadState transition | SetActive는 WaterMonsterStats에서 검증됨. Phase 6에서 스테이지 2 전환 로직이 추가될 것이므로 DeadState 전환 방식이 확장성에 유리하나, Phase 5 범위에서는 SetActive가 단순하고 충분함. |

---

## Architecture Patterns

### Recommended Project Structure
```
Assets/Enemy/WaterSpirit/Script/
├── SpiritController.cs           # BossController 상속, Update() 인터셉트
├── SpiritStats.cs                # BossStatsSystem 상속, 배리어 제거
├── States/
│   ├── SpiritCombatState.cs      # CombatState 상속, 3종 패턴 선택
│   └── Attacks/
│       ├── SpiritCharge.cs       # S1-01 돌진 (IAttackStrategy)
│       ├── SpiritProjectileAttack.cs  # S1-02 투사체 발사 (IAttackStrategy)
│       ├── SpiritProjectile.cs   # S1-02 투사체 비행 MonoBehaviour
│       └── SpiritRepel.cs        # S1-03 튕겨내기 (IAttackStrategy)
```

**총 7개 파일 신규 작성.** `Assets/Enemy/WaterSpirit/Script/` 폴더는 현재 존재하지 않으며 신규 생성 필요.

### Pattern 1: CombatState 교체 인터셉트 (CORE-01 핵심)

**What:** `ChaseState.Execute()`가 `new CombatState()`를 직접 생성하므로 서브클래스 교체가 불가능하다. SpiritController.Update()에서 현재 상태 타입을 체크해 다음 프레임에 SpiritCombatState로 교체한다.

**When to use:** BossController 상속 계열 모든 보스 (WaterMonsterController에서 동일 패턴 사용 중).

```csharp
// SpiritController.cs — WaterMonsterController.Update() 동일 패턴
protected override void Update()
{
    base.Update();

    // ChaseState가 new CombatState()로 전환하면 즉시 SpiritCombatState로 교체
    if (CurrentState != null && CurrentState.GetType() == typeof(CombatState))
    {
        ChangeState(new SpiritCombatState());
    }
}
```

### Pattern 2: SpiritStats — 배리어 없는 직접 HP 차감 (D-01)

**What:** BossStatsSystem의 `TakeDamage(DamageInfo)`는 `IsBarrierActive` 분기를 포함한다. Spirit은 배리어가 없으므로 이 분기를 완전히 제거하고 직접 차감한다. `BossStatsSystem.Start()`는 `_currentHealth = MaxHealth`와 `_currentWater = MaxWater`를 초기화하는데, MaxWater 기본값이 100이면 IsBarrierActive가 true로 시작한다. SpiritStats는 `MaxWater = 0`으로 Reset() 또는 Start()에서 설정해야 한다(WaterMonsterStats.Reset() 참고).

```csharp
// SpiritStats.cs
public class SpiritStats : BossStatsSystem
{
    protected override void TakeDamage(DamageInfo info)
    {
        if (info.amount <= 0f) return;

        _currentHealth -= info.amount;
        Debug.Log($"[SpiritStats] 피격! 데미지: {info.amount}, 남은 체력: {_currentHealth}/{MaxHealth}");

        InvokeOnDamageTaken();  // OnDamageTaken 이벤트 발생

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            Die();
        }
    }

    protected override void Die()
    {
        gameObject.SetActive(false);
    }

    // MaxWater=0으로 IsBarrierActive 항상 false
    protected virtual void Reset()
    {
        MaxWater = 0f;
    }
}
```

**주의:** `BossStatsSystem.TakeDamage(DamageInfo)`에는 `if (_currentHealth <= 0) Die()` 호출이 있으나 base 호출을 하지 않으므로 SpiritStats에서 직접 체크해야 한다.

### Pattern 3: SpiritCombatState — 3종 패턴 거리 선택 (D-03)

**What:** CombatState.Execute()가 SelectAttackStrategy()와 ShouldTransitionToGroggy()를 virtual로 제공한다. SpiritCombatState는 두 메서드를 override하고 3개 SerializeField 거리 임계치로 패턴을 선택한다.

**중요:** `CombatState`는 `SelectAttackStrategy(BossController boss, float dist)`를 `protected virtual`로 선언하고 있다. SpiritCombatState는 field가 아닌 SpiritController 캐스팅을 통해 거리 임계치를 읽어야 한다. 또는 SpiritCombatState에 SerializeField를 직접 선언한다 — 단, `CombatState`는 `IBossState` 구현이지 MonoBehaviour가 아니므로 `[SerializeField]`가 동작하지 않는다. 거리 임계치는 **SpiritController에 SerializeField로 선언하고 SpiritCombatState에서 boss 캐스팅으로 읽어야 한다**.

```csharp
// SpiritCombatState.cs
public class SpiritCombatState : CombatState
{
    protected override bool ShouldTransitionToGroggy(BossController boss) => false;

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is SpiritController spirit)) return null;

        // D-03a: 우선순위 — 근접 → 중거리 → 원거리
        if (dist <= spirit.RepelRange)
            return new SpiritRepel();
        if (dist <= spirit.ChargeRange)
            return new SpiritCharge();
        return new SpiritProjectileAttack();
    }
}
```

### Pattern 4: SpiritCharge — 2단계 Coroutine 돌진 (D-04)

**What:** `IAttackStrategy.ExecuteAttack()`은 동기 호출이므로 Windup 대기를 boss.StartCoroutine()으로 실행한다. 목표점을 코루틴 내부에서 고정하고 linearVelocity로 이동한다.

```csharp
// SpiritCharge.cs
public class SpiritCharge : IAttackStrategy
{
    public float Cooldown => _cooldown;
    public string AnimationName => "";

    private float _cooldown = 3.0f;  // Inspector에서 SpiritController를 통해 설정 가능

    public void ExecuteAttack(BossController boss)
    {
        boss.StartCoroutine(ChargeRoutine(boss));
    }

    private System.Collections.IEnumerator ChargeRoutine(BossController boss)
    {
        if (!(boss is SpiritController spirit)) yield break;

        // 1단계: Windup — 정지 후 대기
        boss.StopMove();
        yield return new WaitForSeconds(spirit.ChargeWindup);

        // 2단계: 목표점 고정 (플레이어 위치 + Overshoot)
        if (boss.Target == null) yield break;
        Vector2 bossPos = boss.transform.position;
        Vector2 playerPos = boss.Target.position;
        Vector2 dir = (playerPos - bossPos).normalized;
        float dist = Vector2.Distance(bossPos, playerPos);
        Vector2 targetPos = playerPos + dir * spirit.OvershotDistance;

        // 3단계: 돌진 — 목표점 도달까지 linearVelocity 적용
        spirit.SetCharging(true);
        while (Vector2.Distance(boss.transform.position, targetPos) > 0.2f)
        {
            boss.transform.GetComponent<Rigidbody2D>().linearVelocity =
                (targetPos - (Vector2)boss.transform.position).normalized * spirit.ChargeSpeed;
            yield return null;
        }

        boss.StopMove();
        spirit.SetCharging(false);
    }
}
```

**주의:** `BossController._rb`는 `protected`이므로 SpiritController에서 접근하거나 `boss.transform.GetComponent<Rigidbody2D>()`로 접근한다. 더 나은 방법은 BossController에 `public void SetVelocity(Vector2 vel)` 헬퍼를 추가하거나, SpiritController에서 `_rb`를 public property로 노출하는 것이다.

**데미지 적용:** 돌진 중 데미지는 SpiritController의 OnTriggerEnter2D에서 처리한다 (SpiritCharge 내부가 아닌 Controller에서 처리 — D-04d).

### Pattern 5: SpiritProjectileAttack — 투사체 발사 (S1-02)

**What:** WaterRangedSpit 패턴과 동일하되, 방향을 `boss.transform.right` 대신 발사 시점 플레이어 방향으로 계산한다.

```csharp
// SpiritProjectileAttack.cs
public class SpiritProjectileAttack : IAttackStrategy
{
    public float Cooldown => 2.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        if (boss.Target == null) return;

        if (!(boss is SpiritController spirit)) return;

        Vector2 dir = ((Vector2)boss.Target.position - (Vector2)boss.transform.position).normalized;
        Vector3 spawnPos = boss.transform.position;

        var prefab = spirit.ProjectilePrefab;
        if (prefab == null) { Debug.LogWarning("[SpiritProjectileAttack] ProjectilePrefab not set."); return; }

        var go = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
        var proj = go.GetComponent<SpiritProjectile>();
        if (proj != null) proj.Direction = dir;
    }
}
```

### Pattern 6: SpiritRepel — knockback + 데미지 (S1-03)

**What:** `Physics2D.OverlapCircleAll`로 Player 레이어 감지 후 `PlayerStats.TakeDamage()` + `PlayerController.ApplyKnockback()` 동시 적용. ApplyKnockback API는 `(Vector2 dir, float force)` 시그니처로 이미 PlayerController에 존재함.

```csharp
// SpiritRepel.cs
public class SpiritRepel : IAttackStrategy
{
    public float Cooldown => 1.5f;
    public string AnimationName => "";

    public void ExecuteAttack(BossController boss)
    {
        if (!(boss is SpiritController spirit)) return;

        Vector2 origin = boss.transform.position;
        var hits = Physics2D.OverlapCircleAll(origin, spirit.RepelRange, LayerMask.GetMask("Player"));

        foreach (var hit in hits)
        {
            var playerStats = hit.GetComponentInParent<PlayerStats>();
            var playerCtrl = hit.GetComponentInParent<PlayerController>();

            Vector2 knockDir = ((Vector2)hit.transform.position - origin).normalized;

            if (playerStats != null) playerStats.TakeDamage(spirit.RepelDamage);
            if (playerCtrl != null) playerCtrl.ApplyKnockback(knockDir, spirit.RepelForce);
        }
    }
}
```

### Anti-Patterns to Avoid

- **CombatState에 SerializeField 사용:** CombatState는 MonoBehaviour가 아닌 순수 C# 클래스다. `[SerializeField]`는 동작하지 않는다. 거리 임계치와 수치 값은 반드시 SpiritController(MonoBehaviour)에 선언하고 boss 캐스팅으로 읽는다.
- **`_rb.velocity` 사용:** Unity 6에서 deprecated. 반드시 `_rb.linearVelocity` 사용.
- **BossController.HandleDamageTaken에 의존:** 이 핸들러는 `IsBarrierActive && !CounterState && !GroggyState` 조건으로 CounterState 전환을 시도한다. SpiritStats에서 MaxWater=0이면 IsBarrierActive=false이므로 자동으로 무해하지만, MaxWater 초기값 설정을 빠뜨리면 CounterState가 잘못 호출된다.
- **IAttackStrategy 내부에서 장기 대기 시 synchronous 처리:** `ExecuteAttack()`은 동기 호출이다. Windup처럼 시간이 걸리는 처리는 반드시 `boss.StartCoroutine()`으로 위임한다.
- **BossStatsSystem.Start() 초기화 순서 무시:** `BossStatsSystem.Start()`에서 `_currentHealth = MaxHealth`와 `_currentWater = MaxWater`를 초기화한다. `SpiritStats`에서 MaxWater=0을 `Reset()`에서 설정하면 Start() 시점에 `_currentWater = 0`이 되어 `IsBarrierActive = false`가 보장된다. Start() override 없이 Reset()으로 처리하면 충분하다.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 플레이어 knockback | 직접 Rigidbody2D 조작 | `PlayerController.ApplyKnockback(dir, force)` | isKnockedBack 플래그, WaitForFixedUpdate 처리, 0.2초 복구 타이밍이 내장되어 있음 |
| 플레이어 데미지 적용 | `HP.health -= dmg` 직접 접근 | `PlayerStats.TakeDamage(float dmg)` | FlashColor 효과, ClampHealth, onHealthChangedCallback, Die() 연쇄 처리 포함 |
| 상태 전환 | `_currentState = newState` 직접 대입 | `boss.ChangeState(IBossState)` | Exit/Enter 호출 순서, `_currentStateName` 디버그 업데이트 포함 |
| 투사체 비행 | 매 Update()에서 transform.position 조작 | Rigidbody2D.linearVelocity 방식 (WaterSpitProjectile 패턴) | 물리 기반 충돌 감지, Destroy(lifetime) 자동 정리 포함 |
| 보스 이동 | `transform.position +=` | `boss.MoveTo(Vector2)` / `boss.StopMove()` | linearVelocity 설정 + LookAtTarget(좌우 반전) 포함 |

**Key insight:** 기반 클래스와 Player 계열 클래스가 공통 유틸리티를 모두 제공한다. 새로운 유틸리티 메서드 없이 기존 API 조합만으로 3종 패턴 구현이 가능하다.

---

## Common Pitfalls

### Pitfall 1: MaxWater 초기값으로 인한 CounterState 오진입
**What goes wrong:** SpiritStats가 BossStatsSystem을 상속하면 MaxWater 기본값이 100f다. Start()에서 `_currentWater = MaxWater = 100`이 되어 `IsBarrierActive = true`가 된다. 플레이어 공격 시 BossController.HandleDamageTaken()이 CounterState로 전환한다.
**Why it happens:** BossController.Start()가 `Stats.OnDamageTaken += HandleDamageTaken`을 구독하고, HandleDamageTaken이 `Stats.IsBarrierActive`를 체크한다.
**How to avoid:** SpiritStats에 `protected virtual void Reset() { MaxWater = 0f; }` 추가 (WaterMonsterStats 패턴 동일). Reset()은 Unity Inspector에서 컴포넌트 추가 시 자동 호출된다.
**Warning signs:** 피격 시 보스가 CounterState로 진입하는 현상.

### Pitfall 2: CombatState 교체 타이밍 누락
**What goes wrong:** SpiritController가 Update() 인터셉트 없이 구현되면, ChaseState → CombatState 전환 시 SpiritCombatState가 아닌 CombatState의 SelectAttackStrategy가 실행된다. 기본 구현은 RangedPokeAttack / HeavyAttack / LightAttack을 반환해 NullReferenceException이나 잘못된 공격이 발생한다.
**Why it happens:** ChaseState.Execute()가 `new CombatState()`를 직접 생성한다. BossController 설계상 factory 메서드가 없다.
**How to avoid:** `SpiritController.Update()` override에서 반드시 `typeof(CombatState)` 체크 후 교체.
**Warning signs:** 전투 시 S1-01/S1-02/S1-03 대신 다른 공격이 실행되거나 RangedPokeAttack NullReferenceException.

### Pitfall 3: IAttackStrategy에서 _decisionTimer 중복 설정
**What goes wrong:** SpiritCombatState.Execute()가 base.Execute() 호출 후 `_decisionTimer`를 덮어쓰려 하면, base에서 이미 `_decisionTimer = attack.Cooldown`으로 설정되어 있어 추가 수정 시 타이밍이 어긋난다.
**Why it happens:** CombatState.Execute()가 `attack.ExecuteAttack()` 호출 후 즉시 `_decisionTimer = attack.Cooldown`을 설정한다. SpiritCombatState.Execute()에서 base 호출 후 `_decisionTimer`를 조작하면 의도한 쿨다운이 적용된다(WaterMonsterCombatState에서 광폭화 배율 적용 패턴과 동일).
**How to avoid:** SpiritCombatState에서는 특별한 Cooldown 조작이 없으면 Execute() override가 필요 없다. SelectAttackStrategy()와 ShouldTransitionToGroggy() override만으로 충분하다.

### Pitfall 4: SpiritCharge에서 _rb 접근
**What goes wrong:** `BossController._rb`는 `protected`이므로 외부 클래스(IAttackStrategy 구현체)에서 직접 접근 불가. `boss.transform.GetComponent<Rigidbody2D>()` 로 매 프레임 GetComponent를 호출하면 성능 저하.
**Why it happens:** BossController가 _rb를 protected로 선언하고 외부 노출 없음.
**How to avoid:** SpiritController에 `public Rigidbody2D Rb => _rb;` property 추가 또는 `public void SetVelocity(Vector2 vel) { _rb.linearVelocity = vel; }` 헬퍼 추가. 돌진 코루틴은 boss를 SpiritController로 캐스팅해 접근.
**Warning signs:** CS0122 컴파일 오류 또는 매 프레임 GetComponent 호출.

### Pitfall 5: 돌진 데미지 중복 적용
**What goes wrong:** S1-01 돌진 중 플레이어와 물리 충돌이 여러 프레임에 걸쳐 OnTriggerStay2D를 발생시키면 데미지가 연속 적용된다.
**Why it happens:** OnTriggerEnter2D는 1회 발생이지만 Collider 크기와 이동 속도에 따라 OnTriggerStay2D가 반복될 수 있다.
**How to avoid:** SpiritController에 `_hasHitPlayer` bool 플래그를 두고 돌진 시작 시 false로 초기화, OnTriggerEnter2D 또는 OnTriggerStay2D에서 `_isCharging && !_hasHitPlayer` 조건 체크 후 true로 설정. D-04d: "데미지 1회 적용" 명시.
**Warning signs:** 돌진 1회에 플레이어 HP가 과도하게 감소하는 현상.

---

## Code Examples

### SpiritController 골격 (전체 구조)
```csharp
// Assets/Enemy/WaterSpirit/Script/SpiritController.cs
[RequireComponent(typeof(SpiritStats))]
public class SpiritController : BossController
{
    [Header("Spirit Combat Ranges")]
    [SerializeField] public float RepelRange = 1.5f;
    [SerializeField] public float ChargeRange = 5.0f;
    // ProjectileRange는 ChargeRange 초과 시 자동 적용이므로 별도 필드 선택사항

    [Header("Charge Settings")]
    [SerializeField] public float ChargeWindup = 0.5f;
    [SerializeField] public float ChargeSpeed = 12f;
    [SerializeField] public float OvershotDistance = 2.0f;

    [Header("Repel Settings")]
    [SerializeField] public float RepelDamage = 10f;
    [SerializeField] public float RepelForce = 8f;

    [Header("Projectile")]
    [SerializeField] public GameObject ProjectilePrefab;

    // 돌진 상태 추적 (데미지 1회 보장용)
    public bool IsCharging { get; private set; }
    private bool _hasHitPlayerThisCharge;

    public void SetCharging(bool value)
    {
        IsCharging = value;
        if (value) _hasHitPlayerThisCharge = false;
    }

    protected override void Update()
    {
        base.Update();
        // 인터셉트: CombatState → SpiritCombatState 교체
        if (CurrentState != null && CurrentState.GetType() == typeof(CombatState))
            ChangeState(new SpiritCombatState());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsCharging || _hasHitPlayerThisCharge) return;
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Player")) == 0) return;

        var ps = other.GetComponentInParent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(/* SpiritStats에서 ChargeDamage 읽기 */);
            _hasHitPlayerThisCharge = true;
        }
    }
}
```

### 목표점 계산 (D-04b)
```csharp
// SpiritCharge.ChargeRoutine() 내부
Vector2 bossPos = boss.transform.position;
Vector2 playerPos = boss.Target.position;
Vector2 dir = (playerPos - bossPos).normalized;
// 플레이어 위치에서 진행 방향으로 OvershotDistance 추가
Vector2 targetPos = playerPos + dir * spirit.OvershotDistance;
// CONTEXT.md D-04b 명시: "bossToPlayer.normalized * (distToPlayer + OvershotDistance)"와 동치
```

### SpiritProjectile 발사 방향 (S1-02)
```csharp
// SpiritProjectileAttack.ExecuteAttack()
// WaterRangedSpit과의 차이: boss.transform.right 대신 실제 플레이어 방향 사용
Vector2 dir = ((Vector2)boss.Target.position - (Vector2)boss.transform.position).normalized;
var go = Object.Instantiate(spirit.ProjectilePrefab, boss.transform.position, Quaternion.identity);
var proj = go.GetComponent<SpiritProjectile>();
if (proj != null) proj.Direction = dir;
```

### SpiritProjectile MonoBehaviour (S1-02)
```csharp
// WaterSpitProjectile과 동일 구조
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SpiritProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 12f;
    [SerializeField] private float lifetime = 4f;

    public Vector2 Direction { get; set; } = Vector2.right;

    private void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Player")) == 0) return;
        var ps = other.GetComponentInParent<PlayerStats>();
        if (ps != null) ps.TakeDamage(damage);
        Destroy(gameObject);
    }
}
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `_rb.velocity` | `_rb.linearVelocity` | Unity 6 | 모든 이동 코드에서 linearVelocity 사용 필수 |
| BossController에 WaterMonster 전용 로직 | 서브클래스 분리 + 인터셉트 패턴 | Phase 1 (v1.0) | Spirit도 동일 인터셉트 패턴 적용 |

**Deprecated/outdated:**
- `_rb.velocity`: Unity 6에서 linearVelocity로 대체됨. 프로젝트 전체에서 linearVelocity 사용 확인됨.
- `Resources.Load<GameObject>()` 방식: WaterRangedSpit에서 사용하나, Inspector SerializeField 방식이 권장됨 (SpiritController에서 ProjectilePrefab 직접 할당).

---

## Open Questions

1. **SpiritCharge에서 _rb 접근 방법**
   - What we know: `BossController._rb`는 protected. IAttackStrategy 구현체에서 직접 접근 불가.
   - What's unclear: SpiritController에 velocity 헬퍼를 추가할지, Rb property를 노출할지.
   - Recommendation: `SpiritController`에 `public void SetVelocity(Vector2 vel) { _rb.linearVelocity = vel; }` 헬퍼 추가. BossController 수정 없이 SpiritController 단독으로 해결.

2. **ChargeDamage 수치 위치**
   - What we know: SpiritController에 SerializeField가 집중되어 있음. SpiritStats에 데미지 수치를 두는 것도 가능.
   - What's unclear: ChargeDamage를 SpiritController vs SpiritStats 중 어디 선언할지.
   - Recommendation: SpiritController에 `[SerializeField] public float ChargeDamage = 15f;` 선언. SpiritStats는 HP 시스템 전용으로 유지.

3. **isDummy 플래그 선언 시기 (Phase 6 준비)**
   - What we know: CONTEXT.md specifics에서 "Phase 6을 위해 isDummy 플래그를 SpiritController에 미리 선언"을 제안.
   - What's unclear: Phase 5 범위 내 포함 여부.
   - Recommendation: 플래너 재량. 선언만 하고 로직 없이 두면 Phase 6 작업 비용 절감. `public bool IsDummy { get; set; } = false;`

---

## Environment Availability

Step 2.6: SKIPPED — 이 Phase는 순수 C# 코드/자산 작성이며 외부 도구, 서비스, CLI, 런타임 외부 의존성이 없다.

---

## Validation Architecture

`workflow.nyquist_validation` 이 `.planning/config.json`에서 `false`로 명시되어 있음. 이 섹션은 생략한다.

---

## Sources

### Primary (HIGH confidence)
- `Assets/Enemy/NewBoss/Script/BossController.cs` — protected 멤버, ChangeState, MoveTo, StopMove, 이벤트 핸들러 전체 확인
- `Assets/Enemy/NewBoss/Script/BossStatsSystem.cs` — TakeDamage 분기 로직, IsBarrierActive, MaxWater 초기값, Die() virtual 확인
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — _decisionTimer, _isAttacking, SelectAttackStrategy/ShouldTransitionToGroggy virtual 확인
- `Assets/Enemy/NewBoss/Script/States/IBossState.cs` — 인터페이스 시그니처 확인
- `Assets/Enemy/NewBoss/Script/States/IdleState.cs` — TargetFound → ChaseState 전환 확인
- `Assets/Enemy/NewBoss/Script/States/ChaseStates.cs` — `new CombatState()` 직접 생성 확인 (인터셉트 패턴 필요 근거)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — CombatState 인터셉트 패턴, SetupHitBox, 이벤트 구독 패턴 확인
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — TakeDamage override, MaxWater=0 Reset() 패턴, Die() 확인
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — ShouldTransitionToGroggy false, SelectAttackStrategy override 패턴 확인
- `Assets/Enemy/WaterMonster/Script/States/Attacks/WaterRangedSpit.cs` — IAttackStrategy 투사체 구현 패턴 확인
- `Assets/Enemy/WaterMonster/Script/States/Attacks/WaterSpitProjectile.cs` — 투사체 비행, OnTriggerEnter2D, PlayerStats.TakeDamage() 호출 확인
- `Assets/Enemy/WaterMonster/Script/States/Attacks/WaterMeleeSwipe.cs` — Physics2D.OverlapCircleAll + PlayerStats.TakeDamage() 패턴 확인
- `Assets/Enemy/Script/EnemyHitBox.cs` — BossStatsSystem.TakeDamageInfo() 호출 파이프라인 확인
- `Assets/Player/Script/PlayerController.cs` — `ApplyKnockback(Vector2, float)` API, KnockbackRoutine, isKnockedBack 플래그 확인
- `Assets/Player/Script/PlayerStats.cs` — TakeDamage(float) 시그니처, HP 상속 구조 확인
- `Assets/Script/HP.cs` — TakeDamage, Die(), ClampHealth, onHealthChangedCallback 확인
- `Assets/Script/Combat/DamageInfo.cs` — DamageInfo struct, DamageElement enum 확인
- `.planning/phases/05-spirit-boss-stage1-patterns/05-CONTEXT.md` — 모든 locked decision 확인

### Secondary (MEDIUM confidence)
- `.planning/REQUIREMENTS.md` — CORE-01/02/04, S1-01/02/03 요구사항 확인
- `.planning/STATE.md` — 현재 Phase 5 시작 상태, 이전 milestone 완료 확인

---

## Metadata

**Confidence breakdown:**
- Standard Stack: HIGH — 모든 클래스를 직접 코드 확인
- Architecture: HIGH — WaterMonster 레퍼런스 구현과 BossController 기반 클래스 완전 분석
- Pitfalls: HIGH — 실제 코드 분기 로직에서 도출 (MaxWater 초기값, ChaseState 직접 생성 등)

**Research date:** 2026-04-30
**Valid until:** 2026-05-30 (프로젝트 내부 코드 기반, 외부 라이브러리 의존 없음)
