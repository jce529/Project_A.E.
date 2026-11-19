---
phase: 03-explosion-gimmick-teleport
plan: 02
subsystem: AI/Movement
tags: [Teleport, State Machine, Positioning]
requires: [IBossState, CombatState, PuddleStackManager]
provides: [WaterTeleportState, Responsive Positioning AI]
affects: [WaterMonsterController, WaterMonsterCombatState]
key-files:
  created: [Assets/Enemy/WaterMonster/Script/Phase3/WaterTeleportState.cs]
  modified: [Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs, Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs]
key-decisions:
  - "플레이어와의 거리에 반비례하는 타겟 웅덩이 선택 로직(D-07)을 통해 보스의 반응형 포지셔닝 구현"
  - "WaterTeleportState 완료 후 직접 WaterMonsterCombatState로 복귀하여 상태 스왑 루프 방지"
  - "텔레포트 쿨다운 관리를 WaterMonsterController로 위임하여 상태 전환 간 데이터 유지"
requirements-completed:
  - REQ-WM-P3-02
  - REQ-WM-02
duration: 15 min
completed: "2026-04-16T17:05:00Z"
---

# Phase 03 Plan 02: Water Teleport State Summary

## One-liner
파괴 불가 웅덩이를 매개체로 하는 반응형 텔레포트 패턴(`WaterTeleportState`) 및 전투 AI 통합 구현.

## Substantive Changes
- **WaterTeleportState.cs**: `IBossState` 구현. 플레이어 근접 시 먼 곳으로, 원거리 시 가까운 곳으로 텔레포트하는 지능형 타겟팅 로직 적용. HP 코스트(MaxHealth 5%) 및 사라짐/나타남 연출 포함.
- **WaterMonsterCombatState.cs**: `SelectAttackStrategy` 오버라이드. 웅덩이 2개 이상 + 쿨다운 경과 시 텔레포트 패턴을 우선순위에 통합.
- **WaterMonsterController.cs**: 텔레포트 쿨다운(기본 8초) 타이머 및 유효성 검사 메서드 추가.

## Deviations from Plan
None - plan executed exactly as written.

## Self-Check: PASSED
- [x] WaterTeleportState implements IBossState correctly.
- [x] Distance-inverse targeting logic implemented.
- [x] HP cost (5%) applied correctly.
- [x] Integrated into CombatState strategy selection.
- [x] Cooldown managed in Controller.
