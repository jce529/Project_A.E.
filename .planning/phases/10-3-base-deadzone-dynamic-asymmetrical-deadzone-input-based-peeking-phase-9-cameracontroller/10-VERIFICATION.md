---
phase: 10-camera-deadzone-three-techniques
verified: 2026-10-26T00:00:00Z
status: human_needed
score: 9/9 static checks passed, Play-mode checklist not run
human_verification:
  - test: "1) Base Deadzone (D-01/D-02/D-03/D-14)"
    expected: "박스 안에서 카메라 완전 정지, 경계를 밀면 지연 없이 하드컷 추종, deadzoneWidth 튜닝 반영, 줌과 무관하게 Gizmo 크기 고정"
  - test: "2) Dynamic Asymmetrical Deadzone (D-04~D-07)"
    expected: "진행 방향 시야가 넓어지고(부호 검증), 방향 전환이 부드럽고, 정지 후 약 0.4초 유지 뒤 복귀하며 플레이어가 정중앙에 안착한다. 박스 안 움직임에서는 오프셋이 생기지 않는다"
  - test: "3) Input-based Peeking (D-08~D-13)"
    expected: "지상 정지 상태에서 0.5초 이상 위/아래 입력 시 피킹, 0.2초 입력은 무시, 이동·대시·피격·공중에서는 취소/불가, movementLocked 시 불가, 씬 전환 후 구독 중복 없음"
  - test: "4) 보스 구역 비활성화 & Phase 9 회귀 (D-15/D-16/D-17)"
    expected: "보스 구역에서 3종 모두 꺼지고 레거시 Lerp 추종, 이탈 시 카메라가 튀지 않고, X 클램프와 맵 끝 반응성이 유지된다"
  - test: "5) Y축 하드컷 데드존 (quick task 260804-q6h)"
    expected: "Y 박스 안에서 정지, 경계를 넘으면 즉시 추종, 플레이어가 노란 박스 밖에 서는 장면이 없고, 피킹·X 오프셋과 독립이며 보스 구역에서는 비활성"
  - test: "부가: minX/maxX 빨간 세로선 Gizmo (fa427d7)"
    expected: "에디터 모드에서 빨간 세로선 2개가 보이고 Inspector 값 변경에 즉시 반응하며, Play 중에도 유지된다"
---

# Phase 10: 카메라 데드존 3종 Verification Report

**Phase Goal:** 일반 스테이지에서 카메라가 (1) 데드존 박스 안에서는 정지하고 경계를 밀 때만 하드컷으로 따라붙으며, (2) 진행 방향으로 데드존 중심이 SmoothDamp 오프셋되고, (3) 접지·정지 상태에서 수직 입력 유지 시 위/아래로 시야를 옮기고 이동·대시·피격 시 즉시 취소된다. 보스 구역(`SetBossZoom(true)`)에서는 3종이 비활성화되어 Phase 9 레거시 Lerp 추종으로 복귀하고, 줌 Lerp 와 X 클램프는 두 경로 모두 Phase 9 그대로 동작한다.
**Status:** human_needed
**Re-verification:** No. 코드는 10-01~10-03 에서 완료됐다. 10-04 의 정적 회귀 검사는 통과했지만 Play 모드 체크리스트는 에이전트 환경에서 실행할 수 없어 전부 미체크이다. 이 문서의 어떤 항목도 PASS 로 추정해 적지 않았다.

## Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | 박스 안에서 카메라가 정지하고 경계에서 하드컷 추종 | NOT VERIFIED | 정적: `UpdateDeadzoneCenter` 본문에 Lerp/SmoothDamp 없음 (PASS). Play 모드 확인 필요 |
| 2 | 진행 방향 오프셋이 SmoothDamp 로 적용된다 | NOT VERIFIED | Play 모드 필요 (특히 부호 방향) |
| 3 | 수직 입력 피킹과 즉시 취소 | NOT VERIFIED | 정적: 구독 대칭 및 캐시 확인됨 (PASS). 동작은 Play 모드 필요 |
| 4 | 보스 구역에서 3종 비활성, Phase 9 회귀 없음 | NOT VERIFIED | 정적: `BossZoomTrigger.cs` 무변경, `SetBossZoom` 시그니처 유지 (PASS). 동작은 Play 모드 필요 |
| 5 | Y축 하드컷 데드존 | NOT VERIFIED | Play 모드 필요 |

## Static Regression Checks (2026-08-04, 9/9 PASS)

| # | 검사 | 판정 |
|---|---|---|
| 1 | 인코딩 게이트 (비-ASCII 5줄 기대, 실제 5) | PASS |
| 2 | 삭제 라인 총량 (3줄, 개별 커밋 합산) | PASS (diff 알고리즘 특성 비고) |
| 3 | `PlayerController.cs` / `InputHandler.cs` 무수정 | PASS |
| 4 | `LateUpdate` 실행 순서 | PASS |
| 5 | `ApplyNormalStageCamera` 내부 순서 | PASS |
| 6 | 하드컷 보존 (`UpdateDeadzoneCenter` 에 Lerp/SmoothDamp 없음) | PASS |
| 7 | 금지 심볼 부재 | PASS (`DontDestroyOnLoad`=1 은 주석 원문, 호출 0건) |
| 8 | 구독 대칭 및 캐시 | PASS |
| 9 | `BossZoomTrigger.cs` 무변경 | PASS |

상세 근거는 `Assets/Camera/Check.md` 191~209행. 10-04 세션이 현재 HEAD 의 `CameraController.cs` 를 다시 점검한 결과도 같았다(비-ASCII 5줄 유지, 구독 대칭 유지).

## Human Verification Required

체크리스트 원본은 `Assets/Camera/Check.md` 88~235행(Phase 10 섹션)이다. 5개 소섹션, 약 41개 항목과 부가 Gizmo 5개 항목이 전부 미체크이다.

1. 사전 준비: Unity 컴파일 에러 0건, `Assets/Scenes/1 stage.unity` 열기, `Main Camera` Inspector 에서 신규 필드 11개 확인, Scene 뷰를 열어 둔 채 Play. 보스 구역은 `TempBossZoneTest`(BoxCollider2D + Is Trigger + `BossZoomTrigger`)를 사용한다.
2. 위 6개 묶음을 순서대로 수행하고, 통과한 항목만 체크한다.
3. 튜닝 값 기록 표(1 stage / Tutorial Map)를 채운다.
4. 결과를 `10-04-SUMMARY.md` 와 이 문서에 PASS/FAIL 로 반영한다. FAIL 이 있으면 `/gsd:plan-phase 10 --gaps` 를 사용한다.

**가장 먼저 볼 항목:** 2) 의 진행 방향 부호 검증(가정 A2). 반대로 보이면 부호가 뒤집힌 것이다. 점프 체감도 하드컷이라 딱딱할 수 있는데, 거슬리면 `Deadzone Height` 를 키워 대응하고 스무딩 재도입은 하지 않는다(DY-01).

## Do Not Re-test (stale)

`Assets/Camera/Check.md` 257~478행의 quick task `260805-m41` / `260805-q2u` 섹션은 체크박스가 전부 미체크이지만, 구현이 `260809-h9k` 로 폐기·대체됐고 `260809-h9k` 는 이미 Play 모드 검증을 마쳤다. 재검증 대상이 아니다.

## Branch Note

10-04 세션이 `0341f3c` (`docs(10-04): complete Phase 10 verification plan summary`)를 브랜치 `claude/pensive-bose-57f8b2` 에 커밋했다. 이 체크아웃에는 아직 병합되지 않았다.

## Gaps Summary

코드 결함은 발견되지 않았다. 알려진 이슈였던 Y축 데드존 부재는 `260804-q6h` 에서 코드로 해결됐고, 그 동작 확인이 위 5) 에 포함되어 있다. 남은 것은 Play 모드 확인뿐이다.
