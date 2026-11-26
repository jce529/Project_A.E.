# Phase 21: 인벤토리 UI + 키 바인딩 + 월드 아이템 획득 피드백 - Research

**Researched:** 2026-11-05
**Domain:** Unity 6000.3.10f1, C#, Input System 1.18.0, uGUI 2.0.0 + TMP (코드 생성 UI)
**Confidence:** HIGH (모든 핵심 사실은 이 레포의 코드/YAML/JSON을 직접 읽어 확인. 외부 라이브러리 신규 도입 없음)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** 새 InputAction `Inventory`를 `InputSystem_Actions.inputactions`의 `Player` 맵에 추가, 기본 바인딩 `<Keyboard>/tab`. 같은 키로 열기/닫기(토글), Esc로도 닫을 수 있다.
- **D-02:** `InputHandler`에 `OnInventoryEvent`와 `inventoryAction` 필드를 기존 11개 액션과 동일한 패턴(Find -> BindCallbacks/UnbindCallbacks -> Handle 메서드)으로 추가.
- **D-03:** `ControlsSettingsPanel`의 리바인딩 목록(Inspector 기반 `RebindEntry`)에 `Inventory` 항목 노출. 저장은 기존 `InputHandler.SaveBindingOverrides` 경로.
- **D-04:** 열리면 `GameState.Inventory`로 전환(timeScale 0 은 GameStateManager가 이미 처리). 닫으면 `Playing`. `InGameHUDController`가 자동으로 HUD 숨김.
- **D-05:** `Playing` 상태에서만 열 수 있다. 열림 중 Esc는 인벤토리 쪽에서 `OnPauseEvent`를 구독해 닫는다(일시정지 메뉴가 같이 열리지 않게). `SaveSlotDialog.HandlePause` 선처리 순서는 건드리지 않는다.
- **D-06:** `ItemData`에 `Sprite icon`, `string displayName` + 읽기 전용 접근자 추가. `HealthPotion`, `AncientKey` 에셋에 값 채움(placeholder 허용). 아이콘이 비면 displayName 텍스트 폴백. 설명(blurb) 필드 없음.
- **D-07:** `ItemData`의 기존 필드/`UseEffect` 계약과 `Id`는 변경 금지(추가만).
- **D-08:** 슬롯 클릭=선택(하이라이트 + 선택 슬롯 displayName/수량 표시), 선택 슬롯 우클릭(또는 더블클릭)=`Inventory.UseItem(index)`. 사용 후 즉시 갱신. 빈 슬롯은 무반응.
- **D-09:** UI는 Progression 타입 아이템 사용을 막는다. `ItemData`에 `Type` 접근자 필요. 가드 위치(UI vs `Inventory.UseItem`)는 계획 재량. Phase 19 세이브 계약(`TryAddItem`/`GetSlot`) 유지.
- **D-10:** 패널과 토스트는 씬/프리팹 수작업 없이 **스크립트가 런타임에 uGUI 생성**. 5x4 `GridLayoutGroup`, TMP. 새 컴포넌트는 `Assets/Player/Script/UI/`.
- **D-11:** 폴링 없이 열릴 때/변경 시점에 갱신. `Inventory`에 `event Action OnChanged` 추가 허용(반환 계약/원자성 유지).
- **D-12:** 획득 성공 시 HUD 영역 토스트("{displayName} x{count} 획득"), `TryAddItem` 실패 시 "인벤토리가 가득 찼습니다" 토스트. 월드 플로팅 텍스트 불채택.
- **D-13:** `WorldItem.Interact`가 UI를 직접 호출하지 않고 이벤트 발행(정적 이벤트 -> 토스트 구독 우선). Phase 18 D-06 "성공 시에만 Destroy" 유지. 시그니처는 재량.
- **D-14:** 토스트는 unscaled 시간으로 센다. 연속 획득은 큐/교체 중 택일(재량), 화면을 가리지 말 것.

### Claude's Discretion
- 패널 크기/색/간격/폰트 크기, 토스트 지속(권장 ~2초), 위치.
- `ItemData` 신규 필드의 직렬화 이름, placeholder 아이콘 제작 방식.
- UI 파일 분할 수준(최소 파일 수, 과설계 금지).
- 열림 중 이동/공격 입력 차단 방식.
- 우클릭 vs 더블클릭 최종 제스처 (선택 개념은 유지).

### Deferred Ideas (OUT OF SCOPE)
- 드래그앤드롭, 아이템 버리기(월드 드롭), 정렬/자동 정리
- 툴팁/상세 설명창(설명 필드)
- 퀵슬롯/핫바, Heal(1키)과 인벤토리 연동
- 획득/사용 사운드
- 슬롯 인덱스 영속화
- 게임패드 내비게이션
</user_constraints>

<phase_requirements>
## Phase Requirements

매핑된 requirement ID 없음(TBD). 아래 "Phase Success Targets"를 계획 시 must-have로 사용.

| ID | Description | Research Support |
|----|-------------|------------------|
| (none) | Tab으로 5x4 패널 토글, Esc 닫기 | 섹션 1(입력 가드), 섹션 3(수명주기), Pattern 1~2 |
| (none) | 새 `Inventory` 액션 + 리바인딩 패널 노출 | 섹션 4, 5 (JSON + 씬 YAML) |
| (none) | 슬롯 선택/사용, Progression 차단 | 섹션 6, Pattern 3 |
| (none) | 월드 아이템 획득/가득 참 토스트 | 섹션 2, Pattern 4 |
</phase_requirements>

## Summary

핵심 발견은 세 가지다. (1) **게임플레이 입력 구독자 중 `GameState`를 보는 곳은 단 한 곳도 없다.** `PauseMenu`/`AutoSaveTimer`/`HUD`만 상태를 본다. `PlayerController`(Jump/Dash), `PlayerInputHandler`->`PlayerAttack`(BasicAttack/Skill1/Skill2/Heal/SkillQ), `PlayerInteraction`(Interact), `PlayerAbsorb`, `PrisonDebuff`는 모두 `InputHandler` 이벤트를 무조건 처리한다. 심지어 현재 Paused 상태에서도 새는 구멍이다(일시정지 메뉴 버튼을 좌클릭하면 `BasicAttack`(= `<Mouse>/leftButton`)이 발화). 인벤토리 슬롯을 좌클릭 선택하는 순간 매번 공격이 발생하므로 **가드는 필수**다. 가장 작은 수정은 `InputHandler`의 8개 Handle 메서드(Jump/Dash/BasicAttack/Skill1/Skill2/Heal/Interact/SkillQ)에서 `GameState.Inventory`일 때 이벤트를 발행하지 않는 것(한 곳, 구독자 수정 0).

(2) **세이브 복원은 `Inventory.TryAddItem`을 직접 호출**하며 `WorldItem`을 거치지 않으므로, 토스트를 `WorldItem`의 정적 이벤트에만 연결하면 복원 중 토스트는 절대 뜨지 않는다. `Inventory.OnChanged`는 복원 중 여러 번 발화하지만 UI는 "dirty 플래그만 세우고 열려 있을 때만 다시 그림"이면 무해하다.

(3) 런타임 UI 부착은 **`Inventory.Awake`(또는 `PlayerController.Awake`가 `PlayerInteraction`을 AddComponent 하는 기존 패턴)에서 `InventoryUI`를 AddComponent** 하는 방식이 씬/프리팹 YAML 수정 0으로 가장 안전하다. `Player.prefab`은 `1 stage`, `Tutorial Map`(빌드 포함)과 `2 stage`, `3 stage main`(빌드 미포함)에서 쓰이고, `Inventory`는 Player와 함께 씬마다 새로 생성된다. 씬 3개 모두 `EventSystem + InputSystemUIInputModule`이 이미 있다.

**Primary recommendation:** `InputHandler`에서 Inventory 상태일 때 8개 게임플레이 이벤트를 억제 + `Inventory.Awake`에서 `InventoryUI` AddComponent(패널+토스트 한 컴포넌트, 슬롯 뷰는 중첩 클래스 또는 동일 파일) + `ItemData` 필드 3개 추가 + `.inputactions` JSON 1액션/1바인딩 추가 + 씬 2곳(`1 stage`, `Tutorial Map`)의 `ControlsSettingsPanel`에 행 1개 추가(YAML 편집 또는 Unity CLI 편집 스크립트).

## Project Constraints (from CLAUDE.md)

- 코딩 전 `.planning/` 계획 확인, Phase 격리(Phase 22+ 코드 미리 작성 금지), 오버엔지니어링 금지(요청 이상 기능/추상화/유연성 금지).
- 정밀한 변경: 인접 코드/주석/포맷 임의 리팩토링 금지. 자신의 변경으로 고아가 된 것만 정리. 기존 데드 코드는 언급만.
- 변경된 모든 줄은 계획에 추적 가능해야 한다.
- 검증 가능한 목표: `[단계] -> 검증: [방법/성공 기준]` 형식으로 계획/검증 명시.
- YOLO 모드: `.planning/config.json`에 `"mode": "yolo"`는 없음(`runtime: codex`, `nyquist_validation: false`) -> Validation Architecture 섹션 생략.
- 사용자 메모리: GSD 코드 실행은 codex CLI 위임, Unity 에디터 구동은 `unity` 명령(`C:/Users/chang/AppData/Local/Unity/bin/unity` 확인됨; eval은 리플렉션, console은 게임 로그, `capture_game_view`는 오버레이 UI까지 캡처).
- 코드 파일 인코딩 주의: 일부 기존 `.cs`(예: `SlidingPuzzleTrigger.cs`, `PlayerAttackBase.cs`, `PlayerInputHandler.cs` 주석)는 비-UTF-8 레거시 인코딩. 이 phase에서는 수정 대상 아님(수정 대상 파일은 모두 UTF-8: `InputHandler.cs`, `Inventory.cs`, `WorldItem.cs`, `ItemData.cs`).

## 1. Open Item (1): 열림 중 입력 구독자 전수 조사

`InputHandler`는 이벤트를 `performed` 콜백에서 무조건 발행한다(`InputHandler.cs:193-209`). timeScale 0 은 `Update`/`FixedUpdate`/`WaitForSeconds`/`Time.time` 진행만 멈출 뿐 입력 이벤트는 막지 못한다.

| 구독자 | 파일 | 이벤트 | 상태 가드 | Inventory 열림 중 실제 동작 | 가드 필요 |
|--------|------|--------|-----------|----------------------------|-----------|
| `PlayerController.OnMove` | `PlayerController.cs:76,109` | Move | 없음 | `moveInput` 갱신(이동은 FixedUpdate 정지라 무해). 키를 떼면 Zero로 갱신되는 편이 오히려 안전 | **아니오** (통과시킬 것) |
| `PlayerController.OnRun` | `:78,145` | Run | 없음 | 플래그만 갱신 | **아니오** |
| `PlayerController.OnJump` | `:77,119` | Jump | `movementLocked`만 | `rigid.linearVelocity` 설정 + `jumpCount++`. 닫는 순간 점프가 발동 | **예** |
| `PlayerController.OnDash` | `:79,137` | Dash | `movementLocked`만 | `DashTowardsMouse` 코루틴 시작, 콜라이더 비활성화, `Time.time` 기반 루프가 timeScale 0 동안 멈춰 **충돌 꺼진 채 고착** | **예** |
| `PlayerAttack.OnBasicAttack` | `PlayerInputHandler.cs:12`, `PlayerAttack.cs:62` | BasicAttack(LMB) | `canAttack`만 | `attackBox` Instantiate; Destroy 코루틴은 `WaitForSeconds`라 멈춤 -> 박스가 남고 `canAttack`도 false로 고착. **슬롯 LMB 선택마다 발화** | **예 (필수)** |
| `PlayerAttack.OnSkillE/R/OnHeal/OnSkillQ` | `PlayerAttack.cs:121-157` | Skill1/2/Heal/Q | 자원/쿨다운만 | `OnHeal`은 물병 소모 + 체력 회복(실효과), Skill2는 부스트 토글, Q는 장애물 파괴 | **예** |
| `PlayerAttackBase` | `PlayerAttackBase.cs:28-31` | Basic/S1/S2/Heal | 없음 | 추상 베이스. `PlayerAttack`은 `PlayerInputHandler`를 상속(동일 이벤트). 현재 `PlayerAttackBase` 사용 클래스는 코드에서 확인 안 됨 | 중앙 가드로 함께 해결 |
| `PlayerInteraction.HandleInteract` | `PlayerInteraction.cs:39,48` | Interact(F) | 없음 | `TryInteract` -> **월드 아이템 획득/체크포인트/포탈이 인벤토리가 열린 채 실행** | **예** |
| `PlayerAbsorb` | `WaterMonster/.../PlayerAbsorb.cs:27-30` | 설정된 1개 | 없음 | 물 흡수 시도 | 중앙 가드로 해결 |
| `PrisonDebuff.OnMash` | `PrisonDebuff.cs:33` | Jump | 없음 | 감옥 디버프 탈출 연타 | 중앙 가드로 해결 |
| `CameraController.HandleMoveInput` | `CameraController.cs:166` | Move | 없음 | 카메라 룩어헤드 입력 | **아니오** |
| `PauseMenu.OnPauseInput` | `PauseMenu.cs:26` | Pause(Esc) | `Playing`일 때만 Open | Inventory 중에는 열리지 않음(OK). **순서 함정은 아래** | 아래 참고 |
| `SlidingPuzzleTrigger.HandlePauseInput` | `SlidingPuzzleTrigger.cs:19,30` | Pause | puzzleUI 활성 시 닫음 | 무관 | 아니오 |

**현재 Paused에서도 같은 누수가 있다**(관찰만, 이번 phase 범위 밖): `PauseMenu`의 버튼 클릭 = LMB = `BasicAttack`. 가드를 `Paused`까지 넓히는 것은 범위 확장이므로 **Inventory에만 한정**하고, 확장 여부는 후속 이슈로 남기는 것을 권장(CLAUDE.md 정밀 변경 원칙).

### 권장 가드: `InputHandler` 중앙 억제 (구독자 수정 0)

```csharp
// Source: 이 레포 InputHandler.cs 패턴 + GameStateManager.cs (직접 분석)
private static bool GameplayInputBlocked =>
    GameStateManager.Instance != null &&
    GameStateManager.Instance.CurrentState == GameStateManager.GameState.Inventory;

private void HandleJumpPerformed(InputAction.CallbackContext c)
{ if (!GameplayInputBlocked) OnJumpEvent?.Invoke(); }
// Dash/BasicAttack/Skill1/Skill2/Heal/Interact/SkillQ 동일. Move/Run/Pause/Inventory는 가드 금지.
```

- **Move/Run은 통과**시킨다: 열림 중 키를 떼는 `canceled`가 막히면 `moveInput`이 stale로 남아 닫은 뒤 캐릭터가 계속 걷는다.
- Esc(Pause)와 신규 Inventory 이벤트는 가드하지 않는다(닫기 입력이므로).
- 대안(구독자별 `GameStateManager` 체크)은 5개 파일 수정 + 향후 구독자 추가 시 누락 위험 -> 비권장. `InputHandler`는 이미 `PersistentManagers`에서 `GameStateManager`와 같은 프리팹(`Resources/PersistentManagers.prefab`)에 있어 `Instance` 접근이 안전하다.
- **"눌린 채 열림" 엣지**: Dash 등은 `performed`(버튼 press) 단발이라 가드로 충분. Heal(1), Skill 키를 열림 중에 눌러도 아무 일 없고 닫은 뒤 새로 눌러야 한다 -> 의도에 부합.

### Esc 순서 함정 (D-05) - 필수 대응
`OnPauseEvent`는 구독 순서대로 호출된다. `PauseMenu.Start`와 `InventoryUI.Start`의 상대 순서는 보장되지 않는다. `InventoryUI`가 Esc 핸들러에서 **동기적으로** `SetState(Playing)` 하고 `PauseMenu`가 그 뒤에 호출되면 상태가 이미 `Playing`이라 **일시정지 메뉴가 같이 열린다.**
- **대응(PauseMenu 무수정):** `InventoryUI`의 Pause 핸들러는 `pendingClose = true`만 세우고, 실제 `Close()`는 `LateUpdate`(timeScale 0 에서도 실행됨)에서 처리. 어느 순서든 `PauseMenu`는 그 프레임에 `Inventory` 상태를 본다.
- Tab 토글은 별도 액션이라 동기 처리해도 무방.
- `SaveSlotDialog.HandlePause()`는 `PauseMenu.OnPauseInput` 내부에서만 호출되므로 인벤토리 쪽이 건드릴 필요 없다(D-05 준수).

### 상태 동기화 (열림 중 외부가 상태를 바꾸는 경우)
- `GameStateManager.HandleSceneLoaded`가 모든 씬 로드에서 `Playing`으로 강제 복귀 + `OnGameStateChange(Playing)` 발행. `TutorialDeadState`는 `Paused`로 설정.
- `InventoryUI`는 `OnGameStateChange`를 구독해 **새 상태가 `Inventory`가 아니면 패널만 숨김**(상태를 되돌려 설정하지 않는다). 자기 자신이 `SetState(Inventory)`를 호출할 때의 이벤트는 무시하도록 처리.
- 열기 조건: `CurrentState == Playing`일 때만 `SetState(Inventory)` (D-05). 닫기 조건: `CurrentState == Inventory`일 때만 `SetState(Playing)`.
- `AutoSaveTimer`는 `Playing`일 때만 저장(`AutoSaveTimer.cs:150`) -> 인벤토리 열림 중 자동 저장 없음. 확인됨, 추가 작업 없음.
- 샌드박스 주의: 스퍼리어스 열림 가능 상황 - 슬라이딩 퍼즐 UI(`SlidingPuzzleTrigger.puzzleUI`)는 `GameState.Puzzle`을 쓰지 않고(Puzzle 상태 사용처 0) `Playing` 상태에서 GameObject만 켠다. 퍼즐 UI 위에서도 Tab이 열릴 수 있다. D-05 기준으로는 허용된 동작이며 이번 phase에서 막지 않는 것을 권장(미해결 질문 2 참고).

## 2. Open Item (2): 이벤트와 SaveLoadManager 복원

- 복원 경로: `SaveLoadManager.LoadSceneAndRestoreRoutine` -> `yield return op` 이후 `ApplyInventoryFromSave()` -> `FindAnyObjectByType<Inventory>()` -> `inv.TryAddItem(item, entry.count)` 반복 (`SaveLoadManager.cs:469-542`). **`WorldItem`을 전혀 거치지 않는다.** 캡처 쪽은 `GetSlot(i)` 읽기만 한다(`:387-401`).
- 결론: `WorldItem`에서만 발행하는 정적 이벤트(`WorldItem.PickupAttempted` 류)를 토스트가 구독하면 복원 시 토스트 0회. 복원 중 `Inventory.OnChanged`는 엔트리 수만큼 발화하지만 `InventoryUI`가 이벤트 핸들러에서 `dirty = true`만 세우고 패널이 열려 있을 때 `Refresh()` 하면 비용/가시 부작용 없음.
- 타이밍: `InventoryUI`가 `Inventory.Awake`에서 붙으면 씬 로드 직후 `Start`까지 완료된 상태로 복원이 호출되므로 구독 누락 없음. 구독은 `Awake`/`OnEnable`에서 `inventory.OnChanged += MarkDirty` 해도 무방.
- `OnChanged`는 `TryAddItem`(성공 경로 return true 직전), `RemoveItem`(성공 시), `UseItem`(성공 시) 세 군데에서 **상태 변경이 확정된 후 한 번** 호출. 실패(return false) 경로에서는 호출하지 않는다. 반환값/원자성은 그대로.
- **`UseItem` 순서 주의:** `slot.item.UseEffect(playerInteraction)` 이후 `count--`. `UseEffect`는 `PlayerStats.Heal`을 호출하며 이는 다른 UI(체력바)를 갱신할 뿐 `Inventory`를 재진입하지 않는다 -> `OnChanged` 재진입 위험 없음.
- 정적 이벤트 위생: 이 레포는 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)] ResetStatics()` 패턴을 쓴다(`InputHandler`, `GameStateManager`). `WorldItem`의 정적 이벤트 구독자는 `OnDestroy`에서 반드시 해제하고, 필요하면 동일 패턴으로 `ResetStatics`에서 이벤트를 null 처리(도메인 리로드 비활성 Play 대비).
- 구독자는 Player에 붙은 `InventoryUI`뿐이므로 씬 전환 시 Player와 함께 파괴된다. 해제를 빠뜨리면 파괴된 객체 참조가 정적 이벤트에 남는다.

## 3. Open Item (3): 수명주기 / 부착 위치 / 렌더링 규약

### 부착 위치 결정: `Inventory.Awake`에서 AddComponent (권장)
- 근거 1: 이 레포의 확립된 패턴 - `PlayerController.Awake`가 `PlayerInteraction`을 AddComponent(`PlayerController.cs:61-62`), `PlayerInteraction.Awake`가 `PlayerInteractionPrompt`를 AddComponent(`PlayerInteraction.cs:16-20`). 후자는 코드 생성 캔버스의 선례.
- 근거 2: `Player.prefab`이 사용되는 모든 씬(`1 stage`, `Tutorial Map`, `2 stage`, `3 stage main`)에서 수동 배선 없이 동작. 빌드 설정 포함 씬은 `MainMenu`, `1 stage`, `Tutorial Map` 3개(`EditorBuildSettings.asset`). `MainMenu`에는 Player가 없으므로 인벤토리 UI도 없다(정상).
- 근거 3: `Inventory`는 씬마다 새로 `Awake`(슬롯 20개 재생성)되어 Player와 수명이 같다. UI도 같은 객체에 두면 참조 획득이 `GetComponent<Inventory>()` 한 줄.
- 대안 `PersistentManagers`: `Inventory`가 씬 단위라 참조 재획득이 필요하고 `PersistentManagers.prefab` YAML 수정이 필요 -> 비권장.
- 대안 씬 UI Canvas: 씬 2개+ YAML 수정 필요, D-10(수작업 없음)에 반함.
- `Player.prefab`의 `Inventory` 컴포넌트 guid: `18a1c4e7b3f04d2ab9c6e5d7f0a1b2c3`. 프리팹은 **수정하지 않는다**.

> 아키텍처 부수 관찰(범위 밖): `Inventory`는 씬 전환(포탈)마다 새로 만들어지므로 씬 이동 시 아이템이 사라진다. Phase 19 저장/로드 흐름에서만 복원된다. 이번 phase에서 해결하지 말 것.

### 캔버스/EventSystem/폰트 규약
| 항목 | 확인된 값 | 근거 |
|------|-----------|------|
| 기존 UI Canvas | Screen Space - Overlay(`m_RenderMode: 0`), `CanvasScaler` Scale With Screen Size, 기준 해상도 **1920x1080**, `m_ScreenMatchMode: 0`(Match W/H), `m_MatchWidthOrHeight: 0`, `sortingOrder: 0` | `Assets/Scenes/1 stage.unity` ("UI" 캔버스 `m_UiScaleMode: 1`) |
| 코드 생성 캔버스 선례 | World Space, `sortingOrder = 100`, 텍스트 `raycastTarget=false` | `PlayerInteractionPrompt.cs:55-68` |
| EventSystem | 3개 빌드 씬 모두 `EventSystem` + `InputSystemUIInputModule`(guid `01614664b831546d2ae94a42149d80ac`, 액션 에셋 `ca9f5fa95ffab41fb9a615ab714db018`) + 레거시 모듈 컴포넌트(`76c392e4...`) 보유 | 씬 YAML 직접 확인. `activeInputHandler: 2`(Both) |
| UI 입력과 InputHandler | `InputHandler`는 `Instantiate`한 런타임 복제 에셋을 쓰고, UI 모듈은 별개 액션 에셋을 쓴다 -> **UI 클릭/포인터 이벤트는 InputHandler와 무관하게 동작** | `InputHandler.cs:70` |
| TMP 기본 폰트 | `TMP Settings`의 기본 = `LiberationSans SDF`(guid `8f586378b4e144a9851e7b34d9b748ee`), **폴백 = `NotoSansKR-Regular SDF`**(guid `b69306c3e6c2d2d448ea53eac684ceb4`). 씬의 TMP 50개 전부 기본 폰트 사용 | `TMP Settings.asset:27,36-37`, 씬 grep |
| 한글 글리프 | `NotoSansKR-Regular SDF`는 정적 아틀라스(`m_AtlasPopulationMode: 0`), 글리프 11930개. "인벤토리가 가득 찼습니다", "획득", "포션", "고대의 열쇠", "체력 회복 물약" 전 글자 포함 확인(스크립트로 검증) | `Assets/TextMesh Pro/Fonts/NotoSansKR-Regular SDF.asset` |

**코드 생성 UI에서 폰트를 지정할 필요 없음:** `new GameObject().AddComponent<TextMeshProUGUI>()`는 `TMP_Settings.defaultFontAsset` + 폴백 체인을 자동으로 사용한다(`PlayerInteractionPrompt`도 폰트 미지정). 한글은 폴백으로 해결. 폰트 에셋 직접 로드 금지.

**생성 캔버스 사양 권장:** `Canvas`(Overlay, `sortingOrder` 50 안팎) + `CanvasScaler`(ScaleWithScreenSize, 1920x1080, `matchWidthOrHeight` 기존과 동일하게 0) + **`GraphicRaycaster`(필수 - 없으면 슬롯 클릭이 영원히 안 먹음)**. 패널 루트는 기본 비활성, 열릴 때 `SetActive(true)`. 토스트는 패널과 별도 캔버스(또는 별도 루트)로 두어 패널이 닫혀도/`InGameHUDController`가 HUD를 숨겨도 영향 받지 않게 한다(`InGameHUDController`는 `hudElements` 배열에 할당된 오브젝트만 토글하므로 코드 생성 UI는 영향 없음).
- 토스트 `raycastTarget=false`, 패널 배경 `Image.raycastTarget=true`(뒤 월드 클릭 흡수; 월드는 UI 모듈이 막지 않지만 어차피 가드로 공격이 억제됨).
- 방어적 코드(선택): `EventSystem.current == null`이면 `EventSystem` + `InputSystemUIInputModule` 생성. 현재 빌드 씬 3개 모두 존재하므로 기본은 생략 가능, 빌드 미포함 `2 stage`/`3 stage main` 확인은 하지 않았다(LOW).

## 4. `InputSystem_Actions.inputactions` 편집 (JSON, 코드 외 파일)

**파일:** `Assets/InputSystem_Actions.inputactions` (JSON, ScriptedImporter, 메타 guid `2bcd2660ca9b64942af0de543d8d7100`). `PersistentManagers.prefab`은 이 guid의 메인 에셋 `fileID: -944628639613478452`를 참조하므로 **참조는 그대로**, 에셋 내용만 바뀜.

확인된 구조(`Player` 맵 id `df70fa95-8a34-4494-b137-73ab6b9c7d37`, 맵은 `Player`, `UI` 2개):
- `actions[]` 항목 형식(Interact 기준): `name`, `type: "Button"`, `id`, `expectedControlType: "Button"`, `processors: ""`, `interactions: ""`, `initialStateCheck: false`.
- `bindings[]` 항목 형식: `name: ""`, `id`, `path`, `interactions: ""`, `processors: ""`, `groups: ""`, `action`, `isComposite: false`, `isPartOfComposite: false`.
- **기존 Player 바인딩의 `groups`는 전부 빈 문자열** (Keyboard&Mouse 스킴 그룹 미할당). 일관성을 위해 신규도 `""`. 컨트롤 스킴은 `Keyboard&Mouse`, `Gamepad`, `Touch`, `Joystick`, `XR`가 정의돼 있으나 Player 바인딩은 사용하지 않음.
- ID는 기존이 손으로 만든 GUID 형태이므로 임의의 유효한 고유 GUID(소문자 8-4-4-4-12)면 된다. 예:
  - 액션 id: `a3b4c5d6-e7f8-4012-6789-012345678901`
  - 바인딩 id: `f2a3b4c5-d6e7-4901-5678-888888888888` (기존 패턴 `...-777777777777` 다음)
  - 이미 쓰인 id와 충돌 없는지 `grep`으로 확인 후 사용.

추가할 JSON (`Player` 맵):
```json
// actions[] 마지막(Interact 뒤)에 추가
{
  "name": "Inventory",
  "type": "Button",
  "id": "a3b4c5d6-e7f8-4012-6789-012345678901",
  "expectedControlType": "Button",
  "processors": "",
  "interactions": "",
  "initialStateCheck": false
}
// bindings[] 마지막에 추가
{
  "name": "",
  "id": "f2a3b4c5-d6e7-4901-5678-888888888888",
  "path": "<Keyboard>/tab",
  "interactions": "",
  "processors": "",
  "groups": "",
  "action": "Inventory",
  "isComposite": false,
  "isPartOfComposite": false
}
```
- 현재 사용 키(Space, X, LShift, Esc, LMB, F, E, R, 1, Q, WASD)와 Tab 충돌 없음(JSON 전수 확인).
- **줄바꿈:** 작업트리는 CRLF(`i/lf w/crlf`, git은 LF 저장). 에디터로 편집하든 스크립트로 하든 기존 줄바꿈을 유지하고 파일 전체 재정렬/재직렬화는 하지 말 것(diff 최소화). 실제 편집은 `Edit`로 두 블록만 삽입.
- 실행 전 `node -e "JSON.parse(...)"` 같은 문법 검증으로 JSON 유효성 확인(Unity import 실패 시 에셋 전체가 깨져 InputHandler가 전부 죽는다 - 치명적).
- C# 쪽(`InputHandler`): 이름은 `"Inventory"`로 `playerMap.FindAction("Inventory", false)`. 기존 패턴(필드 `inventoryAction`, `OnInventoryEvent`, `BindCallbacks`/`UnbindCallbacks`/Handle) 그대로.
- 저장된 바인딩 오버라이드 JSON(`SettingsData.InputBindingsJson`)은 액션 id/이름으로 매칭하므로 신규 액션 추가가 기존 사용자 오버라이드를 깨지 않는다(`LoadBindingOverridesFromJson`은 모르는 항목을 무시) - 코드 확인은 안 했으나 Input System 표준 동작(MEDIUM).

## 5. `ControlsSettingsPanel` 리바인딩 목록 - 씬 YAML 편집 (코드 외)

**직렬화 위치:** `RebindEntry`는 프리팹이 아니라 **씬 안에 직접 직렬화**되어 있다.
| 씬 | 컴포넌트 fileID | 라인 | 빌드 포함 |
|----|----------------|------|-----------|
| `Assets/Scenes/1 stage.unity` | `&324690428` (GameObject `324690426`) | ~1998 | 예 |
| `Assets/Scenes/Tutorial Map.unity` | `&1031190450` (GameObject `1031190446`) | ~10535 | 예 |
| `Assets/_Recovery/0.unity` | (복구용) | - | 아니오 - 무시 |

스크립트 guid `b852cd07d93b8494a8bdcef999f43d48`. 프리팹에는 없음.

**YAML 형태(현재 12개 엔트리):**
```yaml
  entries:
  - actionName: Move
    bindingIndex: 1
    rebindButton: {fileID: 1520025611}
    keyLabel: {fileID: 1143209011}
  ...
  - actionName: Action
    bindingIndex: 0
    rebindButton: {fileID: 1687231206}   # Button 컴포넌트 fileID
    keyLabel: {fileID: 1408623862}       # TMP_Text 컴포넌트 fileID
  listeningOverlay: {fileID: 1739240893}
  resetButton: {fileID: 1898288415}
```
- `rebindButton`/`keyLabel`은 씬 내 **실제 UI 오브젝트**의 컴포넌트 참조다. 즉 엔트리만 추가하면 안 되고 **행(Row) UI 오브젝트 4개를 씬에 새로 만들어야** 한다.
- 행 구조(`Action` 행 기준, `1 stage`): 행 GameObject(`1576333888`, RectTransform `1576333889`, 부모 = 컨텐츠 `1406252274`)가 자식 2개를 가진다: `ActionLabel`(`960624151`, 텍스트) + `KeyBtn`(`1687231204`; RectTransform + CanvasRenderer + Image + **Button** `1687231206`) 및 그 자식 `KeyText`(`1408623860`; TMP 컴포넌트 `1408623862`).
- 레이아웃은 **수동 절대 좌표**(LayoutGroup 없음): 행은 앵커 (0,1)/(0,1), `Action` 행 `m_AnchoredPosition: {x: 238.5, y: -694}`, `m_SizeDelta: {461, 44}`; 라벨 `{157.5, -22}` `{155,36}`; 키 버튼 `{316, -22}` `{130,36}`. 컨텐츠(`Content`, `&1406252274`)는 `m_SizeDelta: {0, 784}`이고 자식 16개 중 `Reset` 버튼(`1631625789`)이 `y=-749`(높이 54)로 맨 아래.
- 새 `Inventory` 행은 `y=-738` 근처에 추가하고, `Reset` 버튼을 한 줄(약 -44) 아래로 이동(`y≈-793`), 컨텐츠 높이를 ~830으로 늘려야 겹치지 않는다. 수치는 시각 확인 필요(재량).
- 두 씬 모두 같은 작업 필요(`m_Name: KeyBtn` 각 12개 = 키 행 수 동일).
- 핵심 주의: 기존 `ControlsSettingsPanel` 엔트리 중 `BasicAttack bindingIndex: 1`은 JSON에 BasicAttack 바인딩이 1개(index 0 = LMB)뿐이라 **범위 밖 인덱스**다(주석은 "1 = 키보드 F"라 하지만 현재 JSON에 없음). 이 phase의 범위 밖 기존 결함이므로 건드리지 말고 계획서에 "관찰됨, 미수정"으로 기록. 신규 `Inventory` 엔트리는 `bindingIndex: 0`.
- `ControlsSettingsPanel.RefreshLabel`은 `action.GetBindingDisplayString(0)` -> Tab 표시. `InputHandler.GetAction("Inventory")`가 런타임 복제 에셋에서 찾으므로 JSON 추가가 선행되어야 한다.

**편집 방식 권장(둘 중 하나, 계획 재량):**
1. **Unity 에디터 헤드리스/CLI 편집 스크립트**(사용자 메모리: `unity` 명령 사용): 기존 `Action` 행을 `Instantiate`해 복제 -> 이름/좌표 수정 -> `ControlsSettingsPanel.entries`에 추가 -> 씬 저장. fileID/직렬화 정합성을 Unity가 보장 -> **권장**(수작업 YAML ~12개 문서 x 2씬은 fileID 충돌/누락 위험이 큼).
2. 수작업 YAML: GameObject(1) + RectTransform(1) + 라벨/버튼/텍스트 각각 GameObject+RectTransform+CanvasRenderer+Image/Button/TMP 컴포넌트 등 약 12개 `--- !u!` 문서 x 씬 2개. 고유 fileID(씬 내 미사용 정수) 할당과 부모 `m_Children` 갱신 필요. 가능하나 위험.
- 어느 쪽이든 편집 후 **씬 로드 상태에서 Play 확인**(키 라벨이 "Tab"으로 보이고 리바인딩 성공 시 `SaveBindingOverrides` 호출).
- 대안(코드 변경만으로 YAML 회피)은 D-03("Inspector 기반 RebindEntry 목록에 노출")과 어긋나므로 채택하지 않는다.

## 6. `ItemData` / 에셋 YAML 편집

### C# (`Assets/Item/Script/ItemData.cs`, guid `501b19c5008706d0a3f2bf69aacf52f6`)
현재 직렬화 필드는 `id`, `type`, `effectType`, `amount`뿐. 추가만 한다(D-07):
```csharp
[SerializeField] private Sprite icon;
[SerializeField] private string displayName;
public Sprite Icon => icon;
public string DisplayName => displayName;
public ItemType Type => type;           // enum ItemData.ItemType { Consumable, Progression } (중첩 public)
```
- 기존 주석 "D-06 minimal schema ... no name/image/blurb" 은 이번 phase에서 해제된 결정이므로 해당 주석을 갱신하는 것이 정확하나, CLAUDE.md 정밀 변경 원칙상 **필드 선언 근처 주석 한 줄만** 갱신(계획에서 명시).
- 필드를 `type` 다음 또는 `amount` 뒤 중 어디에 선언해도 직렬화는 이름 기반이므로 기존 에셋 데이터 보존됨.
- 폴백: `string.IsNullOrEmpty(displayName) ? Id : displayName` 정도를 UI 쪽에서 처리(필드 비어 있으면 id 노출, 에셋에 값을 채우므로 실사용은 아님).

### 에셋 YAML (`Assets/Resources/Items/*.asset`) - 씬/프리팹이 아닌 에셋 YAML 편집
| 에셋 | guid (`.meta`) | `m_Name` | id | type | 현재 필드 |
|------|---------------|----------|-----|------|-----------|
| `HealthPotion.asset` | `1ca8a1cec6cae3595e071afccaf8623e` | HealthPotion | `health_potion_01` | 0 (Consumable) | `effectType: 0`, `amount: 20` |
| `AncientKey.asset` | `65cc29b1f44b856171d66dbe8ddec240` | AncientKey | `ancient_key_01` | 1 (Progression) | `effectType: 0`, `amount: 0` |

**주의: `docs`/`Check.md`의 `Assets/Item/...asset` 경로는 구버전.** 실제 에셋은 `Assets/Resources/Items/`에 있다(`SaveLoadManager`가 `Resources.LoadAll<ItemData>("Items")`로 로드, `:372,645`).

편집 형태(필드 순서는 클래스 선언 순서를 따름; Unity가 재직렬화하며 정렬하므로 내용만 맞으면 됨):
```yaml
  id: health_potion_01
  type: 0
  effectType: 0
  amount: 20
  icon: {fileID: -8637019419371064383, guid: e0ecab6c09cdfbc49b5ff003148564e6, type: 3}
  displayName: 체력 물약
```
- 스프라이트 참조 형식은 `{fileID: <서브스프라이트 internalID>, guid: <텍스처 guid>, type: 3}`. 멀티 스프라이트(spriteMode 2) 텍스처는 **서브 스프라이트의 internalID를 fileID로** 써야 한다(`21300000` 같은 기본값이 아님).
- **Placeholder 후보(`Assets/Sprite/`):**
  | 파일 | 텍스처 guid | 서브스프라이트 | 이름 | 용도 후보 |
  |------|-------------|----------------|------|-----------|
  | `Water.png` | `e0ecab6c09cdfbc49b5ff003148564e6` | internalID `-8637019419371064383` | `white_filled_water_bottle_0` | **HealthPotion 아이콘(병 모양, 의미상 일치)** |
  | `Empty.png` | `c7a1c6f866885424f906b7d57db097d9` | (미조회) | 빈 병 | 대체 후보 |
  | `WaterCorrupted.png`, `EmptyCorrupted.png` | `b1d743fad9a4a314d89e358b9d285c6b`, `1fba4c9e18131f84a86e632e0e7759d5` | (미조회) | 오염 병 | 부적합 |
  | `Assets/ImportedAsset/Porforever/Assets/Sprites/small Potions.png` | `ddf8838d0a06d8d4c9dec9e3bed69260` | internalID `21300000`~`...16`(`small Potions_0..8`) | 포션 시트 | 대체 후보(어느 칸이 어떤 포션인지 시각 확인 필요) |
- **열쇠 아이콘은 레포에 없음**(`*key*` png 검색 결과 0). `AncientKey`는 `icon` 비워 두고 `displayName: 고대의 열쇠`만 채워 **텍스트 폴백 경로를 실사용으로 검증**하는 것을 권장(D-06 허용). 이러면 아이콘 있는 슬롯/없는 슬롯 둘 다 Play에서 확인 가능.
- 에셋이 비-Unity 편집(수작업 YAML)이 가능한 근거: Phase 17에서 이미 손으로 작성한 에셋이 Unity 배치 임포트에서 재직렬화 없이 통과(`Assets/Item/Check.md`). 단 위 sprite 참조의 `type: 3`과 서브 스프라이트 fileID 정확성은 편집 후 Unity 임포트로 확인 필요. 대안은 Unity CLI 에디터 스크립트로 `AssetDatabase.LoadAssetAtPath<Sprite>` + `SerializedObject` 설정(안전).
- 사용자 입장 UI 문자열: 기본 폰트가 아니라 폴백(NotoSansKR)으로 한글이 그려지므로 `displayName`에 한글 허용.

## 7. 파일 분류 (C# vs 씬/프리팹/에셋 YAML)

| 변경 | 파일 | 종류 |
|------|------|------|
| Inventory 액션/바인딩 추가 | `Assets/InputSystem_Actions.inputactions` | **JSON 에셋** (수동/Edit) |
| `OnInventoryEvent`, 8개 게임플레이 가드 | `Assets/Player/Script/InputHandler.cs` | C# |
| `OnChanged` 이벤트(+Progression 가드 선택) | `Assets/Player/Script/Inventory.cs` | C# |
| 획득 성공/실패 정적 이벤트 | `Assets/Item/Script/WorldItem.cs` | C# |
| `Icon`/`DisplayName`/`Type` | `Assets/Item/Script/ItemData.cs` | C# |
| 패널 + 슬롯 뷰 + 토스트(+ Inventory.Awake AddComponent 1줄) | `Assets/Player/Script/UI/InventoryUI.cs` (+ 최대 1~2 파일) | C# (신규, `.meta`는 Unity가 생성) |
| 표시 필드 채움 | `Assets/Resources/Items/HealthPotion.asset`, `AncientKey.asset` | **에셋 YAML** |
| 리바인딩 행 UI + 엔트리 | `Assets/Scenes/1 stage.unity`, `Assets/Scenes/Tutorial Map.unity` | **씬 YAML** (Unity 에디터 스크립트 권장) |
| 프리팹 | `Player.prefab`, `PersistentManagers.prefab` | **변경 없음** |
| SSOT 문서 갱신(Phase 완료 시) | `docs/ssot/systems/item-and-inventory.md`, 필요 시 `player-runtime.md`/`save-and-settings.md` | 문서 |

## Standard Stack

신규 의존성 없음. 모두 기존 패키지.

| 패키지 | 버전 | 용도 |
|--------|------|------|
| `com.unity.inputsystem` | 1.18.0 (`Packages/manifest.json`) | 신규 액션, UI 모듈 |
| `com.unity.ugui` (TMP 포함) | 2.0.0 | `Canvas`, `GridLayoutGroup`, `Image`, `TextMeshProUGUI`, `IPointerClickHandler` |
| Unity Editor | 6000.3.10f1 (`ProjectVersion.txt`) | 씬 편집 스크립트 실행 |

버전 검증은 `npm`이 아니라 `Packages/manifest.json`과 `ProjectVersion.txt`로 확인 완료.

## Architecture Patterns

### 권장 구조
```
Assets/Player/Script/UI/
├── InGameHUDController.cs   (기존)
├── InventoryUI.cs           (신규: 패널 생성/토글/선택/사용 + 토스트. 슬롯 뷰는 중첩 클래스)
└── (필요 시) InventoryToast.cs  (토스트가 길어지면 분리. 기본은 한 파일)
```
과설계 금지: 파일 1~2개가 목표.

### Pattern 1: 열기/닫기 (`InventoryUI`)
```csharp
// Source: 이 레포 PauseMenu.cs / GameStateManager.cs / InGameHUDController.cs 패턴 조합
private void Start() {
    InputHandler.Instance.OnInventoryEvent += HandleToggle;   // Tab
    InputHandler.Instance.OnPauseEvent     += HandlePause;    // Esc -> 지연 닫기
    GameStateManager.Instance.OnGameStateChange += HandleStateChange;
}
private void HandleToggle() {
    var gsm = GameStateManager.Instance;
    if (isOpen) Close();
    else if (gsm.CurrentState == GameStateManager.GameState.Playing) Open();
}
private void HandlePause() { if (isOpen) pendingClose = true; }   // PauseMenu 순서 함정 회피
private void LateUpdate()  { if (pendingClose) { pendingClose = false; if (isOpen) Close(); } }
private void HandleStateChange(GameStateManager.GameState s) {
    if (isOpen && s != GameStateManager.GameState.Inventory) HidePanelOnly();
}
// OnDestroy: 세 구독 모두 해제 (InputHandler.Instance/GameStateManager.Instance null 체크).
```
- Open: `SetState(Inventory)` 후 `Refresh()`; Close: `SetState(Playing)`. 프로젝트는 `Instance?.`를 자주 쓴다.

### Pattern 2: 코드 생성 uGUI 5x4 그리드
`Canvas`+`CanvasScaler`+`GraphicRaycaster` -> 패널 `Image` -> 자식 `GridLayoutGroup`(`constraint = FixedColumnCount`, `constraintCount = 5`, 셀 약 96, spacing 8, `Inventory.SlotCount=20`개 슬롯 오브젝트를 `Start`에서 한 번 생성하고 `Refresh`에서 갱신만). 슬롯 구성: 배경 `Image` + 아이콘 `Image`(`preserveAspect`, sprite 없으면 비활성) + 이름 폴백 `TextMeshProUGUI` + 수량 `TextMeshProUGUI`. 선택 하이라이트는 선택 슬롯 배경 색 변경 또는 별도 테두리 `Image`. 하단에 선택 슬롯의 `displayName`/수량 라벨 1개(D-08).

### Pattern 3: 슬롯 클릭/사용 (`IPointerClickHandler`)
```csharp
// Source: UnityEngine.EventSystems (ugui 2.0.0)
public void OnPointerClick(PointerEventData e) {
    if (e.button == PointerEventData.InputButton.Left) owner.Select(index);
    else if (e.button == PointerEventData.InputButton.Right) owner.UseSelected(index);
    // 또는 e.clickCount >= 2 도 사용 제스처로 허용
}
```
- `Button`은 좌클릭만 처리하므로 우클릭에는 `IPointerClickHandler` 직접 구현이 필요.
- 사용 로직: `slot = inventory.GetSlot(i); if (slot == null || slot.IsEmpty) return; if (slot.item.Type != ItemData.ItemType.Consumable) { /* 무시 */ return; } inventory.UseItem(i);` -> `OnChanged`가 `Refresh` 유발.
- **D-09 가드 위치 권장: UI 쪽.** `Inventory.UseItem`은 Phase 18 UAT(7/7 PASS) 계약이며 `ContextMenu` 검증 훅이 Progression 아이템에도 쓸 수 있어야 할 수 있다. UI에서 `Type` 체크 후 호출하면 Inventory 수정 최소(= `OnChanged`만). 향후 퀵슬롯 등 다른 호출자가 생기면 그때 `Inventory` 쪽으로 이동.
- `UseItem` 후 해당 슬롯이 비면 선택 상태 유지/해제는 재량(빈 슬롯 선택은 무반응으로 둔다).

### Pattern 4: 토스트 + `WorldItem` 정적 이벤트 (D-13/D-14)
```csharp
// WorldItem.cs - Interact 내부 TryAddItem 호출부 (성공 시에만 Destroy 유지)
public static event System.Action<ItemData, int, bool> PickupResolved; // item, count, success
...
bool ok = inventory.TryAddItem(item, count);
PickupResolved?.Invoke(item, count, ok);
if (ok) Destroy(gameObject);
```
- 이벤트는 **Destroy 전에** 발행(`item`/`count`는 필드 값이라 안전). 이벤트 발행이 `Destroy` 흐름을 바꾸지 않음.
- 토스트(`InventoryUI` 또는 별도 컴포넌트)가 `WorldItem.PickupResolved`를 구독(`OnEnable`/`OnDisable` 또는 `Awake`/`OnDestroy`).
- 문자열: 성공 `"{displayName} x{count} 획득"`, 실패 `"인벤토리가 가득 찼습니다"`.
- 타이머: `Update`에서 `Time.unscaledDeltaTime` 누적 (인벤토리 열림 timeScale 0 중에도 진행). 연속 획득은 **교체**(마지막 메시지로 덮어쓰고 타이머 리셋)가 가장 단순하고 화면을 가리지 않음. 큐는 불필요.
- 위치: 화면 상단/하단 중앙 소형 텍스트. `raycastTarget=false`.
- 타이머 대신 코루틴을 쓸 경우 `WaitForSecondsRealtime` 사용(이 레포 `PlayerAttack.HitStop`이 이미 사용).
- 경쟁 조건: 가드로 Inventory 열림 중에는 `Interact` 자체가 막히므로 열림 중 획득 토스트가 뜨지 않는다.

### Anti-Patterns
- **구독자 5개 파일에 각각 상태 체크 추가:** 누락/중복 위험. 중앙 가드 사용.
- **`InventoryUI`에서 Esc를 동기 처리하고 `SetState(Playing)`:** `PauseMenu`가 뒤에 호출되면 일시정지 메뉴가 동시에 열림 -> `LateUpdate` 지연 닫기.
- **`Time.deltaTime`/`WaitForSeconds`로 토스트 타이머:** timeScale 0 에서 멈춤.
- **`GraphicRaycaster` 누락:** 클릭 불가.
- **`Inventory.OnChanged`를 `Refresh()` 즉시 호출로 연결:** 세이브 복원 중 N회 재그림. dirty 플래그 + 열림 상태에서만 재그림.
- **`WorldItem.Interact`에서 UI 직접 호출 / 파괴 순서 변경:** D-13/Phase 18 D-06 위반.

## Don't Hand-Roll

| 문제 | 직접 만들지 말 것 | 사용 | 이유 |
|------|------------------|------|------|
| 5x4 격자 배치 | 좌표 수동 계산 | `GridLayoutGroup` (FixedColumnCount=5) | D-10에서 명시, 해상도 독립 |
| 키 이름 표시 | 문자열 매핑 | `InputAction.GetBindingDisplayString` (`ControlsSettingsPanel`/`PlayerInteractionPrompt`가 이미 사용) | 리바인딩 반영 |
| 리바인딩 UI | 새 리바인드 로직 | 기존 `ControlsSettingsPanel` + `RebindEntry` | D-03 |
| 바인딩 저장 | 별도 저장 | `InputHandler.SaveBindingOverrides` | D-03 |
| 상태 전환/timeScale | `Time.timeScale` 직접 조작 | `GameStateManager.SetState` | D-04 |
| 우클릭 감지 | `Mouse.current` 폴링 | `IPointerClickHandler` + `PointerEventData.button` | 입력 모듈 경유, 새 입력 시스템 호환 |
| 한글 폰트 | 폰트 에셋 직접 지정 | TMP 기본 + 전역 폴백(NotoSansKR) | 이미 구성됨 |

**Key insight:** 이 phase의 어려움은 UI 자체가 아니라 (a) 전역 입력이 UI 상태를 모른다는 점, (b) 두 개의 Esc 구독자 순서, (c) 직렬화 자산(JSON/씬 YAML) 편집의 정확성이다.

## Common Pitfalls

### Pitfall 1: 슬롯 좌클릭 = 공격 발화
**What goes wrong:** `BasicAttack`이 `<Mouse>/leftButton`이라 슬롯 선택 클릭마다 `attackBox`가 생성되고 `canAttack`이 timeScale 0 에서 고착.
**Why:** `InputHandler`는 UI 포인터 소모를 모른다. 구독자에 가드 없음.
**Avoid:** 섹션 1의 중앙 가드. **Warning signs:** Play 중 패널에서 클릭 후 닫으면 공격 박스가 남아 있거나 공격이 안 나감.

### Pitfall 2: Esc로 닫으면 일시정지 메뉴가 같이 열림
**Avoid:** `LateUpdate` 지연 닫기. **Warning signs:** Esc 한 번에 인벤토리 닫힘 + Pause 패널 열림(타이밍/순서 의존이라 간헐적).

### Pitfall 3: `.inputactions` JSON 손상
**What goes wrong:** 쉼표/중괄호 오류 한 개로 에셋 임포트 실패 -> `InputHandler`가 "Player action map could not be found"로 전 입력 사망.
**Avoid:** 삽입 후 JSON 파서로 검증, id 중복 확인, Unity 임포트 콘솔 에러 0 확인.

### Pitfall 4: 리바인딩 행이 씬에 없는데 엔트리만 추가
**What goes wrong:** `rebindButton`/`keyLabel`이 `{fileID: 0}`이면 `?.` 때문에 조용히 무시(라벨은 `?` 대신 null이면 `RefreshLabel`이 return) -> UI에 행이 없음. **Avoid:** 행 4개 오브젝트 + 엔트리 함께, 두 씬 모두.

### Pitfall 5: `Reset` 버튼/컨텐츠 높이 겹침
수동 절대 좌표라 새 행 추가 시 `Reset` 버튼(`y=-749`)과 겹친다. 컨텐츠 `sizeDelta.y`(784)도 늘려야 스크롤 영역에 포함.

### Pitfall 6: 토스트가 `Inventory` 복원 중 뜸
이번 설계(WorldItem 이벤트 전용)에선 발생 안 함. `Inventory.OnChanged`에 토스트를 연결하면 즉시 발생하므로 연결 금지.

### Pitfall 7: 열림 중 다른 상태 전환으로 패널 고착
씬 로드/사망 `Paused` 전환 시 패널이 켜진 채 남을 수 있음 -> `OnGameStateChange` 구독으로 숨김.

### Pitfall 8: 정적 이벤트 구독 누수
`WorldItem` 정적 이벤트를 해제 안 하면 씬 전환 후 파괴된 `InventoryUI`를 호출(MissingReference). `OnDestroy`에서 해제.

### Pitfall 9: `UseItem`의 `playerInteraction` null 가능성
`Inventory.Awake`가 `GetComponent<PlayerInteraction>()`을 캐시. `Player.prefab`에 `PlayerInteraction`이 직접 포함돼(guid 1건 확인) 현재는 안전. `InventoryUI.Awake`(= `Inventory.Awake` 중 AddComponent)는 이 캐시와 무관.

### Pitfall 10: AddComponent 시점의 `InputHandler.Instance`
`Inventory.Awake` 시점에는 `InputHandler.Instance`가 이미 존재(`PersistentManagers`가 `BeforeSceneLoad`에서 생성). 그래도 구독은 `Start`에서 하고 null 체크(기존 `PauseMenu`/`PlayerController` 규약).

## Code Examples

### InputHandler 추가 (D-02) - 기존 패턴 복제
```csharp
// Source: Assets/Player/Script/InputHandler.cs 기존 구조
public event Action OnInventoryEvent;
private InputAction inventoryAction;
// Awake: inventoryAction = playerMap.FindAction("Inventory", false);
// BindCallbacks:   if (inventoryAction != null) inventoryAction.performed += HandleInventoryPerformed;
// UnbindCallbacks: if (inventoryAction != null) inventoryAction.performed -= HandleInventoryPerformed;
private void HandleInventoryPerformed(InputAction.CallbackContext c) => OnInventoryEvent?.Invoke();
```
`OnInventoryEvent`는 가드 대상이 아니다.

### Inventory 변경 이벤트 (D-11)
```csharp
// Source: Assets/Player/Script/Inventory.cs
public event System.Action OnChanged;
// TryAddItem: 성공 return true 직전 OnChanged?.Invoke();
// RemoveItem/UseItem: 성공 시 return true 직전 OnChanged?.Invoke();
```
`GetSlot`/`SlotCountTotal`/`TryAddItem` 시그니처 불변 -> Phase 19 세이브 계약 유지.

### Inventory가 UI를 부착
```csharp
// Inventory.Awake 끝: PlayerController.Awake의 AddComponent 패턴과 동일
if (GetComponent<InventoryUI>() == null) gameObject.AddComponent<InventoryUI>();
```

## State of the Art

| 항목 | 현재 | 영향 |
|------|------|------|
| Input System 1.18 | `InputAction.performed` 콜백 기반 이벤트 (프로젝트 규약) | 신규 액션도 동일 |
| TMP | uGUI 2.0.0 패키지에 통합(`TMPro` 네임스페이스) | 프로젝트도 `using TMPro` |
| 기존 `GameState.Inventory` | 정의만 있고 사용처 0 | 이번 phase가 첫 사용자 |

## Runtime State Inventory

이 phase는 rename/refactor/migration이 아니라 **기능 추가**이므로 해당 섹션 생략. 단, 관련 런타임 상태 메모:
- 저장 데이터: `SettingsData.InputBindingsJson`에 사용자 리바인딩 오버라이드 저장 - 신규 액션은 기본값(Tab) 사용, 마이그레이션 불필요.
- 세이브 파일: `Items`는 `itemId`(= `ItemData.Id`) 기반 -> `ItemData.Id` 불변(D-07)이므로 영향 없음.

## Environment Availability

| 의존성 | 필요처 | 사용 가능 | 버전 | 비고 |
|--------|--------|-----------|------|------|
| Unity Editor | 씬 YAML 편집 스크립트, Play 검증 | 예 | 6000.3.10f1 (Hub에 6000.3.9f1/6000.3.11f1/6000.0.32f1/6000.1.8f1/6000.2.7f2도 설치) | 프로젝트는 6000.3.10f1 |
| `unity` CLI | 에디터 구동/eval/console/capture | 예 | `C:/Users/chang/AppData/Local/Unity/bin/unity` | 사용자 메모리 규약 |
| Node.js | JSON 문법 검증 | 예 | - | 이 환경 `python`은 스텁이라 사용 불가 -> Node 사용 |
| 키 아이콘 스프라이트 | AncientKey 아이콘 | 아니오 | - | 아이콘 비워 displayName 폴백(D-06 허용) |

**Blocking 누락 없음.**

## Validation Architecture

`.planning/config.json`의 `workflow.nyquist_validation: false` -> 이 섹션 생략. 단 CONTEXT에 따라 Play 검증은 실제 키 입력으로 수행(검증 훅 `[ContextMenu]` 불가): `unity` CLI의 `capture_game_view`로 오버레이 UI 캡처 가능(사용자 메모리).

권장 Play 검증 체크리스트(계획의 verification 근거):
1. Tab: 패널 열림 + HUD 숨김 + `Time.timeScale == 0`; Tab/Esc: 닫힘 + 일시정지 메뉴 안 열림 + `timeScale == 1`.
2. 열림 중 LMB 클릭/F/Space/X/1/E/R/Q: 공격/점프/대시/치유/상호작용 무반응, 닫은 뒤 정상.
3. 설정 > 컨트롤 탭에 Inventory 행 표시(라벨 `Tab`), 리바인딩 후 새 키로 토글, 저장/재시작 후 유지.
4. HealthPotion 슬롯 선택(하이라이트+이름/수량) -> 우클릭: 수량 -1, 체력 회복, 0이면 슬롯 비움. AncientKey 우클릭: 수량 불변.
5. 월드 아이템 F: "{이름} x{n} 획득" 토스트 + 오브젝트 소멸. 가득 찬 상태: "인벤토리가 가득 찼습니다" 토스트 + 오브젝트 유지.
6. Phase 19 저장/불러오기: 복원 시 토스트 0회, 아이템 복원 정상.
7. 콘솔 에러 0건(특히 `.inputactions` 임포트, MissingReference).

## Open Questions

1. **가드를 `Paused`까지 넓힐 것인가?**
   - 알고 있는 것: 현재 Paused에서도 `BasicAttack`/`Interact`/`Jump` 등이 누수(메뉴 버튼 LMB 클릭 -> 공격).
   - 불명확: 사용자가 이번 phase에서 함께 고치길 원하는지.
   - 권장: 이번 phase는 `Inventory` 상태만 가드(정밀 변경), 계획서에 "Paused 누수 별도 이슈"로 명시. 조건식 한 줄(`!= Playing`)로 넓히기 쉬움.
2. **퍼즐 UI(`SlidingPuzzleTrigger.puzzleUI`)가 켜진 상태에서 Tab으로 인벤토리를 여는 것을 허용할지.**
   - 알고 있는 것: 퍼즐은 `GameState`를 안 바꾼다(`Puzzle` 상태 미사용), D-05 문구상 `Playing`이면 열림.
   - 권장: 이번 phase는 허용(스코프 최소). 이상하면 후속.
3. **`ControlsSettingsPanel` 행 추가를 에디터 스크립트로 할지 수작업 YAML로 할지.**
   - 권장: Unity CLI 에디터 스크립트(fileID 정합성 보장), 코드 실행은 codex 위임 규약과 정합. 계획에서 Task를 "씬 YAML 편집" 유형으로 분리.
4. **`BasicAttack bindingIndex: 1` 범위 밖 인덱스** (기존 결함): 이번 phase에서 수정하지 않고 기록만. 사용자 확인 필요 시 후속 이슈.
5. **빌드 미포함 씬(`2 stage`, `3 stage main`)의 `ControlsSettingsPanel`/EventSystem**: 이번 조사에서 `ControlsSettingsPanel` 참조가 이들 씬에는 없음(`1 stage`, `Tutorial Map`, `_Recovery/0`만 확인). EventSystem 존재는 미확인(LOW) - 빌드 미포함이라 영향 낮음.

## Sources

### Primary (HIGH confidence) - 이 레포 직접 분석
- `Assets/Player/Script/InputHandler.cs`, `GameStateManager.cs`, `Inventory.cs`, `PlayerController.cs`, `PlayerInputHandler.cs`, `PlayerAttack.cs`, `PlayerAttackBase.cs`, `PlayerInteraction.cs`, `PlayerInteractionPrompt.cs`, `Menu/PauseMenu.cs`, `Menu/ControlsSettingsPanel.cs`, `UI/InGameHUDController.cs`
- `Assets/Item/Script/ItemData.cs`, `WorldItem.cs`; `Assets/SaveSystem/Script/SaveLoadManager.cs`(복원 흐름), `AutoSaveTimer.cs`
- `Assets/map/script/3 stage/SlidingPuzzleTrigger.cs`
- `Assets/InputSystem_Actions.inputactions` (JSON 구조/키/ID 전수 파싱), `.meta`
- `Assets/Scenes/1 stage.unity`, `Tutorial Map.unity` (ControlsSettingsPanel, 행 구조, Canvas/EventSystem), `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings.asset (activeInputHandler: 2)`
- `Assets/Resources/Items/*.asset(.meta)`, `Assets/Sprite/*.png.meta`, TMP Settings / NotoSansKR SDF 에셋 (글리프 포함 여부 스크립트 확인)
- `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`

### Secondary (MEDIUM)
- Input System `LoadBindingOverridesFromJson`이 미지의 항목을 무시한다는 점은 표준 동작에 대한 기억 기반(이 레포에서 직접 검증하지 않음). 계획 단계에서 Play 중 기존 오버라이드가 있는 상태로 1회 확인 권장.

### Tertiary (LOW)
- 빌드 미포함 씬의 EventSystem 존재 여부(미확인).

## Metadata

**Confidence breakdown:**
- 입력 구독자/가드 설계: HIGH - 모든 구독자 코드를 직접 읽음
- 세이브 복원 상호작용: HIGH - 호출 경로 직접 확인
- 수명주기/캔버스/폰트 규약: HIGH - 씬 YAML/TMP 에셋 직접 확인
- `.inputactions` 편집: HIGH - JSON 구조 전수 파싱
- 씬 YAML 행 추가 절차: MEDIUM - 구조는 확인, 실제 좌표/최종 fileID는 편집 시 결정
- 스프라이트 참조 형식/placeholder: MEDIUM - `Water.png` internalID 확인, 임포트 결과는 편집 후 검증 필요

**Research date:** 2026-11-05
**Valid until:** 이 레포 코드가 바뀌기 전까지(외부 의존성 변동 없음, 30일)
