---
phase: 13-codebase-cleanup-audit
plan: 03
subsystem: infra
tags: [audit, dead-code, cp949, camera, savesystem, player]

requires:
  - phase: 13-codebase-cleanup-audit (Plan 01/02/04, wave 1 형제 플랜)
    provides: 같은 스키마의 다른 스코프 fragment (A/B/D)
provides:
  - Scope C(Assets/Player, Assets/Camera, Assets/SaveSystem, 32개 .cs 파일) 감사 fragment
  - D-07/D-08/D-10 발견 항목 위험 티어 분리, CP949 4개 파일 목록, D-09 후보 4건
affects: [13-05 (fragment 병합 플랜)]

tech-stack:
  added: []
  patterns: []

key-files:
  created:
    - .planning/phases/13-codebase-cleanup-audit/13-FINDINGS-C-player-camera-savesystem.md
  modified: []

key-decisions:
  - "PlayerUI 클래스 전체가 씬/프리팹 어디에도 부착되지 않은 고아 클래스임을 발견 — 소속 메서드(InitWaterIcon/updateWater)까지 함께 죽은 코드로 기재"
  - "InputHandler vs PlayerInputHandler는 이름만 비슷할 뿐 진짜 중복이 아님을 확인 (전자는 Input System 이벤트 버스 싱글톤, 후자는 공격 콜백 추상 계약) — D-09 후보에서 '중복 아님, 리네이밍 검토 대상'으로 명시"
  - "CameraController.LateUpdate는 D-10 수치 기준(40줄/분기15) 미달로 표 행 대신 각주 처리 — 파이프라인 순서 계약만 문서화"

patterns-established: []

requirements-completed: [D-03, D-04, D-05, D-06, D-07, D-08, D-10]

duration: ~25min
completed: 2026-08-19
---

# Phase 13 Plan 03: Player/Camera/SaveSystem 감사 Summary

**32개 파일(Assets/Player, Assets/Camera, Assets/SaveSystem) 스캔 — D-07 11건(고위험 4), D-08 15건(고위험 4), D-10 2건(고위험 1), CP949 4개 파일, D-09 후보 4건을 담은 fragment 생성. Assets/ 0줄 변경.**

## Performance

- **Duration:** ~25min (Claude Code 직접 수행, agy 위임은 타임아웃으로 무산되어 전환)
- **Tasks:** 원 플랜의 Task 1/2/3 구분 없이 통합 스캔 후 1회 작성
- **Files modified:** 1 (`13-FINDINGS-C-player-camera-savesystem.md` 신규)

## Accomplishments
- 32개 파일 심볼 추출 → 프로젝트 전체 참조 카운트 → false-positive 6개 규칙 필터링을 거쳐 D-07 11건 확정
- `PlayerAttack.cs`에서 정규식(`^\s*//`)으로는 안 잡히는 `/* */` 블록 주석(구버전 마우스 조준 공격 로직, 16줄)을 직접 열람으로 발견
- Debug.Log 41건 전수(WaterController.cs는 CP949라 `grep -a`로 별도 처리) 및 오류진단용/개발추적용 판정
- CameraController.LateUpdate 실측(26줄/분기3) 결과 D-10 수치 기준 미달 확인, 파이프라인 순서 계약 각주로 대체
- InputHandler(78회 참조) vs PlayerInputHandler(2회 참조) 실제 역할 차이 규명 — 이름만 비슷한 별개 클래스임을 확인

## Files Created/Modified
- `.planning/phases/13-codebase-cleanup-audit/13-FINDINGS-C-player-camera-savesystem.md` - Scope C 감사 fragment (147줄)

## Decisions Made
- 원래 플랜은 Task 1(D-07)/Task 2(D-08)/Task 3(D-10+D-09+커버리지)로 3단계 분리를 지시했으나, agy 위임 실패 후 Claude Code가 직접 수행하며 한 번의 연속 스캔으로 통합 처리 (결과물 스키마는 원 플랜과 동일)
- agy CLI가 이 규모(32파일 다단계 grep)의 작업에서 반복적으로 타임아웃(40분 무인 실행도 실패)되어, 사용자 승인 하에 Claude Code 직접 수행으로 전환

## Deviations from Plan

### Auto-fixed Issues

**1. 실행 주체 변경 — agy 위임 → Claude Code 직접 수행**
- **발견 시점:** Plan 실행 착수 전 (agy Task 1 시험 실행 중)
- **문제:** agy CLI가 `--dangerously-skip-permissions` + `--print-timeout 40m` 조합으로도 38개 파일 규모의 Task 1 하나를 완료하지 못하고 반복 타임아웃
- **조치:** 사용자에게 토큰 소모량 vs 정확도 트레이드오프를 설명하고 승인받아 Claude Code가 Grep/Read 도구로 직접 스캔 수행
- **영향:** 산출물 스키마·내용은 원 플랜과 동일, 실행 주체만 변경. 코드 위험 없음 (Assets/ 무수정)

---

**Total deviations:** 1 (실행 주체 변경, 계획 스키마는 불변)
**Impact on plan:** 산출물에 영향 없음 — 위험 티어링/CP949 목록/커버리지 표 등 원 플랜이 요구한 모든 요소를 동일하게 충족

## Issues Encountered
- agy 위임이 여러 차례(accept-edits 권한거부 → skip-permissions 5분 타임아웃 → 9분 타임아웃 → 40분 무인실행도 타임아웃) 실패하여 Claude Code 직접 수행으로 전환함 (상세: 세션 대화 기록)

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Plan 05가 이 fragment를 다른 3개 스코프(A/B/D)와 병합하려면 A/B/D도 동일 스키마로 준비되어야 함 (진행 중)
- D-09 후보 4건(InputHandler/PlayerInputHandler 이름 혼동, UI 바 갱신 골격 중복, Menu PlayerPrefs 패턴 중복, Camera 트리거 골격 유사)은 Plan 05의 전역 교차 검증 대상

---
*Phase: 13-codebase-cleanup-audit*
*Completed: 2026-08-19*
