# Phase 3: 폭발 기믹 연계 및 보스 순간이동 - Research

**Researched:** 2026-04-16
**Domain:** Unity 2D C# — State machine extension, AoE explosion, coroutine-based IBossState, object pool bulk-return
**Confidence:** HIGH (all findings verified from actual project source)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- D-01: 폭발 방식은 동시 전체 폭발 — 임계치 도달 시 맵의 모든 Indestructible 웅덩이에서 동시에 AoE가 발동. 순차 연쇄가 아닌 1프레임 동시 폭발.
- D-02: 폭발 전 경고 딜레이 2초 — 임계치 도달 즉시 경고 이펙트(빛남/사운드) 발생 → 2초 후 실제 폭발 AoE 실행.
- D-03: 경고 딜레이 수치(2초)는 인스펙터에서 튜닝 가능하게 노출.
- D-04: 폭발 AoE는 각 웅덩이 위치에서 반경 기반 OverlapCircleAll로 Player 레이어에만 대미지 (REQ-WM-X-01 준수).
- D-05: 폭발 후 Indestructible 웅덩이 전부 Pool Return + `_indestructibleCount = 0` 리셋.
- D-06: 리셋 후 플레이어가 다시 웅덩이를 흡수하면 스택이 다시 쌓이는 사이클 반복.
- D-07: 텔레포트 타겟 선택은 플레이어와의 거리에 반비례 — 가까이→가장 먼 웅덩이, 멀리→가장 가까운 웅덩이.
- D-08: 거리 임계치는 기존 `CombatState`의 `dist ≤ 3.0f` 기준을 재사용.
- D-09: 텔레포트 연출: 출발지 사라짐 VFX → position 이동 → 목적지 나타남 VFX. 딜레이 0.2~0.3초.
- D-10: 도착 직후 즉시 CombatState 복귀.
- D-11: 텔레포트 패턴에도 REQ-WM-02 HP 코스트 적용.
- D-12: `WaterMonsterCombatState.SelectAttackStrategy` 내에서 텔레포트를 공격 패턴 후보 중 하나로 통합.
- D-13: 텔레포트 선택 조건: `_indestructibleCount >= 2` 이고 텔레포트 쿨다운이 지났을 때.
- D-14: 텔레포트 쿨다운 수치는 인스펙터 튜닝으로 위임.
- D-15: `WaterTeleportState`는 `IBossState` 구현 — `Enter`에서 타겟 웅덩이 선택 + HP 코스트 차감 + 연출 코루틴 시작. 완료 후 `CombatState` 복귀.

### Claude's Discretion
- 경고 이펙트 구체 에셋 (빛남 파티클 or 웅덩이 색 변화 or 화면 shake)
- 폭발 AoE 반경 수치 (인스펙터 튜닝)
- 텔레포트 VFX 에셋 (기존 이펙트 재사용 가능 여부 확인)
- 텔레포트 쿨다운 수치 (밸런싱, 인스펙터 튜닝)
- 폭발 대미지 수치 (치명적, 튜닝 위임)
- 텔레포트 연출 딜레이 정확한 수치 (0.2~0.3초 범위 내 플래너 결정)

### Deferred Ideas (OUT OF SCOPE)
- 텔레포트 후 추가 패턴 연계 (예: 도착 즉시 AoE 발사) — Phase 4 광폭화에서 고려
- 광폭화 모드 — Phase 4 소관
- 이속/감속 장판 — Phase 4 소관
- 폭발 횟수 제한 또는 강화 — Phase 4 또는 밸런싱 단계
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| REQ-WM-P3-01 | 스택 임계 폭발 구현 — `OnThresholdReached` 이벤트를 실제 연쇄 AoE + 피해 판정으로 구현 | PuddleStackManager.OnThresholdReached 이벤트 시그니처 확인, PuddlePool.Return API 확인 |
| REQ-WM-P3-02 | 보스 순간이동 패턴 — `WaterTeleportState` 신규 State, HP 코스트 적용, 조건부 실행 | IBossState 인터페이스 확인, WaterMonsterStats.SpendHpCost API 확인, WaterMonsterCombatState.SelectAttackStrategy override 위치 확인 |
| REQ-WM-02 | 공격 코스트 — 텔레포트 시전 시 HP 자가 소모, 최소 1 HP 보장 | WaterMonsterStats.SpendHpCost(float) 확인 |
| REQ-WM-X-01 | Layer Damage — 폭발 AoE는 Player 레이어에만 대미지 | WaterMeleeSwipe의 LayerMask.GetMask("Player") 패턴 확인 |
</phase_requirements>

---

## Summary

Phase 3의 두 기능(폭발 기믹, 텔레포트)은 모두 Phase 2에서 구축된 시스템의 연결고리를 채우는 작업이다. 폭발 기믹은 `PuddleStackManager.OnThresholdReached` 이벤트에 구독자를 붙이고, Pool에서 모든 Indestructible 웅덩이를 일괄 반환하면 된다. 텔레포트는 `WaterTeleportState : IBossState`를 신규 생성하고 `WaterMonsterCombatState.SelectAttackStrategy`에 조건부 분기를 추가하면 된다.

IBossState는 coroutine을 직접 실행하지 못하므로 `boss.StartCoroutine()`으로 MonoBehaviour를 경유하는 패턴이 이미 프로젝트에서 사용 중이다(`TutorialGroggyState`, `RootSpikeStrategy` 참조). `WaterTeleportState`의 연출 시퀀스도 동일 패턴으로 구현한다. PuddlePool은 `Return(WaterPuddle)` 메서드 하나만 공개하며 내부적으로 `OnReturnToPool()`을 호출해 `UnregisterIndestructible`까지 처리한다 — 일괄 반환 루프는 Pool의 `_pool` 리스트를 직접 순회할 수 없으므로 `PuddleStackManager`가 활성 Indestructible 웅덩이 목록을 따로 보관하거나, Pool의 전체 활성 항목을 쿼리하는 방식이 필요하다 (아래 Architecture Patterns 참조).

**Primary recommendation:** 폭발 트리거는 `PuddleStackManager`에 이벤트 구독자를 두는 `PuddleExplosionController` MonoBehaviour로 분리한다. `WaterTeleportState`는 IBossState로 구현하며 `Enter()`에서 `boss.StartCoroutine(TeleportSequence(boss))`를 시작한다.

---

## Standard Stack

### Core (재사용)
| 클래스/API | 위치 | Phase 3에서의 역할 |
|-----------|------|-----------------|
| `PuddleStackManager.OnThresholdReached` | `WaterMonster/Script/Phase2/PuddleStackManager.cs` | 폭발 트리거 이벤트 — 구독만 추가하면 됨 |
| `PuddleStackManager.Instance` | 동일 | 싱글턴 접근, `IndestructibleCount` 프로퍼티 제공 |
| `PuddlePool.Instance.Return(WaterPuddle)` | `WaterMonster/Script/Phase2/PuddlePool.cs` | 웅덩이 비활성화 + `OnReturnToPool()` 호출 (Unregister 포함) |
| `WaterMonsterStats.SpendHpCost(float)` | `WaterMonster/Script/WaterMonsterStats.cs` | HP 코스트 차감, 최소 1HP 보장 — REQ-WM-02 |
| `IBossState` | `NewBoss/Script/States/IBossState.cs` | `Enter/Execute/Exit(BossController)` — WaterTeleportState 구현 대상 |
| `BossController.ChangeState(IBossState)` | `NewBoss/Script/BossController.cs` | State 전환 (Exit→Enter 자동 호출) |
| `BossController.StartCoroutine(IEnumerator)` | 동일 | IBossState에서 코루틴 실행 시 경유 필수 |
| `WaterMonsterCombatState.SelectAttackStrategy` | `WaterMonster/Script/States/WaterMonsterCombatState.cs` | 텔레포트 조건 분기 추가 위치 |
| `Physics2D.OverlapCircleAll(pos, radius, layerMask)` | Unity Built-in | AoE 폭발 피해 판정 — Player 레이어 마스크 지정 |
| `LayerMask.GetMask("Player")` | Unity Built-in | REQ-WM-X-01 준수, 기존 WaterMeleeSwipe 동일 패턴 |
| `PlayerStats.TakeDamage(float)` | `Player/Script/PlayerStats.cs` | 폭발 피해 적용 대상 — `HP` 상속, `TakeDamage(float)` 오버라이드 |

### Namespace
모든 WaterMonster Phase 2/3 스크립트는 `namespace WaterMonster.Phase2`를 사용한다. `WaterTeleportState`는 동일 namespace에 배치하거나 `WaterMonster.Phase3`로 신규 생성 가능 (플래너 결정).

---

## Architecture Patterns

### 기존 State Machine 동작 방식
`BossController.ChangeState(IBossState newState)`:
1. `_currentState.Exit(this)` 호출
2. `_currentState = newState`
3. `_currentState.Enter(this)` 호출
4. 이후 매 프레임 `Update()`에서 `_currentState.Execute(this)` 호출

`WaterMonsterController`는 `Update()`에서 `CombatState`를 감지하면 `WaterMonsterCombatState`로 즉시 교체하는 스왑 로직이 있다. 따라서 `WaterTeleportState` 완료 후 `boss.ChangeState(new WaterMonsterCombatState())`를 직접 호출해야 스왑 루프를 거치지 않고 바로 올바른 상태로 진입한다.

### Pattern 1: IBossState + 코루틴 연출 (확립된 프로젝트 패턴)

`TutorialGroggyState`가 정확한 참조 구현이다.

```csharp
// Source: Assets/Enemy/Tutorial/TutorialBoss/State/TutorialGroggyState.cs (프로젝트 내 확인)
public class WaterTeleportState : IBossState
{
    private Coroutine _routine;

    public void Enter(BossController boss)
    {
        // HP 코스트 (D-11, REQ-WM-02)
        if (boss.Stats is WaterMonsterStats wms)
            wms.SpendHpCost(wms.MaxHealth * HpCostPercent);

        // 타겟 웅덩이 선택 (D-07)
        WaterPuddle target = SelectTeleportTarget(boss);
        if (target == null)
        {
            // 웅덩이 없으면 즉시 CombatState 복귀
            boss.ChangeState(new WaterMonsterCombatState());
            return;
        }

        // 연출 코루틴 시작 — IBossState는 MonoBehaviour가 아니므로 boss 경유
        _routine = boss.StartCoroutine(TeleportSequence(boss, target));
    }

    public void Execute(BossController boss) { /* 코루틴이 처리 */ }

    public void Exit(BossController boss)
    {
        if (_routine != null)
        {
            boss.StopCoroutine(_routine);
            _routine = null;
        }
    }

    private IEnumerator TeleportSequence(BossController boss, WaterPuddle target)
    {
        // D-09: 출발지 사라짐 VFX
        // (VFX 에셋 없으면 SpriteRenderer alpha로 대체 — 아래 VFX 섹션 참조)
        
        yield return new WaitForSeconds(disappearDuration); // 0.2~0.3초

        // position 이동
        boss.transform.position = target.transform.position;

        // D-09: 목적지 나타남 VFX
        yield return new WaitForSeconds(appearDuration);

        // D-10: 즉시 CombatState 복귀
        boss.ChangeState(new WaterMonsterCombatState());
    }
}
```

### Pattern 2: SelectAttackStrategy 텔레포트 통합 (D-12, D-13)

```csharp
// Source: Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs (확인됨)
// 기존 override에 텔레포트 조건 추가

public class WaterMonsterCombatState : CombatState
{
    [SerializeField] private float _teleportCooldown = 8f; // 인스펙터 튜닝 (D-14)
    private float _teleportCooldownTimer = 0f;

    // WaterMonsterCombatState는 plain class (MonoBehaviour 아님)이므로
    // 쿨다운 타이머는 Execute()에서 Time.deltaTime 누적
    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        // D-13: 텔레포트 조건
        bool canTeleport = PuddleStackManager.Instance != null
            && PuddleStackManager.Instance.IndestructibleCount >= 2
            && _teleportCooldownTimer <= 0f;

        if (canTeleport)
        {
            _teleportCooldownTimer = _teleportCooldown;
            boss.ChangeState(new WaterTeleportState()); // IBossState이므로 ChangeState
            return null; // ChangeState 후 attack 실행 안 함
        }

        // 기존 패턴 (D-08: dist ≤ 3.0f)
        if (dist <= 3.0f) return new WaterMeleeSwipe();
        return new WaterRangedSpit();
    }
}
```

**주의:** `SelectAttackStrategy`에서 `boss.ChangeState()`를 호출하면 현재 CombatState가 Exit됨. `SelectAttackStrategy`는 `Execute()` 내부에서 호출되므로 `ChangeState` 후 `Execute()`의 이후 코드가 stale 상태로 실행될 수 있다. 안전한 방법: `SelectAttackStrategy`에서 `null` 반환 + `Execute()`에서 null guard 처리. 기존 `CombatState.Execute()`는 이미 `if (attack != null)` 가드가 있어 null 반환 시 아무것도 실행하지 않는다.

쿨다운 타이머(`_teleportCooldownTimer`)는 `Execute()` 진입 시마다 `Time.deltaTime` 감산. `WaterMonsterCombatState`는 plain class이므로 필드 유지 가능 (State 인스턴스가 교체될 때마다 리셋 — 의도적 동작).

### Pattern 3: 폭발 기믹 — PuddleExplosionController

`PuddleStackManager.OnThresholdReached`를 구독하는 별도 MonoBehaviour. 이 접근이 `PuddleStackManager`를 수정하지 않고 폭발 로직을 추가하는 Open/Closed 방식이다.

```csharp
// 신규 파일: Assets/Enemy/WaterMonster/Script/Phase3/PuddleExplosionController.cs
using WaterMonster.Phase2;

public class PuddleExplosionController : MonoBehaviour
{
    [SerializeField] private float warningDuration = 2f;  // D-03
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionDamage = 50f;

    private Coroutine _explosionRoutine;

    private void OnEnable()
    {
        if (PuddleStackManager.Instance != null)
            PuddleStackManager.Instance.OnThresholdReached += OnThresholdReached;
    }

    private void OnDisable()
    {
        if (PuddleStackManager.Instance != null)
            PuddleStackManager.Instance.OnThresholdReached -= OnThresholdReached;
    }

    private void OnThresholdReached()
    {
        if (_explosionRoutine != null) StopCoroutine(_explosionRoutine);
        _explosionRoutine = StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        // D-02: 경고 이펙트 (2초)
        TriggerWarningEffect();
        yield return new WaitForSeconds(warningDuration);

        // D-01: 동시 전체 폭발
        ExplodeAllIndestructible();

        // D-05: 폭발 후 전체 Pool Return + count reset
        ReturnAllIndestructibleToPool();
    }
    
    // ... (아래 섹션 참조)
}
```

### Pattern 4: 전체 Indestructible 웅덩이 일괄 반환

**핵심 발견:** `PuddlePool._pool`은 `private`이고, `PuddleStackManager`는 Indestructible 웅덩이의 참조 목록이 아닌 `_indestructibleCount` int만 유지한다. 따라서 "활성 Indestructible 웅덩이 전체 목록"을 얻는 직접적인 API가 현재 없다.

**해결 방법 (추천):** `PuddleStackManager`에 `List<WaterPuddle> _indestructiblePuddles` 참조 목록을 추가한다.

```csharp
// PuddleStackManager에 추가할 멤버
private List<WaterPuddle> _indestructiblePuddles = new List<WaterPuddle>();

public void RegisterIndestructible(WaterPuddle puddle)
{
    _indestructibleCount++;
    _indestructiblePuddles.Add(puddle); // 참조 보관
    if (_indestructibleCount >= explosionThreshold)
        OnThresholdReached?.Invoke();
}

public void UnregisterIndestructible(WaterPuddle puddle)
{
    if (!puddle.isDestructible)
    {
        _indestructibleCount = Mathf.Max(0, _indestructibleCount - 1);
        _indestructiblePuddles.Remove(puddle); // 참조 제거
    }
}

// Phase 3에서 사용할 일괄 반환 메서드
public void ReturnAllIndestructibleToPool()
{
    // 복사본으로 순회 (Return 중 목록 변경 방지)
    var toReturn = new List<WaterPuddle>(_indestructiblePuddles);
    foreach (var puddle in toReturn)
    {
        PuddlePool.Instance.Return(puddle); // OnReturnToPool → UnregisterIndestructible 호출
    }
    // UnregisterIndestructible이 목록을 비워주므로 추가 리셋 불필요
    // 단, count 안전 리셋:
    _indestructibleCount = 0;
    _indestructiblePuddles.Clear();
}
```

**대안 (PuddleStackManager 수정 최소화):** `PuddlePool`에 `GetAllActive()` 메서드 추가 후 `isDestructible == false` 필터링. 하지만 PuddleStackManager 목록 추가 방식이 더 직접적이고 D-05의 의도와 일치한다.

### Anti-Patterns to Avoid
- **IBossState에서 직접 StartCoroutine 호출:** IBossState는 MonoBehaviour가 아니므로 불가. 반드시 `boss.StartCoroutine()` 경유.
- **Exit() 없이 코루틴 방치:** `WaterTeleportState.Exit()`에서 `_routine`을 `StopCoroutine`으로 반드시 정리. 외부에서 State가 강제 교체될 때(ChaseState 전환 등) 코루틴이 살아있으면 `ChangeState`가 중복 호출될 수 있음.
- **_pool 직접 접근:** `PuddlePool._pool`은 private — `Return()` 공개 API를 통해서만 접근.
- **WaterMonsterController의 CombatState 스왑 로직 충돌:** `WaterTeleportState` 완료 후 `new CombatState()`가 아닌 `new WaterMonsterCombatState()`를 호출해야 함. `new CombatState()` 호출 시 다음 Update()에서 스왑되지만 한 프레임 지연이 발생.
- **OnThresholdReached 중복 발화:** `PuddleStackManager.RegisterIndestructible`은 매번 임계치 초과 여부를 체크하지만, 이미 임계치를 넘어 이벤트가 발화된 이후에도 추가 등록 시 재발화된다. 폭발 진행 중 새 웅덩이가 등록되면 이벤트가 중복 발화될 수 있음. `PuddleExplosionController`에서 `_isExploding` 플래그로 중복 처리 방지 필요.

---

## Don't Hand-Roll

| 문제 | 직접 구현 금지 | 사용할 것 | 이유 |
|-----|------------|---------|-----|
| HP 코스트 차감 | `_currentHealth -= cost` 직접 작성 | `WaterMonsterStats.SpendHpCost(float)` | 최소 1HP 클램프, 이벤트 미발화 보장이 이미 구현됨 |
| Player 레이어 대미지 | `GetComponent<HP>()` 등 일반 탐색 | `Physics2D.OverlapCircleAll(pos, r, LayerMask.GetMask("Player"))` + `GetComponentInParent<PlayerStats>()` | 기존 WaterMeleeSwipe 패턴 — REQ-WM-X-01 준수 보장 |
| 웅덩이 반환 | `Destroy(gameObject)` | `PuddlePool.Instance.Return(puddle)` | `OnReturnToPool()`이 isDestructible 리셋 + 색상 복원 + UnregisterIndestructible 일괄 처리 |
| 코루틴 실행 (State에서) | 별도 CoroutineRunner GameObject | `boss.StartCoroutine()` | 프로젝트 기존 패턴 (TutorialGroggyState 참조) |
| State 전환 | `_currentState = newState` 직접 | `boss.ChangeState(newState)` | Exit/Enter 자동 호출 보장 |

---

## VFX Asset 현황 (Claude's Discretion 항목)

**확인된 기존 에셋:**
- `Assets/Enemy/WaterMonster/Resources/HealPopup.prefab` — 팝업 텍스트 (폭발에는 부적합)
- `Assets/Enemy/WaterMonster/Resources/WaterSpitProjectile.prefab` — 투사체 (재사용 불가)
- `Assets/Enemy/Boss/Prefab/GroundAttackWarning.prefab` — 경고 박스 (Tutorial WoodBoss용, 형태가 다름)
- `Assets/ImportedAsset/Hero Knight - Pixel Art/Demo/SlideDust.prefab` — 작은 파티클 (느낌이 맞지 않을 수 있음)
- `Assets/Script/Square.prefab` — 단순 사각형 (임시 경고 표시 가능)

**VFX 에셋 결론:** 전용 폭발 VFX 및 텔레포트 VFX 에셋이 현재 없다. 플래너는 다음 중 선택:

1. **SpriteRenderer 색상 변화** — 경고 시 웅덩이 `SpriteRenderer.color`를 빨간색/흰색으로 깜빡임 (코드만, 에셋 불필요). 신규 에셋 필요 없으므로 권장.
2. **임시 Square.prefab 재사용** — 폭발 범위 시각화에 `Square.prefab`을 일시적으로 생성 후 삭제.
3. **텔레포트 VFX** — `SpriteRenderer.enabled = false/true` + 0.2초 딜레이로 사라짐/나타남 연출 (에셋 없이 구현 가능).

---

## Common Pitfalls

### Pitfall 1: PuddleStackManager 중복 폭발 발화
**What goes wrong:** 폭발 진행 중(경고 딜레이 2초 동안) 플레이어가 웅덩이를 추가로 흡수하면 `RegisterIndestructible`이 다시 임계치를 체크해 `OnThresholdReached`를 재발화. `PuddleExplosionController`에 코루틴이 중복 시작됨.
**Why it happens:** `PuddleStackManager.RegisterIndestructible`은 매 호출마다 `if (_indestructibleCount >= explosionThreshold)` 체크. 폭발 중에도 새 등록이 가능.
**How to avoid:** `PuddleExplosionController`에 `_isExploding` bool 플래그. `OnThresholdReached` 핸들러에서 `if (_isExploding) return;` 가드 추가.
**Warning signs:** 폭발 애니메이션이 2초 경고 없이 즉시 발동하거나 연속 발동.

### Pitfall 2: WaterTeleportState에서 Exit 없이 코루틴 방치
**What goes wrong:** 외부 이벤트(플레이어 킬, 씬 전환 등)로 State가 강제 교체될 때 `TeleportSequence` 코루틴이 계속 실행되어 `boss.ChangeState(new WaterMonsterCombatState())`가 의도치 않은 타이밍에 호출됨.
**Why it happens:** `IBossState.Exit()`가 호출될 때 진행 중인 코루틴을 정리하지 않으면 MonoBehaviour(boss) 위에서 독립적으로 실행 지속.
**How to avoid:** `Exit(BossController boss)` 구현: `if (_routine != null) { boss.StopCoroutine(_routine); _routine = null; }`
**Warning signs:** 보스가 이미 다른 State에 있는데 갑자기 WaterMonsterCombatState로 전환되는 현상.

### Pitfall 3: `PuddlePool._pool` 직접 접근 시도
**What goes wrong:** 전체 Indestructible 웅덩이 목록이 필요할 때 `PuddlePool._pool`에 접근하려 하지만 `private` 필드라 컴파일 오류.
**Why it happens:** `PuddlePool`은 `ActiveCount` 프로퍼티만 공개하고, `_pool`은 private.
**How to avoid:** `PuddleStackManager`에 `_indestructiblePuddles` 목록을 추가하거나, `PuddlePool`에 `ReturnAllIndestructible()` 메서드를 추가. Architecture Patterns §4 참조.

### Pitfall 4: CombatState Execute()에서 ChangeState() 후 계속 실행
**What goes wrong:** `SelectAttackStrategy()`에서 `boss.ChangeState(new WaterTeleportState())`를 호출해도 현재 `Execute()` 메서드는 반환되지 않고 계속 실행. `attack = null` 반환 + 기존 `if (attack != null)` 가드가 필요.
**Why it happens:** C#에서 메서드 호출 중 외부 상태 변경은 현재 실행 흐름을 중단하지 않음.
**How to avoid:** `SelectAttackStrategy`에서 텔레포트 조건 해당 시 `boss.ChangeState()`를 직접 호출하는 대신 `null`을 반환하고, `Execute()`의 호출부에서 null 반환을 "텔레포트 State로 전환 신호"로 처리. 또는 `SelectAttackStrategy`는 IBossState를 반환할 수 있도록 시그니처 변경 고려. **가장 단순한 방법:** `SelectAttackStrategy`에서 `boss.ChangeState(new WaterTeleportState()); return null;` — `Execute()`의 기존 `if (attack != null)` 가드가 이후 공격 실행을 막아줌. 테스트 필요.

### Pitfall 5: OnReturnToPool의 UnregisterIndestructible 타이밍
**What goes wrong:** `WaterPuddle.OnReturnToPool()`은 `PuddleStackManager.Instance.UnregisterIndestructible(this)`를 호출하는데, 이때 `puddle.isDestructible`이 이미 `true`로 리셋된 후라면 `UnregisterIndestructible`이 `if (!puddle.isDestructible)` 조건 때문에 카운트를 감소시키지 않는다.

**실제 코드 확인 결과:**
```csharp
// WaterPuddle.OnReturnToPool() — 현재 구현
public void OnReturnToPool()
{
    isDestructible = true;           // ← 먼저 true로 리셋
    playerInRange = false;
    if (_sr != null) _sr.color = Color.white;
    PuddleStackManager.Instance.UnregisterIndestructible(this); // ← 이미 isDestructible=true
    gameObject.SetActive(false);
}

// UnregisterIndestructible 조건
public void UnregisterIndestructible(WaterPuddle puddle)
{
    if (!puddle.isDestructible)      // ← false일 때만 감소 → 항상 skip됨!
        _indestructibleCount = ...
}
```

**이것은 기존 버그다.** `PuddleExplosionController`에서 `ReturnAllIndestructibleToPool()`을 호출할 때 `OnReturnToPool`의 UnregisterIndestructible가 항상 스킵되므로 `_indestructibleCount`가 감소하지 않는다. D-05의 "count reset"은 `PuddleStackManager.ReturnAllIndestructibleToPool()` 내에서 `_indestructibleCount = 0; _indestructiblePuddles.Clear();`로 강제 리셋해야 한다 (Pool Return 이후).

---

## Code Examples

### 폭발 AoE 피해 판정 (기존 WaterMeleeSwipe 패턴 직접 적용)
```csharp
// Source: Assets/Enemy/WaterMonster/Script/States/Attacks/WaterMeleeSwipe.cs (확인됨)
// AoE 폭발 피해 — 각 웅덩이 위치에서 실행
private void ApplyExplosionDamage(Vector3 puddlePosition, float radius, float damage)
{
    var hits = Physics2D.OverlapCircleAll(puddlePosition, radius, LayerMask.GetMask("Player"));
    foreach (var hit in hits)
    {
        var playerStats = hit.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
            playerStats.TakeDamage(damage);
    }
}
```

### 텔레포트 타겟 선택 (D-07 반비례 로직)
```csharp
// Source: D-07, D-08 결정사항 기반
private WaterPuddle SelectTeleportTarget(BossController boss)
{
    var puddles = PuddleStackManager.Instance.IndestructiblePuddles; // 목록 공개 필요
    if (puddles == null || puddles.Count == 0) return null;

    float dist = Vector2.Distance(boss.transform.position, boss.Target.position);
    bool isClose = dist <= 3.0f; // D-08: 기존 CombatState 기준 재사용

    WaterPuddle selected = null;
    float bestDist = isClose ? float.MinValue : float.MaxValue;

    foreach (var puddle in puddles)
    {
        float d = Vector2.Distance(boss.Target.position, puddle.transform.position);
        if (isClose && d > bestDist) { bestDist = d; selected = puddle; }      // 가장 먼 웅덩이
        else if (!isClose && d < bestDist) { bestDist = d; selected = puddle; } // 가장 가까운 웅덩이
    }
    return selected;
}
```

### SpendHpCost 호출 패턴 (Phase 1 확립 패턴)
```csharp
// Source: Assets/Enemy/WaterMonster/Script/States/Attacks/WaterMeleeSwipe.cs (확인됨)
// HpCostPercent는 const float (예: 0.04f)
if (boss.Stats is WaterMonsterStats wms)
    wms.SpendHpCost(wms.MaxHealth * HpCostPercent);
```

---

## Integration Points Summary

| 연결 대상 | 변경 유형 | 메서드/필드 |
|----------|---------|-----------|
| `PuddleStackManager` | 수정 — 목록 추가 + ReturnAll 메서드 | `_indestructiblePuddles`, `RegisterIndestructible`, `UnregisterIndestructible`, `ReturnAllIndestructibleToPool()`, `IndestructiblePuddles` 프로퍼티 공개 |
| `WaterMonsterCombatState` | 수정 — SelectAttackStrategy 텔레포트 분기 추가 | `SelectAttackStrategy(BossController, float)` override, `_teleportCooldownTimer` 필드, Execute() 타이머 감산 |
| `PuddleExplosionController` | 신규 MonoBehaviour | `OnThresholdReached` 구독, `ExplosionSequence` 코루틴, `_isExploding` 플래그 |
| `WaterTeleportState` | 신규 IBossState | `Enter/Execute/Exit`, `TeleportSequence` 코루틴, 타겟 선택 로직 |

---

## Open Questions

1. **PuddleExplosionController 배치 위치**
   - What we know: MonoBehaviour가 필요하므로 씬의 GameObject에 부착 필요
   - What's unclear: `WaterMonsterController`에 직접 통합할지, 별도 GameObject로 분리할지
   - Recommendation: 별도 GameObject (WeatherController와 같은 패턴). 씬 Setup은 Phase 2의 PlaceWaterMonsterInScene 에디터 스크립트에 이미 패턴 존재.

2. **텔레포트 쿨다운 타이머 위치**
   - What we know: `WaterMonsterCombatState`는 plain class이고 state 교체 시 인스턴스가 새로 생성됨 (`new WaterMonsterCombatState()`)
   - What's unclear: 텔레포트 쿨다운이 Combat State 인스턴스 수명에 묶이면 State 전환 시마다 리셋됨
   - Recommendation: `WaterMonsterController`에 `float _teleportCooldownTimer` 필드를 두고, `WaterMonsterCombatState`가 `boss as WaterMonsterController`로 접근. 또는 WaterTeleportState 완료 시각을 `Time.time`으로 기록 — 후자가 단순함.

3. **경고 이펙트 구현 방식**
   - What we know: 전용 VFX 에셋 없음. `SpriteRenderer.color` 변경 가능.
   - Recommendation: 경고 시 Indestructible 웅덩이들의 `SpriteRenderer.color`를 빨간색으로 변경 → 2초 후 폭발 → Pool Return. 추가 에셋 불필요, 플레이어가 즉시 인지 가능.

---

## Environment Availability

Step 2.6: SKIPPED — Phase 3은 순수 코드 변경이며 외부 도구/서비스/CLI에 의존하지 않음.

---

## Sources

### PRIMARY (HIGH confidence — 프로젝트 소스 직접 확인)
- `Assets/Enemy/WaterMonster/Script/Phase2/PuddleStackManager.cs` — `OnThresholdReached` 시그니처, `_indestructibleCount`, `RegisterIndestructible`, `UnregisterIndestructible`
- `Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs` — `isDestructible`, `OnReturnToPool()`, 버그 확인
- `Assets/Enemy/WaterMonster/Script/Phase2/PuddlePool.cs` — `Return(WaterPuddle)`, `ActiveCount`, `_pool` private 확인
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — `SelectAttackStrategy` 구조, `dist <= 3.0f`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — CombatState 스왑 로직, Phase 2 진입 트리거 패턴
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — `SpendHpCost(float)`, `CurrentHealth`, Phase 1 HP 코스트 패턴
- `Assets/Enemy/NewBoss/Script/States/IBossState.cs` — `Enter/Execute/Exit(BossController)` 인터페이스
- `Assets/Enemy/NewBoss/Script/BossController.cs` — `ChangeState`, `StartCoroutine`, `CurrentState`, `Target`
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — `Execute()` 흐름, `SelectAttackStrategy`, null guard
- `Assets/Enemy/Tutorial/TutorialBoss/State/TutorialGroggyState.cs` — IBossState + 코루틴 확립 패턴
- `Assets/Enemy/WaterMonster/Script/States/Attacks/WaterMeleeSwipe.cs` — Layer 마스크 AoE 패턴, SpendHpCost 호출 패턴
- `Assets/Player/Script/PlayerStats.cs` — `TakeDamage(float)` 시그니처 확인

---

## Metadata

**Confidence breakdown:**
- Integration points (PuddleStackManager, PuddlePool, WaterMonsterStats): HIGH — 소스 직접 확인
- Architecture (IBossState + coroutine 패턴): HIGH — TutorialGroggyState 실제 구현 확인
- Pitfalls (OnReturnToPool 버그, CombatState Execute 흐름): HIGH — 소스 직접 확인
- VFX 에셋 현황: HIGH — 파일시스템 직접 확인, 전용 에셋 없음 확인

**Research date:** 2026-04-16
**Valid until:** Phase 3 구현 완료까지 (코드 변경 반영 필요)
