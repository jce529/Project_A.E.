# Phase 24 Plan 01 Summary

구현 완료(정적 점검만, Play/Unity CLI 미검증). 모델 라우팅은 코드가 ~150줄 자족적이라 위임 없이 직접 작성.

## 신규 파일 (`Assets/map/script/Stage1/`)
`Stage1Progress.cs`, `Stage1Trigger.cs`, `Stage1SlideResult.cs`, `Stage1ProgressGate.cs`

## `s1.*` 키 목록 (phase-28 통합용)
| 키 | 의미 | 세우는 주체 | 해제 |
|---|---|---|---|
| `s1.lower.entered` | 하층 진입 | Stage1Trigger | - |
| `s1.platform.done` | 물 상승 장판 통과 | Stage1Trigger (requires lower.entered) | 슬라이드 실패 |
| `s1.upper.entered` | 상층 진입 | Stage1Trigger (requires platform.done) | 슬라이드 실패 |
| `s1.slide.success` | 워터 슬라이드 성공 | Stage1SlideResult.OnSuccess | 슬라이드 실패 |
| `s1.stepping.done` | 징검다리 통과 | Stage1Trigger (requires slide.success) | - |
| `s1.waterfall.done` | 폭포 통과 | Stage1Trigger (requires stepping.done) | - |
| `s1.split.entered` | 물가르기 구간 진입 | Stage1Trigger (requires waterfall.done) | - |
| `s1.reward.entered` | 보상방 진입 | Stage1Trigger (requires split.entered) | - |

## Inspector 연결 목록 (`1 stage.unity`는 미수정)
1. 각 구간 경계에 빈 오브젝트 + BoxCollider2D(isTrigger) + `Stage1Trigger`: key/requires를 위 표대로 설정 (하층 시작 지점, 장판 끝, 상층 입구, 징검다리 끝, 폭포 끝, 물가르기 입구, 보상방 입구).
2. 슬라이드 끝(성공)/낙하 지점(실패)에 빈 오브젝트 `Stage1SlideResult` 1개. `lowerCheckpoint`에 하층의 `Checkpoint` 오브젝트(예: `check`) Transform 지정. 전금재 슬라이드의 성공/실패 UnityEvent 또는 트리거에서 `OnSuccess()`/`OnFail()` 호출.
3. 다음 구간 문/발판은 `Stage1ProgressGate`: key=해당 선행 키, activateWhenSet/deactivateWhenSet에 오브젝트 지정.
4. 체크포인트 저장 시 플래그가 기록되므로 하층/징검다리 이후에 `Checkpoint`를 배치.

## 남은 이슈
- Phase 23 병합 전에는 `SetGimmick/GetGimmick` 미존재로 컴파일 안 됨 (이 브랜치 단독). 병합 후 시그니처 확인 필요.
- 전금재 기믹 코드가 저장소에 없어 실제 이벤트 연결은 씬 작업 때 확정.
- `SetGimmick`이 즉시 저장하지 않는다면 플래그는 다음 Save 시 기록됨(A4).
