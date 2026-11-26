# 26-01 SUMMARY

구현: `Assets/Script/Stage22/` 4개 스크립트. Play/Unity 검증 안 함(정적 점검만). SaveLoadManager.SetGimmick/GetGimmick은 Phase 23 병합 필요(미병합 상태에선 컴파일 불가).

## s2.b.* 키
| 키 | 작성자 | 의미 |
|---|---|---|
| s2.b.waterway.changed | phase-27 레버 | 5번에서 물길 변경 → 8번 진입 허용 |
| s2.b.waterfall.stopped | phase-27 수문 | 폭포 정지 → 석판 접근 허용 |
| s2.b.tablet.read | phase-27 석판 | 석판 조사 완료 → 9번 진행 허용 |
| s2.b.loop.completed | phase-26 | 9→1 복귀 완료 |

## phase-27 합의 인터페이스 (전달됨, 이름 불일치 시 phase-26이 맞춤)
| 항목 | 내용 |
|---|---|
| 방식 | 이벤트 없음, SetGimmick 플래그만 기록. 상태 머신은 GetGimmick으로 읽음 |
| 석판 가드 | `Stage22Progress.CanAccessTablet()` (static) |
| 상태 머신 | `Stage22ProgressMachine.Instance.Current / TryEnter / OnZoneChanged / OnMoveBlocked` |
phase-27 회신(확정): 파일 `Assets/map/script/Stage2/`, 클래스별 `event Action<bool> Changed`(값 변경 시만) + 상태 프로퍼티.
| 클래스 | 프로퍼티 | 키 |
|---|---|---|
| WaterwayLever | IsChanged | s2.b.waterway.changed (일치) |
| FloodgateWaterfall | Stopped | s2.b.waterfall.stopped (일치) |
| WaterWheel | Running | s2.b.waterwheel.running (phase-26 미사용) |
| RisingLift | Raised | s2.b.lift.raised (phase-26 미사용) |

석판(`s2.b.tablet.read`) 클래스는 회신에 없음. 키만 일치하면 되고 이름 변경은 불필요.

## Inspector 연결 목록 (씬 직접 편집 안 함)
- 2-2 씬에 빈 오브젝트 1개: `Stage22ProgressMachine`
- 구역 1~9마다 트리거 Collider2D(isTrigger) + `Stage22ZoneTrigger` (zone 지정). 플레이어 태그 "Player".
- 차단 지점마다 `Stage22Gate` + blocker(Collider 있는 오브젝트): 5→8, 8→9, 7→8(항상 차단), (from/to/blocker 지정)

## 남은 이슈
- Phase 23(b43dd2c)은 bfd7c16으로 병합 완료. .meta는 에디터 열 때 생성.
- 현재 구역 비저장(A5), 석판 위치 8번 가정(A3).
