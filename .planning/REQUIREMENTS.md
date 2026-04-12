# Project A.E — 최종보스 물괴물 (Water Monster) Requirements

마일스톤: **보스_물괴물_구현**
베이스 코드: `Assets/Enemy/NewBoss/Script/BossController.cs`, `BossStatsSystem.cs`
구현 방식: 신규 `WaterMonsterController : BossController` 상속 + `WaterMonsterStats : BossStatsSystem` 상속 (기존 스크립트 수정 최소화)

---

## 1. Core Mechanics (전체 페이즈 공통)

### REQ-WM-01: 물 속성 회복 (Elemental Healing Filter)
- 플레이어가 **물(Water) 속성 태그**를 가진 스킬로 보스를 타격할 경우, 대미지가 **음수**로 처리되어 보스 HP가 **회복**됨.
- 판정 위치: 기존 `BossStatsSystem.TakeDamage` 의 대미지 파이프라인에 **속성 필터**를 추가 (오버라이드 또는 이벤트 훅).
- 물 외 속성/무속성 공격은 정상 대미지.

### REQ-WM-02: 공격 코스트 (Self-HP Attack Cost)
- 보스가 어떤 공격 패턴을 시전할 때마다 **자신의 현재 HP 혹은 Max HP 비율만큼 자가 소모**. (LoL 자크 방식)
- 코스트 값은 패턴별로 정의 가능 (ScriptableObject 또는 인스펙터 필드).
- HP 코스트로 인해 보스가 사망하지는 않음 (최소 1 HP 보장).

### REQ-WM-03: NewBoss 상속 구조 재사용
- **BossController / BossStatsSystem / IBossState** 계층을 상속해 재사용.
- 기존 `CounterState`, `GroggyState`, `IdleState`, `CombatState`, `ChaseStates` 는 가능하면 재활용하고, 물괴물 고유 패턴은 `States/WaterMonster/` 하위에 신규 State 클래스로 추가.
- 기존 `BossStatsSystem` 의 Water/Barrier 개념은 **보스의 자원이 아닌 "물괴물의 약점 자원"** 개념으로 재해석하거나, 혼동을 피하기 위해 물괴물 전용 `WaterMonsterStats` 에서 **별도 HP-코스트 로직**으로 오버라이드.

---

## 2. Phase 1 — 기본 패턴 및 자원 관리 (HP 100% ~ N%)

### REQ-WM-P1-01: 근접/원거리 기본 패턴
- 최소 2종 이상의 기본 공격 패턴 (근접 1 + 원거리 1).
- 각 패턴은 REQ-WM-02 에 따라 시전 시 자가 HP 소모.

### REQ-WM-P1-02: 물 속성 힐 차단 설계 유도
- 플레이어가 물 스킬로 공격 시 보스가 회복되는 것이 **즉시 체감**되어야 함 (피드백: 힐 숫자/이펙트).
- 플레이어의 비-물 속성 스킬로만 대미지 누적이 가능함.

### REQ-WM-P1-03: 기획적 의도 달성 검증
- 1페이즈 클리어 시점에 플레이어 체력이 높을수록 2페이즈에서 유리하도록 튜닝 여지 확보 (밸런싱은 Phase 2 단계에서 조정).

---

## 3. Phase 2 — 비 날씨 & 웅덩이 스택 (HP ≤ N%)

### REQ-WM-P2-01: 비(Rain) 환경 시스템 [x]
- 페이즈 2 진입 시 맵 전체에 비 파티클/이펙트 발생.
- 날씨 컨트롤러는 보스와 분리된 독립 컴포넌트(`WeatherController`).

### REQ-WM-P2-02: 물 웅덩이 스포너 [x]
- 비 시작과 함께 맵 랜덤 위치에 `WaterPuddle` 오브젝트 주기적 생성.
- 생성 위치/주기는 인스펙터 튜닝 가능.

### REQ-WM-P2-03: '물 가르기' 파괴 판정
- 플레이어의 `WaveSlice` 스킬이 `WaterPuddle` 을 **완전히 파괴**할 수 있어야 함.
- 파괴 가능 상태 (`Destructible`) 인 웅덩이만 파괴 판정을 받음.

### REQ-WM-P2-04: 수분 흡수 상호작용
- 플레이어가 별도 상호작용(흡수 스킬/키)으로 웅덩이의 물을 흡수하면 플레이어 수분(Water 자원) 회복.
- 흡수된 웅덩이는 **파괴 불가(`Indestructible`)** 상태로 전환.
- 파괴 불가 웅덩이는 시각적으로 구분 가능해야 함 (색/이펙트).

### REQ-WM-P2-05: 파괴 불가 웅덩이 스택 매니저 [x]
- 맵 상의 `Indestructible` 웅덩이 개수를 중앙에서 카운트 (`PuddleStackManager`).
- 임계치(Threshold) 이상 누적 시 **연쇄 AoE 폭발** 이벤트 트리거.
- 폭발은 플레이어에게 치명적 대미지 — 수치는 튜닝.

---

## 4. Phase 3 — 폭발 연계 & 보스 순간이동 (Phase 2 기믹 심화)

### REQ-WM-P3-01: 스택 임계 폭발 구현
- REQ-WM-P2-05 의 폭발 이벤트를 실제 연쇄 AoE 효과 및 피해 판정으로 구현.

### REQ-WM-P3-02: 보스 순간이동 패턴
- 보스는 맵에 존재하는 **파괴 불가 웅덩이**를 타겟으로 순간이동하는 전용 State 추가 (`WaterTeleportState`).
- 순간이동 역시 REQ-WM-02 공격 코스트 적용.
- 이동 대상 웅덩이가 없으면 패턴 사용 불가 (조건부).

---

## 5. Phase 4 — 광폭화 & 장판 시스템 (HP ≤ M%)

### REQ-WM-P4-01: 이속/감속 장판
- 맵에 무작위로 `SpeedUpZone` / `SlowDownZone` AoE 를 보스가 생성.
- 두 장판은 레이어 기반으로 **플레이어에게만** 적용 (REQ-BOSS-03 Layer Damage 유지).

### REQ-WM-P4-02: 광폭화 상태
- 진입 조건: HP ≤ M%.
- 공격 쿨타임 계수 감소, 패턴 시전 속도/빈도 상승.
- 자가 HP 소모 속도 증가 → 긴장감 있는 마무리 페이즈.

### REQ-WM-P4-03: 탄막/생존 AI
- 광폭화 상태에서는 보스가 장판과 패턴을 번갈아 사용하는 AI 로직 (신규 State 또는 상태 수정).

---

## 6. Cross-cutting Requirements

### REQ-WM-X-01: Layer Damage
- 보스의 모든 공격/폭발/장판은 **Player 레이어에만** 대미지/효과를 줌. (기존 REQ-BOSS-03 유지)

### REQ-WM-X-02: 속성 태그 시스템
- 플레이어 스킬 각각에 **속성 태그(Water / Non-Water)** 를 부여할 수 있는 경량 시스템 필요.
- 기존 `WaveSlice`, `PlayerAttack` 에 태그 필드 추가.

### REQ-WM-X-03: 보스 전용 UI/피드백
- HP 바, 힐 피드백, 페이즈 전환 연출 (세부 UI 는 Phase 별 계획 단계에서 상세화).
