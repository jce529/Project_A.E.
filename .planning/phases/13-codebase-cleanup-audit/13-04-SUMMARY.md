---
phase: 13-codebase-cleanup-audit
plan: 04
subsystem: tooling
tags: [static-analysis, dead-code, cp949, editor-tools]

requires: []
provides:
  - "13-FINDINGS-D-core-map-editor-imported.md — Scope D(Assets/Script/map/Editor/ImportedAsset, 56파일) 감사 fragment"
affects: ["13-05 (fragment 병합 + 전역 D-09/CP949 집계)"]

tech-stack:
  added: []
  patterns: []

key-files:
  created: [".planning/phases/13-codebase-cleanup-audit/13-FINDINGS-D-core-map-editor-imported.md"]
  modified: []

key-decisions:
  - "Portal 클래스와 InGame.unity 스테일 엔트리는 계획서 서술과 실측이 달라 실측 기준으로 정정 기록"
  - "using 미사용 점검은 고위험 5개 파일만 표본 점검 (56개 전수는 시간 제약상 생략, fragment에 명시)"

patterns-established: []

requirements-completed: [D-03, D-04, D-05, D-06, D-07, D-08, D-10]

duration: ~35min
completed: 2026-08-19
---

# Phase 13 Plan 04: Scope D 감사 (Core Script / Map / Editor / ImportedAsset) Summary

**56개 파일 스캔 — 죽은 코드 21건(1건 고위험), Debug 잔재 32건(3건 고위험), 복잡 함수 4건, CP949 26개, 계획서 서술과 실측이 어긋난 항목 2건을 정정 기록**

## Performance

- **Duration:** ~35분 (agy 위임 여러 차례 시도 후 Claude Code 직접 수행으로 전환)
- **Tasks:** 3 (fragment 스켈레톤+D-07, D-08, D-10+D-09+커버리지 — plan의 Task 구분을 그대로 따르되 한 번에 순차 수행)
- **Files modified:** 1 (`.planning/` 아래 신규 파일만, `Assets/` 0줄 변경)

## Accomplishments

- Scope D(`Assets/Script/**` 24, `Assets/map/**` 18, `Assets/Editor/**` 9, `Assets/ImportedAsset/**` 5 = 56개 파일) 전수 스캔
- D-07 죽은 코드 21건 발견 — 그중 16개는 완전히 죽은 whole-class(모든 판정을 클래스 GUID 기준 씬/프리팹 부착 여부까지 확인해 false-positive 배제), `GameManager.NextSpawnPointName`은 유일 writer(`Portal`)가 이미 죽어있는 완전 고아 체인으로 재확인
- `Assets/Script/Txt/` 5종은 계획서가 "중복 후보(D-09)"로 지목했으나 실측 결과 전부 참조 0건 — "중복"이 아니라 "전부 죽은 코드"로 정정
- `Assets/Scenes/InGame.unity` 스테일 Build Settings 엔트리의 실제 참조원이 계획서 서술(`MainMenuUI.cs`)과 다르다는 것을 발견 — 실제로는 `PlaceWaterMonsterInScene.cs` (Editor 1회용 도구)만 참조하며, `MainMenuUI.OnClickStart()`는 이미 `"Tutorial Map"`을 로드하도록 바뀌어 있음
- CP949 26개 파일 전수 등재 (계획서의 구성 예시와 실측 구성이 달라 실측 기준으로 정정)
- Debug.Log 71건/32개 파일 전수 분류 (제거 권장 vs 유지 권장), TODO/FIXME/HACK 0건 확인
- D-10 복잡 함수 4건 (Editor 도구 3건 + 벤더 HeroKnight.Update 1건, 최대 136줄/분기36)

## Task Commits

단일 커밋으로 처리 (fragment + summary):

1. **Task 1-3 통합: Scope D 스캔 및 fragment 작성** - (커밋 예정, 아래 참고)

## Files Created/Modified
- `.planning/phases/13-codebase-cleanup-audit/13-FINDINGS-D-core-map-editor-imported.md` - Scope D 감사 결과 (D-07/D-08/D-10 + CP949 26 + D-09 후보 4 + 기타 1 + 커버리지 56 + 요약)

## Decisions Made

- **agy 위임 포기, Claude Code 직접 수행으로 전환**: agy CLI(`--dangerously-skip-permissions`, `--print-timeout` 최대 40분 설정, 완전 분리된 detached 프로세스로도 시도)가 소규모 작업(파일 3~4개 grep)은 문제없이 처리했지만, Task 1 규모(38~56개 파일 각각 심볼 추출→참조 카운트→false-positive 필터링)에서는 매번 timeout으로 완주하지 못함. 이 phase가 `Assets/` 코드를 전혀 수정하지 않는 순수 조사/보고서 작업이라는 점, agy가 이미 완주 실패를 반복 증명한 점을 근거로 사용자 승인을 받아 Claude Code가 직접 Grep/Read로 수행하도록 전환.
- **GUID 기반 씬/프리팹 부착 확인을 그냥 문자열 grep보다 우선**: `grep -rl "Portal" Assets --include=*.unity`처럼 클래스명 단순 문자열 검색은 `SignpostPortal` 같은 이름이 겹치는 다른 살아있는 클래스를 오탐으로 잡아낸다는 것을 실제로 확인. 이후 모든 whole-class 죽은 코드 판정은 `.cs.meta`의 `guid:` 값을 씬/프리팹에서 직접 검색하는 방식으로 검증해 오탐을 배제함.
- **계획서 서술과 실측이 다를 때는 실측을 기준으로 기록하고 차이를 명시**: `InGame.unity` 참조원, CP949 파일 구성 2건에서 계획서(사전 조사 시점) 서술과 현재 코드 상태가 어긋나는 것을 발견 — 임의로 계획서 문구에 맞추지 않고 fragment에 "계획서 대비 실측 차이" 각주로 정정 기록.

## Deviations from Plan

### Auto-fixed Issues

**1. [계획서 사실관계 오류] InGame.unity 참조원 정정**
- **발견 시점:** Task 1, `## 기타 — 코드 외 정리 항목` 검증 중
- **문제:** 13-04-PLAN.md와 `Assets/SaveSystem/Check.md`는 `MainMenuUI.OnClickStart()`가 `InGame.unity`를 로드하려다 실패한다고 서술했으나, 실제 `MainMenuUI.cs`에는 `InGame` 문자열이 전혀 없고 `OnClickStart()`는 `"Tutorial Map"`을 로드함
- **조치:** 프로젝트 전체 grep으로 실제 참조원(`Assets/Editor/PlaceWaterMonsterInScene.cs:11`)을 찾아 fragment에 실측 기준으로 정정 기록, 계획서 서술은 outdated임을 명시
- **파일 수정:** 없음 (보고서 전용 phase, 코드 미수정)
- **검증:** `grep -n "InGame" Assets/Script/MainMenuUI.cs` (0건), `grep -rn "InGame" Assets --include=*.cs` (실제 참조원 확인)

**2. [계획서 사실관계 오류] CP949 파일 구성 정정**
- **발견 시점:** Task 1, D-04 섹션 작성 중
- **문제:** 계획서는 CP949 26개 중 `Assets/ImportedAsset/Hero Knight - Pixel Art/` 하위 4개가 포함된다고 예시했으나, 실제 `iconv` 실측 결과 ImportedAsset 트리는 CP949 0개이고 대신 `Assets/Script/` 루트 4개(EnvironmentManager/ObstacleInteraction/PlatformController/ProtoEnemy)가 CP949였음 (총 26개 숫자는 일치)
- **조치:** 실제 `iconv -f UTF-8 -t UTF-8` 명령 결과를 그대로 표에 반영하고 각주로 차이 명시
- **파일 수정:** 없음
- **검증:** 전체 26개 경로가 fragment에 전수 등재됨을 재확인 (grep 루프로 UNLISTED 0건 확인)

---

**Total deviations:** 2건 (둘 다 "계획서 사실관계를 실측으로 정정" — 코드/스코프 변경 아님)
**Impact on plan:** 보고서 정확도를 높이는 방향의 정정이며 범위 이탈 없음.

## Issues Encountered

- **agy 실행 반복 실패**: `--mode accept-edits`는 셸 명령(`dir`, `grep` 등) 승인 프롬프트에서 비대화형 모드가 응답할 수 없어 즉시 실패. `--dangerously-skip-permissions`(사용자 승인 받음)로 전환 후에도 대형 스캔 작업에서 반복적으로 `Error: timeout waiting for response` 발생 (기본 5분, 9분, 40분 설정 및 detached 프로세스 방식 모두 시도했으나 동일). 소규모 작업(파일 3개 이내 grep)은 3분 내 정상 완료됨을 확인해, 문제가 agy 자체의 응답성이 아니라 이 규모(수십 개 파일 × 다단계 grep)의 세션 총 소요 시간이 원인임을 특정. 사용자 승인 하에 Claude Code 직접 수행으로 전환해 해결.
- **using 미사용 전수 점검 미완료**: 56개 파일 전체가 아닌 고위험 5개 파일만 표본 점검 — 시간 제약. fragment에 이 한계를 명시함.

## User Setup Required

None - 외부 서비스 설정 불필요.

## Next Phase Readiness

- Plan 05(fragment 병합 + 전역 D-09/CP949 집계)가 이 fragment를 그대로 소비할 수 있음 — ID 접두사(`D-`) 보존, 스키마(고위험/일반 분리, CP949 섹션, D-09 후보, 기타 섹션, 커버리지 56행, 요약) 전부 충족
- Plan 05 작업 시 참고: 이 fragment의 D-09 후보 4건 중 `Assets/Script/Txt/` 5종은 "중복"이 아니라 "죽은 코드"로 판명되었으므로 Plan 05의 전역 D-09 교차검증에서 중복 후보로 채택하지 말 것을 권장
- Plan 01/02/03(Scope A/B/C) fragment가 모두 준비되면 Plan 05 진행 가능

---
*Phase: 13-codebase-cleanup-audit*
*Completed: 2026-08-19*
