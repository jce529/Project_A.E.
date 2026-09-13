---
status: partial
phase: 15-load-timing-and-load-scope
source: [15-01-SUMMARY.md, 15-02-SUMMARY.md, 15-03-SUMMARY.md, Check.md]
started: 2026-09-11T12:06:00+09:00
updated: 2026-09-11T13:10:00+09:00
---

## Current Test

[testing paused — D-02 and BUG-007 outstanding]

## Tests

### 1. Death automatically loads the last checkpoint
expected: Player death reloads Tutorial Map once, preserves the Player until transition, and restores at the saved checkpoint with full valid health.
result: pass
evidence: Player remained active immediately after Die(); the scene reloaded at check3 position (273.05, -40.50, 0.00) with health 100/100/200 and Time.timeScale 1.

### 2. Death without any save restarts the current scene
expected: With all three save slots absent, death restarts Tutorial Map from its default start with health 100/100/200.
result: blocked
blocked_by: other
reason: "All three user save files exist and Unity MCP rejected runtime file move/delete commands."

### 3. Invalid scene load returns to MainMenu without altering the save
expected: Loading a save whose SceneName is not registered returns to MainMenu and leaves the file unchanged.
result: pass
evidence: NoSuchScene produced the expected recovery error, returned to MainMenu, and the 255-character save content matched exactly before and after failure.

### 4. Defeated TutorialBoss stays absent without replaying clear UI
expected: A defeated TutorialBoss is inactive after reload, ClearPanel remains hidden, time is not paused, and the boss-room wall is unlocked.
result: pass
evidence: Unity MCP verified the save-data lookup and self-unlock path; the user then confirmed the complete slot-2 interaction path unlocks the wall in actual play.

### 5. Restored health obeys the invariant and progression starts at 100/200
expected: Initial health is 100/100/200, AddHealth raises maxHealth to 101, and an out-of-range 400/200 save normalizes to 200/200/200.
result: pass
evidence: Unity MCP observed 100/100/200, then 101/101/200, then 200/200/200.

### 6. Input remains available after transitioning to 1 stage
expected: The persistent InputHandler retains a valid InputSystem_Actions asset and Player action map after Tutorial Map transitions to 1 stage.
result: issue
reported: "InputHandler loses its action asset after the scene transition and the player cannot move."
severity: major
evidence: Unity MCP confirmed both InputManager objects are children of Manager, DontDestroyOnLoad rejects the Tutorial handler, and 1 stage serializes inputActions as null while the fallback Resources.Load path also returns null.

## Additional Findings

- [BUG-009](bugs/BUG-009-interaction-prompt-camera-jitter.md): 2026-09-12 사용자 후속 보고 — quick-260912-kih의 대상 위 키 UI가 플레이어 이동 중 떨림. 2026-09-13 사용자 승인으로 World Space Canvas로 전환. 대상의 자식에 높이 오프셋을 적용하며 줌에 따라 글자도 확대/축소한다. 화면 좌표 계산과 렌더 콜백은 제거했고 C# 빌드 통과(오류 0). 2026-09-13 사용자 “확인완료했어 완료처리해줘”로 Play 수용 확인 및 종료 승인하여 해결됨. 세부 시나리오별 계측 로그는 미수집. 기존 6개 UAT 테스트 집계와 별도 후속 발견.

## Summary

total: 6
passed: 4
issues: 1
pending: 0
skipped: 0
blocked: 1

## Gaps

- truth: "Player input remains initialized across Tutorial Map to 1 stage transition."
  status: failed
  reason: "The Tutorial InputManager is not a root object so DontDestroyOnLoad fails; the replacement 1 stage InputManager has no InputActionAsset and its Resources fallback cannot locate the asset."
  severity: major
  test: 6
  artifacts:
    - path: "Assets/Player/Script/InputHandler.cs"
      issue: "Calls DontDestroyOnLoad on a child GameObject and relies on a Resources path that does not exist."
    - path: "Assets/Scenes/1 stage.unity"
      issue: "InputManager.inputActions is null."
  bug: ".planning/phases/15-load-timing-and-load-scope/bugs/BUG-007-inputhandler-lost-on-scene-transition.md"

## Data Safety

- Backup directory: `C:\Users\chang\AppData\Local\Temp\phase15-livecheck-20260911-120718`
- `save.json`, `save_1.json`, and `save_2.json` were restored and matched their backups exactly.
- Unity Play Mode was stopped after verification.
