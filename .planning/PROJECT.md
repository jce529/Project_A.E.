# Project A.E

## Overview
Unity 2D 액션 게임. 다양한 보스 구현을 중심으로 진행 중.

## Tech Stack
- **Engine:** Unity 6 (Physics 2D, `Rigidbody2D.linearVelocity` API 사용)
- **Language:** C#
- **Architecture:** State Pattern 기반 보스 AI (`IBossState` 인터페이스 + State 클래스 분리)
- **Base code location:** `Assets/Enemy/NewBoss/Script/`

## Current Milestone: v2.0 물의_정령_보스_구현

**Goal:** 2스테이지 구조의 '분노한 물의 정령' 신규 보스를 순수 로직/상태머신으로 구현

**Target features:**
- [스테이지 1] 빠른 중거리 돌진 공격
- [스테이지 1] 자동추적 원거리 투사체 발사
- [스테이지 1] 근접 시 플레이어 튕겨내기 (거리유지)
- [스테이지 2] HP 50% 이하 전환, 스테이지 1 패턴 전부 유지
- [스테이지 2] 은신: 잠깐 사라졌다 다른 위치에 순간이동 재등장
- [스테이지 2] 분신: 진짜 1 + 분신 2 = 총 3개, 분신은 공격 모션 O, 데미지 0

**Constraints:** 애니메이션·시각 이펙트 제외, 순수 로직·상태머신만 구현

## Completed Milestones

### v1.0 보스_물괴물_구현 (완료)
- [x] **Phase 1: 보스 기본 엔티티 및 코어 메커니즘** (2026-04-12)
- [x] **Phase 2: 날씨 시스템 및 물 웅덩이 상호작용** (2026-04-16)
- [x] **Phase 3: 폭발 기믹 연계 및 보스 순간이동** (2026-04-16)
- [x] **Phase 4: 광폭화 및 장판 시스템** (2026-04-16)

## Validated Requirements

### v1.0 (물괴물)
- **REQ-WM-P4-01**: 이속/감속 장판 (Phase 4 완료 시 검증됨)
- **REQ-WM-P4-02**: 광폭화 상태 (Phase 4 완료 시 검증됨)
- **REQ-WM-P4-03**: 탄막/생존 AI (Phase 4 완료 시 검증됨)
- **REQ-WM-P3-01**: 스택 임계 연쇄 폭발 (Phase 3 완료 시 검증됨)
- **REQ-WM-P3-02**: 보스 순간이동 패턴 (Phase 3 완료 시 검증됨)
- **REQ-WM-02**: 공격 시 자가 HP 소모 (Phase 4 광폭화 tick drain 적용 시 재확인됨)
- **REQ-WM-X-01**: Player 레이어 타겟팅 (Phase 4 장판 적용 시 재확인됨)

### v2.0 (물의 정령)
- **CORE-01**: 정령 보스 독립 엔티티 (Phase 5 완료 시 검증됨)
- **CORE-02**: 배리어 없는 직접 HP 시스템 (Phase 5 완료 시 검증됨)
- **CORE-04**: 사망 시 오브젝트 비활성화 (Phase 5 완료 시 검증됨)
- **S1-01**: 2단계 코루틴 돌진 패턴 (Phase 5 완료 시 검증됨)
- **S1-02**: 플레이어 방향 투사체 발사 (Phase 5 완료 시 검증됨)
- **S1-03**: 넉백 적용 튕겨내기 패턴 (Phase 5 완료 시 검증됨)

## Key Architectural Decisions
- **상속 기반 확장**: 보스는 기존 `BossController` / `BossStatsSystem` 을 **상속**해 구현. 기존 튜토리얼/일반 보스 스크립트 수정은 최소화.
- **상태 패턴 재사용**: `IdleState`, `CombatState`, `CounterState`, `GroggyState` 등 기존 State 는 재사용하고, 보스 전용 패턴은 별도 하위 폴더에 추가.
- **상태 인터셉트 패턴**: `BossController.Update()` 를 오버라이드하여 기본 `CombatState` 를 보스 전용 하위 클래스(예: `SpiritCombatState`)로 가로채 전환.
- **레이어 분리**: 보스 공격은 Player 레이어에만 영향 (기존 원칙 유지).
- **물의 정령 독립 구현**: WaterMonsterController와 별도의 독립 엔티티로 구현. 코드 공유는 기반 클래스(`BossController`, `BossStatsSystem`) 수준으로 제한.
- **분신 구현**: 분신은 별도 GameObject로, 동일한 상태머신 구조를 가지되 `isDummy` 플래그로 데미지 처리 분기.

## Known Base Assets (재사용 대상)
- `BossController.cs` — protected virtual Awake, State 전환/이동/쿨다운 기본 기능
- `BossStatsSystem.cs` — HP / Water / Barrier / 이벤트 (OnDamageTaken, OnWaterDepleted)
- `States/` — IdleState, ChaseStates, CombatState, CounterState, GroggyState, IBossState
- `States/Attacks/` — 기존 공격 패턴
- Player: `WaveSlice.cs` (물 가르기), `PlayerAttack.cs`

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-04-30 — Phase 5 완료*
