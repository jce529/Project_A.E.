# Phase 26: 2-2 진행 상태 머신 - Context

discuss 생략(총괄 지시). 아래는 사용자 확인 없이 둔 가정이다.

## Assumptions
- A1. 구역 인접: 1-2-3-4-5-6-7 체인 + 5-8, 8-9 분기(양방향). 7-8, 6-8은 인접 아님 → 직접 이동 차단. 9→1은 단방향 복귀.
- A2. 5→8은 `s2.b.waterway.changed`, 8→9는 `s2.b.tablet.read` 필요. 역방향(8→5, 9→8)은 항상 허용.
- A3. 석판은 8번 구역에 있고, `s2.b.waterfall.stopped` 전에는 접근 불가(`Stage22Progress.CanAccessTablet()`).
- A4. 기믹(레버/수문/폭포/석판)은 phase-27이 `SetGimmick(key,true)`로 플래그만 쓴다. 이벤트 없음.
- A5. 현재 구역은 저장하지 않는다. 로드 후 첫 트리거 진입이 어느 구역이든 수락(Current==None).
- A6. 9→1 완료 시 `s2.b.loop.completed`=true.
- A7. 물리적 차단은 Stage22Gate(blocker 오브젝트 on/off), 구역 판정은 Stage22ZoneTrigger(Player 태그).
- A8. SaveLoadManager.SetGimmick/GetGimmick는 Phase 23 병합 후 존재(그 전엔 컴파일 불가).
