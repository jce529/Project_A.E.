# 저장 및 설정

## 책임

`SaveLoadManager`가 3개 진행 슬롯과 별도 설정 파일의 메모리 모델, 직렬화, 로드를 소유한다. 메뉴 UI, 체크포인트, 보스는 공개 API를 호출할 뿐 파일 형식이나 경로를 소유하지 않는다.

## 런타임 구조

| 파일 | 모델 | 내용 |
|---|---|---|
| `save.json`, `save_1.json`, `save_2.json` | 슬롯별 `SaveData` | 스키마 버전, 씬/스폰 이름, 플레이어 체력, 보스 진행, 맵 gimmick 사전, 아이템 목록 |
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

## 제약

- `MapGimmickState`와 `Items`는 스키마만 존재하며 현재 작성자가 없다.
- 파일 쓰기 예외는 `Save`와 `SaveSettings`에서 catch하지 않는다.
- 설정/진행 스키마 버전 필드는 존재하지만 migration 로직은 없다.

## 근거

- `Assets/SaveSystem/Script/SaveData.cs:9`
- `Assets/SaveSystem/Script/SettingsData.cs:6`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:27`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:68`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:80`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:89`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:99`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:110`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:156`
- `Assets/SaveSystem/Script/SaveLoadManager.cs:210`
- `Packages/manifest.json`

## 검증

`partial` — 커밋 `4392d3ee4320ad620b323a866734ab1253d0800b`와 2026-09-03 작업 트리를 확인했다. Unity 6000.3.10f1 스크립트 컴파일은 성공했지만 슬롯별 파일 I/O와 이전 저장 호환성은 Play Mode에서 검증하지 않았다.
