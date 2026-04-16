# Phase 4: 광폭화 및 장판 시스템 - Research

**Researched:** 2026-04-16
**Domain:** Unity 2D C# — 보스 State 플래그 광폭화, Zone Prefab Trigger, WaterMonsterStats tick 소모
**Confidence:** HIGH (전량 실제 코드 직접 확인)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**광폭화 State 구조**
- D-01: 광폭화는 별도 State 클래스 없이 `WaterMonsterCombatState`에 `_isEnraged bool` 플래그로 구현. 플래그가 true일 때 SelectAttackStrategy 내 쿨다운 배율 및 장판 생성 후보가 활성화됨.
- D-02: 광폭화 활성화는 `WaterMonsterController`에서 HP 임계치(`_enrageHpThreshold`) 감지 → `WaterMonsterCombatState.SetEnraged(true)` 호출. Phase 2의 `CheckPhase2Trigger` 패턴을 동일하게 재사용.
- D-03: 광폭화 쿨타임 배율(`enrageCooldownMultiplier`)은 Inspector 튜닝 위임. `WaterMonsterCombatState`에 [SerializeField] 필드로 노출.
- D-04: 광폭화 시 장판 생성을 SelectAttackStrategy 후보 중 하나로 통합 — Phase 3 텔레포트 통합 패턴(D-12)과 동일한 구조. 장판 쿨다운이 지났을 때 후보에 포함.

**장판(Zone) 생성 구조**
- D-05: 장판 생성 주체는 `WaterMonsterController`. 별도 ZoneSpawner 컴포넌트 없음.
- D-06: 기존에 외부에서 만든 Zone 오브젝트(프리팹)를 Inspector에서 직접 참조:
  - `[SerializeField] private GameObject _speedUpZonePrefab`
  - `[SerializeField] private GameObject _slowDownZonePrefab`
  - SpeedUp/SlowDown 구분은 프리팹 변형으로만 — 코드 로직(이속 효과)은 이미 기존 Zone에 구현되어 있다고 가정.
- D-07: 장판은 일정 시간 후 자동 비활성화 (`zoneDuration` Inspector 튜닝). Instantiate 후 타이머로 Destroy or SetActive(false).
- D-08: 장판 생성 위치는 맵 랜덤 위치 (Inspector에서 범위 튜닝 가능). Player 레이어에만 영향 (REQ-WM-X-01 준수).

**HP 코스트 가속**
- D-09: 광폭화 시 별도 주기적 tick HP 소모 추가 (기존 공격 코스트와 별개로 누적).
- D-10: tick 소모 위치: `WaterMonsterStats.Update` — `_isEnraged` 플래그 기반으로 처리. `SpendHpCost` 기존 메서드 재사용.
- D-11: tick 수치(`enrageTickInterval`, `enrageTickAmount`)는 Inspector 튜닝 위임.
- D-12: 광폭화 진입 임계치 M% → `_enrageHpThreshold` 필드로 Inspector 노출. Phase 2의 `_phase2HpThreshold` 패턴과 동일.

### Claude's Discretion
- 장판 생성 위치 랜덤 범위 계산 방식 (Camera bound 기반 or Collider bound 기반)
- 기존 Zone 프리팹의 이속 적용 인터페이스 확인 (OnTriggerEnter/Exit 여부, Player 레이어 필터 구현 여부)
- 광폭화 쿨타임 배율 기본값 (Inspector 튜닝 위임이지만 합리적 기본값 제안)
- 장판 개수 상한선 (동시에 몇 개까지 맵에 존재 가능한지)
- `SetEnraged` 호출 시 `WaterMonsterCombatState` 참조 방법 (`CurrentState as WaterMonsterCombatState` 캐스팅 또는 Controller 필드)

### Deferred Ideas (OUT OF SCOPE)
- 광폭화에서 텔레포트 후 즉시 AoE 발사 연계 — 밸런싱 단계
- 폭발 강화 (n번째 폭발이 더 강함) — 밸런싱 단계
- SpeedUp/SlowDown Zone의 시각적 구분 강화 (입자 이펙트 추가 등)
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| REQ-WM-P4-01 | 맵에 무작위로 SpeedUpZone / SlowDownZone AoE를 보스가 생성. 레이어 기반으로 플레이어에게만 적용 (REQ-WM-X-01 준수). | Zone Prefab을 WaterMonsterController에서 Instantiate. 위치는 WeatherController와 동일한 BoxCollider2D bounds 패턴 재사용. Player 레이어 필터는 LayerMask.GetMask("Player") 패턴(이미 MeleeSwipe, Explosion에서 확립됨). |
| REQ-WM-P4-02 | 진입 조건: HP ≤ M%. 공격 쿨타임 계수 감소, 패턴 시전 속도/빈도 상승. 자가 HP 소모 속도 증가. | `_isEnraged` 플래그 + `enrageCooldownMultiplier`로 `_decisionTimer` 배율 적용. tick 소모는 WaterMonsterStats.Update에 추가. |
| REQ-WM-P4-03 | 광폭화 상태에서 보스가 장판과 패턴을 번갈아 사용하는 AI 로직. | SelectAttackStrategy에 장판 생성 후보 추가 (Phase 3 텔레포트 통합 패턴 D-12 동일 구조). 장판 쿨다운이 지났을 때 후보 포함. |
| REQ-WM-X-01 | 보스의 모든 공격/폭발/장판은 Player 레이어에만 대미지/효과. | Zone 프리팹의 Trigger 콜라이더가 Layer 필터(`LayerMask.GetMask("Player")` 또는 `CompareTag`/`layer` 체크)로 구현됨을 가정. Zone 프리팹이 없으면 신규 작성 필요 — 현재 코드베이스에 Zone 스크립트 없음(아래 참조). |
</phase_requirements>

---

## Summary

Phase 4는 `WaterMonsterCombatState`에 `_isEnraged bool` 플래그를 추가해 광폭화를 표현하고, `WaterMonsterController`에서 HP 임계치 감지 후 `SetEnraged(true)`를 호출하는 것이 핵심이다. Phase 2의 `CheckPhase2Trigger` 패턴과 Phase 3의 `SelectAttackStrategy` 통합 패턴을 그대로 재사용한다.

Zone 프리팹(SpeedUpZone / SlowDownZone)은 현재 프로젝트 코드베이스에 존재하지 않는다. CONTEXT.md D-06에서 "외부에서 만든 Zone 오브젝트"를 참조한다고 했으나, Assets 전체 탐색 결과 해당 프리팹/스크립트를 찾지 못했다. 플래너는 Zone 프리팹이 존재하지 않을 경우를 대비해 신규 Zone 스크립트 작성을 Wave 0 태스크로 포함해야 한다.

`WaterMonsterStats`에는 이미 `SpendHpCost(float)` 메서드가 있고 `BossStatsSystem.Update()`는 `IsBarrierActive` 가드로 WaterMonster에서 자동 비활성화된다. 광폭화 tick 소모는 `WaterMonsterStats`를 `Update` 오버라이드하거나 별도 `Update` 로직을 추가해 `_isEnraged` 플래그 시 `SpendHpCost`를 호출하면 된다.

**Primary recommendation:** Zone 프리팹 유무를 Wave 0에서 확인하고 없으면 신규 작성. 광폭화 구조는 Phase 2/3 패턴을 그대로 반복 적용.

---

## Standard Stack

### Core (이번 페이즈에서 수정/추가하는 파일)

| 파일 | 역할 | 변경 유형 |
|------|------|----------|
| `WaterMonsterController.cs` | 광폭화 임계치 감지, Zone 프리팹 참조, SpawnZone 메서드 | 수정 (필드+메서드 추가) |
| `WaterMonsterCombatState.cs` | `_isEnraged` 플래그, `SetEnraged()`, SelectAttackStrategy 장판 후보 | 수정 (플래그+분기 추가) |
| `WaterMonsterStats.cs` | 광폭화 tick HP 소모 로직 | 수정 (Update 오버라이드 또는 추가) |
| `SpeedUpZone.cs` (신규 또는 기존) | Player Trigger 진입 시 이속 버프 적용 | 신규 (Zone 프리팹 없을 경우) |
| `SlowDownZone.cs` (신규 또는 기존) | Player Trigger 진입 시 이속 디버프 적용 | 신규 (Zone 프리팹 없을 경우) |

### 재사용 패턴

| 패턴 | 출처 | Phase 4 재사용 |
|------|------|--------------|
| HP 임계치 트리거 (`CheckPhase2Trigger`) | WaterMonsterController.cs | `CheckEnrageTrigger` 동일 구조 |
| `bool _phaseXTriggered` 가드 | WaterMonsterController.cs | `bool _enrageTriggered` |
| 쿨다운 관리 (`_lastTeleportTime`, `CanTeleport()`) | WaterMonsterController.cs | `_lastZoneTime`, `CanSpawnZone()` |
| SelectAttackStrategy 후보 통합 | WaterMonsterCombatState.cs | 장판 생성 후보 분기 추가 |
| `SpendHpCost(float)` | WaterMonsterStats.cs | tick 소모에 직접 재사용 |
| BoxCollider2D bounds 랜덤 위치 | PuddleSpawner.cs | Zone 생성 위치 계산 동일 패턴 |
| `LayerMask.GetMask("Player")` | WaterMeleeSwipe.cs, PuddleExplosionController.cs | Zone Trigger 레이어 필터 |

---

## Architecture Patterns

### Phase 4 추가 구조

```
Assets/Enemy/WaterMonster/Script/
├── WaterMonsterController.cs       ← [수정] 광폭화 트리거 + Zone 생성 메서드 추가
├── WaterMonsterStats.cs            ← [수정] 광폭화 tick 소모 추가
├── States/
│   └── WaterMonsterCombatState.cs  ← [수정] _isEnraged 플래그 + SelectAttackStrategy 분기
└── Phase4/                         ← [신규 폴더]
    ├── SpeedUpZone.cs
    └── SlowDownZone.cs
```

### Pattern 1: 광폭화 트리거 (CheckPhase2Trigger 재사용)

`WaterMonsterController`에 `CheckEnrageTrigger` 메서드를 추가하고 `OnDamageTaken` 이벤트에 구독.

```csharp
// WaterMonsterController.cs에 추가
[Header("Phase 4 Settings")]
[SerializeField] [Range(0f, 1f)] private float _enrageHpThreshold = 0.30f;
private bool _enrageTriggered = false;

// Start() 에서 추가:
WaterStats.OnDamageTaken += CheckEnrageTrigger;

// OnDestroy() 에서 추가:
WaterStats.OnDamageTaken -= CheckEnrageTrigger;

private void CheckEnrageTrigger()
{
    if (_enrageTriggered) return;
    if (WaterStats.CurrentHealth / WaterStats.MaxHealth <= _enrageHpThreshold)
    {
        _enrageTriggered = true;
        // CurrentState를 WaterMonsterCombatState로 캐스팅해 SetEnraged 호출
        if (CurrentState is WaterMonsterCombatState combatState)
        {
            combatState.SetEnraged(true);
        }
    }
}
```

**중요:** `OnDamageTaken` 이벤트는 `WaterMonsterStats.TakeDamage`에서 비-Water 대미지 시에만 발화된다 (코드 확인 완료). 광폭화 tick 소모는 `SpendHpCost`를 호출하므로 `OnDamageTaken`을 발화하지 않는다 — 즉 tick 소모로 HP가 임계치 이하로 떨어져도 `CheckEnrageTrigger`가 호출되지 않는다. 이를 보완하려면 `WaterMonsterStats.Update` 오버라이드 안에서도 임계치를 체크하거나, 처음 임계치 도달 시 트리거를 보장하는 Update 기반 폴백을 추가해야 한다. (플래너가 선택: 이벤트만 or Update 폴백 병행)

### Pattern 2: SetEnraged + 쿨다운 배율

```csharp
// WaterMonsterCombatState.cs에 추가
[SerializeField] private float _enrageCooldownMultiplier = 0.5f; // Inspector 노출

private bool _isEnraged = false;

public void SetEnraged(bool value)
{
    _isEnraged = value;
}

protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
{
    // Zone 생성 후보 (광폭화 시에만)
    bool canSpawnZone = _isEnraged
        && boss is WaterMonsterController wmc
        && wmc.CanSpawnZone();

    if (canSpawnZone)
    {
        wmc.SpawnRandomZone();
        wmc.RecordZoneTime();
        // Zone 생성은 실제 공격이 아니므로 쿨다운을 별도 설정
        _decisionTimer = /* 장판 후 짧은 대기 */ 1.0f;
        return null; // 이번 프레임 공격 없음
    }

    // 기존 텔레포트 조건
    bool canTeleport = PuddleStackManager.Instance != null
        && PuddleStackManager.Instance.IndestructibleCount >= 2
        && boss is WaterMonsterController wmc2
        && wmc2.CanTeleport();

    if (canTeleport)
    {
        boss.ChangeState(new WaterTeleportState());
        return null;
    }

    // 기본 패턴
    float cooldownScale = _isEnraged ? _enrageCooldownMultiplier : 1.0f;
    // 주의: CombatState._decisionTimer는 attack.Cooldown으로 설정됨.
    // 쿨다운 배율은 attack 반환 후 _decisionTimer 재설정으로는 적용이 불가.
    // 대신 Cooldown 프로퍼티를 래핑하는 방식이 필요 (아래 Anti-Pattern 참조).
    if (dist <= 3.0f) return new WaterMeleeSwipe();
    return new WaterRangedSpit();
}
```

### Pattern 3: 쿨다운 배율 적용 방법 (구조적 제약 해결)

`CombatState.Execute`는 `_decisionTimer = attack.Cooldown`으로 쿨다운을 설정한다. `IAttackStrategy.Cooldown`은 현재 const/고정값이다. 광폭화 배율을 적용하는 방법은 두 가지:

**옵션 A (권장):** `WaterMonsterCombatState.Execute`를 오버라이드해 `_decisionTimer`에 배율을 곱한다.

```csharp
// WaterMonsterCombatState — Execute 오버라이드
// 기반 클래스 Execute 호출 후 _decisionTimer에 배율 적용
// 단, _decisionTimer는 private — protected로 승격이 필요하거나
// base.Execute() 내부에서 처리해야 함
```

**옵션 B:** `CombatState._decisionTimer`를 `protected`로 변경하고 `WaterMonsterCombatState.Execute`에서 base 호출 후 배율 재적용. 단, `CombatState`는 NewBoss에 속하므로 한 줄 수정이 필요하다.

**옵션 C (최소 침습):** 각 패턴 전략 클래스(WaterMeleeSwipe, WaterRangedSpit)의 Cooldown 프로퍼티를 외부에서 설정 가능하게 바꾸고 SelectAttackStrategy에서 배율을 주입.

플래너는 옵션 중 하나를 선택해야 한다. 옵션 B가 가장 깔끔하지만 CombatState.cs 1줄 수정이 필요하다.

### Pattern 4: Zone 생성 (WaterMonsterController)

```csharp
// WaterMonsterController.cs에 추가
[Header("Phase 4 Zone Settings")]
[SerializeField] private GameObject _speedUpZonePrefab;
[SerializeField] private GameObject _slowDownZonePrefab;
[SerializeField] private float _zoneCooldown = 5f;
[SerializeField] private float _zoneDuration = 8f;
[SerializeField] private BoxCollider2D _zoneBounds; // WeatherController의 spawnBounds와 공유 가능
[SerializeField] private int _maxActiveZones = 4;

private float _lastZoneTime = -999f;
private int _activeZoneCount = 0; // 현재 활성 Zone 개수 추적

public bool CanSpawnZone()
{
    return Time.time - _lastZoneTime >= _zoneCooldown
           && _activeZoneCount < _maxActiveZones;
}

public void RecordZoneTime()
{
    _lastZoneTime = Time.time;
}

public void SpawnRandomZone()
{
    if (_zoneBounds == null) return;

    GameObject prefab = Random.value > 0.5f ? _speedUpZonePrefab : _slowDownZonePrefab;
    if (prefab == null) return;

    Bounds b = _zoneBounds.bounds;
    Vector2 pos = new Vector2(
        Random.Range(b.min.x, b.max.x),
        Random.Range(b.min.y, b.max.y));

    var zone = Object.Instantiate(prefab, pos, Quaternion.identity);
    _activeZoneCount++;
    // Zone 자동 소멸 후 카운트 감소: Zone 스크립트에서 콜백 또는 코루틴으로 처리
    StartCoroutine(DestroyZoneAfter(zone, _zoneDuration));
}

private System.Collections.IEnumerator DestroyZoneAfter(GameObject zone, float duration)
{
    yield return new WaitForSeconds(duration);
    if (zone != null)
    {
        Object.Destroy(zone);
        _activeZoneCount--;
    }
}
```

### Pattern 5: Zone 스크립트 (Zone 프리팹이 없을 경우 신규 작성)

```csharp
// SpeedUpZone.cs — Phase 4
using UnityEngine;

namespace WaterMonster.Phase4
{
    public class SpeedUpZone : MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 1.5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // REQ-WM-X-01: Player 레이어에만 적용
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null) ApplyBuff(pc);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null) RemoveBuff(pc);
        }

        private void ApplyBuff(PlayerController pc) { /* defaultSpeed *= speedMultiplier */ }
        private void RemoveBuff(PlayerController pc) { /* 원래 값 복원 */ }
    }
}
```

**중요 제약:** `PlayerController`의 이동 속도는 `defaultSpeed`와 `runSpeed` 두 개의 public float 필드로 관리된다. Zone이 `defaultSpeed`를 직접 곱하거나 더하면 Zone이 중복 적용될 때 값이 누적되는 문제가 생긴다. 권장 방식: Zone 진입/퇴장 카운터를 PlayerController에 추가하거나, Zone이 원래 값을 기억하고 Exit 시 복원하도록 구현. 또는 `PlayerController`에 `speedModifier float` 필드를 추가해 multiplier로 최종 속도에 적용.

### Pattern 6: 광폭화 tick 소모 (WaterMonsterStats)

`BossStatsSystem.Update()`는 `IsBarrierActive` 가드로 WaterMonster에서 자동 스킵된다. `WaterMonsterStats`는 별도 `Update`를 오버라이드해 추가하면 된다.

```csharp
// WaterMonsterStats.cs에 추가
[Header("Enrage Tick")]
[SerializeField] private float enrageTickInterval = 1.5f;
[SerializeField] private float enrageTickAmount = 5f;

private bool _isEnraged = false;
private float _lastTickTime = 0f;

public void SetEnraged(bool value) { _isEnraged = value; }

// BossStatsSystem.Update()는 IsBarrierActive == false이므로 아무것도 하지 않음
// WaterMonsterStats가 자체 Update를 갖거나 아래를 추가
protected virtual void Update()
{
    // base.Update()는 IsBarrierActive 가드로 아무것도 하지 않음
    if (!_isEnraged) return;
    if (Time.time - _lastTickTime < enrageTickInterval) return;
    _lastTickTime = Time.time;
    SpendHpCost(enrageTickAmount);
}
```

**주의:** `BossStatsSystem`의 `Update()`는 `void Update()` (non-virtual, non-protected)다. `WaterMonsterStats`에서 `Update`를 새로 정의하면 `new` 키워드를 써야 하거나 숨김(hiding)이 발생한다. 가장 안전한 방법: `WaterMonsterStats.Update()`를 `new void Update()`로 작성하거나, `BossStatsSystem.Update()`를 `protected virtual void Update()`로 변경(한 줄 수정). 플래너는 둘 중 하나를 선택해야 한다.

### Anti-Patterns to Avoid

- **`_decisionTimer`를 직접 외부에서 건드리는 것:** `CombatState`의 `_decisionTimer`는 private이다. 외부에서 직접 수정 불가. 접근을 위해 protected로 승격하거나 Execute 오버라이드가 필요하다.
- **Zone 속도 값을 직접 누적 변경:** `PlayerController.defaultSpeed`에 배율을 직접 곱하면 Zone 중복 적용 시 값이 누적된다. 원본 값 저장 후 복원 패턴을 사용.
- **광폭화 tick 소모로 인한 OnDamageTaken 발화:** `SpendHpCost`는 `OnDamageTaken`을 발화하지 않으므로 `CheckEnrageTrigger`가 tick 소모로 HP 감소 시 호출되지 않는다. Update 기반 폴백 체크가 필요하다.
- **Zone 개수 무제한 생성:** 장판 쿨다운 외에 동시 활성 Zone 개수 상한(`_maxActiveZones`)이 없으면 맵이 Zone으로 가득 찰 수 있다.
- **SetEnraged를 WaterMonsterStats와 WaterMonsterCombatState 두 곳에 별개로 구현:** 플래그가 동기화되지 않을 위험. 컨트롤러가 두 곳을 모두 호출해야 함을 명시.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Zone 생성 위치 범위 | 별도 ZoneArea 컴포넌트 | WeatherController의 `BoxCollider2D spawnBounds` 패턴 재사용 | 이미 같은 목적으로 PuddleSpawner에 확립된 패턴 |
| Zone 자동 소멸 타이머 | 별도 ZoneLifetime 컴포넌트 | WaterMonsterController.StartCoroutine + DestroyAfter | MonoBehaviour 코루틴으로 충분 |
| Player 레이어 필터 | 태그/이름 비교 | `LayerMask.NameToLayer("Player")` | 이미 WaterMeleeSwipe, PuddleExplosionController에서 동일 방식 사용 |
| 쿨다운 타이머 | 카운트다운 클래스 | `float _lastXTime = -999f; Time.time - _lastXTime >= cooldown` | 모든 Phase에서 확립된 패턴 |

---

## Claude's Discretion — Research Findings

### 장판 생성 위치 범위 계산 방식

**결론: BoxCollider2D bounds 기반 (Collider bound) 권장.**

`PuddleSpawner.cs`에서 동일한 패턴이 이미 구현되어 있다:
```csharp
private Vector2 GetRandomPosition()
{
    Bounds bounds = spawnBounds.bounds;
    float x = Random.Range(bounds.min.x, bounds.max.x);
    float y = Random.Range(bounds.min.y, bounds.max.y);
    return new Vector2(x, y);
}
```
`WeatherController`와 `PuddleSpawner`가 동일한 `BoxCollider2D`를 참조하고 있으며, Zone도 같은 `spawnBounds`를 사용하면 Inspector에서 범위 하나만 관리하면 된다. Camera bound 기반은 카메라가 이동할 경우 범위가 달라지는 문제가 있으므로 적합하지 않다.

### 기존 Zone 프리팹의 이속 적용 인터페이스

**결론: 기존 Zone 프리팹/스크립트가 존재하지 않는다.**

`Assets` 전체를 탐색한 결과 `SpeedUpZone`, `SlowDownZone`, `Zone`을 포함한 파일이 전혀 없다. CONTEXT.md D-06의 "기존에 외부에서 만든 Zone 오브젝트"는 아직 존재하지 않는 것으로 보인다. 플래너는 Zone 스크립트를 Phase 4 Wave 0에서 신규 작성하는 태스크를 포함해야 한다.

**Zone 스크립트 설계 시 PlayerController 속도 수정 접근 방식:**

`PlayerController.cs`는 다음 이동 속도 필드를 가진다:
- `public float defaultSpeed = 3f`
- `public float runSpeed = 7f`

이동은 `HandleGroundMovement()`에서 `rigid.linearVelocity = new Vector2(moveInput.x * maxSpeed, ...)` 방식이다. Zone이 속도를 수정하는 안전한 방법:

**옵션 A (권장):** `PlayerController`에 `public float speedModifier = 1f` 필드를 추가하고, `maxSpeed` 계산에 곱함. Zone이 이 값만 수정.
```csharp
maxSpeed = (isRunning ? runSpeed : defaultSpeed) * speedModifier;
```

**옵션 B:** Zone이 원본 `defaultSpeed`/`runSpeed` 값을 캐시해 Exit 시 복원. Zone 중복 적용 시 문제 발생 가능.

**옵션 A가 더 안전하다.** PlayerController에 한 줄 필드 추가가 필요하다.

### 광폭화 쿨타임 배율 기본값

**결론: `enrageCooldownMultiplier = 0.5f` (기본 쿨다운의 50%) 권장.**

현재 공격 쿨다운:
- `WaterMeleeSwipe.Cooldown = 1.4f` → 광폭화 후 `0.7f`
- `WaterRangedSpit.Cooldown = 2.0f` → 광폭화 후 `1.0f`

0.5배는 공격 빈도를 두 배로 만들어 긴장감 있는 마무리를 표현하면서, 플레이어가 반응 가능한 범위를 유지한다. Inspector에서 튜닝 가능하므로 기본값은 참고용.

### 장판 개수 상한선

**결론: `_maxActiveZones = 4` 권장.**

맵 범위 내에 동시에 4개의 Zone이 존재하면 맵 공간의 상당 부분이 덮이며 플레이어 압박이 충분하다. Zone 지속 시간 8초 + 쿨다운 5초 기준으로 한 사이클당 최대 1~2개가 추가되는 속도이므로, 4개 상한에서 자연스럽게 순환된다. Inspector에서 튜닝 가능.

### SetEnraged 호출 시 WaterMonsterCombatState 참조 방법

**결론: `CurrentState is WaterMonsterCombatState combatState` 패턴 권장.**

```csharp
if (CurrentState is WaterMonsterCombatState combatState)
{
    combatState.SetEnraged(true);
}
```

`WaterMonsterController.Update()`에서 이미 `CurrentState.GetType() == typeof(CombatState)`로 State 스왑을 하고 있으므로, `CurrentState`가 `WaterMonsterCombatState` 타입임이 보장된다. 별도 필드 캐싱 불필요.

단, 광폭화 트리거 시점에 boss가 `WaterTeleportState`에 있을 수 있다. 이 경우 `is` 패턴 캐스팅이 실패한다. 보완책: `_enrageTriggered` 플래그가 true가 된 후 `WaterMonsterCombatState.Enter()`에서 `_isEnraged`를 확인하도록 Controller에서 플래그를 전달하거나, `WaterMonsterController`에 `bool IsEnraged { get; private set; }` 프로퍼티를 두고 CombatState.Enter에서 읽어오는 방법을 사용.

**권장 구현:** `WaterMonsterController`에 `public bool IsEnraged { get; private set; }` 프로퍼티 추가. `WaterMonsterCombatState.Enter(boss)`에서 `if (boss is WaterMonsterController wmc) _isEnraged = wmc.IsEnraged;` 로 초기화.

---

## Common Pitfalls

### Pitfall 1: tick 소모로 HP 임계치 도달 시 광폭화 트리거 누락

**What goes wrong:** `CheckEnrageTrigger`는 `OnDamageTaken` 이벤트로 호출된다. `SpendHpCost`는 `OnDamageTaken`을 발화하지 않으므로, tick 소모로 HP가 임계치 이하로 떨어져도 광폭화가 트리거되지 않는다.
**Why it happens:** `WaterMonsterStats.TakeDamage`에서만 `InvokeOnDamageTaken()`이 호출되며 `SpendHpCost`는 단순 HP 감소만 한다.
**How to avoid:** `CheckEnrageTrigger`를 `WaterMonsterController.Update()`에서 매 프레임 또는 일정 주기로 폴링하거나, `SpendHpCost` 호출 후 별도로 임계치 체크를 트리거.
**Warning signs:** 플레이어가 보스를 공격하지 않고 기다릴 때 HP가 임계치 이하로 떨어져도 광폭화 이펙트가 없음.

### Pitfall 2: BossStatsSystem.Update() 와 WaterMonsterStats.Update() 중복/충돌

**What goes wrong:** `BossStatsSystem.Update()`는 `protected` 또는 `virtual`이 아닌 `void Update()`다. `WaterMonsterStats`에서 `void Update()`를 정의하면 기반 클래스의 Update를 숨김(hiding)하지 override하지 않는다. Unity는 두 Update를 모두 호출할 수 있어 예상치 못한 동작이 생길 수 있다.
**Why it happens:** Unity의 MonoBehaviour Update는 reflection 기반 호출이므로 상속 체계와 무관하게 두 클래스 모두 Update가 호출된다.
**How to avoid:** `BossStatsSystem.Update()`를 `protected virtual void Update()`로 변경하고 `WaterMonsterStats`에서 `protected override void Update()` 사용. 또는 별도 tick 타이머를 다른 메서드에서 호출.
**Warning signs:** tick 소모가 두 배로 적용되거나 BossStatsSystem Update 로직이 갑자기 작동.

### Pitfall 3: Zone 중복 적용 시 이속 값 누적

**What goes wrong:** Zone이 플레이어의 `defaultSpeed`를 직접 `*= 1.5f`로 수정하면, SpeedUp Zone 두 개에 동시에 들어가면 2.25배가 된다. Zone 퇴장 후 복원 로직이 없으면 영구 변경.
**Why it happens:** 원본 값 저장 없이 배율 직접 적용.
**How to avoid:** `PlayerController`에 `speedModifier` 필드를 추가해 Zone이 multiplier만 수정. 또는 Zone이 원본 값을 캐시해 Exit 시 복원.

### Pitfall 4: WaterMonsterCombatState._decisionTimer가 private

**What goes wrong:** 쿨다운 배율을 적용하려면 `_decisionTimer`를 수정해야 하나, `CombatState._decisionTimer`가 `private`이다.
**How to avoid:** `CombatState._decisionTimer`를 `protected`로 변경(한 줄 수정) 후 `WaterMonsterCombatState.Execute`를 오버라이드해 base 호출 후 배율 재적용. 또는 공격 전략 클래스의 Cooldown에 배율 주입.

### Pitfall 5: Zone 개수 무제한 + 코루틴 누출

**What goes wrong:** `_activeZoneCount`를 추적하지 않으면 장판이 무제한으로 쌓인다. 또한 Zone GameObject가 외부(다른 코드)에 의해 Destroy될 경우 `DestroyZoneAfter` 코루틴이 null을 Destroy하려 해 오류.
**How to avoid:** `_maxActiveZones` 상한 체크. 코루틴 내 `if (zone != null)` 검사 필수.

---

## Code Examples

### 광폭화 트리거 (Phase 2 패턴 재사용)

```csharp
// Source: WaterMonsterController.cs (기존 CheckPhase2Trigger 패턴)
[Header("Phase 4 Settings")]
[SerializeField] [Range(0f, 1f)] private float _enrageHpThreshold = 0.30f;
public bool IsEnraged { get; private set; } = false;
private bool _enrageTriggered = false;

private void CheckEnrageTrigger()
{
    if (_enrageTriggered) return;
    if (WaterStats.CurrentHealth / WaterStats.MaxHealth <= _enrageHpThreshold)
    {
        _enrageTriggered = true;
        IsEnraged = true;
        if (CurrentState is WaterMonsterCombatState cs)
            cs.SetEnraged(true);
    }
}
```

### 장판 쿨다운 (Phase 3 텔레포트 패턴 재사용)

```csharp
// Source: WaterMonsterController.cs (기존 CanTeleport/RecordTeleportTime 패턴)
[SerializeField] private float _zoneCooldown = 5f;
private float _lastZoneTime = -999f;

public bool CanSpawnZone() => Time.time - _lastZoneTime >= _zoneCooldown
                              && _activeZoneCount < _maxActiveZones;
public void RecordZoneTime() => _lastZoneTime = Time.time;
```

### Zone Trigger (REQ-WM-X-01 준수)

```csharp
// Source: 신규 작성 (WaterMeleeSwipe의 LayerMask.GetMask 패턴 참조)
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
    // 속도 버프/디버프 적용
}
```

### SelectAttackStrategy 장판 후보 통합

```csharp
// Source: WaterMonsterCombatState.cs (Phase 3 텔레포트 통합 D-12 패턴 재사용)
protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
{
    if (_isEnraged && boss is WaterMonsterController wmc && wmc.CanSpawnZone())
    {
        wmc.SpawnRandomZone();
        wmc.RecordZoneTime();
        _decisionTimer = 1.0f; // 장판 생성 후 짧은 대기 (protected 필요)
        return null;
    }

    // ... 기존 텔레포트/근접/원거리 분기
}
```

---

## State of the Art

| 기존 접근 | Phase 4 접근 | 비고 |
|-----------|------------|------|
| Phase 2: 별도 WeatherController 신규 클래스 | Phase 4: 기존 CombatState 플래그로 처리 | 별도 State 불필요 |
| Phase 3: IBossState 신규 WaterTeleportState | Phase 4: SelectAttackStrategy 분기 내 inline | D-01 결정 |
| PuddleSpawner: MonoBehaviour 컴포넌트 | Phase 4: WaterMonsterController에 직접 메서드 | D-05 결정 |

---

## Open Questions

1. **Zone 프리팹 존재 여부**
   - What we know: 코드베이스 전체 탐색 결과 Zone 관련 파일이 없음
   - What's unclear: "외부에서 만든 Zone"이 아직 미구현인지, 에셋 파일로만 존재하는지
   - Recommendation: Wave 0에서 Zone 스크립트를 신규 작성하는 태스크를 포함. Inspector 참조 방식(D-06)은 유지하되 스크립트를 직접 작성.

2. **BossStatsSystem.Update() hiding vs override**
   - What we know: `BossStatsSystem.Update()`가 non-virtual `void Update()`
   - What's unclear: Unity의 MonoBehaviour Update 호출 방식상 두 Update가 모두 호출되는지
   - Recommendation: `BossStatsSystem.Update()`를 `protected virtual void Update()`로 변경하는 1줄 수정이 가장 안전. 대안으로 tick 로직을 별도 메서드로 분리해 Controller에서 호출.

3. **CombatState._decisionTimer 접근성**
   - What we know: `private float _decisionTimer`
   - What's unclear: 플래너가 옵션 A/B/C 중 어떤 방식으로 쿨다운 배율을 구현할지
   - Recommendation: `CombatState._decisionTimer`를 `protected`로 변경(옵션 B). NewBoss 코드는 Phase 1에서 이미 `TakeDamage`를 virtual로 변경한 선례가 있어 1줄 수정이 허용 범위.

4. **광폭화 진입 시 현재 State가 WaterTeleportState인 경우**
   - What we know: 광폭화 트리거 시점에 보스가 WaterTeleportState에 있을 수 있음
   - Recommendation: `WaterMonsterController.IsEnraged` 프로퍼티 추가. `WaterMonsterCombatState.Enter()`에서 `wmc.IsEnraged`를 읽어 `_isEnraged` 초기화. 이렇게 하면 어떤 State에서 광폭화가 트리거돼도 다음 CombatState 진입 시 자동 반영.

---

## Environment Availability

Step 2.6: SKIPPED (순수 C# 코드 추가/수정 — 외부 도구, 런타임, CLI 의존성 없음)

---

## Sources

### Primary (HIGH confidence — 직접 코드 확인)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — CheckPhase2Trigger 패턴, CanTeleport/RecordTeleportTime 패턴
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — SpendHpCost, CurrentHealth 프로퍼티, TakeDamage 오버라이드
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — SelectAttackStrategy 오버라이드 현황
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — _decisionTimer private, Execute 로직, Cooldown 적용 지점
- `Assets/Enemy/NewBoss/Script/BossStatesSystem.cs` — Update() non-virtual 확인
- `Assets/Enemy/NewBoss/Script/BossController.cs` — ChangeState, CurrentState, StartCoroutine 가능 여부
- `Assets/Enemy/WaterMonster/Script/Phase2/PuddleSpawner.cs` — BoxCollider2D bounds 랜덤 위치 패턴
- `Assets/Enemy/WaterMonster/Script/Phase3/WaterTeleportState.cs` — IBossState 코루틴 패턴, HP 코스트 적용
- `Assets/Enemy/WaterMonster/Script/Phase3/PuddleExplosionController.cs` — LayerMask.GetMask("Player") 패턴
- `Assets/Player/Script/PlayerController.cs` — defaultSpeed, runSpeed 필드 구조 확인
- `Assets/Player/Script/PlayerStats.cs` — PlayerStats : HP 상속 구조

### Secondary (MEDIUM confidence)
- Assets 전체 파일 탐색 결과: Zone 관련 파일 없음 확인 (find 명령 결과)

---

## Metadata

**Confidence breakdown:**
- Architecture: HIGH — 기존 Phase 1~3 코드 직접 확인, 재사용 패턴 명확
- Integration points: HIGH — 각 파일의 실제 코드 구조 확인 완료
- Zone 스크립트: MEDIUM — Zone이 없어 신규 작성 필요하나 PlayerController 구조는 확인됨
- Pitfalls: HIGH — 실제 코드의 접근 제한자, 이벤트 발화 조건 직접 확인

**Research date:** 2026-04-16
**Valid until:** 2026-05-16 (Unity 버전 변경 없을 경우)
