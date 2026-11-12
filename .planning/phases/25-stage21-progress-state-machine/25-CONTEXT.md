# Phase 25 Context: 2-1 진행 상태 머신

discuss 생략(상위 세션 지시). 아래는 사용자에게 묻지 않고 둔 가정.

## Assumptions
- A1: 플래그 저장은 Phase 23 API `SaveLoadManager.SetGimmick/GetGimmick`(bool). 키 `s2.a.*` 소문자.
- A2: 구역 흐름은 선형 1→6. N번은 N-1번 방문 후에만 진입 허용(`s2.a.areaN.visited`). 현재 구역 번호는 bool 저장소 한계상 저장하지 않음.
- A3: 5번 전투 종료 = `Stage2Events.AreaCleared(5)` → `s2.a.tablet.unlocked`. 이벤트 발행은 전투 담당(스텁 계약만 정의).
- A4: 2-2 진입 허용 = `s2.a.lantern.solved`. 차단/허용 연출은 UnityEvent로 씬에서 연결.
- A5: 안개 제거는 퍼즐 해결 후 지정 구역 진입 시 `s2.a.fog.removed=true`, 영구(로드 시 Start에서 재적용).
- A6: 상태 머신은 씬 오브젝트 없는 static 클래스 + 소형 컴포넌트. 씬 파일은 편집하지 않음.
- A7: Phase 23 헬퍼는 아직 미병합 — 계약대로 코딩, 병합 전엔 컴파일 불가(SetGimmick 미존재).
