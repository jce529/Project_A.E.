# Phase 24: 1스테이지 진행 연결 - Context

Discuss 생략. ROADMAP과 코드에서 세운 가정만 기록한다.

## Assumptions
- A1. Phase 23 API 확정: `SaveLoadManager.SetGimmick(string,bool)` / `GetGimmick(string,bool=false)`. 키는 `s1.{area}.{name}` 소문자. 이 브랜치에는 아직 없음(`SaveData.MapGimmickState` 스키마만 있음) -> 계약대로 코딩, phase-23 병합 후 컴파일 확인.
- A2. 전금재 1스테이지 기믹(장판, 슬라이드, 징검다리, 폭포) 코드는 코드베이스와 `1 stage.unity`에 없다(Checkpoint/FallZone/portal/Secretwall만 존재). 기믹은 만들지 않고 계약으로만 연결한다.
- A3. 기믹 계약(최소): 기믹은 완료/실패 시점에 `Stage1Trigger.Report()` 또는 `Stage1SlideResult.OnSuccess()/OnFail()`을 UnityEvent/호출로 부르거나, `Stage1Progress.Set(key,true)`를 직접 부른다. 기믹이 알아야 할 것은 키 문자열뿐이다.
- A4. 진행 단계는 별도 저장 없이 플래그에서 파생(`Stage1Progress.CurrentStep()`). 플래그는 `MapGimmickState`에 저장되고 체크포인트 저장(`SaveAtCheckpoint` -> `Save`) 때 파일에 기록된다. 체크포인트 이후 세운 플래그는 재로드 시 사라진다(체크포인트 저장 의미와 동일).
- A5. 슬라이드 실패 = 하층으로 낙하. 하층 구간(장판->상층->슬라이드)을 다시 풀게 `platform.done`, `upper.entered`, `slide.success`를 false로 되돌리고 하층 체크포인트 Transform으로 `PlayerRespawn`을 갱신 후 `RespawnPosition()`한다. 하층 체크포인트는 씬의 `Checkpoint` 오브젝트를 Inspector로 지정.
- A6. 순서 보장은 `Stage1Trigger.requires`(선행 키 모두 true일 때만 동작)로 처리한다.
- A7. 씬 파일은 수정하지 않는다. 배치는 SUMMARY의 Inspector 연결 목록으로 남긴다.
