---
phase: 04-enrage-zone-system
plan: 02
status: complete
date: 2026-04-16
---

# Summary: Enrage Mode Logic & Integration

Phase 4의 핵심인 광폭화(Enrage) 시스템과 장판 생성 로직의 전체 통합을 완료했습니다. 보스의 체력이 낮아질수록 전투가 더욱 치열해지는 메커니즘이 완성되었습니다.

## Key Changes

### 1. Enrage Trigger System
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs`
  - `_enrageHpThreshold` (30%) 기반 트리거 구현
  - `OnDamageTaken` 이벤트 구독 및 `Update` 폴백 체크를 통한 누락 없는 상태 감지
  - 광폭화 진입 시 `WaterMonsterStats` 및 `WaterMonsterCombatState`에 플래그 전달

### 2. Attack Frequency & Cooldown
- `Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs`
  - `_enrageCooldownMultiplier` (0.5x) 적용: 광폭화 시 모든 공격의 쿨타임이 절반으로 감소
  - `Execute` 오버라이드를 통해 `_decisionTimer`에 배율을 곱하는 방식으로 구현

### 3. Dynamic Zone Spawning
- `WaterMonsterController.cs`에 장판 생성 관리 로직 추가
  - `SpawnRandomZone()`: 지정된 범위(`_zoneBounds`) 내 무작위 위치에 속도 증가/감소 장판 생성
  - 생성된 장판은 `_zoneDuration` (8초) 후 자동으로 소멸
  - `_maxActiveZones` (4개) 및 `_zoneCooldown` (5초)을 통한 무분별한 생성 방지
- `SelectAttackStrategy`에서 광폭화 상태일 때 장판 생성을 우선순위 후보로 포함

### 4. Enrage Risk (Tick Drain)
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs`
  - `Update` 오버라이드를 통해 광폭화 상태에서 1.5초마다 5f의 HP 자가 소모 구현
  - 보스의 공격력은 강해지지만, 스스로 소멸해가는 긴박한 연출 강화

## Verification Results

- [x] HP 30% 이하 시 `IsEnraged` 플래그 활성화 확인
- [x] 광폭화 상태에서 공격 쿨다운이 0.5배로 감소하여 빈도가 증가함
- [x] 전투 중 장판(SpeedUp/SlowDown)이 무작위로 생성되고 자동 소멸됨
- [x] 광폭화 시 보스의 HP가 주기적으로 소모됨 (SpendHpCost 연동)
- [x] 기존 텔레포트 및 근접/원거리 패턴과 조화롭게 작동함

## Next Steps
- **Phase Verification**: Phase 4의 전체 목표(REQ-WM-P4-01 ~ 03) 달성 여부 최종 검증
- **Milestone v1.0 Completion**: 모든 페이즈 완료에 따른 마일스톤 종료 및 정리
