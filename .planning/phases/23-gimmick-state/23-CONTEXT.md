# Phase 23: 맵 기믹 상태 저장 헬퍼 - Context

discuss 생략(작은 phase). 아래는 확정 계약과 가정이다.

## 확정 API (변경 금지, Phase 24~28이 이 계약으로 코딩)
- `SaveLoadManager.SetGimmick(string key, bool value)`
- `SaveLoadManager.GetGimmick(string key, bool defaultValue = false)`

## 결정/가정
- D-01: 저장소는 기존 `SaveData.MapGimmickState` (Dictionary<string,bool>). 새 필드 없음 -> `SaveVersion` 3 유지, 구버전 세이브 호환.
- D-02: Set은 메모리 캐시(`_data`)만 갱신. 파일 I/O는 기존 `Save()` 트리거(체크포인트·보스 격파·자동저장·일시정지 저장)에서만.
- D-03: 키 규칙 `{stage}.{area}.{name}` 소문자. 호출부 실수를 막기 위해 Set/Get 모두 Trim + ToLowerInvariant 정규화.
- D-04: 빈/공백 키: Set은 경고 후 무시, Get은 defaultValue 반환.
- D-05: 슬롯(Phase 14)은 `_data`가 슬롯 단위이므로 `NewGame`/`LoadGame`/`LoadSlot`이 이미 슬롯별로 교체한다. 별도 처리 불필요.
- D-06: `EnsureCollections()`로 null 딕셔너리(수동 편집·구 세이브) 방어. `BossProgress`/`LoadData` 경로는 무수정.
- D-07: 로드 중 변경 이벤트·구독 API는 범위 밖(필요하면 후속 phase).
- 범위: `Assets/SaveSystem/Script/SaveLoadManager.cs`, `SaveData.cs`, `.planning/phases/23-gimmick-state/` 뿐.
