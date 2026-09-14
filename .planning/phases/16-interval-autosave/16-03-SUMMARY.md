---
phase: 16-interval-autosave
plan: 03
subsystem: verification
tags: [verification, play-mode, unity-cli, regression]
requires: [16-01, 16-02]
provides: [phase16-verification]
affects: []
key-files:
  created: [.planning/phases/16-interval-autosave/Check.md]
  modified: []
metrics:
  tasks_completed: 2
  completed: 2026-09-14
requirements-completed: [D-01, D-03, D-03b, D-03c, D-04, D-05, D-07, D-08, D-10]
---

# Phase 16 Plan 03 — 검증 문서화와 Play 모드 실측

Phase 16 의 자동저장을 문서 전용으로 검증했다. `Assets/` 0줄 변경.
정적 회귀 22항목과 Play 모드 29항목을 모두 실측으로 닫았다.

## Task commits

| Task | Commit | 변경 |
|---|---|---|
| 1. 정적 회귀 + Check.md 작성 | bab5f21 | `Check.md` 생성 (원문 18/22 PASS, 대체 22/22 PASS) |
| 1b. 검증 명세 오탐 수정 | 37b24b5 | [BUG-010](bugs/BUG-010-plan-acceptance-false-positives.md) — 주석과 코드 구분 |
| 2. Play 모드 실측 | (이 커밋) | `Check.md` 체크리스트 29/29 PASS 로 갱신 |

## 결과

**정적 회귀:** 원문 기준 18/22 PASS. FAIL 4건은 전부 검증 명세 자체의 오탐(주석을 코드로 계수)이며 BUG-010 으로 기록했다.
소스를 고쳐 FAIL 을 숨기지 않았고, 주석을 제외한 대체 검사에서 22/22 PASS 했다. 빌드 오류 0 / 기존 경고 6 유지.

**Play 모드: 29항목 29 PASS / 0 FAIL / 0 미검증.**
1차(Unity MCP)에서 15항목, 2차(Unity CLI)에서 나머지 13항목과 항목 28 의 육안 확인을 채웠다.

플랜이 완료 조건으로 못박은 두 그룹이 모두 실측 통과했다.

- **B 그룹 (D-03c, 메인메뉴 슬롯 오염 차단)** — 계획의 "5분 방치"보다 강한 조건으로 측정했다.
  `_elapsedSeconds` 를 **179/180**(한 틱 뒤 발동)으로 무장한 뒤 `MainMenu` 에서 **298초** 방치했으나
  카운터는 179에서 한 번도 증가하지 않았고 `save.json` 의 `SceneName` 은 `Tutorial Map` 그대로였다.
  `GameStateManager.HandleSceneLoaded()` 가 `MainMenu` 에서도 `CurrentState` 를 `Playing` 으로 강제하는 것이 런타임에서 확인됐고,
  실제로 오염을 막는 것은 두 번째 가드 `PlayerStats.Instance != null` 임이 증명됐다.
- **E 그룹 (D-03b, 기존 수동 저장 무회귀)** — 슬롯 대화상자 UI 전 구간을 실제 버튼 `onClick` 으로 구동했다.
  `GameSettingsPanel.OnSaveProgressBtnClick()` → 슬롯 선택 → `"덮어쓰기"` → 선택한 슬롯(`save_1.json`)에 기록 → 대화상자 종료 → `Playing` 복귀.
  체크포인트 경로(`Checkpoint.Interact()`)도 동일하게 정상이며 체력 풀회복이 유지된다.

그 밖의 핵심 실측:

- **D-05 리셋** — 수동 저장이 카운트를 실제로 되돌린다. 일시정지 메뉴 저장에서 `elapsed` 50→0, 체크포인트 저장에서 90→0.
- **D-07 씬 갱신** — 체크포인트를 밟은 뒤 `1 stage` 로 이동하자 새 씬에서 **자연 발동**했고,
  `SceneName="1 stage"` · `SpawnPointName=""`(비워짐)로 기록됐다.
- **D-08 사망 로드** — `Die()` → `Restored stats: 100/100 (maxTotal 200), scene=1 stage, spawnPoint=`.
  마지막 체크포인트가 아니라 새 씬의 기본 시작 위치에서 풀피 부활하는, 계획이 수용한 동작 그대로다.
- **D-10 알림** — 합성 화면 캡처(1920x1080)에서 우하단 "자동 저장됨" 이 읽히는 것을 확인했다.
  소멸 시간도 측정했다: t=0 `alpha=1` → t=1.720s `alpha=0.125` → t=3.422s `alpha=0`.
  코드 모델(Hold 1.2s + Fade 0.6s)의 예측값 0.133 과 일치하며 1.8초 안에 사라진다.
- **검증 훅** — `Phase16/1. Force AutoSave Now` / `Phase16/2. Log Timer State` 둘 다 동작한다.
  `ContextMenu` 어트리뷰트로 기어 메뉴가 바인딩하는 바로 그 메서드임을 확인한 뒤 호출했다.

## 1차 실측이 남긴 13항목의 실제 원인

1차는 "플레이어 루프가 멈췄다"며 13항목을 포기했고 원인을 창 포커스/`runInBackground` 로 추정했다. **오진이었다.**
2차에서 `EditorApplication.isPaused` 를 직접 읽어 `True` 임을 확인했다 — Console 의 **Error Pause 가 켜진 상태**에서
씬 로드 직후 기존 미해결 [BUG-008](../15-load-timing-and-load-scope/bugs/BUG-008-tutorialboss-animator-missing.md)
(`MissingComponentException: There is no 'Animator' attached to the "Tutorial Boss"`)이 발생해 **Play 모드가 자동 일시정지**된 것이다.
Error Pause 를 끄자 재발하지 않았고 남은 항목을 전부 측정할 수 있었다. 자동저장 로직과는 무관하다.

1차가 기록한 도구 제약 2건도 Unity MCP 한정이었음이 확인됐다.

| 1차가 기록한 제약 | 2차 결과 |
|---|---|
| 리플렉션 차단으로 `[ContextMenu]` 훅 호출 불가 (항목 5·8) | `unity command eval`(Roslyn)은 리플렉션 허용 → 실측 완료 |
| 콘솔 조회에 런타임 게임 로그 미포함 (항목 6 문자열 기준) | `unity command console` 이 스택트레이스까지 반환 → 원문 확보 |
| `ScreenSpaceOverlay` 는 캡처 불가 (항목 28) | `capture_game_view --source screen` 이 합성 백버퍼를 캡처 → 육안 확인 완료 |

## 부수 발견 (Phase 16 무관, 수정하지 않음)

- **[BUG-008]** 기존 미해결. `Tutorial Map` / `1 stage` 로드 시마다 `MissingComponentException` 이 발생한다.
  Error Pause 가 켜져 있으면 Play 모드가 멈추므로 이 프로젝트의 Play 모드 검증 작업 전반을 방해한다.
- `GameSettingsPanel.saveProgressFeedbackText` 가 `Tutorial Map` 에서 미할당이라 저장 완료 문구가 표시되지 않는다.
  코드상 선택 필드(`// 저장 결과 안내 (선택 - 비워둬도 동작함)`)이고 Phase 16 은 씬 0줄 변경이므로 기존 상태다.
- `Assets/_Recovery/0 (3).unity`(untracked, 12MB)가 mtime 19:13 으로 존재한다. 첫 에디터 명령(19:16:22)보다 앞서므로
  이 검증이 만든 것이 아니다. Unity 의 씬 복구 백업이라 임의로 지우지 않았다.

## 검증 무결성

- **프로젝트 파일 0줄 변경.** `git status --porcelain ProjectSettings/` 빈 출력,
  `Assets/` 의 status 항목 집합이 세션 시작과 동일, 관련 스크립트·씬의 mtime 이 전부 첫 에디터 명령 이전.
- **측정 전용 임시 변경 2건은 원복했다** — `PlayerSettings.runInBackground`(측정 중 true → false 복구),
  Console Error Pause(측정 중 off → on 복구).
- **세이브 파일 복구 완료.** 측정 전 3슬롯을 백업하고 측정 후 복구했으며, 3슬롯 모두 SHA-256 일치를 확인했다.
  측정 직후 상태는 `save*.json.phase16-after-test` 로 보존했다.
