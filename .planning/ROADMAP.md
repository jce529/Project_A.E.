# Roadmap: Project A.E — 최종보스 물괴물 (Water Monster)

## Overview

최종보스 '물괴물'을 4개 페이즈로 구현한다. 물 속성으로 회복되는 치유 필터, 공격 시 자가 HP 소모, 비/웅덩이 환경 기믹, 스택 기반 연쇄 폭발, 보스 순간이동, 이속/감속 장판, 광폭화까지 점진적으로 쌓아 올린다. 기존 `Assets/Enemy/NewBoss/Script/` 의 `BossController` / `BossStatsSystem` / State 패턴을 상속해 재사용한다.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3, 4): 물괴물 보스 구현 순서
- Decimal phases (e.g., 2.1): 긴급 삽입 작업 (없음)

- [x] **Phase 1: 보스 기본 엔티티 및 코어 메커니즘** — 물괴물 엔티티, 물 속성 힐링 필터, HP 코스트 공격 (2026-04-12)
- [x] **Phase 2: 날씨 시스템 및 물 웅덩이 상호작용** — 비 날씨, 웅덩이 스포너/파괴/흡수, 파괴 불가 상태 전환 (2026-04-16)
- [ ] **Phase 3: 폭발 기믹 연계 및 보스 순간이동** — 스택 임계 연쇄 폭발, 파괴 불가 웅덩이 텔레포트 패턴
- [ ] **Phase 4: 광폭화 및 장판 시스템** — 이속/감속 장판, 광폭화 모드 AI

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
**Plans**: TBD

Plans:
- [ ] 03-TBD: 상세 계획은 `/gsd:plan-phase 3` 시 생성

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
**Plans**: TBD

Plans:
- [ ] 04-TBD: 상세 계획은 `/gsd:plan-phase 4` 시 생성

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. 보스 기본 엔티티 및 코어 메커니즘 | 5/5 | Complete | 2026-04-12 |
| 2. 날씨 시스템 및 물 웅덩이 상호작용 | 3/3 | Complete | 2026-04-16 |
| 3. 폭발 기믹 연계 및 보스 순간이동 | 0/TBD | Not started | - |
| 4. 광폭화 및 장판 시스템 | 0/TBD | Not started | - |
