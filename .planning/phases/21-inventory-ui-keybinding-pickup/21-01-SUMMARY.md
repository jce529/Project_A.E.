---
phase: 21-inventory-ui-keybinding-pickup
plan: 01
subsystem: input
tags: [unity, input-system, inventory]
requirements-completed: [D-01, D-02, D-05]
completed: 2026-11-05
---

# Phase 21 Plan 01: Inventory 입력 액션과 게임플레이 입력 억제 Summary

`Inventory`(Tab) 액션과 `InputHandler.OnInventoryEvent`를 추가하고, `GameState.Inventory` 동안 게임플레이 입력 8종을 `InputHandler` 한 곳에서 억제했다.

## Task Commits

| Task | 커밋 |
|---|---|
| 1. `.inputactions` 에 액션 + Tab 바인딩 | a034d2d |
| 2. `InputHandler` 이벤트 + `GameplayInputBlocked` 가드(8개 핸들러) | 485a3f9 |

## 실행 편차

- **numstat 기준 불일치:** 계획의 "삭제 2 / 추가 22" 는 git 이 기존 닫는 중괄호를 새 블록의 것과 매칭해 **추가 20 / 삭제 0** 으로 표시한다. 내용은 계획의 두 블록과 동일하고 (`}` -> `},` 포함) JSON 검증은 통과했다. 기준 자체의 오류로 판단해 예외로 인정했다.
- **커밋 주체:** Codex 샌드박스가 워크트리 `.git`(index.lock)에 쓰지 못해 편집은 Codex(`gpt-6.1-terra`, medium), 커밋은 오케스트레이터가 했다.

## Play 검증

검증 환경: 메인 체크아웃의 Unity 에디터(Tutorial Map). 작업 트리 파일을 메인에 복사해 Play 했다(검증 후 원복). 포커스가 없는 세션이라 실제 키 입력 대신 가상 `Keyboard`/`Mouse` 장치에 `InputSystem.QueueStateEvent` 로 키를 넣었다.

- [x] 1. 컴파일/임포트: `recompile_status` `failed:false`, `Inventory` 액션 로드, 임포트 에러 0
- [x] 2. `GetAction("Inventory")` non-null, 바인딩 `<Keyboard>/tab`, Tab 입력으로 패널 토글 동작
- [x] 3a. Playing 에서 Tab -> `inv` 카운터 +1 (`inv=1`)
- [x] 3b. `GameState.Inventory` 에서 Space/X/1/E/R/Q/F/LMB -> 실측 `jump=0 dash=0 atk=0 s1=0 s2=0 heal=0 inter=0 sq=0`, W -> `move=1` (통과), Tab -> `inv=1` (통과)
- [x] 3c. Playing 으로 복귀 후 같은 키 -> `jump=1 dash=1 atk=1 s1=1 s2=1 heal=1 inter=1 sq=1 move=1` (기존 동작 유지)
- [x] 4. 구독자 파일(`PlayerController`, `PlayerInteraction`, `PauseMenu`) 0줄 변경

Paused 입력 누수 - 관찰됨, 미수정 (일시정지 메뉴 버튼의 LMB 클릭이 BasicAttack 으로 새는 기존 동작. 이번 phase 는 Inventory 상태만 막았다).

## 특이사항

Play 검증 중 장치 목록이 비는 에디터 문제를 일으켰다. 21-06-SUMMARY 의 "검증 환경 사고" 참고.
