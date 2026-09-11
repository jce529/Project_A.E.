# BUG-006: TutorialBoss 격파 상태 로드 시 보스방 벽이 해제되지 않음

- 심각도: 중간
- 상태: 해결됨
- 발견일: 2026-09-11
- 발견 경로: Phase 15 Unity MCP 라이브 체크
- 영향 범위: `Tutorial Map`의 TutorialBoss 격파 진행도 복원과 보스방 통행
- 관련 결정: Phase 15 D-06, D-07

## 현상

TutorialBoss를 격파한 세이브를 다시 로드하면 보스 GameObject는 정상적으로 비활성화되지만, 보스방 출구 벽을 해제하기 위한 `TutorialBossController.WallToUnlock` 참조가 비어 있다.

따라서 `TutorialBossController.Awake()`의 격파 상태 복원 분기에서 `UnlockWall()`이 호출되지 않는다. 플레이어는 이미 제거된 보스를 다시 처치할 수도 없으므로, 벽이 기본 활성 상태라면 진행이 막힐 수 있다.

## 재현 환경

- Unity: 6000.3.10f1
- 씬: `Assets/Scenes/Tutorial Map.unity`
- 검증 방식: Unity MCP Play 모드
- 검증일: 2026-09-11

## 재현 절차

1. `Tutorial Map`을 Play 모드로 실행한다.
2. `SaveLoadManager.SaveOnBossDefeated("TutorialBoss")`로 TutorialBoss 격파 상태를 저장한다.
3. `SaveLoadManager.LoadGame()`으로 같은 세이브를 다시 로드한다.
4. 재로드된 `TutorialBossController`의 활성 상태와 `WallToUnlock` 참조를 확인한다.
5. 보스방 벽 또는 `wallTilemap`의 활성 상태를 확인한다.

## 기대 결과

- TutorialBoss가 비활성화되어 다시 등장하지 않는다.
- `WallToUnlock.UnlockWall()`이 실행된다.
- 연결된 `wallTilemap`이 비활성화되어 보스방 출구를 통과할 수 있다.
- `ClearPanel`은 다시 표시되지 않는다.
- 게임은 일시정지 상태로 전환되지 않는다.

## 실제 결과

- TutorialBoss: `activeSelf=false`, `activeInHierarchy=false` — 통과
- ClearPanel: 비활성 — 통과
- `Time.timeScale=1` — 통과
- `SaveLoadManager.LoadData("BossProgress.TutorialBoss")`가 슬롯 2에서 `true` 반환 — 통과
- 플레이어 상호작용 후 `Secretwall`과 `wallTilemap` 비활성화 — 통과

## 근거

- 런타임 Unity MCP 측정에서 재로드 후 `WallRef=False`가 확인됐다.
- `Assets/Scenes/Tutorial Map.unity`의 TutorialBoss 인스턴스 오버라이드에 다음 값이 직렬화되어 있다.

```yaml
propertyPath: WallToUnlock
objectReference: {fileID: 0}
```

- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs`의 복원 분기는 null일 때 벽 해제를 건너뛴다.

```csharp
if (WallToUnlock != null) WallToUnlock.UnlockWall();
```

## 원인

`Tutorial Map`에 배치된 TutorialBoss 인스턴스의 `WallToUnlock` 필드에 올바른 `InteractableWall` 컴포넌트가 연결되어 있지 않다. 런타임 코드의 격파 상태 판정과 보스 비활성화는 정상이나, 씬 배선 누락으로 진행도에 따른 벽 복원이 실행되지 않는다.

## 수정 방향

1. `SaveLoadManager.LoadData(string)`가 live save cache의 값을 JSON 경로로 조회하고 문자열로 반환한다.
2. `InteractableWall`이 플레이어 상호작용 이벤트를 직접 구독한다.
3. 벽이 `BossProgress.TutorialBoss` 값을 `bool`로 파싱하고 `true`일 때 자신의 `UnlockWall()`을 호출한다.
4. 격파 상태 세이브를 로드한 뒤 벽과 접촉하여 상호작용했을 때 벽이 해제되는지 재검증한다.

이 경로는 누락된 `TutorialBossController.WallToUnlock` 참조에 의존하지 않는다. 기존 외부 호출 경로와 null 방어 로직은 다른 정상 호출자를 위해 유지한다.

## 완료 조건

- [x] `Tutorial Map.unity`의 `Secretwall`에 `BossProgress.TutorialBoss` 조회 키가 직렬화되어 있다.
- [x] `Secretwall.wallTilemap` 참조가 유효하다.
- [x] `SaveLoadManager.LoadData(string)`가 저장 캐시 값을 문자열로 반환한다.
- [x] `InteractableWall`이 상호작용 시 조회 결과를 파싱해 스스로 `UnlockWall()`을 호출한다.
- [x] 격파 상태 로드 후 TutorialBoss가 비활성 상태다.
- [x] 격파 상태 로드 후 보스방 벽 타일맵이 비활성 상태다.
- [x] ClearPanel이 다시 표시되지 않는다.
- [x] `Time.timeScale`이 1로 유지된다.
- [x] 벽 해금 흐름에서 관련 컴파일 오류나 런타임 예외가 없다.
- [x] 검증 일자와 현재 커밋 상태를 아래 해결 기록에 남겼다.

## 해결 기록

- 해결일: 2026-09-11
- 검증 일자: 2026-09-11
- 수정 커밋: `e81edbf`
- 비고:
  - Unity MCP Edit 모드: `Tutorial Map`의 `Secretwall`, `wallTilemap`, `BossProgress.TutorialBoss` 키 배선 확인.
  - Unity MCP Play 모드: 메모리의 `BossProgress["TutorialBoss"] = true` 상태에서 `LoadData`가 `true`를 반환하고 `TryUnlockFromSaveData()`가 `true`를 반환함.
  - 호출 후 `Secretwall.activeSelf=false`, `wallTilemap.activeSelf=false` 확인.
  - Unity Console 오류/예외/Assert 0개.
  - MCP가 주입한 F 입력은 `InputAction.WasPerformedThisFrame()=false`여서 실제 입력 이벤트 경로의 자동 검증 증거로 사용하지 않음.
  - 사용자가 세이브 슬롯 2의 실제 플레이 상호작용에서 정상 해금됨을 확인함.

## 관련 문서

- `.planning/phases/15-load-timing-and-load-scope/15-UAT.md`
- `.planning/phases/15-load-timing-and-load-scope/Check.md`
- `.planning/phases/15-load-timing-and-load-scope/15-CONTEXT.md`
- `.planning/phases/15-load-timing-and-load-scope/bugs/README.md`
