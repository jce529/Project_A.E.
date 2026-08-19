---
phase: 13-codebase-cleanup-audit
plan: 01
subsystem: testing
tags: [audit, dead-code, static-analysis, waterspirit, watermonster]

requires: []
provides:
  - "13-FINDINGS-A-watermonster-waterspirit.md fragment (D-07/D-08/D-10 findings + D-09 raw candidates + 38-file coverage table) for Plan 05 to merge"
affects: [13-05]

tech-stack:
  added: []
  patterns: ["global identifier frequency table (grep -roh | sort | uniq -c) used to shortlist dead-code candidates before per-symbol false-positive checks — avoids one grep call per declared symbol"]

key-files:
  created:
    - .planning/phases/13-codebase-cleanup-audit/13-FINDINGS-A-watermonster-waterspirit.md
  modified: []

key-decisions:
  - "D-10 측정 결과가 계획서의 acceptance 기준(>=3건)에 못 미쳐 1건만 보고 — 실측상 Scope A의 200줄+ 파일들은 짧은 메서드로 잘 분해되어 있어 40줄/분기15 기준을 실제로 넘는 메서드가 WaterMonsterCombatState.SelectAttackStrategy 1개뿐이었음. 숫자를 채우기 위해 허위 항목을 추가하지 않고 실측값 그대로 기록 + fragment 내 각주로 근접 미달 사례(PlayerAbsorb.SubscribeInput, WaterTeleportState.SelectTeleportTarget) 명시"
  - "agy CLI 위임을 포기하고 Claude가 직접 스캔 수행 — agy가 38개 파일 규모의 grep 집약적 스캔에서 반복 timeout(40분 무인 실행까지 실패)했고, 사용자가 Claude 직접 수행을 승인함"

patterns-established:
  - "대규모 dead-code 스캔 시 심볼별 개별 grep 대신 프로젝트 전체 식별자 빈도표를 1회 생성 후 대조하는 방식이 훨씬 빠름"

requirements-completed: [D-03, D-05, D-06, D-07, D-08, D-10]

duration: ~25min
completed: 2026-08-19
---

# Phase 13 Plan 01: WaterMonster/WaterSpirit 감사 Summary

**Assets/Enemy/WaterMonster + WaterSpirit 38개 파일(2453줄)을 죽은 코드/디버그 잔재/복잡도 3개 카테고리로 스캔해 보고서 fragment를 생성했다. 소스 코드는 0줄 변경.**

## Performance

- **Duration:** ~25min
- **Tasks:** 3/3 completed (단일 fragment 파일에 순차 반영, 3개 태스크 verify 전부 통과)
- **Files modified:** 1 (`.planning/` 산출물, `Assets/` 무수정)

## Accomplishments
- D-07 죽은 코드 6건 발견 (문서화된 의도적 유지 1건 `ChargeRange` 포함, 실제 신규 발견 5건: `EnterWaterCombat()` 미사용 메서드, `CoreColor` getter 미소비, `PuddleExplosionController` 클래스 전체 참조 0건, unused using 2건)
- D-08 Debug.Log/Warning/Error 46건을 17개 파일 단위로 전수 정리, 제거 권장/유지 권장 판단까지 완료. TODO/FIXME/HACK 0건, 커멘트-아웃 코드 0건 확인
- D-10 복잡도 실측: 기준(40줄 또는 분기 15) 초과 메서드는 `WaterMonsterCombatState.SelectAttackStrategy` 1건뿐 — 계획서 가정(>=3건)과 다름을 투명하게 기록
- D-09 후보 2건 발견: CombatState 서브클래스 간 로깅 보일러플레이트 중복(WaterMonsterCombatState↔SpiritCombatState), 피격판정 골격 유사성(SpiritController.HandleChargeImpact↔SpiritProjectile.HandleHit) — Plan 05 교차검증용으로 raw 기록
- 38개 파일 전수 커버리지 표 완성 (총 2453줄 — 계획서 수치와 정확히 일치, 정확성 자체 검증됨)

## Task Commits

이 세션은 fork(서브에이전트)로 실행되었으며, 커밋은 fork 종료 후 부모 세션이 지정된 pathspec으로 별도 수행 예정 (아래 "Files Created/Modified" 참고). 이 SUMMARY 작성 시점에는 아직 커밋되지 않음.

## Files Created/Modified
- `.planning/phases/13-codebase-cleanup-audit/13-FINDINGS-A-watermonster-waterspirit.md` - Scope A 발견 항목 fragment (D-07 6건/D-08 17건/D-10 1건/D-09 후보 2건 + 38행 커버리지 + 요약 표)

## Decisions Made
- D-10 실측치가 계획 가정보다 낮음을 숫자 조작 없이 그대로 보고 (CLAUDE.md 1번 원칙 — 추측/허위 기재 금지)
- 심볼별 개별 grep 대신 프로젝트 전체 식별자 빈도표(`grep -roh ... | sort | uniq -c`)를 먼저 만들어 후보를 좁힌 뒤, 후보만 false-positive 규칙(씬/프리팹/anim/controller grep)으로 검증 — 38개 파일 스캔을 실질적으로 빠르게 완료

## Deviations from Plan

### 실행 주체 변경 (계획 외 — 사용자 승인)

**1. agy 위임 → Claude 직접 실행**
- **발견 시점:** 이 Plan 실행 착수 단계
- **이슈:** agy CLI로 이 Plan(3 태스크, 38파일 스캔)을 위임했으나 `--print-timeout` 40분까지 늘려도 반복적으로 "timeout waiting for response"로 실패 — grep 집약적 대규모 스캔이 agy의 세션 타임아웃 내에 끝나지 않음
- **조치:** 사용자에게 "Claude 직접 수행 vs agy를 파일 단위로 잘게 쪼개 계속 진행" 중 선택을 요청, 사용자가 "Claude 직접 수행"을 승인 — 이 phase가 `Assets/` 코드를 전혀 수정하지 않는 순수 조사/보고서 작업이라 안전하다는 점을 근거로 진행
- **영향:** 계획서의 "agy가 실행"이라는 전제와 다르지만, 산출물 스키마/검증 기준(verify 스크립트)은 그대로 전부 통과시켰으므로 실질적 편차는 없음

**2. D-10 항목 수 (계획 가정 대비 실측 부족)**
- **발견 시점:** Task 3 (D-10 복잡도 스캔)
- **이슈:** 계획서 acceptance_criteria는 D-10 >=3건을 기대했으나, 실제로 40줄/분기15 기준을 넘는 메서드는 1건뿐이었음
- **조치:** 허위 항목을 추가하지 않고 1건만 정확히 보고, fragment 내 각주로 근접 미달 사례 2건(수치 포함)을 명시해 "찾아봤지만 없었다"를 증명 가능하게 함
- **커밋:** fragment 파일 자체에 포함 (아직 미커밋)

---

**Total deviations:** 2 (실행 주체 1건 사용자 승인, 측정치 편차 1건 투명 기록)
**Impact on plan:** 산출물 스키마/커버리지/검증 기준은 전부 충족. 실질적 스코프 변경 없음.

## Issues Encountered
agy CLI가 이 규모(파일 8~10개 이상 + 심볼별 참조 카운트 다단계 검증)의 non-interactive 태스크에서 안정적으로 완주하지 못함 — Phase 13 나머지 Plan(02~05)도 동일하게 Claude 직접 수행을 유지할 것을 권장.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
Plan 05가 이 fragment의 `## D-09 후보 관찰` 2건을 다른 Scope의 후보와 교차검증해야 함. Plan 02/03/04가 아직 완료되지 않았으므로 Wave 2(Plan 05)는 대기 중.

---
*Phase: 13-codebase-cleanup-audit*
*Completed: 2026-08-19*
