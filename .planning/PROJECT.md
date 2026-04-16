# Project A.E

## Overview
Unity 2D 액션 게임. 현재 마일스톤은 **최종보스 물괴물(Water Monster) 구현**.

## Tech Stack
- **Engine:** Unity 6 (Physics 2D, `Rigidbody2D.linearVelocity` API 사용)
- **Language:** C#
- **Architecture:** State Pattern 기반 보스 AI (`IBossState` 인터페이스 + State 클래스 분리)
- **Base code location:** `Assets/Enemy/NewBoss/Script/`

## Current Milestone
**보스_물괴물_구현** — 4개 페이즈로 구성된 최종보스 구현. 상세는 `.planning/ROADMAP.md`, `.planning/REQUIREMENTS.md` 참조.

## Phase Progress
- [x] **Phase 1: 보스 기본 엔티티 및 코어 메커니즘** (2026-04-12)
- [x] **Phase 2: 날씨 시스템 및 물 웅덩이 상호작용** (2026-04-16)
- [x] **Phase 3: 폭발 기믹 연계 및 보스 순간이동** (2026-04-16)
- [ ] **Phase 4: 광폭화 및 장판 시스템** (In Progress)

## Validated Requirements
- **REQ-WM-P3-01**: 스택 임계 연쇄 폭발 (Phase 3 완료 시 검증됨)
- **REQ-WM-P3-02**: 보스 순간이동 패턴 (Phase 3 완료 시 검증됨)
- **REQ-WM-02**: 공격 시 자가 HP 소모 (Phase 3 텔레포트 적용 시 재확인됨)
- **REQ-WM-X-01**: Player 레이어 타겟팅 (Phase 3 폭발 시 재확인됨)

## Key Architectural Decisions
- **상속 기반 확장**: 물괴물 보스는 기존 `BossController` / `BossStatsSystem` 을 **상속**해 구현 (`WaterMonsterController`, `WaterMonsterStats`). 기존 튜토리얼/일반 보스 스크립트 수정은 최소화.
- **상태 패턴 재사용**: `IdleState`, `CombatState`, `CounterState`, `GroggyState` 등 기존 State 는 재사용하고, 물괴물 전용 패턴은 `States/WaterMonster/` 하위에 추가.
- **속성 태그 시스템**: 플레이어 스킬의 Water / Non-Water 속성 판별을 위해 경량 태그 시스템 신규 도입.
- **레이어 분리**: 보스 공격/폭발/장판은 Player 레이어에만 영향 (기존 원칙 유지).

## Known Base Assets (재사용 대상)
- `BossController.cs` — protected virtual Awake, State 전환/이동/쿨다운 기본 기능
- `BossStatsSystem.cs` — HP / Water / Barrier / 이벤트 (OnDamageTaken, OnWaterDepleted)
- `States/` — IdleState, ChaseStates, CombatState, CounterState, GroggyState, IBossState
- `States/Attacks/` — 기존 공격 패턴
- Player: `WaveSlice.cs` (물 가르기), `PlayerAttack.cs`
