# Phase 21: 인벤토리 UI + 키 바인딩 + 월드 아이템 획득 피드백 - Context

**Gathered:** 2026-11-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 18/19에서 만든 `Inventory`(고정 20슬롯, 스택 99)를 플레이어가 실제로 보고 다룰 수 있게 하는
UI 레이어까지가 이번 phase다.

1. 인벤토리 패널: 5x4 슬롯 그리드 표시(아이콘+수량), 열기/닫기 토글.
2. 새 입력 액션 `Inventory`(기본 Tab) 추가 및 리바인딩 패널 노출.
3. 슬롯 선택 + 아이템 사용(`Inventory.UseItem`) 동작.
4. 월드 아이템(`WorldItem`) 상호작용 획득 시 토스트, 인벤토리가 가득 차 실패했을 때 메시지.

**범위 밖**: 드래그앤드롭, 아이템 버리기/월드 드롭, 정렬/자동 정리, 툴팁/상세 설명창,
슬롯 인덱스 영속화, 퀵슬롯/핫바, 획득 사운드(오디오 phase 소관), 신규 아이템 종류 추가.

</domain>

<decisions>
## Implementation Decisions

### 입력/키 바인딩
- **D-01:** 새 InputAction `Inventory`를 `InputSystem_Actions.inputactions`의 `Player` 맵에 추가하고
  기본 바인딩은 `<Keyboard>/tab`. 이미 사용 중인 키(Space, X, LShift, Esc, LMB, F, E, R, 1, Q)와
  겹치지 않는다. 같은 키로 열기/닫기(토글)하며 Esc로도 닫을 수 있다.
- **D-02:** `InputHandler`에 `OnInventoryEvent`와 `inventoryAction` 필드를 기존 11개 액션과 동일한
  패턴(Find → BindCallbacks/UnbindCallbacks → Handle 메서드)으로 추가한다.
- **D-03:** `ControlsSettingsPanel`의 리바인딩 목록(Inspector 기반 `RebindEntry`)에 `Inventory`
  항목을 노출해 사용자가 키를 바꿀 수 있게 한다. 바인딩 오버라이드 저장은 기존
  `InputHandler.SaveBindingOverrides` 경로를 그대로 쓴다.

### 열림 상태 동작
- **D-04:** 인벤토리가 열리면 선언만 돼 있고 아무도 쓰지 않는 `GameState.Inventory`로 전환한다
  (`GameStateManager`가 이미 `Time.timeScale = 0` 처리). 닫으면 `Playing`으로 복귀.
  `InGameHUDController`는 `Playing`일 때만 HUD를 보이므로 자동으로 HUD가 숨겨진다.
- **D-05:** 인벤토리는 `Playing` 상태에서만 열 수 있다(Paused/Loading/Puzzle/GameClear 중에는 무시).
  `PauseMenu.OnPauseInput`은 `Playing`일 때만 열리므로, 인벤토리 열림 중의 Esc는 인벤토리 쪽에서
  `OnPauseEvent`를 구독해 닫는다(일시정지 메뉴가 같이 열리지 않게). `SaveSlotDialog.HandlePause`
  선처리 순서는 건드리지 않는다.

### 아이템 표시 스키마
- **D-06:** Phase 17 D-06(표시 메타데이터 보류)을 이번 phase에서 해제한다. `ItemData`에
  `Sprite icon`, `string displayName` 필드와 읽기 전용 접근자를 추가한다. 기존 `HealthPotion`,
  `AncientKey` 에셋에 값을 채운다(아이콘은 placeholder 스프라이트 허용). 아이콘이 비어 있으면
  `displayName` 텍스트로 폴백한다. 설명(blurb) 필드는 추가하지 않는다(툴팁이 범위 밖).
- **D-07:** `ItemData`의 기존 필드/`UseEffect` 계약과 `Id`는 변경하지 않는다(Phase 19 세이브가 `Id`에
  의존). 추가만 하고 수정은 하지 않는다.

### 슬롯 선택과 사용
- **D-08:** 슬롯 클릭으로 선택(선택 슬롯 하이라이트 + 선택 슬롯의 `displayName`/수량 표시),
  선택된 슬롯에 우클릭(또는 더블클릭)하면 `Inventory.UseItem(index)` 호출. 사용 후 UI는 즉시
  갱신한다(스택 0이면 슬롯 비움 - Phase 18 D-08 그대로). 빈 슬롯은 선택/사용해도 아무 일도 없다.
- **D-09:** Progression 타입은 `UseEffect`가 no-op이지만 `UseItem`이 count를 깎는다. 이번 phase의
  UI는 **Progression 아이템 사용을 막는다**(열쇠류 실수 소모 방지). `ItemData`에 `Type` 접근자가
  필요하며 D-06의 접근자 추가에 포함한다. 가드 위치(UI 쪽 vs `Inventory.UseItem`)는 계획 단계 재량.
  `Inventory`를 수정하더라도 Phase 19 세이브 계약(`TryAddItem`/`GetSlot`)을 깨지 않아야 한다.

### UI 구성 방식
- **D-10:** 인벤토리 패널과 토스트는 씬/프리팹 수작업 없이 **스크립트가 런타임에 uGUI를 생성**한다
  (`PlayerInteractionPrompt`가 캔버스를 코드로 만드는 것과 동일 계열). 5x4 `GridLayoutGroup`,
  TMP 텍스트 사용. 새 컴포넌트는 `Assets/Player/Script/UI/`에 둔다(기존 HUD 스크립트 위치).
- **D-11:** UI는 `Inventory`를 폴링하지 않고 열릴 때와 변경 시점(사용/획득)에 갱신한다. 이를 위해
  `Inventory`에 변경 이벤트(`event Action OnChanged`) 추가는 허용 - 단 `TryAddItem`/`RemoveItem`/
  `UseItem`의 반환 계약과 원자성은 유지한다.

### 월드 아이템 획득 피드백
- **D-12:** 획득 성공 시 화면 HUD 영역에 토스트("{displayName} x{count} 획득")를 잠깐 표시한다.
  인벤토리가 가득 차 `TryAddItem`이 실패하면 "인벤토리가 가득 찼습니다" 토스트를 표시한다
  (현재는 무반응 - Phase 18 D-06은 아이템 유지만 규정). 월드 플로팅 텍스트는 채택하지 않는다.
- **D-13:** 피드백 연결은 `WorldItem.Interact`가 UI를 직접 호출하지 않고 이벤트를 발행하는 방식을
  우선한다(예: `WorldItem`의 정적 이벤트 -> 토스트 컴포넌트 구독). 이유: Phase 18 D-06의
  "성공 시에만 Destroy" 로직을 그대로 두고 UI 결합도를 낮추기 위함. 정확한 이벤트 위치/시그니처는
  계획 단계 재량.
- **D-14:** 토스트는 `Time.timeScale == 0` 구간과 무관하게 동작해야 하므로 unscaled 시간으로
  표시 시간을 센다. 연속 획득은 큐/교체 중 하나로 처리(계획 단계 재량, 단 화면을 가리지 않을 것).

### Claude's Discretion
- 패널 크기/색상/슬롯 간격/폰트 크기 등 시각 디자인, 토스트 지속 시간(권장 약 2초), 위치.
- `ItemData` 신규 필드의 정확한 직렬화 이름, placeholder 아이콘 제작 방식.
- UI 컴포넌트의 파일 분할(패널/슬롯 뷰/토스트) 수준 - 과설계 금지, 최소 파일 수.
- 열림 중 이동/공격 입력 차단 방식(아래 Open Items 참고).
- 우클릭 vs 더블클릭 중 최종 사용 제스처 (단 D-08의 선택 개념은 유지).

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 선행 구현 (계약 - 필요한 최소 추가만)
- `Assets/Player/Script/Inventory.cs` - `SlotCount=20`/`MaxStack=99`, `GetSlot(int)`(범위 밖 null),
  `SlotCountTotal`, `TryAddItem`/`RemoveItem`/`UseItem`. `InventorySlot`은 `class`. Awake에서
  슬롯 전부 새로 생성.
- `Assets/Item/Script/ItemData.cs` - `Id`만 공개. 이번 phase에서 icon/displayName/Type 접근자 추가.
- `Assets/Item/Script/WorldItem.cs` - `Interact`가 `TryAddItem` 성공 시에만 `Destroy`. 피드백 훅 위치.
- `Assets/Player/Script/PlayerInteraction.cs`, `PlayerInteractionPrompt.cs` - 상호작용 파이프라인과
  코드 생성 캔버스 패턴 참조.
- `Assets/Player/Script/InputHandler.cs` - 액션 추가 패턴(11개 액션 동일 구조), 런타임 복제 액션 에셋,
  `LoadBindingOverrides`/`SaveBindingOverrides`.
- `Assets/InputSystem_Actions.inputactions` - `Player` 맵. 현재 바인딩: Space/X/LShift/Esc/LMB/F(Interact)/
  E(Skill_1)/R(Skill_2)/1(Heal)/Q(Action).
- `Assets/Player/Script/Menu/ControlsSettingsPanel.cs` - Inspector 기반 `RebindEntry`(`actionName`).
- `Assets/Player/Script/Menu/PauseMenu.cs` - `OnPauseEvent` 구독, `Playing`에서만 열림, `Close()`는
  `Playing` 복귀.
- `Assets/Player/Script/GameStateManager.cs` - `GameState.Inventory` 이미 정의(미사용), timeScale 처리,
  씬 로드 시 `Playing` 강제 복귀.
- `Assets/Player/Script/UI/InGameHUDController.cs` - `Playing`일 때만 HUD 표시.
- `Assets/Resources/Items/HealthPotion.asset`, `AncientKey.asset` - 표시 필드를 채울 대상 에셋.

### 선행 페이즈 문서
- `.planning/phases/18-inventory-system-depends-on-phase-17/18-CONTEXT.md` - D-05(월드 아이템은
  interact 키), D-06(가득 차면 월드 유지), D-08(슬롯 위치 고정).
- `.planning/phases/18-inventory-system-depends-on-phase-17/18-UAT.md` - 7/7 PASS 기준선.
- `.planning/phases/19-savedata-items-list-itemsaveentry-itemid-count-saveversion-saveloadmanager-depends-on-phase-18/19-CONTEXT.md`
  - D-02(슬롯 위치 미보존), `Id` 기반 복원 의존.
- `.planning/phases/17-iitem-itemdata-scriptableobject-id-useeffect-ui/17-CONTEXT.md` - D-06(이번에
  해제하는 보류 결정).
- `docs/ssot/systems/item-and-inventory.md` - 현재 시스템 SSOT. 이 phase 완료 시 갱신 대상.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `GameStateManager.GameState.Inventory` - 이미 있음, 코드에서 한 번도 안 쓰임. timeScale 0 처리 포함.
- `InGameHUDController` - 상태 이벤트로 HUD를 숨김. 인벤토리 열림에도 그대로 작동.
- `InputHandler` 이벤트 패턴, `PlayerInteractionPrompt`의 코드 생성 Canvas 패턴.

### Established Patterns
- `InputHandler.Instance`를 `Start`에서 구독, `OnDestroy`에서 해제 (PauseMenu 참고).
- 프로젝트는 대체로 null 가드를 생략하되 `Instance` 접근은 `?.`를 쓰는 곳이 많다.
- `[ContextMenu]` 검증 훅은 UI가 생기므로 이번 phase의 Play 검증은 실제 키 입력으로 수행한다.

### Integration Points
- `InputSystem_Actions.inputactions`(JSON) + `InputHandler.cs` + `ControlsSettingsPanel`(씬/프리팹의
  RebindEntry 목록, YAML 수정 필요 가능).
- `PersistentManagers.prefab`의 `InputHandler` 설정은 변경 없음(같은 에셋 참조).
- UI 컴포넌트 부착 위치: 게임 씬 UI Canvas 또는 Player/PersistentManagers - 계획 단계에서 확정
  (런타임 생성이므로 부착 대상 GameObject 하나만 필요).

### Open Items for Research/Plan
- `Time.timeScale = 0`만으로 이동/공격/상호작용 이벤트가 충분히 막히는지 확인 필요. `InputHandler`는
  이벤트 기반이라 timeScale 0에서도 `OnInteractEvent`/`OnBasicAttackEvent`가 발행되며, 구독자가
  `GameState`를 확인하지 않으면 열림 중 상호작용(월드 아이템 획득)·공격이 동작할 수 있다.
  구독자별 가드가 필요한지 조사한다.
- `Inventory` 이벤트 추가가 Phase 19 세이브 복원 흐름(다수 `TryAddItem` 연속 호출)에서 불필요한
  토스트를 발생시키지 않는지 확인한다 - 획득 토스트는 `WorldItem` 경로에서만 떠야 한다.
- 씬 로드 시 `Inventory`(Player에 부착)와 UI 컴포넌트의 생명주기 및 참조 획득 방식.

</code_context>

<specifics>
## Specific Ideas

- 기본 키는 Tab으로 확정(사용자 선택). 같은 키 토글 + Esc 닫기.
- 획득 토스트와 "가득 참" 토스트를 둘 다 넣는 것이 사용자 선택 - 현재 가득 참 실패가 무반응이라는
  점이 UX 개선 포인트.
- Progression 아이템(열쇠 등)은 UI에서 실수로 소모되지 않도록 사용을 막아야 한다는 의도.
- 시각 디자인은 최소한으로: 기능 검증이 우선, 폴리싱은 후속.

</specifics>

<deferred>
## Deferred Ideas

- 드래그앤드롭 슬롯 재배치, 아이템 버리기(월드 드롭), 정렬/자동 정리.
- 아이템 툴팁/상세 설명창(설명 필드).
- 퀵슬롯/핫바 및 Heal(1키)과 인벤토리 연동.
- 획득/사용 사운드 (오디오 중앙화 phase 20 이후 연동).
- 슬롯 인덱스 영속화 (Phase 19에서 이미 제외된 항목).
- 게임패드 내비게이션 지원.

### Reviewed Todos (not folded)
None - 매칭된 todo 확인 안 함.

</deferred>

---

*Phase: 21-inventory-ui-keybinding-pickup*
*Context gathered: 2026-11-05*
