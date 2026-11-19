# Phase 6: 스테이지 전환 및 스테이지 2 은신·분신 시스템 - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning

<domain>
## Phase Boundary

HP가 50% 이하로 최초 도달하면 스테이지 2로 1회 전환한다.
스테이지 2는 고정 사이클 구조로 동작한다:
1. **분신 생성** — 진짜 보스 1 + 분신 2 = 총 3개 동시 존재, 순간 무적
2. **일반 패턴 단계** — 진짜/분신 각자 S1 거리 기반 패턴 독립 실행 (N회, 플래너 재량)
3. **헤비콤보 단계** — 전체 동시 은신(콜라이더 off → 대기 → 링 범위 텔레포트) + 돌진
4. **그로기 전환 + 분신 삭제** — 돌진 종료 후 그로기 상태, 분신 전체 Destroy
5. 그로기 해제 → 사이클 1번부터 반복

**범위 외:**
- 애니메이션·시각 이펙트 연동 (v3.0+)
- HP 50% 이후 추가 전환 (1회로 고정)
- 스테이지 전환 연출 (컷씬, 화면 효과)

</domain>

<decisions>
## Implementation Decisions

### D-01: HP 50% 체크 위치
- **D-01a:** `SpiritStats.TakeDamage` 내부에서 `_currentHealth <= MaxHealth * 0.5f` 조건 체크.
- **D-01b:** 최초 1회만 발동 — `_stage2Triggered` bool 가드 사용 (`SpiritStats` 또는 `SpiritController`에 위치, 플래너 재량).
- **D-01c:** 50% 도달 시 `SpiritController.OnStage2Trigger()` 콜백 호출 (또는 이벤트 발행).

### D-02: 스테이지 2 상태 관리
- **D-02a:** `Stage2CombatState : SpiritCombatState` 신규 클래스 작성.
- **D-02b:** S2 진입 시 `boss.ChangeState(new Stage2CombatState())` 호출.
- **D-02c:** `SpiritController.Update()` 인터셉트에 Stage 2 복구 후 `GroggyState` → `Stage2CombatState` 재진입 로직 포함 (Phase 5의 `CombatState → SpiritCombatState` 인터셉트 패턴 연장).

### D-03: 스테이지 2 진입 즉시 실행
- **D-03a:** `Stage2CombatState.Enter()` 에서 `DummyPrefab` 2개 즉시 `Instantiate`.
- **D-03b:** 진입 직후 순간 무적 — 별도 대기 없이 `SpiritStats.IsInvincible` (또는 콜라이더 off) 로 1프레임 수준 처리. 구체적 구현은 플래너 재량.

### D-04: 은신 패턴 구조 (S2-02)
- **D-04a:** 실행 순서: 콜라이더 off → `StealthDuration` 초 대기 → 텔레포트 → 콜라이더 on.
- **D-04b:** `StealthDuration [SerializeField]` Inspector 노출.
- **D-04c:** 은신 중 보스 피격 불가 (콜라이더 비활성화).

### D-05: 은신 텔레포트 목적지 (플레이어 중심 고리 범위)
- **D-05a:** 플레이어 위치 중심, 반지름 `[MinTeleportRadius, MaxTeleportRadius]` 범위 내 랜덤 위치로 텔레포트.
- **D-05b:** `MinTeleportRadius` / `MaxTeleportRadius` [SerializeField] Inspector 노출.
- **D-05c:** 샘플링: `Random.insideUnitCircle` × MaxRadius 시도 후 거리가 [Min, Max] 범위 안에 들어올 때까지 재시도 (또는 극좌표 샘플링).

### D-06: 분신 생성 및 스폰
- **D-06a:** `SpiritController`에 `DummyPrefab [SerializeField]` 노출.
- **D-06b:** 스폰 방식: `Instantiate(DummyPrefab, ...)`. 오브젝트 풀 불필요 (빈도 낮음).
- **D-06c:** 분신 스폰 시 `SpiritStats.IsDummy = true` 설정.

### D-07: 분신 데미지 분기 (S2-05)
- **D-07a:** `SpiritStats.TakeDamage` 에서 `IsDummy == true` 면 데미지 0으로 처리 (기존 `IsDummy` 플래그 활용).
- **D-07b:** `SpiritController.IsDummy` 프로퍼티는 Phase 5에서 이미 구현됨 — 재사용.

### D-08: 분신 소멸 조건
- **D-08a:** 헤비콤보(은신+돌진) 단계 완료 후 그로기 전환 시 분신 전체 Destroy.
- **D-08b:** `Stage2CombatState`가 보유한 분신 리스트를 순회하며 `Destroy(clone.gameObject)`.
- **D-08c:** 그로기 해제 후 사이클 재진입 시 분신 재스폰.

### D-09: 분신 동기화 — 헤비콤보 명령 전달
- **D-09a:** `Stage2CombatState`가 분신 `SpiritController` 리스트 보유.
- **D-09b:** 헤비콤보 단계 진입 시 각 분신에게 `TriggerHeavyCombo()` (또는 동등한 메서드) 호출.
- **D-09c:** 분신 AI는 명령 수신 시 자체 은신+돌진 코루틴 실행.

### D-10: 스테이지 2 일반 패턴 선택
- **D-10a:** `Stage2CombatState.SelectAttackStrategy` 일반 단계에서 `SpiritCombatState`와 동일한 거리 기반 로직 재사용 (별도 구현 불필요).

### Claude's Discretion
- 패턴 카운터 구조 (일반 단계 최대 N회 추적 방식): 플래너 재량
- 그로기 해제 후 Stage2CombatState 재진입 시 구체적 인터셉트 구현
- 헤비콤보 단계 진입 조건 (카운터 >= N, 타이머 기반 등): 플래너 재량
- 분신 스폰 정확한 위치 (보스 주변 반경 등): 플래너 재량
- 순간 무적의 구체적 구현 (콜라이더 off vs IsInvincible 플래그): 플래너 재량
- 그로기 복구 시간 (`GroggyState._recoveryTime`): 기본값 5f 유지 또는 조정 가능

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase 6 구현 대상 (수정 필요 파일)
- `Assets/Enemy/WaterSpirit/Script/SpiritController.cs` — isDummy 프로퍼티 존재, Update() 인터셉트 구조 (Stage 2 재진입 로직 추가 필요), DummyPrefab 필드 추가
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs` — TakeDamage() 에 HP 50% 체크 + IsDummy 데미지 0 분기 추가
- `Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs` — Stage2CombatState 상속 대상, SelectAttackStrategy 재사용

### 기반 클래스 (상속/참고)
- `Assets/Enemy/NewBoss/Script/BossController.cs` — ChangeState(), MoveTo(), StopMove(), LookAtTarget(), StartHeavyAttackCooldown()
- `Assets/Enemy/NewBoss/Script/BossStatsSystem.cs` — HP/_currentHealth, TakeDamage, Die(), InvokeOnDamageTaken()
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` — _decisionTimer, _isAttacking, ShouldTransitionToGroggy() virtual
- `Assets/Enemy/NewBoss/Script/States/GroggyState.cs` — _recoveryTime=5f, Execute()에서 new CombatState()로 복구 (Stage 2 인터셉트 필요)
- `Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs` — 은신/헤비콤보 전략 구현 인터페이스

### 레퍼런스 구현 (패턴 참고)
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritCharge.cs` — 돌진 전략 (헤비콤보의 돌진 단계 참고)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` — Update() 인터셉트로 CombatState 교체 패턴

### 프로젝트 요구사항
- `.planning/REQUIREMENTS.md` — CORE-03, S2-01, S2-02, S2-03, S2-04, S2-05 상세 조건
- `.planning/ROADMAP.md` §Phase 6 — 성공 기준 5개 항목

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SpiritController.IsDummy` 프로퍼티 — Phase 5에서 이미 구현됨, 분신 분기에 즉시 사용 가능
- `SpiritCombatState.SelectAttackStrategy` — Stage2의 일반 패턴 단계에서 그대로 재사용
- `GroggyState` — 5초 복구 후 CombatState 전환. SpiritController 인터셉트로 Stage2CombatState 재진입 처리
- `BossController.ChangeState()` — 상태 전환 API 그대로 사용
- `SpiritCharge` — 헤비콤보의 돌진 단계 구현 참고

### Established Patterns
- 상태 교체 인터셉트: `SpiritController.Update()` 에서 `typeof(CombatState)` 체크 → 서브클래스 교체. Stage 2 재진입도 동일 패턴으로 확장
- HP 이벤트: `InvokeOnDamageTaken()` 후 조건 체크 패턴 (SpiritStats.TakeDamage에서 이미 사용)
- Inspector 노출: `[SerializeField] [Range]` 적극 활용 (Phase 5와 일관성 유지)
- Unity 6 Physics 2D: `_rb.linearVelocity` 사용

### Integration Points
- `TakeDmg.cs` (Assets/Script/) → `BossStatsSystem.TakeDamageInfo(DamageInfo)` → `SpiritStats.TakeDamage` — HP 50% 체크 여기서 발생
- 분신 스폰: `DummyPrefab`은 `SpiritController + SpiritStats` 컴포넌트 포함 프리팹이어야 함
- 씬 InGame.unity — 기존 보스 배치에 DummyPrefab Inspector 연결

</code_context>

<specifics>
## Specific Ideas

- 고리형 텔레포트 위치 샘플링: `Random.insideUnitCircle` 정규화 후 `MinTeleportRadius + Random.Range(0, MaxTeleportRadius - MinTeleportRadius)` 를 곱해 정확한 환형 분포 구현 가능
- `Stage2CombatState` 가 클론 리스트를 `List<SpiritController>` 로 보유, null 체크 후 순회
- `GroggyState` 복구 시 `new CombatState()` 를 반환하는데, `SpiritController.Update()` 인터셉트에서 `_isStage2 && CurrentState.GetType() == typeof(CombatState)` → `ChangeState(new Stage2CombatState())` 로 잡아줄 수 있음
- 분신은 DummyPrefab으로 Instantiate 후 `GetComponent<SpiritStats>().IsDummy = true` 설정

</specifics>

<deferred>
## Deferred Ideas

- 애니메이션·이펙트 — v3.0+
- 은신 시각 효과 (투명화 등) — v3.0+
- 스테이지 전환 연출 (컷씬, 화면 효과) — v3.0+
- 분신과 진짜 보스 시각 구별 방법 — v3.0+ (현재는 로직으로만 구분)

</deferred>

---

*Phase: 06-spirit-boss-stage2-system*
*Context gathered: 2026-04-30*
