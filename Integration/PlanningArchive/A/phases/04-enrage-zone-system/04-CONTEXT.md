# Phase 4: 광폭화 및 장판 시스템 - Context

**Gathered:** 2026-04-16
**Status:** Ready for planning

<domain>
## Phase Boundary

보스 HP가 M% 이하로 떨어지면 광폭화 모드 진입 — 공격 쿨타임 감소, 이속/감속 장판 주기적 생성, HP tick 자가 소모 증가. 기존 `WaterMonsterCombatState` 내부 플래그로 광폭화를 표현하며 새 State 클래스는 만들지 않는다.

**이번 페이즈 포함하지 않음:** 새 공격 패턴 추가, 연쇄 폭발 강화, 텔레포트 패턴 변경.

</domain>

<decisions>
## Implementation Decisions

### 광폭화 State 구조
- **D-01:** 광폭화는 별도 State 클래스 없이 `WaterMonsterCombatState`에 `_isEnraged bool` 플래그로 구현. 플래그가 true일 때 SelectAttackStrategy 내 쿨다운 배율 및 장판 생성 후보가 활성화됨.
- **D-02:** 광폭화 활성화는 `WaterMonsterController`에서 HP 임계치(`_enrageHpThreshold`) 감지 → `WaterMonsterCombatState.SetEnraged(true)` 호출. Phase 2의 `CheckPhase2Trigger` 패턴을 동일하게 재사용.
- **D-03:** 광폭화 쿨타임 배율(`enrageCooldownMultiplier`)은 Inspector 튜닝 위임. `WaterMonsterCombatState`에 [SerializeField] 필드로 노출.
- **D-04:** 광폭화 시 장판 생성을 **SelectAttackStrategy 후보 중 하나**로 통합 — Phase 3 텔레포트 통합 패턴(`D-12`)과 동일한 구조. 장판 쿨다운이 지났을 때 후보에 포함.

### 장판(Zone) 생성 구조
- **D-05:** 장판 생성 주체는 `WaterMonsterController`. 별도 ZoneSpawner 컴포넌트 없음.
- **D-06:** 기존에 외부에서 만든 Zone 오브젝트(프리팹)를 Inspector에서 직접 참조할 수 있도록 필드 노출:
  - `[SerializeField] private GameObject _speedUpZonePrefab`
  - `[SerializeField] private GameObject _slowDownZonePrefab`
  - SpeedUp/SlowDown 구분은 프리팹 변형으로만 — 코드 로직(이속 효과)은 이미 기존 Zone에 구현되어 있다고 가정.
- **D-07:** 장판은 일정 시간 후 **자동 비활성화** (`zoneDuration` Inspector 튜닝). Instantiate 후 타이머로 Destroy or SetActive(false).
- **D-08:** 장판 생성 위치는 맵 랜덤 위치 (Inspector에서 범위 튜닝 가능). Player 레이어에만 영향 (REQ-WM-X-01 준수).

### HP 코스트 가속
- **D-09:** 광폭화 시 **별도 주기적 tick HP 소모** 추가 (기존 공격 코스트와 별개로 누적). 긴장감 있는 마무리를 표현.
- **D-10:** tick 소모 위치: `WaterMonsterStats.Update` — `_isEnraged` 플래그 기반으로 처리. `SpendHpCost` 기존 메서드 재사용.
- **D-11:** tick 수치(`enrageTickInterval`, `enrageTickAmount`)는 Inspector 튜닝 위임.
- **D-12:** 광폭화 진입 임계치 M% → `_enrageHpThreshold` 필드로 Inspector 노출. Phase 2의 `_phase2HpThreshold` 패턴과 동일.

### Claude's Discretion
다음 항목은 리서처/플래너가 코드 확인 후 결정:
- 장판 생성 위치 랜덤 범위 계산 방식 (Camera bound 기반 or Collider bound 기반)
- 기존 Zone 프리팹의 이속 적용 인터페이스 확인 (OnTriggerEnter/Exit 여부, Player 레이어 필터 구현 여부)
- 광폭화 쿨타임 배율 기본값 (Inspector 튜닝 위임이지만 합리적 기본값 제안)
- 장판 개수 상한선 (동시에 몇 개까지 맵에 존재 가능한지)
- `SetEnraged` 호출 시 `WaterMonsterCombatState` 참조 방법 (`CurrentState as WaterMonsterCombatState` 캐스팅 또는 Controller 필드)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 프로젝트 결정사항
- `.planning/PROJECT.md` — 상속 기반 확장 원칙, 레이어 분리 원칙
- `.planning/REQUIREMENTS.md` — REQ-WM-P4-01, REQ-WM-P4-02, REQ-WM-P4-03, REQ-WM-02, REQ-WM-X-01
- `.planning/ROADMAP.md` §`Phase 4` — Goal, Success Criteria (특히 SC-5: HP 자가 소모 유지 + 소모 속도 증가)

### Phase 1~3 결정사항 (이어받음)
- `.planning/phases/01-boss-core-mechanics/01-CONTEXT.md` — SpendHpCost 패턴, DamageElement 구조
- `.planning/phases/02-weather-puddle-interaction/02-CONTEXT.md` — CheckPhase2Trigger HP 임계치 패턴 (Phase 4 광폭화 트리거가 동일 패턴 재사용)
- `.planning/phases/03-explosion-gimmick-teleport/03-CONTEXT.md` — SelectAttackStrategy 후보 통합 패턴 (D-12/D-13), CombatState 쿨다운 관리 패턴

### Phase 2~3 구현 코드 (직접 연결 대상)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — CheckPhase2Trigger 구조, _phase2HpThreshold 패턴 (Phase 4 트리거 추가 위치)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` — SpendHpCost, Update 추가 위치 (D-10 tick 소모)
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs` — SelectAttackStrategy 오버라이드 (D-04 장판 생성 후보 추가 위치)

### 베이스 구조
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — _attackCooldown 필드, 쿨다운 로직 (D-03 배율 적용 대상)
- `Assets/Enemy/NewBoss/Script/BossStatsSystem.cs` — Update 패턴 확인 (D-10 tick 추가 시 충돌 여부)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `WaterMonsterStats.SpendHpCost(float)` — tick 소모에 직접 재사용 (D-10)
- `WaterMonsterController.CheckPhase2Trigger` — 광폭화 임계치 감지에 동일 패턴 재사용 (D-02)
- `WaterMonsterCombatState.SelectAttackStrategy` — 장판 생성 후보 추가 위치 (D-04)
- `WaterMonsterController.CanTeleport` / `RecordTeleportTime` — 장판 쿨다운 관리에 동일 패턴 재사용

### Established Patterns
- HP 임계치 트리거: `_phase2HpThreshold` 인스펙터 필드 + `bool _phaseXTriggered` 가드 패턴
- 쿨다운 관리: Controller에 float `_lastXTime` + `float xCooldown` [SerializeField] 패턴
- 공격 후보 선택: `SelectAttackStrategy(BossController boss, float dist)` 조건부 분기

### Integration Points
- `WaterMonsterController` → 광폭화 임계치 감지 + 장판 프리팹 참조 + 장판 생성 메서드 추가
- `WaterMonsterStats.Update` → 광폭화 tick 소모 추가
- `WaterMonsterCombatState.SelectAttackStrategy` → 장판 생성 후보 + 쿨다운 배율 분기 추가

</code_context>

<specifics>
## Specific Ideas

- 기존에 다른 팀원이 만든 Zone 프리팹(SpeedUpZone, SlowDownZone)을 코드 수정 없이 Inspector 참조로만 연결. 새 Zone 스크립트 작성 불필요.
- 광폭화 tick 소모는 공격 코스트와 별개 — 보스가 공격을 아끼더라도 시간이 지나면 HP가 줄어 자연스러운 타임 리밋 역할.

</specifics>

<deferred>
## Deferred Ideas

- 광폭화에서 텔레포트 후 즉시 AoE 발사 연계 (Phase 3 deferred에서 이어짐) — 밸런싱 단계
- 폭발 강화 (n번째 폭발이 더 강함) — 밸런싱 단계
- SpeedUp/SlowDown Zone의 시각적 구분 강화 (입자 이펙트 추가 등)

</deferred>

---

*Phase: 04-enrage-zone-system*
*Context gathered: 2026-04-16 via /gsd:discuss-phase*
