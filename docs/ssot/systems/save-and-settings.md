# 저장 및 설정

## 책임

`SaveLoadManager`가 3개 진행 슬롯과 별도 설정 파일의 메모리 모델, 직렬화, 로드를 소유한다. 메뉴 UI, 체크포인트, 보스는 공개 API를 호출할 뿐 파일 형식이나 경로를 소유하지 않는다.

## 런타임 구조

| 파일 | 모델 | 내용 |
|---|---|---|
| `save.json`, `save_1.json`, `save_2.json` | 슬롯별 `SaveData` | 스키마 버전(현재 3), 씬/스폰 이름, 플레이어 체력, 보스 진행, 맵 gimmick 사전, `Items: List<ItemSaveEntry>{itemId,count}` |
| `setting.json` | `SettingsData` | 언어, 화면 흔들림, 튜토리얼 힌트, 화면 모드, BGM/SFX 볼륨, Input System 바인딩 JSON |

모든 파일은 `Application.persistentDataPath` 아래에 Newtonsoft.Json의 들여쓰기 형식으로 저장된다. 기존 `save.json`은 이름 변경 없이 슬롯 0이며, 슬롯 1과 2만 새 파일명을 사용한다. `SaveLoadManager`는 `BeforeSceneLoad` bootstrap에서 생성되고 `DontDestroyOnLoad` singleton으로 유지된다.

## 동작과 실패 경로

- `SelectSlot`이 `CurrentSlot`을 정하고 무인자 `Save`/`LoadGame`은 해당 슬롯 경로를 사용한다. `Save`는 현재 `PlayerStats`를 메모리 데이터에 캡처한 뒤 선택된 슬롯 파일을 쓴다.
- `PeekSlotData`는 카드 표시용으로 특정 슬롯을 역직렬화하지만 현재 슬롯과 런타임 `_data`는 바꾸지 않는다. 손상 또는 읽기 실패 시 null을 반환한다.
- 체크포인트 저장은 활성 씬 이름과 체크포인트 GameObject 이름을 위치 식별자로 사용한다. 보스 저장은 보스 ID를 true로 기록하되 마지막 체크포인트 위치를 유지한다.
- `LoadGame`은 파일 부재, 읽기/역직렬화 예외, null 데이터, 빈 씬 이름에서 중단한다. null collection은 빈 객체로 보정한다.
- `NewGame`은 메모리만 초기화한다. 기존 디스크 파일은 다음 저장 전까지 남는다.
- `NewGameInSlot`과 `LoadSlot`은 슬롯을 선택한 뒤 기존 새 게임/로드 경로를 재사용한다.
- 설정은 manager `Awake`에서 읽고, 설정 패널은 메모리 객체를 수정하며 `SaveSettings` 호출에서만 디스크에 기록한다.
- `Save()`는 플레이어 스탯 캡처 직후 `CaptureInventoryItems()`를 호출해 씬의 `Inventory`를 순회, 빈 슬롯이 아닌 것만 `itemId`/`count`로 직렬화한다. 씬에 `Inventory`가 없으면(예: 메인메뉴) 경고만 남기고 기존 저장된 `Items`를 그대로 보존한다(덮어쓰지 않음).
- `LoadGame`은 씬 로드 완료(코루틴 yield) 및 플레이어 스탯 복원 이후 `ApplyInventoryFromSave()`로 `ResolveItemData(itemId)`(`Resources.LoadAll<ItemData>("Items")` 캐시 조회) 결과를 `Inventory.TryAddItem`으로 재적재한다. 알 수 없는 itemId는 경고 후 스킵 — 로드 자체를 중단하거나 메인메뉴로 튕기지 않는다.
- 이 과정에서 BGM/SFX 볼륨은 `SettingsData.BgmVolume`/`SfxVolume`에만 저장되며, 실제 재생 볼륨은 `AudioManager`(Phase 20, `Assets/Script/AudioManager.cs`)가 `PersistentManagers`의 `DontDestroyOnLoad` 루트에서 소유한다 — `SaveLoadManager`는 여전히 설정값의 영속화만 담당하고 오디오 재생 자체는 건드리지 않는다.

## 제약

- `MapGimmickState`는 스키마만 존재하며 현재 작성자가 없다. `Items`는 Phase 19에서 실제 캡처/복원 작성자가 생겼다(더 이상 스텁 아님).
- 파일 쓰기 예외는 `Save`와 `SaveSettings`에서 catch하지 않는다.
- 설정/진행 스키마 버전 필드는 존재하지만(`SaveVersion` 현재 3) 과거 버전에 대한 명시적 migration 로직은 없다 — 구버전 필드 부재는 null-collection 보정으로만 흡수된다.

## 근거

- `Assets/SaveSystem/Script/SaveData.cs:9`
- `Assets/SaveSystem/Script/SettingsData.cs:6`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:27`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:68`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:80`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:89`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:99`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:110`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:141` (`CaptureInventoryItems()` 호출)
- `Assets/SaveSystem/Script/SaveLoadManager.cs:156`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:210`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:367` (`ResolveItemData`)
- `Assets/SaveSystem/Script/SaveLoadManager.cs:387` (`CaptureInventoryItems`)
- `Assets/SaveSystem/Script/SaveLoadManager.cs:491`/`515` (`ApplyInventoryFromSave`)
- `Packages/manifest.json`

## 검증

`partial` — 커밋 `ec03fd9510a7c5d59d3a8e4397896046c768245a`(2026-09-21, `주창은` 브랜치)와 2026-09-24 작업 트리를 확인했다. Phase 19 인벤토리 저장/로드 왕복은 `Assets/SaveSystem/Check.md` "Phase 19" 절에서 Unity CLI로 Play Mode 6/6 PASS 실측됐다(itemId 오류 처리 포함). 슬롯별 파일 I/O 자체와 구버전(→v3) 마이그레이션 경로는 여전히 Play Mode에서 별도 검증되지 않았다.
