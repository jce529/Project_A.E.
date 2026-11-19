# AQUA ECLIPSE 구현 SSOT

이 디렉터리는 현재 체크아웃된 Unity/C# 구현을 설명한다. `.planning/` 문서는 의도와 향후 작업을 다루며, 여기의 문서는 코드·직렬화 에셋·프로젝트 설정에서 직접 확인된 현재 상태만 기록한다.

## 검증 경계

- Git 브랜치: `주창은`
- 커밋: `4392d3ee4320ad620b323a866734ab1253d0800b`
- 작업 트리 변경 포함: 예. Phase 14 세이브 슬롯 코드와 앞선 데드 코드 제거를 포함해 확인했다.
- Unity 6000.3.10f1 에디터의 스크립트 컴파일 성공을 Editor.log에서 확인했다. Play Mode 검증은 수행하지 않았다.

## 문서

### 시스템

- [플레이어 런타임](systems/player-runtime.md)
- [보스 전투](systems/boss-combat.md)
- [저장 및 설정](systems/save-and-settings.md)

### 흐름

- [저장·로드·스폰 복원](flows/save-load-and-spawn.md)
- [씬 진입과 전환](flows/scene-entry-and-transition.md)

### 프리팹

- [Player 프리팹](prefabs/player.md)

## 미해결 및 Editor 확인 필요

- `Assets/Player.prefab`의 `PlayerAttack.attackBox` 참조가 null이다. 기본 공격 시 `Instantiate`가 유효한 공격 박스를 생성하는지는 Editor에서 구성 또는 런타임 생성 경로를 확인해야 한다.
- `Assets/Player.prefab`은 `PlayerStats.maxHealth = 400`, `maxTotalHealth = 200`을 저장한다. 성장 상한이 현재 최대 체력보다 낮은 구성이 의도인지 확인이 필요하다.
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs`는 코드로 확인되지만, 해당 스크립트 GUID를 참조하는 `.unity`/`.prefab`을 찾지 못했다. 실제 씬 배치는 확인되지 않았다.
- Build Settings에는 `MainMenu`, `1 stage`, `Tutorial Map`만 활성화되어 있다. 포털이 다른 씬 이름을 가리킬 경우 로드 가능 여부를 Editor에서 확인해야 한다.
- Phase 14의 `SlotSelectPanel`과 `OverwriteConfirmPanel` 스크립트는 컴파일되었지만 `MainMenu.unity` 배치·Inspector 참조·버튼 OnClick 배선은 아직 필요하다.
