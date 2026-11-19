---
phase: quick-260912-kih
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - Assets/Player/Script/IPlayerInteractable.cs
  - Assets/Player/Script/PlayerInteraction.cs
  - Assets/Player/Script/PlayerController.cs
  - Assets/map/script/SignpostPortal.cs
  - Assets/map/script/Checkpoint.cs
  - Assets/map/script/InteractableWall.cs
  - Assets/map/script/3 stage/SlidingPuzzleTrigger.cs
  - Assets/Enemy/WaterMonster/Script/Phase2/PlayerAbsorb.cs
  - Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs
  - Assets/Editor/PlayerInteractionVerification.cs
autonomous: true
requirements: [QUICK-INTERACTION-01, QUICK-INTERACTION-02, QUICK-INTERACTION-03]
must_haves:
  truths:
    - The player owns a configurable interaction radius and a key press executes only the nearest eligible object's action.
    - World interaction objects neither track player proximity nor subscribe to interaction input.
    - Checkpoint save/respawn, portal destinations, wall unlock conditions, puzzle behavior and puddle absorption remain functional.
    - Interact-mode puddle absorption competes with world objects for one action; other absorption input modes remain functional.
  artifacts:
    - path: Assets/Player/Script/PlayerInteraction.cs
      provides: Player-owned input subscription, range query and nearest eligible dispatch
    - path: Assets/Player/Script/IPlayerInteractable.cs
      provides: Eligibility and action contract
    - path: Assets/Editor/PlayerInteractionVerification.cs
      provides: Executable behavioral regression checks
  key_links:
    - from: Assets/Player/Script/PlayerController.cs
      to: Assets/Player/Script/PlayerInteraction.cs
      via: Duplicate-safe Awake component installation for existing scene players
    - from: Assets/Player/Script/PlayerInteraction.cs
      to: IPlayerInteractable
      via: InputHandler.OnInteractEvent -> range query -> eligibility -> nearest -> one action
---

<objective>
플레이어가 자체 반경 내 가장 가까운 실행 가능한 객체 하나를 선택하고, 객체는 자신의 상호작용 액션만 실행하도록 변경한다.
Purpose: 입력 구독을 중앙화하고 한 키로 여러 객체가 동시에 작동하는 문제를 제거한다. 현재 네 월드 객체는 Update 폴링이 아닌 물리 trigger/collision 기반이므로 프레임당 메모리 낭비가 측정되었다고 주장하지 않는다.
Output: 플레이어 감지/디스패치, 기존 객체와 물웅덩이의 연동, 자동 검증.
</objective>

<context>
@.planning/STATE.md
@Assets/Player/Script/PlayerController.cs
@Assets/Player/Script/InputHandler.cs
@Assets/map/script/SignpostPortal.cs
@Assets/map/script/Checkpoint.cs
@Assets/map/script/InteractableWall.cs
@Assets/map/script/3 stage/SlidingPuzzleTrigger.cs
@Assets/Enemy/WaterMonster/Script/Phase2/PlayerAbsorb.cs
@Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs

기존 계약: InputHandler.Instance.OnInteractEvent는 Action 이벤트이며 키 재설정을 이미 지원한다. Checkpoint는 SaveAtCheckpoint(gameObject.name)와 PlayerRespawn.UpdateCheckpoint(transform)를 호출한다. SignpostPortal은 PlayerSpawner.targetSpawnPointName을 설정하고 nextSceneName을 로드한다. InteractableWall.TryUnlockFromSaveData()/UnlockWall()는 외부 호출을 유지해야 한다. SlidingPuzzleTrigger.Interact()/OnPuzzleCleared()와 Pause 이벤트 UI 닫기는 유지한다. PlayerAbsorb.InputType은 Interact/BasicAttack/Skill1/Skill2/Heal이며 WaterPuddle.SetIndestructible()가 색상/스택 등록을 담당한다.

사용자 결정: D-01 감지 범위는 플레이어 소유. D-02 키 입력 시 가장 가까운 객체 하나 실행. D-03 객체의 액션은 객체에 유지. Quick 작업이므로 ROADMAP phase 수정 및 연구 단계는 없다. 신규 의존성을 설치하지 않는다. 기존 파일의 인코딩을 보존하고 주변 깨진 주석을 일괄 변환하지 않는다.
</context>

<tasks>
<task type="auto">
  <name>Task 1: 플레이어 소유 범위 탐색과 단일 액션 계약 구현</name>
  <files>Assets/Player/Script/IPlayerInteractable.cs, Assets/Player/Script/PlayerInteraction.cs, Assets/Player/Script/PlayerController.cs, Assets/Editor/PlayerInteractionVerification.cs, 신규 파일의 .meta</files>
  <action>D-01/D-02/D-03에 따라 IPlayerInteractable에 CanInteract(PlayerInteraction player), Interact(PlayerInteraction player)를 정의한다. PlayerInteraction에는 SerializeField 반경(기본 2f), LayerMask(기본 전체), 선택 Gizmo를 제공한다. 키 입력 시에만 Physics2D 원형 쿼리를 재사용 버퍼 또는 재사용 List로 수행하며 trigger도 포함한다. 버퍼 포화로 후보를 조용히 버리지 않는다. 충돌체 자신과 부모에서 계약을 찾고 중복 충돌체를 동일 객체로 합친다. 거리 기준은 플레이어 위치에서 객체 transform 위치까지 2D 제곱 거리로 통일하고 이 반경 밖 객체를 제외한다. 동률은 instance ID로 결정한다. 비활성/파괴/disabled/CanInteract=false 후보는 제외하고 최종 후보 하나만 재검증하여 실행한다. 모든 객체 전역 검색, LINQ/OverlapCircleAll 반복 할당, 객체별 Update를 도입하지 않는다. OnEnable와 Start에서 중복 방지 구독을 시도하고 구독한 InputHandler 인스턴스를 저장하여 OnDisable에서 정확히 해제한다. PlayerController.Awake에서 누락된 PlayerInteraction을 AddComponent하되 이미 있으면 재사용하여 기존 scene/prefab 플레이어도 즉시 동작하게 한다. 이 런타임 설치를 Inspector 설정용 명시 부착과 함께 사용할 수 있게 한다. 에디터 검증 진입점 PlayerInteractionVerification.Run을 만들고 임시 GameObject/Collider2D와 작은 테스트 계약 구현으로 거리 역순 후보, 범위 밖/비활성 후보, 동률, 중복 충돌체, 비어 있는 후보를 검증한다. 실제 씬/저장 데이터를 변경하지 않고 finally로 임시 객체를 정리한다.</action>
  <verify><automated>Unity Editor execute-menu 또는 -executeMethod PlayerInteractionVerification.Run 실행: nearest/empty/outside/disabled/duplicate/tie 검증 모두 성공. Unity가 이미 열려 있으면 동일 프로젝트 batch 실행 대신 연결된 Editor 실행을 사용한다.</automated></verify>
  <done>모든 기존 플레이어에 중앙 디스패처가 생성되며 입력 한 번당 반경 내 실행 가능한 후보 최대 하나가 호출된다.</done>
</task>

<task type="auto">
  <name>Task 2: 네 월드 객체를 액션 전용 계약으로 이관</name>
  <files>Assets/map/script/SignpostPortal.cs, Assets/map/script/Checkpoint.cs, Assets/map/script/InteractableWall.cs, Assets/map/script/3 stage/SlidingPuzzleTrigger.cs</files>
  <action>D-03에 따라 네 컴포넌트가 IPlayerInteractable을 구현하도록 하고 OnInteract 구독과 플레이어 감지 trigger/collision 및 range bool을 제거한다. 기존 물리 collider와 serialized public 필드, wall unlock 공개 API, puzzle 공개 API를 유지한다. Checkpoint는 전달받은 player에서 PlayerRespawn을 얻어 기존 저장/리스폰 변경을 실행한다. Portal은 설정된 유효 목적지일 때만 후보가 되고 기존 spawn point 설정 순서를 유지한다. Wall의 CanInteract는 현재 SaveLoadManager와 unlockDataKey의 true 여부를 부작용 없이 검사하고 실제 변경은 기존 TryUnlockFromSaveData에 맡긴다. 잠긴 wall/puzzle이 더 먼 실행 가능한 객체를 가로막지 않게 한다. Puzzle은 잠기지 않았고 puzzleUI가 존재할 때만 실행 가능하며 Pause 입력 구독/닫기와 OnPuzzleCleared 진행 갱신을 보존한다. 객체 자체의 위치를 공통 거리 기준으로 사용한다.</action>
  <verify><automated>Unity 스크립트 컴파일 오류 0; rg -n 'OnInteractEvent|isPlayerInRange|isPlayerNearby|OnTriggerEnter2D|OnCollisionEnter2D' Assets/map/script/SignpostPortal.cs Assets/map/script/Checkpoint.cs Assets/map/script/InteractableWall.cs 'Assets/map/script/3 stage/SlidingPuzzleTrigger.cs' 결과에서 실행 코드의 구독/감지 잔존 0을 확인한다.</automated></verify>
  <done>네 객체는 플레이어 감지를 수행하지 않고 선택된 객체만 기존 액션을 실행하며 잠긴 객체는 선택 대상에서 제외된다.</done>
</task>

<task type="auto">
  <name>Task 3: 물웅덩이 입력 경합 제거 및 회귀 검증 완료</name>
  <files>Assets/Enemy/WaterMonster/Script/Phase2/PlayerAbsorb.cs, Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs, Assets/Editor/PlayerInteractionVerification.cs</files>
  <action>D-02를 모든 Interact 구독자에 적용한다. PlayerAbsorb의 Interact 모드 직접 구독을 제거하고 WaterPuddle을 IPlayerInteractable로 연결한다. 해당 플레이어에 활성 PlayerAbsorb가 있고 Interact 모드이며 puddle이 destructible일 때 중앙 반경의 후보로 포함한다. PlayerAbsorb에 대상 puddle의 흡수 가능 여부/단일 흡수 메서드를 제공하여 RecoveryWater와 SetIndestructible 효과를 보존한다. 다른 네 입력 모드 구독과 absorbRadius를 보존하되 플레이어 자체 쿼리로 최근접 유효 puddle을 선택하고 객체 소유 playerInRange에 의존하지 않도록 한다. WaterPuddle의 playerInRange/trigger 관리 및 풀 반환 range 초기화는 참조를 검사한 뒤 제거한다. 풀 반환 destructible/색상/등록 해제는 유지한다. 검증에 잠긴 객체 skip, 월드 객체와 Interact puddle 동시 범위에서 하나만 실행, 흡수 완료/풀 반환, 다른 입력 모드 비경합, enable/disable 재구독 단일 실행을 추가한다. Save/scene 전환은 실제 진행 데이터를 바꾸지 않는 검증과 Play 확인 항목으로 구분한다. 마지막으로 변경 범위 git diff --check와 Unity 컴파일/검증을 실행하고 실제 실행하지 못한 Play 확인은 SUMMARY에 미검증으로 명시한다.</action>
  <verify><automated>PlayerInteractionVerification.Run 전체 성공; Unity 컴파일 오류 0; git diff --check; rg -n 'OnInteractEvent' Assets -g '*.cs'로 프로덕션 구독자가 PlayerInteraction 하나임을 확인한다.</automated></verify>
  <done>상호작용 키 하나로 월드 객체와 물웅덩이가 함께 실행되지 않고 타 입력 흡수, 잠금, 저장/리스폰, 포털, 퍼즐의 기존 효과가 보존된다. 검증 결과와 한계가 기록된다.</done>
</task>
</tasks>

<threat_model>
## Trust Boundaries
로컬 입력 이벤트에서 월드 상태 변경으로 넘어가는 경계만 존재한다. 외부 서비스/패키지/네트워크 경계는 추가하지 않는다.

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|---|---|---|---|---|
| T-quick-01 | Tampering | locked wall/puzzle dispatch | mitigate | CanInteract에서 기존 해금/잠금 조건을 검사하고 액션의 기존 검사도 유지 |
| T-quick-02 | Denial of service | duplicated subscriptions/collider hits | mitigate | 저장한 입력 인스턴스의 구독 해제, 중복 후보 제거, 단일 실행 |
</threat_model>

<verification>
Unity 컴파일 및 에디터 행동 검증을 실행한다. Play 가능 시 서로 겹친 두 객체, 거리 변경, 비활성화, scene 재진입, 키 재설정, 체크포인트, 포털, 잠금 벽/퍼즐, 물웅덩이 경합을 확인한다. Unity를 사용할 수 없는 경우 컴파일/Play 성공으로 표기하지 않고 사용한 대체 검증과 미확인 항목을 명시한다.
</verification>

<success_criteria>
입력 구독 및 범위 선택은 플레이어에 집중되고 가장 가까운 실행 가능한 객체 하나만 작동한다. 네 월드 객체와 물웅덩이의 기존 액션/공개 API가 보존된다. 플레이어 scene 연결 누락과 입력 중복 구독이 없다.
</success_criteria>

## Source coverage audit
| Source | Item | Coverage |
|---|---|---|
| GOAL | 플레이어 범위/최근접 객체 단일 실행 | Tasks 1–3 |
| REQ | QUICK-INTERACTION-01 player range | Task 1 |
| REQ | QUICK-INTERACTION-02 nearest single action | Tasks 1, 3 |
| REQ | QUICK-INTERACTION-03 preserve object actions | Tasks 2, 3 |
| RESEARCH | 없음: standard quick, 신규 의존성 없음 | N/A |
| CONTEXT | D-01/D-02/D-03 사용자 요청 | Tasks 1–3 |

요구사항 ID는 quick 작업의 로컬 추적용이며 ROADMAP phase 요구사항을 변경하지 않는다.

<output>
Create `.planning/quick/260912-kih-centralize-interaction-detection-on-play/260912-kih-SUMMARY.md` when done. commit_docs=false를 존중한다.
</output>

## User scope update during execution

The user additionally requested a UI string above the current interaction target displaying the actual interaction key. Task 1 now includes `Assets/Player/Script/PlayerInteractionPrompt.cs` and its meta: a single player-owned TMP overlay follows the nearest eligible target and reads the current Input System binding display string, refreshing after rebinding. The central player query therefore runs in LateUpdate for target/prompt maintenance as well as on input for revalidation; it reuses buffers and introduces no object-owned proximity scan. Task 3 includes target-clear and binding-override checks. Formatted prompt text is cached rather than allocated every frame.
