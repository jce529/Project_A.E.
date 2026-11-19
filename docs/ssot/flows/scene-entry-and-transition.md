# 씬 진입과 전환

## 책임

메인 메뉴 시작/이어하기와 표지판 포털의 씬 전환 경로를 설명한다. 각 씬 내부의 전체 GameObject 구성은 이 문서의 범위가 아니다.

## 구성

Build Settings에서 활성화된 순서는 다음과 같다.

1. `Assets/Scenes/MainMenu.unity`
2. `Assets/Scenes/1 stage.unity`
3. `Assets/Scenes/Tutorial Map.unity`

## 동작

- 새 게임 버튼은 첫 빈 슬롯이 있으면 `NewGameInSlot`으로 메모리를 초기화하고 `Tutorial Map`을 동기 로드한다. 모두 점유 상태면 새 게임 의도의 슬롯 선택 패널을 연다.
- 이어하기 버튼은 슬롯 선택 패널을 열고, 사용자가 점유 슬롯을 고른 뒤 저장된 `SceneName`을 비동기로 로드한다.
- `SignpostPortal`은 플레이어가 trigger 범위 안에서 interact할 때 목적 스폰 이름을 static 필드에 넣고 `nextSceneName`을 동기 로드한다.
- `GameStateManager`는 scene 간 유지되며 Playing/Paused/Inventory/Loading/GameClear/Puzzle 상태를 제공한다. Paused, Inventory, GameClear, Puzzle은 `Time.timeScale = 0`; Playing은 1로 만든다. Loading은 time scale을 명시적으로 바꾸지 않는다.

## 제약

- 포털의 `nextSceneName`/`spawnPointName`은 씬 직렬화 값에 의존한다.
- Assets에 존재하는 다른 stage 씬들은 Build Settings에 등록되지 않았으므로 이름 기반 runtime load 가능성이 확인되지 않았다.
- 신규 `SlotSelectPanel`과 `OverwriteConfirmPanel`의 `MainMenu.unity` GameObject 및 Inspector 참조는 아직 직렬화되지 않았다.

## 근거

- `ProjectSettings/EditorBuildSettings.asset`
- `Assets/Script/MainMenuUI.cs:26`
- `Assets/Script/MainMenuUI.cs:33`
- `Assets/map/script/SignpostPortal.cs:24`
- `Assets/Player/Script/GameStateManager.cs:6`
- `Assets/Player/Script/GameStateManager.cs:45`

## 검증

`editor-verification-required` — 커밋 `4392d3ee4320ad620b323a866734ab1253d0800b`와 2026-09-03 작업 트리. 스크립트 컴파일은 성공했으나 신규 메뉴 패널 배선과 실제 로드는 확인이 필요하다.
