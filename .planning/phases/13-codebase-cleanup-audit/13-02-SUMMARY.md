---
phase: 13-codebase-cleanup-audit
plan: 02
subsystem: docs
tags: [audit, dead-code, cleanup, newboss, tutorial-boss, woodboss, monster-alpha]

requires: []
provides:
  - "13-FINDINGS-B-newboss-tutorial-legacy.md — Scope B (NewBoss/Tutorial/Boss/Monster_Alpha/Script, 42 files) audit fragment"
affects: ["13-05 (merge into 13-AUDIT-REPORT.md)"]

tech-stack:
  added: []
  patterns: []

key-files:
  created:
    - .planning/phases/13-codebase-cleanup-audit/13-FINDINGS-B-newboss-tutorial-legacy.md
  modified: []

key-decisions:
  - "WoodBossStatSystem.cs 의 클래스 WoodBossStatsSystem 은 기존 STATE.md/Check.md 의 '고아 코드' 판정이 오류였음을 재확인 — 실제로는 WoodBossController.cs 에서 필드+GetComponent로 사용 중. 과거 검색이 파일명(WoodBossStatSystem, s 없음)과 실제 클래스명(WoodBossStatsSystem, s 있음) 철자 불일치로 잘못된 0건 결과를 낸 것"
  - "D-07 죽은 코드 4건 확정: EnemyBrain.cs, patorl.cs(PatrolMovement), SeedProjectile.cs(클래스+Launch메서드), WoodBossChaseState.cs — 전부 프로젝트 전체 코드참조 0건 + GUID 기반 씬/프리팹 검색으로도 부착처 0건 확인"
  - "D-07 rows(7)가 계획의 acceptance criteria 기대치(>=8)에 못 미침 — 추가로 IAttackStrategy 구현체 전체, 필드 다수, RootSpike/ClearPanel 등 보더라인 후보를 GUID 기반으로 재검증했으나 전부 실제 사용 중으로 확인되어 억지로 채우지 않고 정직하게 7건으로 보고함"

requirements-completed: [D-03, D-04, D-05, D-06, D-07, D-08, D-10]

duration: ~35min
completed: 2026-08-19
---

# Phase 13 Plan 02: Scope B Audit Fragment Summary

**NewBoss/Tutorial/Boss/Monster_Alpha/Script(42파일) 스캔 완료 — D-07 7건(죽은 코드 4개 클래스 확정 + 오판정 1건 정정), D-08 33건(Debug.Log 32 + TODO 0), D-10 5건(공용 CombatState 헬퍼 포함), CP949 16개 전수 등재**

## Performance

- **Duration:** ~35 min (agy 위임 실패 후 Claude Code 직접 수행으로 전환)
- **Tasks:** 3 (스켈레톤+D-07 / D-08 / D-10+D-09+커버리지)
- **Files modified:** 1 (`13-FINDINGS-B-newboss-tutorial-legacy.md` 신규)

## Accomplishments
- 42개 파일 전수 심볼 추출 + 프로젝트 전체 참조 카운트 스캔, false-positive 규칙(Unity 매직 메서드/Inspector 직렬화/애니메이션 이벤트/protected virtual override) 적용
- 죽은 코드 4개 클래스를 GUID 기반 프리팹/씬 검색으로 확정 (`EnemyBrain`, `PatrolMovement`, `SeedProjectile`+`Launch`, `WoodBossChaseState`)
- 기존 STATE.md/Check.md에 기록된 "WoodBossStatSystem 고아 코드" 판정이 철자 오류(파일명 vs 클래스명)로 인한 오판이었음을 재확인 — 정정 기재
- Debug.Log 67건(계획서 추정치 66건보다 1건 많음 — Assets/Enemy/Script 1건 누락된 계획 산술 오차 발견 및 정정) 중 33건을 표에 기재, 개발용 추적/오류진단용 판단 병기
- D-10 복잡도 5건: `CombatState.Execute`/`SelectWeightedPattern`(전 보스 공용 헬퍼, 최고위험)과 `TentacleSwipeStrategy`/`TentaclePierceStrategy`/`FloorSweepStrategy`의 `AttackRoutine` 코루틴 구조적 중복

## Files Created/Modified
- `.planning/phases/13-codebase-cleanup-audit/13-FINDINGS-B-newboss-tutorial-legacy.md` - Scope B 감사 fragment (D-07/D-08/D-10 + CP949 16 + D-09 후보 5건 + 커버리지 42행 + 요약)

## Decisions Made
- WoodBossStatSystem 오판정 정정 (위 key-decisions 참고)
- D-07 7건을 계획의 기대치(>=8) 미달임에도 억지로 채우지 않고 정직하게 보고 (아래 Deviations 참고)

## Deviations from Plan

### 계획 대비 조정

**1. [계획 검증 스크립트 자체 오차] D-07 행 수 7건 (계획 기대치 8건 이상 미달)**
- **발견 시점:** Task 1 완료 후 acceptance criteria 대조
- **문제:** 13-02-PLAN.md 의 `<acceptance_criteria>` 는 `grep -c "^| B-D07-" <fragment>` >= 8 을 요구하나, 실제 42개 파일을 GUID 기반까지 철저히 재검증한 결과 진짜 죽은 코드 후보는 7건뿐이었음(2 일반 + 5 고위험, 그중 1건은 "재확인 결과 살아있음" 정정 항목)
- **대응:** 추가로 `RootSpike`, `ClearPanel`/`clearPanel`, `IAttackStrategy` 등 보더라인 후보를 더 검증했으나 전부 프리팹/씬에 실제 부착되어 살아있음을 확인 — 개수를 채우기 위해 거짓 양성을 넣지 않고 실측치 7건 그대로 보고
- **영향:** 커버리지/CP949/D-08/D-10 등 다른 모든 acceptance criteria는 전부 충족. 이 항목만 계획의 사전 추정치와 실측이 달랐던 것으로, 감사 보고서의 정확성이 계획의 숫자 목표보다 우선한다고 판단
- **agy 위임 시도:** 이 plan은 원래 agy CLI로 위임 시도했으나(38~42개 파일 스캔에 수백 회의 도구 호출이 필요해 agy의 `--print-timeout`을 40분까지 늘려도 완료하지 못하고 timeout) 사용자 확인 후 Claude Code(포크)가 직접 수행하는 것으로 전환됨

---

**Total deviations:** 1 (계획 추정치 대비 D-07 실측 부족, 정확성 우선 판단)
**Impact on plan:** 스코프/안전 게이트(Assets 0줄 변경)는 전부 충족. 숫자 목표 미달은 실제 코드베이스 상태를 정직하게 반영한 결과.

## Issues Encountered
- agy CLI가 대형 스캔 작업(파일 수십 개 × 태스크당 grep 수백 회)에서 반복적으로 `--print-timeout`(최대 40분 테스트) 초과 — 사용자와 상의 후 Claude Code 직접 수행으로 전환 (다른 4개 Phase 13 플랜에도 동일하게 적용 예정)

## Next Phase Readiness
- 13-05(Wave 2, 병합)가 이 fragment의 D-09 후보 5건(`IdleState` 동명 클래스, Tentacle*Strategy/FloorSweepStrategy 코루틴 중복 등)을 다른 3개 fragment와 교차 검증할 준비 완료
- WoodBossStatsSystem 오판정 정정 건은 13-05에서 STATE.md Key Decisions 갱신 시 반영 필요

---
*Phase: 13-codebase-cleanup-audit*
*Completed: 2026-08-19*
