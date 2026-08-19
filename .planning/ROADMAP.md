# Roadmap: Project A.E — 최종보스 물괴물 (Water Monster)

## Overview

최종보스 '물괴물'을 4개 페이즈로 구현한다. 물 속성으로 회복되는 치유 필터, 공격 시 자가 HP 소모, 비/웅덩이 환경 기믹, 스택 기반 연쇄 폭발, 보스 순간이동, 이속/감속 장판, 광폭화까지 점진적으로 쌓아 올린다. 기존 `Assets/Enemy/NewBoss/Script/` 의 `BossController` / `BossStatsSystem` / State 패턴을 상속해 재사용한다.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3, 4): 물괴물 보스 구현 순서
- Decimal phases (e.g., 2.1): 긴급 삽입 작업 (없음)

- [x] **Phase 1: 보스 기본 엔티티 및 코어 메커니즘** — 물괴물 엔티티, 물 속성 힐링 필터, HP 코스트 공격 (2026-04-12)
- [x] **Phase 2: 날씨 시스템 및 물 웅덩이 상호작용** — 비 날씨, 웅덩이 스포너/파괴/흡수, 파괴 불가 상태 전환 (2026-04-16)
- [x] **Phase 3: 폭발 기믹 연계 및 보스 순간이동** — 스택 임계 연쇄 폭발, 파괴 불가 웅덩이 텔레포트 패턴 (2026-04-16)
- [x] **Phase 4: 광폭화 및 장판 시스템** — 이속/감속 장판, 광폭화 모드 AI (completed 2026-04-16)

## Phase Details

### Phase 1: 보스 기본 엔티티 및 코어 메커니즘
**Goal**: 물괴물 보스가 씬에 존재하며, 물 속성 스킬에 맞으면 HP가 회복되고, 비-물 속성 스킬에는 대미지를 입으며, 공격 패턴 시전 시 자신의 HP를 스스로 소모한다.
**Depends on**: Nothing (first phase)
**Requirements**: REQ-WM-01, REQ-WM-02, REQ-WM-03, REQ-WM-P1-01, REQ-WM-P1-02, REQ-WM-P1-03, REQ-WM-X-01, REQ-WM-X-02
**Success Criteria** (what must be TRUE):
  1. 씬에 물괴물 보스(`WaterMonsterController : BossController`)가 배치되어 Idle/Chase/Combat 상태 전환이 동작한다.
  2. 플레이어가 Water 태그가 붙은 스킬로 보스를 타격하면 보스 HP가 회복된다 (시각 피드백 포함).
  3. 플레이어가 비-Water 스킬로 보스를 타격하면 정상 대미지가 들어간다.
  4. 보스가 공격 패턴을 시전할 때마다 보스의 HP가 패턴별로 정의된 값만큼 자가 소모된다 (최소 1 HP 보장).
  5. 최소 2종 기본 패턴(근접 1 + 원거리 1)이 동작하며 각 패턴은 HP 코스트를 가진다.
**Plans**: 5 plans

Plans:
- [x] 01-01-PLAN.md — 데미지 파이프라인 기반 (DamageInfo, BossStatsSystem 상속 친화화, CombatState 가상 가드)
- [x] 01-02-PLAN.md — WaterMonsterStats/Controller/CombatState 엔티티 3종
- [x] 01-03-PLAN.md — 플레이어 스킬 element 태그 + BossStatsSystem 우선 데미지 라우팅
- [x] 01-04-PLAN.md — Water 공격 패턴 (Melee/Ranged/Projectile) + Heal popup + SelectAttackStrategy override
- [x] 01-05-PLAN.md — Scene/Prefab/Animator 셋업 + Play Mode 수동 검증 체크포인트

### Phase 2: 날씨 시스템 및 물 웅덩이 상호작용
**Goal**: 2페이즈 진입 시 맵 전체에 비가 내리고, 랜덤하게 물 웅덩이가 스폰되며, 플레이어는 '물 가르기'로 파괴하거나 흡수하여 파괴 불가 상태로 전환할 수 있다.
**Depends on**: Phase 1
**Requirements**: REQ-WM-P2-01, REQ-WM-P2-02, REQ-WM-P2-03, REQ-WM-P2-04, REQ-WM-P2-05, REQ-WM-X-01
**Success Criteria** (what must be TRUE):
  1. 보스 HP가 특정 임계치(N%) 이하로 떨어지면 맵 전체에 비 이펙트가 발생한다.
  2. 비가 내리는 동안 맵 랜덤 위치에 `WaterPuddle` 오브젝트가 주기적으로 스폰된다.
  3. 플레이어의 `WaveSlice` 스킬이 파괴 가능 상태의 웅덩이를 파괴한다.
  4. 플레이어가 흡수 상호작용을 사용하면 플레이어 수분이 회복되고 해당 웅덩이는 파괴 불가 상태로 전환되며 시각적으로 구분된다.
  5. `PuddleStackManager` 가 파괴 불가 웅덩이 개수를 중앙에서 카운팅한다.
**Plans**: 3 plans

Plans:
- [x] 02-01-PLAN.md — Phase 2 인프라 (WeatherController, WaterPuddle, PuddlePool, PuddleSpawner, PuddleStackManager) + 페이즈 전환 트리거
- [x] 02-02-PLAN.md — 플레이어 상호작용 (WaveSlice 웅덩이 파괴 + F키 흡수 메커니즘)
- [x] 02-03-PLAN.md — 에디터 셋업 (프리팹/태그/씬 배치) + Play Mode 검증 체크포인트

### Phase 3: 폭발 기믹 연계 및 보스 순간이동
**Goal**: 파괴 불가 웅덩이가 임계치 이상 누적되면 연쇄 AoE 폭발이 발동해 플레이어에게 치명적 대미지를 주고, 보스는 파괴 불가 웅덩이를 매개체로 순간이동 패턴을 사용한다.
**Depends on**: Phase 2
**Requirements**: REQ-WM-P3-01, REQ-WM-P3-02, REQ-WM-02, REQ-WM-X-01
**Success Criteria** (what must be TRUE):
  1. 파괴 불가 웅덩이 개수가 설정된 임계치에 도달하면 연쇄 AoE 폭발 이벤트가 트리거된다.
  2. 폭발은 Player 레이어에만 대미지를 주며 시각적/사운드 피드백이 명확하다.
  3. 보스에 `WaterTeleportState` 가 추가되어 맵의 파괴 불가 웅덩이로 순간이동한다.
  4. 순간이동 패턴에도 REQ-WM-02 자가 HP 소모가 적용된다.
  5. 파괴 불가 웅덩이가 하나도 없을 땐 순간이동 패턴이 사용되지 않는다.
**Plans**: 2 plans

Plans:
- [x] 03-01-PLAN.md — 스택 임계 폭발 (PuddleStackManager 목록 API + PuddleExplosionController 경고/AoE/Pool Return)
- [x] 03-02-PLAN.md — 보스 순간이동 패턴 (WaterTeleportState + CombatState 텔레포트 분기 + 쿨다운)

### Phase 4: 광폭화 및 장판 시스템
**Goal**: 보스가 HP M% 이하에서 광폭화 모드로 진입해 공격 쿨타임이 대폭 감소하고, 맵에 이속/감속 장판을 생성하며 패턴을 난사한다.
**Depends on**: Phase 3
**Requirements**: REQ-WM-P4-01, REQ-WM-P4-02, REQ-WM-P4-03, REQ-WM-X-01
**Success Criteria** (what must be TRUE):
  1. 보스 HP가 M% 이하로 떨어지면 광폭화 상태로 전환되어 공격 쿨타임이 감소하고 패턴 시전 빈도가 증가한다.
  2. 보스가 맵에 `SpeedUpZone` / `SlowDownZone` 장판을 무작위 위치에 생성한다.
  3. 두 장판은 Player 레이어에만 이동 속도 버프/디버프를 적용한다.
  4. 광폭화 상태 전용 AI 로직이 장판 생성과 패턴 난사를 번갈아 수행한다.
  5. 광폭화 상태에서도 REQ-WM-02 HP 자가 소모는 유지되며 소모 속도가 증가한다.
**Plans**: 2 plans

Plans:
- [x] 04-01-PLAN.md — Zone 스크립트 신규 작성 + PlayerController speedModifier + 베이스 클래스 접근자 수정
- [x] 04-02-PLAN.md — 광폭화 트리거 + Zone 생성 + tick HP 소모 + CombatState 쿨다운 배율/장판 AI

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. 보스 기본 엔티티 및 코어 메커니즘 | 5/5 | Complete | 2026-04-12 |
| 2. 날씨 시스템 및 물 웅덩이 상호작용 | 3/3 | Complete | 2026-04-16 |
| 3. 폭발 기믹 연계 및 보스 순간이동 | 2/2 | Complete | 2026-04-16 |
| 4. 광폭화 및 장판 시스템 | 0/2 | Complete    | 2026-04-16 |

# Roadmap: Project A.E — Milestone v2.0 물의_정령_보스_구현

## Overview

2스테이지 구조의 '분노한 물의 정령' 신규 보스를 순수 로직/상태머신으로 구현한다. 기존 `BossController` / `BossStatsSystem` 상속 구조를 재사용하며, 스테이지 1 3종 공격 패턴, 스테이지 2 은신·분신 시스템을 2개 페이즈로 완성한다. 애니메이션·시각 이펙트 없이 로직과 상태머신만 구현한다.

## Phases

**Phase Numbering:**
- v1.0 Phase 1~4 완료 이후 Phase 5부터 시작
- Integer phases (5, 6, 7): 물의 정령 보스 구현 순서 + 공격 패턴 판단 로직 리팩토링

- [x] **Phase 5: 보스 기반 엔티티 및 스테이지 1 공격 패턴** — 물의 정령 독립 엔티티, HP 시스템, 사망 처리, 돌진/투사체/튕겨내기 3종 패턴 (2026-04-30)
- [x] **Phase 6: 스테이지 전환 및 스테이지 2 은신·분신 시스템** — HP 50% 스테이지 2 전환, 은신 순간이동, 분신 3개 동시 존재 (2026-04-30)
- [ ] **Phase 7: 보스 공격 패턴 판단 로직 리팩토링** — CombatState 공유 기반에 TutorialBoss 스타일 조건부 판단 로직 도입, WaterSpirit 보스에 적용

## Phase Details

### Phase 5: 보스 기반 엔티티 및 스테이지 1 공격 패턴
**Goal**: 물의 정령 보스가 씬에 독립 엔티티로 존재하며, 플레이어를 감지해 전투 상태로 진입하고, 돌진/투사체/튕겨내기 3종 공격 패턴이 쿨다운 기반으로 동작하며, HP가 0이 되면 사망 처리된다.
**Depends on**: Nothing (first phase of milestone v2.0)
**Requirements**: CORE-01, CORE-02, CORE-04, S1-01, S1-02, S1-03
**Success Criteria** (what must be TRUE):
  1. 씬에 `SpiritController : BossController` 가 배치되어 플레이어 감지 시 Idle → Combat 상태 전환이 동작한다.
  2. 보스가 플레이어 피격을 받으면 `BossStatsSystem` 상속 HP가 감소하고, HP 0 도달 시 사망 처리(오브젝트 비활성화 또는 DeadState 전환)가 실행된다.
  3. 중거리 조건을 만족하면 돌진 패턴(S1-01)이 발동해 보스가 빠른 속도로 플레이어 방향으로 직선 이동 후 쿨다운 상태로 전환된다.
  4. 원거리 조건에서 투사체 패턴(S1-02)이 발동해 발사 시점의 플레이어 위치를 향해 Projectile이 날아가고, 히트 시 Player 레이어에 데미지가 적용된다.
  5. 플레이어가 근접 거리 내에 있을 때 튕겨내기 패턴(S1-03)이 발동해 플레이어에게 knockback과 데미지가 동시에 적용된다.
**Plans**: 2 plans

Plans:
- [x] 05-01-PLAN.md — SpiritStats + SpiritController + SpiritCombatState 기반 엔티티 3종
- [x] 05-02-PLAN.md — 3종 공격 패턴 (SpiritCharge / SpiritProjectileAttack+Projectile / SpiritRepel)

### Phase 6: 스테이지 전환 및 스테이지 2 은신·분신 시스템
**Goal**: 보스 HP가 50% 이하로 떨어지면 스테이지 2로 1회 전환되어 기존 3종 패턴을 유지하면서 은신(순간이동 재등장)과 분신 3개 동시 존재 메커니즘이 추가된다. 분신은 공격 모션을 수행하되 데미지는 0이며, 진짜 보스만 피격 데미지를 정상 적용한다.
**Depends on**: Phase 5
**Requirements**: CORE-03, S2-01, S2-02, S2-03, S2-04, S2-05
**Success Criteria** (what must be TRUE):
  1. 보스 HP가 50% 이하로 최초 도달 시 스테이지 2 전환이 정확히 1회 발동되고, 이후 동일 조건에서 재발동되지 않는다.
  2. 스테이지 2에서도 S1-01(돌진), S1-02(투사체), S1-03(튕겨내기) 3종 패턴이 정상 동작한다.
  3. 은신 패턴 발동 시 보스 콜라이더가 비활성화되고 피격이 불가능한 상태에서 다른 위치로 순간이동 후 재등장한다.
  4. 분신 생성 패턴 발동 시 진짜 보스 1개 + 분신 2개 = 총 3개의 GameObject가 씬에 동시 존재하며, 각 분신은 동일한 공격 패턴 상태머신을 실행한다.
  5. `isDummy` 플래그가 true인 분신은 피격 시 데미지가 0으로 처리되고, false인 진짜 보스만 정상 HP 감소가 발생한다.
**Plans**: 2 plans

Plans:
- [x] 06-01-PLAN.md — Stage 2 인프라 (SpiritStats HP 50% 트리거 + IsDummy 데미지 가드 + SpiritController DummyPrefab/Stealth 파라미터/OnStage2Trigger 콜백/Stage2 인터셉트 + Stage2CombatState 컴파일 스텁)
- [x] 06-02-PLAN.md — Stage 2 오케스트레이션 (SpiritStealth 어택 전략 + Stage2CombatState 분신 관리/사이클 카운터/헤비콤보 분배/그로기 전환 + SpiritController.TriggerHeavyCombo)

### Phase 7: 보스 공격 패턴 판단 로직 리팩토링
**Goal**: 정령 보스가 고정 순서 라운드로빈이 아니라, 거리(선택적)·쿨다운·연속사용금지 조건을 통과한 후보들 중 가중치 랜덤으로 공격 패턴을 고르며, SpiritExhaustion 직후에는 반드시 SpiritWakeRepel 이 이어진다. 이 판단 로직은 CombatState 의 범용 헬퍼로 구현되어 다른 보스도 재사용할 수 있다.
**Depends on**: Phase 6
**Requirements**: D-01, D-02, D-03, D-04, D-05, D-06, D-07, D-08 (07-CONTEXT.md 잠금 결정)
**Success Criteria** (what must be TRUE):
  1. `CombatState` 에 거리 조건이 선택적(`float?`)인 `PatternCandidate` 와 가중치 랜덤 선택 헬퍼 `SelectWeightedPattern` / 강제 선택 `ForceSelectPattern` 이 존재하며, 보스 종속 코드가 없다 (D-01).
  2. `SpiritCombatState` 의 `_pattern` 라운드로빈 배열과 `_patternIndex` 가 완전히 제거되고, 후보 4종(Charge/Exhaustion/WakeRepel/FarProjectile)만 선언하는 얇은 데이터 레이어가 된다 (D-01b, D-06a).
  3. SpiritCharge / SpiritFarProjectile / SpiritExhaustion 은 거리 조건 없이, SpiritWakeRepel 만 RepelRange 이내일 때 후보에 오른다 (D-02).
  4. 직전에 실행한 패턴과 동일한 패턴은 다음 판단에서 제외된다 (D-05).
  5. SpiritExhaustion 실행 직후에는 조건 평가를 건너뛰고 SpiritWakeRepel 이 강제 실행되며, 이후 일반 판단 풀로 복귀한다 (D-04).
  6. `Stage2CombatState.cs` 와 `WaterMonsterCombatState.cs` 는 단 한 줄도 변경되지 않고, Stage 2 헤비콤보 카운터가 기존대로 동작한다 (D-07, D-08b).
**Plans**: 2 plans

Plans:
- [x] 07-01-PLAN.md — CombatState 범용 패턴 후보 평가 헬퍼 추가 + SpiritCombatState 후보 선언 데이터 레이어 전환
- [ ] 07-02-PLAN.md — Play 모드 검증 체크포인트 (랜덤성/연속금지/강제 체인/Stage 2 사이클)

## Progress

**Execution Order:**
Phases execute in numeric order: 5 -> 6 -> 7

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 5. 보스 기반 엔티티 및 스테이지 1 공격 패턴 | 2/2 | Complete | 2026-04-30 |
| 6. 스테이지 전환 및 스테이지 2 은신·분신 시스템 | 2/2 | Complete | 2026-04-30 |
| 7. 보스 공격 패턴 판단 로직 리팩토링 | 1/2 | In Progress|  |

### Phase 8: WaterMonster 보스를 CombatState 기반 패턴 판단 로직으로 마이그레이션

**Goal:** 물괴물 보스가 수작업 `List<IAttackStrategy>` + `Random.Range` 풀-랜덤이 아니라,
Phase 7 의 `CombatState` 범용 헬퍼(`PatternCandidate` + `SelectWeightedPattern`)를 통해
거리/쿨다운/직전패턴 가중치 감쇠 조건을 반영한 가중치 랜덤으로 공격 패턴을 고른다.
`WaterWavePush` 45초 특수 잠금과 광폭화 0.5배 배율은 헬퍼 경로에서도 유지되며,
`SpiritCombatState` 의 기존 완전배제 동작은 회귀 없이 공존한다.
**Requirements**: D-01 ~ D-06 (08-CONTEXT.md 잠금 결정 — 공식 REQ-ID 미할당 페이즈)
**Depends on:** Phase 7
**Success Criteria** (what must be TRUE):
  1. `CombatState` 에 쿨다운 오버라이드(`PatternCandidate.CooldownOverride`), 광폭화 배율 훅
     (`GetPatternCooldownMultiplier`), 직전 패턴 가중치 감쇠 옵션이 존재하며 전부 기본값이
     기존 동작이다 (D-01d, D-03b, D-04a).
  2. `WaterMonsterCombatState` 에 `Random.Range` / `List<IAttackStrategy>` / `_lastWaveTime` 이
     존재하지 않고, 8개 `PatternCandidate` 만 선언하는 데이터 레이어가 된다 (D-01~D-06).
  3. `WaterWavePush` 가 45초(광폭화 시 22.5초) 재사용 잠금을 유지한다 (D-04a, D-04b).
  4. 직전 사용 패턴이 완전배제되지 않고 가중치 0.5배로 후보에 남는다 (D-01a~c).
  5. 전투 도중 페이즈가 바뀌면 다음 판단부터 해당 페이즈의 프리즌 변형이 후보에 오른다 (D-06c).
  6. 장판 스폰/텔레포트 사전 가드와 `ShouldTransitionToGroggy() => false` 가 원형 그대로다 (D-05a).
  7. `SpiritCombatState.cs` 변경 0 라인으로 완전배제 연속금지가 회귀 없이 동작한다.
**Plans:** 3 plans

Plans:
- [x] 08-01-PLAN.md — CombatState 헬퍼 확장 (쿨다운 오버라이드 + 광폭화 배율 훅 + 직전 패턴 가중치 감쇠 모드)
- [x] 08-02-PLAN.md — WaterMonsterCombatState 후보 선언 데이터 레이어 전환 + WaterMonster Check.md 작성
- [ ] 08-03-PLAN.md — 정적 회귀 검사 + WaterSpirit/TutorialBoss/WaterMonster 3종 일괄 Play 모드 검증 체크포인트

### Phase 9: 일반 스테이지와 보스 스테이지 진입 시 카메라 크기(줌) 변화

**Goal:** 플레이어가 씬 내부에 배치된 보스 구역 트리거 콜라이더에 들어가면 카메라 orthographic size 가
일반 스테이지 값(5)에서 보스 값(7)으로 부드럽게 전환되고, 트리거를 벗어나면 자동으로 일반 값으로
복귀한다. 동시에 카메라가 `minX`/`maxX` Inspector 경계 밖으로 나가지 않으며, 클램프는 현재 줌의
화면 반폭(`orthographicSize * aspect`)을 반영해 줌 5/7 어느 쪽에서도 맵 경계 바깥이 보이지 않는다.
**Requirements**: D-01 ~ D-11 (09-CONTEXT.md 잠금 결정 — 공식 REQ-ID 미할당 페이즈)
**Depends on:** Phase 8 (코드 의존성은 없음 — 카메라 스크립트에만 국한된 순수 추가 작업)
**Success Criteria** (what must be TRUE):
  1. `CameraController` 에 씬 로컬 싱글톤 `Instance` 와 `SetBossZoom(bool)` 이 존재하며,
     `DontDestroyOnLoad` 는 사용하지 않는다 (스테이지마다 자기 카메라를 가짐).
  2. `BossZoomTrigger` 가 `OnTriggerEnter2D` 에서 보스 줌, `OnTriggerExit2D` 에서 일반 줌으로
     되돌리며, 필드가 없어 어느 보스 구역에나 그대로 붙일 수 있다 (D-01, D-02, D-03).
  3. 줌 값(일반 5 / 보스 7)과 줌 전환 속도가 Inspector 필드이며, 줌 속도는 기존 위치 추종
     `smoothing`(5) 과 분리된 `zoomSmoothing` 이다 (D-04, D-05, D-06, D-07).
  4. `LateUpdate` 실행 순서가 위치 추종 Lerp → 줌 Lerp → X 클램프이며, 클램프가 그 프레임의
     최신 `orthographicSize` 를 사용한다.
  5. X축만 `minX + halfWidth` ~ `maxX - halfWidth` 로 클램프되고 Y축은 제한되지 않는다
     (D-09, D-10, D-11).
  6. 기존 `CameraController.cs` 의 위치 추종 로직과 CP949 한글 주석이 한 줄도 삭제/변경되지 않는다
     (삽입 전용 편집).
  7. 실제 보스 씬에 트리거 콜라이더를 배치하는 에디터 작업은 수행하지 않는다 (D-08 — 사용자 몫).
**Plans:** 3 plans

Plans:
- [x] 09-01-PLAN.md — CameraController 확장 (싱글톤 + 줌 필드/SetBossZoom/줌 Lerp + 화면 반폭 반영 X축 클램프)
- [x] 09-02-PLAN.md — BossZoomTrigger.cs 신규 트리거 컴포넌트 + Assets/Camera/Check.md 검증 체크리스트
- [x] 09-03-PLAN.md — 정적 회귀 검사 (통과) + Unity Play 모드 검증 체크포인트 (사용자 결정으로 생략)

### Phase 10: 카메라 데드존 기법 3종 (Base Deadzone, Dynamic Asymmetrical Deadzone, Input-based Peeking) 구현 — Phase 9 CameraController에 레이어링

**Goal:** 일반 스테이지에서 카메라가 플레이어를 무조건 따라다니지 않고, (1) 데드존 박스 안에서는 완전히 정지하며 경계를 밀 때만 하드컷으로 따라붙고, (2) 경계를 미는 동안 진행 방향으로 시야가 열리도록 데드존 박스 중심이 SmoothDamp 오프셋되며, (3) 접지·정지 상태에서 수직 입력을 유지하면 카메라가 위/아래로 시야를 옮기고 이동·대시·피격 시 즉시 취소된다. 보스 구역(`SetBossZoom(true)`) 안에서는 이 3개 기법이 전부 비활성화되어 Phase 9의 레거시 Lerp 추종으로 완전히 복귀하며, 줌 Lerp와 X축 클램프는 두 경로 모두에서 Phase 9 그대로 동작한다.
**Requirements**: D-01 ~ D-17 (10-CONTEXT.md 잠금 결정 — 공식 REQ-ID 미할당 페이즈)
**Depends on:** Phase 9
**Success Criteria** (what must be TRUE):
  1. `CameraController` 에 `deadzoneWidth`/`deadzoneHeight` 월드 유닛 Inspector 필드가 있고, `OnDrawGizmos` 로 박스가 Scene 뷰에 표시되며, 줌 비율에 따라 스케일되지 않는다 (D-01, D-02, D-03).
  2. 일반 스테이지 카메라 X 가 `_deadzoneCenterX` 하드컷으로만 이동한다 — 데드존 계산 경로에 `Lerp`/`SmoothDamp` 가 없다 (D-14).
  3. 데드존 경계를 밀고 있을 때만(별도 속도 임계값 없이) 오프셋이 발동하고, 정지 후 `offsetHoldDuration` 유지 뒤 `SmoothDamp` 로 복귀하며, 진행 방향 시야가 열린다 (D-04 ~ D-07).
  4. 수직 입력을 `InputHandler.Instance.OnMoveEvent` 구독으로만 읽고, `movementLocked`·`IsGrounded()`·정지 근사·입력 유지 4조건이 모두 충족될 때만 피킹이 발동하며, 이동량 급증(대시/피격 프록시)으로 즉시 취소된다 (D-08 ~ D-13).
  5. `PlayerController.cs` 와 `InputHandler.cs` 는 0줄 변경이다 (`isDashing`/`isKnockedBack` 접근자 추가 없음).
  6. 보스 구역에서는 데드존/오프셋/피킹이 전부 꺼지고 Phase 9 레거시 `Vector3.Lerp` 결과가 그대로 유지되며, 줌 Lerp 와 X 클램프 순서가 보존된다 (D-15, D-16, D-17).
  7. `CameraController.cs` 의 기존 CP949 한글 주석이 한 줄도 훼손되지 않는다 (삭제 라인 3줄 전부 ASCII, 비-ASCII 라인 수 5 유지).
**Plans:** 3/4 plans executed

Plans:
- [x] 10-01-PLAN.md — 하드컷 Base Deadzone + `_isBossZone` 분기 구조(`ApplyNormalStageCamera`/`ResetNormalStageState`) + 데드존 Gizmo
- [x] 10-02-PLAN.md — Dynamic Asymmetrical Deadzone (밀기 방향 추적 + 유지 타이머 + SmoothDamp 오프셋 합성)
- [x] 10-03-PLAN.md — Input-based Peeking (OnMoveEvent 구독 라이프사이클 + 4조건 가드 + 수직 SmoothDamp)
- [ ] 10-04-PLAN.md — Assets/Camera/Check.md Phase 10 체크리스트 + 정적 회귀 검사 9종 + Play 모드 검증 체크포인트

### Phase 11: Newtonsoft.Json 기반 싱글톤 세이브/로드 매니저 - DontDestroyOnLoad, 메모리 캐싱(플레이 중 파일 I/O 없음), 로드 시점(이어하기/체크포인트 부활), 저장 시점(체크포인트 상호작용/보스 격파 자동저장), 확장 가능한 데이터 클래스(씬+좌표, 플레이어 스탯 하위클래스, 보스 진행도 Dictionary, 맵 기믹 상태 Dictionary, 아이템 목록), 비동기 씬 로드 완료 후 좌표 이동, Application.persistentDataPath에 .json 저장

**Goal:** 게임에 `SaveLoadManager` 싱글톤(DontDestroyOnLoad)이 존재해, 플레이 중에는 메모리 캐시만 갱신하고 파일 I/O 를 전혀 하지 않으며, 체크포인트 S키 상호작용과 보스 4종 격파 시점에만 `Application.persistentDataPath/save.json` 단일 슬롯 파일을 기록한다. 로드(이어하기/체크포인트 부활)는 저장된 씬을 코루틴 기반 `LoadSceneAsync` 로 비동기 로드한 뒤, 좌표는 기존 `PlayerSpawner.targetSpawnPointName` 경로로, 플레이어 스탯은 신규 `PlayerStats.RestoreStats()` 로 복원한다. 데이터 스키마는 씬+스폰포인트(문자열), 플레이어 스탯, 보스 진행도 Dictionary, 맵 기믹 Dictionary, 아이템 List 로 확장 가능하게 구성하되 뒤 세 개는 스텁 수준으로만 채운다. 메뉴 UI 연동은 범위 밖.
**Requirements**: D-01 ~ D-06 (11-CONTEXT.md 잠금 결정 — 공식 REQ-ID 미할당 페이즈)
**Depends on:** Phase 10
**Success Criteria** (what must be TRUE):
  1. `SaveLoadManager` 가 `DontDestroyOnLoad` 싱글톤으로 존재하며, 씬에 수동 배치하지 않아도 어느 씬에서 Play 를 시작하든 `SaveLoadManager.Instance` 가 유효하다 (D-01).
  2. 저장 파일은 `Application.persistentDataPath` 아래 `save.json` 단 하나이며, 직렬화는 Newtonsoft.Json 으로 수행되고 `Dictionary<string,bool>` 이 그대로 왕복한다 (D-02, D-03).
  3. `Packages/manifest.json` 에 `"com.unity.nuget.newtonsoft-json": "3.2.2"` 가 직접 선언되어, AI 패키지 제거 시 간접 의존성이 조용히 사라지는 위험이 제거된다.
  4. 저장 트리거가 정확히 5곳이다 — `Checkpoint.cs` S키 활성화 1곳 + 보스 4종. 보스는 두 아키텍처로 나뉘어 각각 다른 지점에 연결된다: Group A(`HP.OnDeath` → `HandleDeath()`)의 TutorialBoss/WoodBoss, Group B(이벤트 없음 → `BossStatsSystem.Die()` 오버라이드 본문)의 WaterSpirit/WaterMonster (D-01).
  5. 저장 데이터의 위치 정보가 원시 XY 좌표가 아니라 `SceneName` + `SpawnPointName` 문자열이며, 로드 시 `PlayerSpawner.targetSpawnPointName` 을 씬 로드 **전에** 세팅해 기존 경로를 그대로 재사용한다 (D-05).
  6. 로드 코루틴이 `PlayerSpawner` 세팅 → `LoadSceneAsync` → `yield return op` → `PlayerStats.RestoreStats()` 순서로 실행되며, `async`/`await`/`Task` 를 전혀 사용하지 않는다 (프로젝트 무-async 컨벤션).
  7. `NewGame()` 은 메모리만 리셋하고 디스크의 `save.json` 을 즉시 덮어쓰지 않는다 (D-06).
  8. `MainMenuUI.cs`(이어하기 버튼), `Portal.cs`/`GameManager.NextSpawnPointName`, `WoodBossStatSystem.cs`, `HP.cs`, `BossStatesSystem.cs` 는 0줄 변경이다 (UI 범위 밖 D-04 + 고아 코드/공유 베이스 클래스 불가침).
  9. 기존 5개 파일 편집이 전부 순수 삽입(삭제 0줄)이며 CP949 인코딩 파일 2종(`Checkpoint.cs`, `WoodBossController.cs`)의 한글 주석이 훼손되지 않는다.
**Plans:** 3/4 plans executed

**Execution Waves:**

| Wave | Plans | Autonomous |
|------|-------|------------|
| 1 | 11-01 | yes |
| 2 | 11-02 | yes |
| 3 | 11-03 | yes |
| 4 | 11-04 | no (Play 모드 검증 체크포인트) |

Plans:
- [x] 11-01-PLAN.md — Newtonsoft.Json manifest 직접 고정 + SaveData/PlayerStatsSaveData 스키마 신규 + PlayerStats.RestoreStats additive 메서드
- [x] 11-02-PLAN.md — SaveLoadManager 싱글톤(부트스트랩/DontDestroyOnLoad) + 메모리 캐시 + save.json I/O + 코루틴 LoadSceneAsync 로드 흐름
- [x] 11-03-PLAN.md — 저장 트리거 5곳 통합 (Checkpoint S키 + Group A 2종 HandleDeath + Group B 2종 Die 오버라이드)
- [ ] 11-04-PLAN.md — ContextMenu 검증 훅 + Assets/SaveSystem/Check.md 체크리스트 + 정적 회귀 15항목 + Play 모드 검증 체크포인트

### Phase 12: 피격 시 카메라 흔들림 (Camera Shake on Hit)

**Goal:** 플레이어가 피격당하면 카메라가 짧게 랜덤 방향으로 흔들렸다가 `shakeDuration` 초 안에 선형
감쇠해 완전히 멈춘다. 흔들림은 `CameraController` 파이프라인(위치추종/데드존 → 줌 Lerp → 경계 클램프 →
데드존 재앵커) 맨 **위에 얹히는 독립 레이어**로, `LateUpdate()` 의 마지막 문장에서 재앵커 블록 **바깥**에
무조건 적용된다 — 따라서 보스 구역에서도 동작하고(D-07), 흔들림 값이 데드존 앵커에 누적되지 않는다(D-08).
트리거는 `PlayerStats.TakeDamage` 단 한 곳이며 `HP.cs` 는 무수정이라 보스 피격 시에는 흔들리지 않는다.
**Requirements**: D-01 ~ D-09 (12-CONTEXT.md 잠금 결정 — 공식 REQ-ID 미할당 페이즈)
**Depends on:** Phase 11 (코드 의존성 없음 — 카메라/플레이어 스크립트에만 국한된 순수 삽입 작업)
**Success Criteria** (what must be TRUE):
  1. 플레이어가 데미지를 받으면 카메라가 랜덤 방향으로 짧게 흔들리고, `shakeDuration` 초 안에
     완전히 멈춘다 (D-01, D-05).
  2. 강도가 데미지량에 비례하지 않는 고정값이며, 감쇠 중 재피격 시 지속시간 타이머만 최대치로
     리프레시되고 강도는 누적되지 않는다 (D-04, D-06).
  3. 보스 구역(`_isBossZone` true, 줌 확대 상태)에서도 흔들림이 동일하게 발동한다 — Phase 10 의
     D-15(보스존에서 데드존/오프셋/피킹 비활성화)는 흔들림에 적용되지 않는다 (D-07).
  4. 흔들림 오프셋이 경계 클램프와 데드존 재앵커 블록 **이후** 최종 적용되고, 적용 후 다시 클램프하지
     않는다 — 경계를 살짝 뚫는 것은 의도된 동작이다 (D-08).
  5. Inspector 신규 노출 필드가 `shakeMagnitude` / `shakeDuration` 정확히 2개다. `AnimationCurve`
     감쇠 곡선 노출은 범위 밖 (D-09).
  6. `HP.cs` 는 0줄 변경이며 공용 `OnHit` 이벤트도 신설하지 않는다 — 보스가 맞을 때는 흔들리지
     않는다 (D-01, D-02).
  7. `CameraController.cs` / `PlayerStats.cs` 편집이 전부 순수 삽입(삭제 0줄)이고,
     `CameraController.cs` 의 비-ASCII 라인 수가 5로 유지된다 (12-RESEARCH 가 하향 조정한 인코딩
     위험 판정의 사후 검증).
**Plans:** 1 plan

**Execution Waves:**

| Wave | Plans | Autonomous |
|------|-------|------------|
| 1 | 12-01 | no (Play 모드 검증 체크포인트 포함) |

Plans:
- [ ] 12-01-PLAN.md — CameraController Hit Shake 레이어(필드 2개 + Shake() + ApplyHitShake() + LateUpdate 무조건 호출) + PlayerStats.TakeDamage 호출 지점 + 정적 회귀 12항목 + Check.md Phase 12 체크리스트 + Play 모드 검증 체크포인트

### Phase 13: 프로젝트 폴더를 돌면서 의미 없는 코드나, 주석, 리펙토링이 필요한 코드 살펴보는 페이즈

**Goal:** `Assets/` 아래 168개 C# 스크립트 전체를 스캔해 죽은 코드(D-07) / TODO·디버그 잔재(D-08) /
중복 로직(D-09) / 과도하게 긴 함수(D-10) 4개 카테고리로 분류한 감사 보고서를 만든다. 이미 Play 모드로
검증된 보스·카메라·세이브 코드의 발견 항목은 "회귀 위험 높음"으로 분리하고, CP949 인코딩 파일 46개는
별도 목록으로 표시한다. **이 phase는 보고서만 만든다 — 실제 삭제/리팩토링은 사용자가 항목별로 승인한
뒤 별도 작업으로 진행한다 (D-01/D-02).**
**Requirements**: D-01 ~ D-10 (13-CONTEXT.md 잠금 결정 — 공식 REQ-ID 미할당 페이즈)
**Depends on:** Phase 12
**Success Criteria** (what must be TRUE):
  1. `.planning/phases/13-codebase-cleanup-audit/13-AUDIT-REPORT.md` 가 존재하고, `Assets/` 아래
     **168개 .cs 파일 전부**가 스캔 커버리지에 포함된다 (Scope A 38 + B 42 + C 32 + D 56).
  2. 모든 발견 항목에 파일 경로 + 줄번호 + 카테고리(D-07~D-10) + 사유가 있다 (CONTEXT.md D-01).
  3. Play 모드 검증된 코드(WaterMonster/WaterSpirit/NewBoss/Tutorial 보스, CameraController,
     SaveLoadManager, PlayerStats/PlayerController/InputHandler, HP.cs 등)의 항목이
     `## 회귀 위험 높음 — 신중 검토 필요` 섹션에 일반 항목과 분리 기재된다 (D-05, D-06).
  4. CP949(비-UTF-8) 인코딩 파일 46개가 별도 섹션에 전수 나열되고, 수정 시 `git show HEAD:<path>` +
     순수 바이트 스크립트 프로토콜이 필요하다는 경고가 붙는다 (D-04).
  5. D-09는 PROJECT.md/STATE.md 에 이유가 기록된 의도적 비공유 12건(E-01~E-12)을 배제 목록으로
     남기고, 그 외의 중복만 문제로 보고한다 (D-09).
  6. 기존 식별 고아 코드(`Portal.cs`, `GameManager.NextSpawnPointName`, `WoodBossStatSystem.cs`)와
     스테일 씬 엔트리(`Assets/Scenes/InGame.unity`)가 재확인되어 기재된다.
  7. 모든 항목에 미체크 승인 체크박스 `[ ]` 가 있어 사용자가 항목별로 승인/보류를 표시할 수 있다 (D-01 2단계).
  8. **`git status --porcelain Assets` 가 비어 있다 — 소스 코드 0줄 변경** (D-01/D-02, 각 플랜의 인수 기준).
**Plans:** 5 plans

**Execution Waves:**

| Wave | Plans | Autonomous |
|------|-------|------------|
| 1 | 13-01, 13-02, 13-03, 13-04 | yes, yes, yes, yes |
| 2 | 13-05 | yes |

Plans:
- [x] 13-01-PLAN.md — Scope A 스캔 (WaterMonster/WaterSpirit 38파일, 전 범위 고위험) → FINDINGS-A
- [x] 13-02-PLAN.md — Scope B 스캔 (NewBoss/Tutorial/Boss/Monster_Alpha 42파일, 혼합 위험 + CP949 16개 + WoodBossStatSystem 고아 재확인) → FINDINGS-B
- [x] 13-03-PLAN.md — Scope C 스캔 (Player/Camera/SaveSystem 32파일, Phase 9~12 수정 파일 8종 고위험 + CP949 4개) → FINDINGS-C
- [x] 13-04-PLAN.md — Scope D 스캔 (Script/map/Editor/ImportedAsset 56파일, CP949 26개 + Portal/NextSpawnPointName 고아 + InGame.unity 스테일 엔트리) → FINDINGS-D
- [x] 13-05-PLAN.md — 전역 D-09 중복 교차 분석 + 4 fragment 병합 + CP949 46개 전역 집계 + 승인 체크리스트 → 13-AUDIT-REPORT.md
