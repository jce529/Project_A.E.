---
phase: 21-inventory-ui-keybinding-pickup
plan: 06
subsystem: docs
tags: [ssot, verification]
requirements-completed: [D-01, D-02, D-03, D-04, D-05, D-06, D-07, D-08, D-09, D-10, D-11, D-12, D-13, D-14]
completed: 2026-11-05
---

# Phase 21 Plan 06: SSOT 갱신과 통합 Play 검증 Summary

`item-and-inventory.md`, `player-runtime.md` 를 Phase 21 구현 사실로 갱신하고, `Assets/Item/Check.md` 에 `## Phase 21 인벤토리 UI 검증` 과 `## 결과 기록 (Phase 21)` 을 기록했다.

## 결과

**PASS 11 / FAIL 1 / 미확인 2** (상세는 `Assets/Item/Check.md`).

- 미확인: 디스크 `Save()`/`LoadGame()` 왕복(사용자의 `save.json` 을 덮어쓰므로 미실행, 메모리 수준 캡처/복원은 PASS), Phase 18 UAT 7항목 중 4항목 재실행.
- FAIL: Console 예외 0건 — `CameraBoundsTrigger.OnTriggerExit2D` NRE 1회(원인 미확정)와 컨트롤 탭 Reset 의 기존 `ArgumentOutOfRangeException`(BUG-011). Phase 21 코드에서 난 예외는 0건.
- 확인하지 못한 것: 실제 물리 키보드/마우스 입력, `1 stage` 씬 Play, 사용자가 직접 보는 화면 느낌.

## Task Commits

| Task | 커밋 |
|---|---|
| 1. SSOT 2개 갱신 | 이 plan 의 문서 커밋 (`docs(21-06)`) |
| 2. 통합 Play 검증 기록 | 같은 커밋 (Check.md) |

## 실행 편차

- **검증 환경:** 워크트리에는 `Library` 가 없어 메인 체크아웃의 에디터를 썼다. 메인에는 커밋 안 된 Spirit 변경과 다른 세션(Phase 23)의 코드 수정/커밋이 있었다. 변경 파일 9개를 메인 작업 트리에 복사해 검증하고 끝나면 `git restore`/삭제/원본 씬 복사로 원복했다. 메인에는 커밋하지 않았고, Phase 23 의 커밋에 제 파일이 섞이지 않은 것을 확인했다. 그 세션의 재컴파일로 도메인 리로드가 두 번 있어 Play 세션이 두 번 초기화됐다.
- **Play 중 자동 정지:** 프로젝트의 Console Error Pause 때문에 NRE 1회에 Play 가 멈췄다. 일시정지를 풀고 이어갔다.

## 검증 환경 사고 (사용자 확인 필요)

검증 중 입력이 액션에 전달되지 않자, 에디터의 Input System 설정을 메모리 복제본으로 바꿔 Play 를 반복했다. Play 종료 때 Input System 이 파괴된 복제본 설정을 복원하려다 실패했고(`ArgumentNullException ... InputManager.set_settings`), 에디터가 입력 장치를 잃었다. 저장된 상태 객체(`InputSystemObject`)를 제거하고 스크립트를 다시 로드해 에러는 멈췄지만, **에디터의 입력 장치 목록이 비어 있어(물리 키보드/마우스 미등록) 에디터를 재시작해야 정상으로 돌아올 것으로 보인다.** 이후 검증은 가상 장치를 추가해서 진행했다.
또한 마지막 원복 직후 에디터가 응답하지 않는다(씬 파일이 외부에서 바뀌어 Unity 가 대화상자를 띄운 것으로 추정). 그 대화상자에서 "Reload"(디스크의 원본)를 선택하고, 메인 에디터를 재시작하는 것을 권한다. 메인의 씬 파일은 검증 전 상태(사용자의 Spirit 변경 포함)로 복원돼 있다.

## 후속 (이 phase 밖)

- BUG-011: `BasicAttack bindingIndex 1` 근본 수정.
- Paused 상태 입력 누수(메뉴 버튼 LMB -> BasicAttack).
- `CameraBoundsTrigger.OnTriggerExit2D` NRE 원인 조사.
- 실제 키보드 입력으로 인벤토리 Play 확인(사용자), `1 stage` 씬 컨트롤 탭 확인.
- `ROADMAP.md` Phase 21 Goal/플랜 목록 반영(총괄).
