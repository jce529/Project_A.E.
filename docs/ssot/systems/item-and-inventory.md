# 아이템 및 인벤토리

## 책임

`ItemData`(ScriptableObject)가 아이템 정적 데이터를, `Inventory`(Player 컴포넌트)가 슬롯/스택 자료구조와 추가/제거/사용 API를, `WorldItem`이 월드 픽업 상호작용을 각각 소유한다. `InventoryUI`(Player 런타임 부착)가 패널 표시, 슬롯 선택/사용, 획득 토스트를 소유한다.

## 런타임 구조

| 컴포넌트 | 위치 | 내용 |
|---|---|---|
| `ItemData : ScriptableObject, IItem` | `Assets/Item/Script/ItemData.cs`, 에셋은 `Assets/Resources/Items/*.asset` | `id`(string), `type`(Consumable\|Progression), `effectType`(Heal), `amount`(float), `icon`(Sprite), `displayName`(string) + 읽기 전용 `Icon`/`DisplayName`/`Type` 접근자. 설명 필드는 없음 |
| `Inventory : MonoBehaviour` | `Assets/Player/Script/Inventory.cs`, Player.prefab에 부착 | 고정 `List<InventorySlot>`(`SlotCount=20`, `MaxStack=99`), 각 슬롯은 `ItemData` 참조 + `count`(int). 성공한 추가/제거/사용 직후 `event Action OnChanged`를 1회 발행 |
| `WorldItem : MonoBehaviour, IPlayerInteractable` | `Assets/Item/Script/WorldItem.cs` | 월드에 배치된 픽업 오브젝트. `Checkpoint.cs`와 동일한 `IPlayerInteractable` 패턴. 정적 이벤트 `PickupResolved(ItemData, int, bool)`로 성공/실패를 발행 |
| `InventoryUI : MonoBehaviour` | `Assets/Player/Script/UI/InventoryUI.cs`, `Inventory.Awake`가 `AddComponent` | 런타임 uGUI(Overlay 캔버스 sortingOrder 50, 5x4 `GridLayoutGroup`). Tab 토글, Esc 지연 닫기, 좌클릭 선택/우클릭 사용, 픽업 토스트 |

`Resources.LoadAll<ItemData>("Items")`로 런타임에 모든 `ItemData` 에셋을 탐색할 수 있다(`SaveLoadManager.ResolveItemData`가 이 방식으로 캐시 조회).

## 동작과 실패 경로

- `Inventory.TryAddItem(item, amount)`는 원자적이다 — 먼저 `AvailableCapacityFor`로 전체 용량을 확인하고 부족하면 아무 슬롯도 건드리지 않고 false를 반환한다. 성공 시 기존 부분 스택을 먼저 채우고(Pass 1) 남으면 빈 슬롯을 연다(Pass 2).
- `WorldItem.Interact`는 `Inventory.TryAddItem`이 성공한 경우에만 `Destroy(gameObject)`한다 — 인벤토리가 가득 차면 월드 아이템은 파괴되지 않고 그대로 남는다(D-06, 데이터 유실 없음 원칙).
- `Inventory.UseItem(slotIndex)`는 `ItemData.UseEffect(PlayerInteraction)`를 호출한 뒤 count를 1 감소시키고, 0이 되면 슬롯을 완전히 비운다(`Clear()`) — 리스트에서 엔트리를 제거해 뒤 슬롯을 당기지 않으므로 슬롯 배열 길이는 항상 고정 20이다.
- `ItemData.UseEffect`는 Consumable/Heal일 때만 `player.GetComponent<PlayerStats>().Heal(amount)`를 널 가드 없이 호출하고, Progression 타입은 의도적 no-op이다.
- 인벤토리는 `Playing` 상태에서만 열린다. 열리면 `GameState.Inventory`(`Time.timeScale` 0, `InGameHUDController`가 HUD 숨김)로 전환하고 Tab 또는 Esc 로 닫으면 `Playing`으로 돌아온다. Esc 는 플래그만 세우고 `LateUpdate`에서 닫기 때문에 같은 키 입력에서 일시정지 메뉴가 함께 열리지 않는다.
- 슬롯을 우클릭하면 이미 선택된 슬롯일 때만 `Inventory.UseItem`을 호출한다. Progression 아이템은 UI 에서 사용을 막는다(`Inventory.UseItem` 자체는 Phase 18 계약대로 count 를 줄인다).
- 획득 토스트는 `WorldItem.PickupResolved`에서만 발생한다. `SaveLoadManager`의 복원은 `Inventory.TryAddItem`을 직접 호출하므로 토스트가 뜨지 않는다. 토스트는 `Time.unscaledDeltaTime`으로 약 2초 표시하고 연속 획득은 최신 메시지로 교체한다. 가득 찬 인벤토리에서 줍기를 시도하면 "인벤토리가 가득 찼습니다"를 표시하고 월드 오브젝트는 남는다.
- Player Awake 시점에 `Inventory`는 20개 슬롯을 전부 새로 생성해 초기화한다 — 이 컴포넌트 자체는 세이브/로드나 씬 전환 지속성을 신경 쓰지 않는다(그 역할은 `SaveLoadManager`가 별도로 수행, [save-and-settings.md](./save-and-settings.md) 참고).

## 제약

- 인벤토리 UI 는 Phase 21 범위로 슬롯 표시, 선택, 우클릭 사용, 획득 토스트까지만 구현했다. 드래그앤드롭, 버리기, 정렬, 툴팁/설명창, 퀵슬롯, 게임패드 내비게이션은 범위 밖이다. `[ContextMenu]` 검증 훅(`Phase18: Use Slot 0` 등)은 에디터 전용으로 남아 있다.
- 표시 메타데이터는 `icon`/`displayName`뿐이다(설명 필드 없음). `AncientKey`는 아이콘이 비어 있어 `displayName` 텍스트로 표시한다. `HealthPotion` 아이콘은 `Assets/Sprite/Water.png`의 서브 스프라이트(임시 placeholder)다.
- 내부 자료구조는 `List<InventorySlot>`이며 `Dictionary<itemId, count>` 방식이 아니다 — 슬롯 위치가 의미를 가지므로(향후 드래그앤드롭 대비) 구조 변경 시 이 계약을 깨지 않아야 한다.
- `Inventory`와 `ItemData`는 Phase 19(세이브 연동) 작업 동안 0줄 변경 — 세이브/로드는 순전히 `SaveLoadManager` 쪽에서 `TryAddItem`/`Id`를 재사용해 구현됐다. Phase 21 에서도 `Inventory`는 `OnChanged`와 `AddComponent` 1줄만 추가했고 반환 계약과 원자성은 그대로다. `ItemData`는 필드/접근자 추가만 했고 `Id`와 `UseEffect`는 바뀌지 않았다.
- 컨트롤 탭의 Inventory 리바인딩 행은 `ControlsSettingsPanel.entries`에서 `BasicAttack` 엔트리(`bindingIndex 1`이 범위 밖인 기존 결함, BUG-011) 앞에 둔다. 뒤에 두면 라벨이 갱신되지 않는다.

## 근거

- `Assets/Item/Script/ItemData.cs`
- `Assets/Item/Script/IItem.cs`
- `Assets/Item/Script/WorldItem.cs`
- `Assets/Player/Script/Inventory.cs`
- `Assets/Player/Script/UI/InventoryUI.cs`
- `Assets/Player/Script/InputHandler.cs` (`OnInventoryEvent`, 게임플레이 입력 억제)
- `Assets/InputSystem_Actions.inputactions` (`Inventory` 액션, Tab)
- `Assets/Player/Script/Menu/ControlsSettingsPanel.cs` (씬 `1 stage`, `Tutorial Map`의 Inventory 행)
- `Assets/Player/Script/PlayerInteraction.cs` (`FindNearest`/`TryInteract`, `WorldItem`이 그대로 재사용)
- `Assets/Item/Check.md` (Phase 17 정적/Play 모드 검증), `.planning/phases/18-inventory-system-depends-on-phase-17/18-UAT.md` (Phase 18 UAT 7/7 PASS)

## 검증

`partial` — 커밋 `ec03fd9510a7c5d59d3a8e4397896046c768245a`(2026-09-21)와 2026-09-24 작업 트리를 확인했다. Phase 17(아이템 코어)·Phase 18(인벤토리 로직+월드 픽업)은 Unity CLI로 구동한 Play 모드에서 각각 실측 완료(Phase 17 Play 모드 체크리스트, Phase 18 UAT 7/7 PASS). Phase 21(인벤토리 UI)은 메인 에디터 Play 에서 가상 입력 장치로 검증했다: PASS 11 / FAIL 1 / 미확인 2 (Assets/Item/Check.md, 디스크 저장 왕복과 Phase 18 UAT 나머지 4항목은 미확인, 실제 키보드 입력 체감은 미확인).
