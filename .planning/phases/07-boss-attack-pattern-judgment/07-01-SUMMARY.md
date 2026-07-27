---
phase: 07-boss-attack-pattern-judgment
plan: 01
subsystem: ai
tags: [state-pattern, boss-ai, csharp, unity, pattern-selection]

# Dependency graph
requires:
  - phase: 05-boss-foundation-and-stage1-attacks
    provides: SpiritController / SpiritCombatState / Stage1 공격 전략(SpiritCharge, SpiritExhaustion, SpiritWakeRepel, SpiritFarProjectile)
  - phase: 06-stage-transition-and-stage2-stealth-clone
    provides: Stage2CombatState (base.SelectAttackStrategy null/non-null 계약 소비자)
provides:
  - "CombatState 에 재사용 가능한 범용 패턴 후보 평가 헬퍼(PatternCandidate/SelectWeightedPattern/ForceSelectPattern/CommitSelection)"
  - "SpiritCombatState 의 고정 라운드로빈 배열을 조건 기반 가중치 랜덤 후보 선언으로 전환"
  - "D-04 강제 체인(SpiritExhaustion → SpiritWakeRepel) 구현"
affects: [WaterMonsterCombatState 마이그레이션 후보 (Deferred), 08-이후 보스 밸런싱]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "패턴 후보 평가 헬퍼: 파생 State 는 PatternCandidate 목록만 선언, 베이스 클래스가 거리/쿨다운/연속금지/가중치 랜덤 판단 담당"
    - "타입 키(System.Type) 기반 쿨다운·연속금지 추적 (슬롯 인덱스 대신)"

key-files:
  created: []
  modified:
    - Assets/Enemy/NewBoss/Script/States/CombatState.cs
    - Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs

key-decisions:
  - "SpiritCharge/SpiritFarProjectile/SpiritExhaustion 은 거리 조건 없이 쿨다운+연속금지만으로 판단 (D-02a). SpiritController.ChargeRange 는 기존 데드 필드로 재활용하지 않고 그대로 둠 — 재활용 시 AttackRange(2.5)/ChargeRange(5.0) 구간 불일치로 SpiritFarProjectile 이 영구 선택 불가가 되기 때문."
  - "SpiritExhaustion 도 거리 조건 없음 — ExecuteAttack 이 StopMove 만 수행하고 플레이어 위치를 참조하지 않으며, D-04 체인의 시작점이므로 거리 무관 발동이 맞음. 부수 효과로 거리 무조건 후보 3개 확보(Charge/Exhaustion/FarProjectile)로 후보 고갈 위험이 구조적으로 완화됨."
  - "SpiritWakeRepel 에만 maxDistance: spirit.RepelRange(1.5) 적용 — 내부에서 호출하는 SpiritRepel 의 실제 판정 반경과 일치"
  - "가중치: SpiritCharge 1.0 / SpiritExhaustion 0.6 / SpiritWakeRepel 1.0 / SpiritFarProjectile 1.0. Exhaustion 이 낮은 이유는 D-04 체인으로 실질 2패턴 분량(Exhaustion+강제 WakeRepel)을 소비하기 때문. 튜닝 시 SpiritCombatState.cs 상단 상수 4개만 조정하면 됨(선택 로직 변경 불필요)."
  - "쿨다운 추적은 Time.time 절대 시각 비교 방식 채택 (WaterMonsterCombatState._lastWaveTime 선례와 동일) — CombatState.Execute() 의 early-return 분기 때문에 매 프레임 감산 방식은 조용히 누락될 수 있음"
  - "추적 키는 슬롯 인덱스가 아닌 System.Type — 이번 페이즈가 고치는 버그가 정확히 인덱스 기반 장부였으므로 절대 인덱스로 회귀하지 않음"

patterns-established:
  - "PatternCandidate / SelectWeightedPattern / ForceSelectPattern: 보스 종속성 없는 범용 헬퍼로 CombatState 에 위치, 향후 다른 보스(WaterMonster 등)가 재사용 가능"

requirements-completed: [D-01, D-02, D-03, D-04, D-05, D-06, D-07, D-08]

duration: 5min
completed: 2026-07-27
---

# Phase 07 Plan 01: 보스 공격 패턴 판단 로직 리팩토링 Summary

**CombatState 에 거리 선택적 조건 + Time.time 쿨다운 + System.Type 연속사용금지 + 누적가중치 룰렛랜덤을 갖춘 범용 PatternCandidate 헬퍼를 추가하고, SpiritCombatState 의 고정 라운드로빈 배열(`_pattern`/`_patternIndex`)을 4종 후보 선언 + D-04 강제 체인(Exhaustion→WakeRepel)으로 교체**

## Performance

- **Duration:** 5 min
- **Started:** 2026-07-27T05:50:27Z
- **Completed:** 2026-07-27T05:53:22Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- `CombatState.cs` 에 `PatternCandidate` 중첩 클래스, `SelectWeightedPattern`(조건 통과 후보 중 가중치 랜덤 선택), `ForceSelectPattern`(전체 우회 강제 선택), `CommitSelection`(선택 확정 시 연속금지/쿨다운 갱신) 추가 — 순수 추가 변경(삭제 라인 0), 기존 `Enter`/`Execute`/`SelectAttackStrategy`/`Exit` 흐름 미변경
- `SpiritCombatState.cs` 를 인덱스 기반 라운드로빈에서 조건부 가중치 랜덤 후보 선언 데이터 레이어로 전환, `_pattern`/`_patternIndex` 완전 제거
- D-04 강제 체인 구현: `LastUsedPatternType == typeof(SpiritExhaustion)` 이면 일반 판단을 건너뛰고 `ForceSelectPattern(new SpiritWakeRepel())` 무조건 반환
- `Stage2CombatState.cs`, `WaterMonsterCombatState.cs`, `SpiritController.cs` 는 한 줄도 변경되지 않음 (D-07, D-08 준수 확인)

## Task Commits

Each task was committed atomically:

1. **Task 1: CombatState 에 범용 패턴 후보 평가 헬퍼 추가** - `41117c9` (feat)
2. **Task 2: SpiritCombatState 를 후보 선언 데이터 레이어로 교체** - `f77a890` (feat)

**Plan metadata:** (committed after this file is written)

## Files Created/Modified
- `Assets/Enemy/NewBoss/Script/States/CombatState.cs` - PatternCandidate/SelectWeightedPattern/ForceSelectPattern/CommitSelection 헬퍼 및 LastUsedPatternType/_patternReadyAt 추적 필드 추가 (88줄 추가, 0줄 삭제)
- `Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs` - 라운드로빈 배열 제거, BuildCandidates 로 4종 후보 선언 + D-04 체인 분기 추가 (53줄 추가, 13줄 삭제)

## Decisions Made

플래너가 사전 확정한 결정을 그대로 적용 (실행자 재해석 없음):

1. **결정 1 (ChargeRange 미재활용):** RESEARCH.md 는 `SpiritFarProjectile` 에 `minDistance: spirit.ChargeRange`(5.0) 를 권고했으나, 플래너가 실제 소스를 검증해 채택하지 않음. `Spirit Clone.prefab` 의 `AttackRange: 2.5` 로 인해 `SelectAttackStrategy` 호출 구간이 `dist <= 3.5` 뿐이라 `minDistance=5.0` 은 이 구간과 절대 겹치지 않아 영구 선택 불가가 됨. `ChargeRange` 필드는 기존부터 있던 데드 코드이므로(CLAUDE.md 3항) 손대지 않고 그대로 둠. 최종 적용 결과: `SpiritCombatState.cs` 에 `ChargeRange` 참조 0회 (grep 확인).
2. **결정 2 (Exhaustion 거리 조건 없음):** RESEARCH.md Open Question 1 을 플래너가 "거리 조건 없음"으로 확정. `SpiritExhaustion.ExecuteAttack` 이 `boss.StopMove()` 만 수행하고 플레이어 위치를 전혀 참조하지 않으며, D-04 체인(취약→급습)의 시작점이므로 거리와 무관하게 발동해야 함이 근거. 부수 효과로 거리 무조건 후보 3개(Charge/Exhaustion/FarProjectile) 확보되어 후보 고갈로 인한 null 스핀 위험이 구조적으로 완화됨 (최종 적용: `BuildCandidates` 에서 세 패턴 모두 `minDistance`/`maxDistance` 인자 미부여).
3. 가중치 확정값: `ChargeWeight = 1.0f`, `ExhaustionWeight = 0.6f`, `WakeRepelWeight = 1.0f`, `FarProjectileWeight = 1.0f`. 모두 `SpiritCombatState.cs` 상단 private const 4개로 위치, 밸런싱 시 이 값만 조정하면 선택 로직 변경 불필요.
4. 쿨다운은 `Time.time` 절대 시각 비교 방식 채택 (`WaterMonsterCombatState._lastWaveTime` 선례와 일치) — `Execute()` 의 early-return 분기로 인해 매 프레임 감산 타이머는 누락 위험이 있기 때문.
5. 추적 키는 슬롯 인덱스가 아닌 `System.Type` 채택 — 이번 페이즈가 고치는 버그가 정확히 "인덱스 기반 장부" 였으므로, `SpiritCharge` 중복 슬롯이 타입 키로 인해 자연히 후보 1개로 정리됨(D-06a).

## Deviations from Plan

None - plan executed exactly as written. 코드 내용은 07-01-PLAN.md 의 `<action>` 블록에 지정된 텍스트를 그대로 적용했다.

## Issues Encountered

None.

## Known Interactions (플랜 <output> 요구사항에 따른 기록)

**Stage 2 헤비콤보가 D-04 체인을 선점하는 경우 (수용된 동작):** `Stage2CombatState` 에서 `SpiritExhaustion` 이 3번째 일반 패턴(`_patternsExecuted >= 3` 직전)으로 실행되면, 다음 `SelectAttackStrategy` 호출은 `_patternsExecuted >= PatternsBeforeHeavyCombo` 조건이 먼저 걸려 헤비콤보로 진입하므로, 같은 사이클에서는 D-04 체인(WakeRepel 강제)이 선점되어 발동하지 않는다. 이는 D-07a("Stage2 카운터 로직 그대로 유지")를 지키기 위한 필연적 귀결이며 플랜에서 의도된 동작으로 명시적으로 수용되었다. `Stage2CombatState.cs` 는 이번 페이즈에서 한 줄도 변경되지 않았으므로(검증: `git diff --exit-code` 성공) 이 상호작용은 코드 변경 없이 기존 계약(`base.SelectAttackStrategy` null/non-null) 만으로 발생하는 자연스러운 결과다.

## Deferred (후속 페이즈 후보)

- `WaterMonsterCombatState` 를 이번에 추가한 `PatternCandidate`/`SelectWeightedPattern` 범용 헬퍼로 마이그레이션 — 이번 페이즈 범위 밖(D-08b)이며 현재는 자체 풀 랜덤 로직을 그대로 유지 중. CONTEXT.md 에 Deferred 항목으로 이미 기록됨.

## User Setup Required

None - no external service configuration required.

## Self-Check: PASSED

- FOUND: Assets/Enemy/NewBoss/Script/States/CombatState.cs
- FOUND: Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs
- FOUND commit: 41117c9
- FOUND commit: f77a890

## Next Phase Readiness

- `CombatState` 범용 헬퍼가 확보되어 향후 다른 보스도 동일 패턴으로 조건 기반 판단을 도입할 수 있음
- `SpiritCombatState` 는 이제 정령 보스의 패턴 선택이 고정 순서가 아닌 조건+가중치 기반으로 동작 (Phase 5 D-03a 의도 복원)
- Unity Editor 컴파일 검증은 병렬 실행 환경(다른 워크트리 에이전트와의 락 경합 방지)을 위해 이 실행에서는 수행하지 않음 — 오케스트레이터의 훅 검증 단계 또는 Unity Editor 수동 확인 필요. 코드는 플랜의 `<action>` 지정 텍스트를 문자 그대로 적용했고 모든 acceptance_criteria grep 검증을 통과함.

---
*Phase: 07-boss-attack-pattern-judgment*
*Completed: 2026-07-27*
