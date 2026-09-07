# 저장·로드·스폰 복원 흐름

## 책임

체크포인트/보스/메뉴에서 시작되는 저장과 로드 호출을 파일 데이터, 씬 로드, 플레이어 위치 및 체력 복원까지 연결한다. 좌표 자체를 저장하지 않고 씬 이름과 GameObject 이름을 연결 키로 사용한다.

## 흐름

### 체크포인트 저장

1. `Checkpoint`가 `InputHandler.OnInteractEvent`를 받고 플레이어 범위와 비활성 상태를 확인한다.
2. 다른 체크포인트를 비활성화하고 자신을 활성화한다.
3. `SaveAtCheckpoint(gameObject.name)`가 활성 씬과 체크포인트 이름을 기록하고 플레이어 체력을 캡처해 파일을 쓴다.
4. 같은 Transform을 현 세션의 `PlayerRespawn.respawnPoint`로 설정한다.

### 이어하기/로드

1. `MainMenuUI`는 세 슬롯 중 하나라도 파일이 있으면 load 버튼을 활성화한다. 클릭 시 자동 로드하지 않고 `SlotSelectPanel`을 연다.
2. 슬롯 카드는 `PeekSlotData`로 진행도를 표시하며, 점유 슬롯을 선택하면 `LoadSlot`이 해당 슬롯을 활성화하고 JSON을 역직렬화한다.
3. manager가 `PlayerSpawner.targetSpawnPointName`을 씬 로드 전에 설정한다.
4. `SceneManager.LoadSceneAsync` 완료 과정에서 새 Player의 `PlayerSpawner.Start`가 같은 이름의 GameObject를 찾아 위치를 옮기고 `PlayerRespawn`의 시작점/체크포인트를 동기화한다.
5. scene async operation 완료 후 manager가 `PlayerStats.RestoreStats`를 호출한다.

### 새 게임과 덮어쓰기

- 빈 슬롯이 있으면 메인 메뉴가 가장 낮은 번호를 골라 메모리만 초기화하고 `Tutorial Map`을 연다.
- 세 슬롯이 모두 차면 슬롯 패널을 열며, 점유 슬롯의 새 게임은 `OverwriteConfirmPanel` 확인 콜백을 거쳐야만 시작한다.
- 확인 후에도 파일은 즉시 지워지지 않고 다음 저장 트리거에서 덮어쓴다.

### 실패 및 경계

- 스폰 이름이 비어 있으면 기본 prefab 위치가 유지된다.
- 이름이 존재하지 않으면 경고하고 static 목표 이름을 지우지 않는다.
- 동일 이름 GameObject가 여러 개면 object enumeration에서 먼저 만난 대상을 사용한다.
- 저장 씬이 Build Settings에 없으면 비동기 로드 성공을 보장할 수 없다.
- 현 세션의 `RespawnPosition`은 파일을 다시 읽지 않고 메모리 Transform으로 이동한다.

## 근거

- `Assets/map/script/Checkpoint.cs:21`
- `Assets/map/script/PlayerRespawn.cs:22`
- `Assets/map/script/PlayerSpawner.cs:13`
- `Assets/Script/MainMenuUI.cs:11`
- `Assets/Script/MainMenuUI.cs:33`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:99`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:210`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:259`
- `Assets/Player/Script/PlayerStats.cs:70`

## 검증

`editor-verification-required` — 커밋 `4392d3ee4320ad620b323a866734ab1253d0800b`와 2026-09-03 작업 트리. 코드 컴파일은 성공했지만 `MainMenu.unity`에 신규 패널이 아직 배선되지 않아 실제 슬롯 선택과 round trip은 검증되지 않았다.
