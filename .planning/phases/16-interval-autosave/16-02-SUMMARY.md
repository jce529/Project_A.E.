---
phase: 16-interval-autosave
plan: 02
subsystem: save-ui
tags: [autosave, tmp, coroutine]
requires: [16-01]
provides: [autosave-notice]
affects: [16-03]
key-files:
  created: [Assets/SaveSystem/Script/AutoSaveNotice.cs, Assets/SaveSystem/Script/AutoSaveNotice.cs.meta]
  modified: [Assets/SaveSystem/Script/AutoSaveTimer.cs]
metrics:
  tasks_completed: 2
  completed: 2026-09-14
requirements-completed: [D-10]
---

# Phase 16 Plan 02 — 자동저장 알림

자동저장이 성공하면 영속 AutoSave 오브젝트의 알림 컴포넌트가 우하단에 `자동 저장됨`을 표시한다. 1.2초 유지하고 0.6초 동안 사라지며, 일시정지 중에도 unscaled 시간으로 페이드가 진행된다.

## Task commits

| Task | Commit | 변경 |
|---|---|---|
| 1 | b683d94 | AutoSaveNotice와 고유 GUID 생성 |
| 2 | 05e4dec | 타이머 부트스트랩 부착 및 저장 성공 후 Show 연결 |

## 검증

- 두 태스크 모두 dotnet build 오류 0·기존 경고 6. 신규 Notice/Timer가 실제 Compile 항목에 각각 1회 포함됨.
- 최종 정적 검사 50/50 PASS: 계획 코드 일치, 저장 뒤 알림 1회, 수동 저장에 알림 없음, unscaled 유지/페이드, 우하단 앵커, 입력 비차단, GUID 유일성, 기존 Assets 바이트 보존.
- 한국어 문구 UTF-8: `ec9e90eb8f9920eca080ec9ea5eb90a8`. Notice UTF-8/LF/BOM 없음, Timer ASCII/CRLF/BOM 없음.
- 실제 폰트 글리프·배치·타이밍·씬 전환은 16-03 Unity Play 모드 검증 대기.

## Deviations

- [BUG-010](bugs/BUG-010-plan-acceptance-false-positives.md): 계획의 주석 포함 grep는 금지 의존성을 오탐하므로 주석 제외 코드로 대체 검증했다.
- Task 2의 제시 코드에는 빈 줄 2개가 있어 실제 변경은 +8/-0줄이다(계획의 +7/-0 기대값 오기). 제시 코드를 그대로 구현했다.
- AutoSaveNotice는 2줄에 총 3회 등장한다. 행 수와 출현 횟수를 각각 검증했다.

## Self-Check: PASSED

두 구현 태스크와 컴파일/정적 검증 완료. 커밋 두 개 존재. Play 모드 미검증.
