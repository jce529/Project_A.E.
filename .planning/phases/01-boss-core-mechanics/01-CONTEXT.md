# Phase 1: 보스 기본 엔티티 및 코어 메커니즘 - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning

<domain>
## Phase Boundary

씬에 물괴물 보스(`WaterMonsterController : BossController`)를 배치하고, 다음 코어 메커니즘이 동작하도록 한다:
1. **물 속성 힐링 필터** — 플레이어가 Water 속성 스킬로 타격 시 보스 HP 회복
2. **HP 코스트 공격** — 보스가 공격 패턴을 시전할 때마다 자신의 HP를 자가 소모 (최소 1 HP 보장)
3. **최소 2종 기본 패턴** — 근접 1개 + 원거리 1개

새 capability(웅덩이, 비, 폭발, 장판, 광폭화)는 다른 페이즈 소관이며 이번 페이즈에서 다루지 않는다.

</domain>

<decisions>
## Implementation Decisions

### 속성 태그 시스템
- **D-01:** 속성 태그는 `enum DamageElement { None, Water }` 로 표현. (Phase 1 범위; 후속 페이즈에서 Fire 등 추가 가능)
- **D-02:** 데미지 전달은 `struct DamageInfo { float amount; DamageElement element; }` 로 통합.
- **D-03:** `DamageElement` enum 과 `DamageInfo` struct 는 `Assets/Script/Combat/DamageInfo.cs` 한 파일에 함께 배치 (신규 폴더 `Assets/Script/Combat/` 생성).
- **D-04:** 필드는 최소 세트만 (`amount`, `element`). 향후 source/critical/knockback 등은 필요 시 확장 (YAGNI).

### 데미지 파이프라인 통합
- **D-05:** 보스의 정통 데미지 진입점은 `BossStatsSystem.TakeDamage(DamageInfo)`. 기존 `TakeDamage(float)` 시그니처는 내부에서 `TakeDamage(new DamageInfo { amount = damage, element = DamageElement.None })` 으로 forward 하여 기존 호출자(`HP.cs`, `WaveSlice.cs` 등) 호환을 유지한다.
- **D-06:** 호출부에서 속성을 결정 — 플레이어 스킬(`WaveSlice`, `FlashSlice`, `PlayerAttack`)이 자신의 `DamageElement` 를 인스펙터 필드로 가지고, 타격 시 `DamageInfo` 를 만들어 보스에 전달한다. 보스는 호출자를 신뢰하고 element 만 보고 분기.
- **D-07:** Phase 1 시점에서 `WaveSlice` 의 element 는 `Water`, 그 외 플레이어 스킬(`FlashSlice`, `PlayerAttack`)은 일단 `None` (Non-Water) 로 시작. (Water 외 속성 추가는 후속 페이즈)

### WaterMonsterStats 설계
- **D-08:** `WaterMonsterStats : BossStatsSystem` 으로 상속. 기존 `BossStatsSystem` 은 그대로 두고 물괴물 전용 동작은 override / 신규 메서드로 추가.
- **D-09:** `MaxWater = 0` 으로 설정하여 `IsBarrierActive` 가 항상 false 가 되게 한다 → 기존 베리어/물 자연감소 로직이 자연스럽게 무력화. 다른 보스(튜토리얼 보스 등)는 베리어 시스템을 계속 사용 가능.
- **D-10:** `BossStatsSystem.TakeDamage` 를 `protected virtual` 로 변경하고 `WaterMonsterStats` 가 override (이건 기존 코드 한 줄 수정이 필요함 — 허용 범위로 합의).
- **D-11:** Override 동작 규칙:
  - `info.element == DamageElement.Water` → `_currentHealth += info.amount` (`MaxHealth` 로 클램프), 회복 이벤트 발화
  - `info.element != Water` → 정상 대미지 처리 (`_currentHealth -= info.amount`)
  - HP 코스트로 인한 사망은 막지만, 외부 대미지로 인한 사망은 `Die()` 호출 (기존 동작 유지)
- **D-12:** `OnDamageTaken` 이벤트는 그대로 두고 `BossController.HandleDamageTaken` 에 연결되지만, `IsBarrierActive == false` 이므로 `CounterState` 전환 조건은 자연스럽게 차단된다 (코드 변경 없이 차단됨). `OnWaterDepleted` 는 `MaxWater = 0` 으로 시작하므로 발생하지 않음.
- **D-13:** `Update()` 의 `WaterDecayRate` 자연 소모 로직은 `IsBarrierActive == false` 라 자동 스킵됨 — 추가 수정 불필요.

### Claude's Discretion
다음 항목은 리서치/플래너가 코드와 베스트 프랙티스를 보고 결정한다:
- **HP 코스트 정의 방식** (ScriptableObject `AttackData` SO vs State 클래스 필드 vs `BossController` 인스펙터 dictionary). 비율(%) vs 고정값. → planner 권장: ScriptableObject `AttackPatternData` (확장성) 또는 State 필드(간단함) 중 선택.
- **Phase 1 의 근접 1 + 원거리 1 패턴 구체 동작** (휘두르기/내려찍기/돌진, 물줄기/투척/원형 파동 등) — 기존 `States/Attacks/` 에 어떤 패턴이 있는지 확인 후 재사용 또는 신규.
- **힐 피드백 연출** — 플로팅 텍스트/파티클/사운드 중 어떤 조합. 기존 프로젝트에 플로팅 텍스트 시스템이 있는지 확인 필요.
- **보스 HP 시스템과 기존 `Assets/Script/HP.cs` 의 관계** — `HP.cs` 는 일반 적/플레이어용으로 두고 보스는 `BossStatsSystem` 만 쓰는 분리 유지가 무난. `WaveSlice` 는 호출 분기 처리.

### Folded Todos
없음

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 프로젝트 결정사항
- `.planning/PROJECT.md` — 상속 기반 확장 원칙, 기존 NewBoss 재사용 결정, 속성 태그 시스템 도입 결정
- `.planning/REQUIREMENTS.md` — REQ-WM-01 ~ REQ-WM-X-03 전체 요구사항
- `.planning/ROADMAP.md` §`Phase 1` — Goal, Success Criteria, 의존성

### 베이스 코드 (상속/재사용 대상)
- `Assets/Enemy/NewBoss/Script/BossController.cs` — `protected virtual Awake`, State 전환, `HandleDamageTaken`/`HandleWaterDepleted` 이벤트 핸들러
- `Assets/Enemy/NewBoss/Script/BossStatsSystem.cs` — HP/Water/Barrier 필드, `TakeDamage(float)`, `OnDamageTaken`/`OnWaterDepleted` 이벤트, `Die()`. **D-10 에 따라 `TakeDamage` 를 `protected virtual` 로 변경 필요**
- `Assets/Enemy/NewBoss/Script/States/IBossState.cs` — State 인터페이스
- `Assets/Enemy/NewBoss/Script/States/IdleState.cs`, `ChaseStates.cs`, `CombatState.cs`, `CounterState.cs`, `GroggyState.cs` — 재사용 후보
- `Assets/Enemy/NewBoss/Script/States/Attacks/` — 기존 공격 패턴 (planner 가 폴더 내용 확인 후 재사용 결정)

### 플레이어 스킬 (속성 태그 부착 대상)
- `Assets/Player/Script/SkillScript/WaveSlice.cs` — Phase 1 에서 element=Water 로 표시
- `Assets/Player/Script/SkillScript/FlashSlice.cs` — element=None 으로 시작
- `Assets/Player/Script/PlayerAttack.cs` — element=None 으로 시작

### 기존 HP 시스템
- `Assets/Script/TakeDmg.cs` — `GiveDmg.DealtoTarget` 이 `HP.TakeDamage(float)` 호출. 보스용 분기 처리 시 참조
- `Assets/Script/HP.cs` — 일반 적/플레이어용 HP, 보스는 별개

</canonical_refs>

<specifics>
## Specific Ideas

- 호출자 측에서 속성을 결정하는 방식이 채택됨 — 플레이어 스킬 MonoBehaviour 의 `[SerializeField] DamageElement element;` 필드를 인스펙터에서 디자이너가 설정한다.
- `TakeDamage(float)` 의 forward 동작은 외부 호출자 기존 동작을 깨지 않기 위함 — 향후 호출부 마이그레이션 시 deprecate 가능.
- 다른 보스(예: TutorialBossController)의 베리어 시스템은 그대로 동작 — `WaterMonsterStats` 가 `MaxWater=0` 으로 베리어를 비활성화하는 패턴은 물괴물 한정.

</specifics>

<deferred>
## Deferred Ideas

- **기타 속성(Fire, Earth, ...)** — Phase 1 범위 외, 후속 보스/스킬 추가 시 enum 확장
- **DamageInfo 확장 필드** (source, isCritical, knockback) — 필요해질 때 추가
- **힐 피드백 시각/사운드 연출 상세** — Claude's Discretion (planner 단계)
- **HP 코스트 데이터 저장 방식 확정** (ScriptableObject vs State 필드) — Claude's Discretion (planner 단계)
- **Phase 1 패턴의 구체 동작 디자인** — Claude's Discretion (planner 단계, 기존 `States/Attacks/` 확인 후)
- **다른 페이즈의 가믹** (비/웅덩이/폭발/순간이동/장판/광폭화) — Phase 2~4 소관

</deferred>

---

*Phase: 01-boss-core-mechanics*
*Context gathered: 2026-04-08 via /gsd:discuss-phase*
