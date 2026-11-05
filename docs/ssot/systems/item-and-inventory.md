# 아이템 및 인벤토리

## 책임

`ItemData`(ScriptableObject)가 아이템 정적 데이터를, `Inventory`(Player 컴포넌트)가 슬롯/스택 자료구조와 추가/제거/사용 API를, `WorldItem`이 월드 픽업 상호작용을 각각 소유한다. 인벤토리 UI와 키 바인딩은 아직 존재하지 않는다 — 이 시스템은 데이터/로직 레이어까지만 구현됐다.

## 런타임 구조

| 컴포넌트 | 위치 | 내용 |
|---|---|---|
| `ItemData : ScriptableObject, IItem` | `Assets/Item/Script/ItemData.cs`, 에셋은 `Assets/Resources/Items/*.asset` | `id`(string), `type`(Consumable\|Progression), `effectType`(Heal), `amount`(float) — 표시용 필드(아이콘/이름/설명) 없음 |
| `Inventory : MonoBehaviour` | `Assets/Player/Script/Inventory.cs`, Player.prefab에 부착 | 고정 `List<InventorySlot>`(`SlotCount=20`, `MaxStack=99`), 각 슬롯은 `ItemData` 참조 + `count`(int) |
| `WorldItem : MonoBehaviour, IPlayerInteractable` | `Assets/Item/Script/WorldItem.cs` | 월드에 배치된 픽업 오브젝트. `Checkpoint.cs`와 동일한 `IPlayerInteractable` 패턴 |

`Resources.LoadAll<ItemData>("Items")`로 런타임에 모든 `ItemData` 에셋을 탐색할 수 있다(`SaveLoadManager.ResolveItemData`가 이 방식으로 캐시 조회).

## 동작과 실패 경로

- `Inventory.TryAddItem(item, amount)`는 원자적이다 — 먼저 `AvailableCapacityFor`로 전체 용량을 확인하고 부족하면 아무 슬롯도 건드리지 않고 false를 반환한다. 성공 시 기존 부분 스택을 먼저 채우고(Pass 1) 남으면 빈 슬롯을 연다(Pass 2).
- `WorldItem.Interact`는 `Inventory.TryAddItem`이 성공한 경우에만 `Destroy(gameObject)`한다 — 인벤토리가 가득 차면 월드 아이템은 파괴되지 않고 그대로 남는다(D-06, 데이터 유실 없음 원칙).
- `Inventory.UseItem(slotIndex)`는 `ItemData.UseEffect(PlayerInteraction)`를 호출한 뒤 count를 1 감소시키고, 0이 되면 슬롯을 완전히 비운다(`Clear()`) — 리스트에서 엔트리를 제거해 뒤 슬롯을 당기지 않으므로 슬롯 배열 길이는 항상 고정 20이다.
- `ItemData.UseEffect`는 Consumable/Heal일 때만 `player.GetComponent<PlayerStats>().Heal(amount)`를 널 가드 없이 호출하고, Progression 타입은 의도적 no-op이다.
- Player Awake 시점에 `Inventory`는 20개 슬롯을 전부 새로 생성해 초기화한다 — 이 컴포넌트 자체는 세이브/로드나 씬 전환 지속성을 신경 쓰지 않는다(그 역할은 `SaveLoadManager`가 별도로 수행, [save-and-settings.md](./save-and-settings.md) 참고).

## 제약

- **인벤토리 UI는 존재하지 않는다.** 아이템 사용/조회는 `#if UNITY_EDITOR`로 감싼 `[ContextMenu]` 훅(`Phase18: Use Slot 0` 등)으로만 Play 모드에서 가능하다 — 실제 플레이어가 사용할 UI/키 입력은 명시적으로 범위 밖("추후 UI phase")이며 phase 번호도 아직 배정되지 않았다.
- 아이콘/이름/설명 등 표시용 메타데이터 필드가 `ItemData`에 전혀 없다 — UI를 붙이려면 이 스키마 확장이 선행돼야 한다.
- 내부 자료구조는 `List<InventorySlot>`이며 `Dictionary<itemId, count>` 방식이 아니다 — 슬롯 위치가 의미를 가지므로(향후 드래그앤드롭 대비) 구조 변경 시 이 계약을 깨지 않아야 한다.
- `Inventory`와 `ItemData`는 Phase 19(세이브 연동) 작업 동안 0줄 변경 — 세이브/로드는 순전히 `SaveLoadManager` 쪽에서 `TryAddItem`/`Id`를 재사용해 구현됐다.

## 근거

- `Assets/Item/Script/ItemData.cs`
- `Assets/Item/Script/IItem.cs`
- `Assets/Item/Script/WorldItem.cs`
- `Assets/Player/Script/Inventory.cs`
- `Assets/Player/Script/PlayerInteraction.cs` (`FindNearest`/`TryInteract`, `WorldItem`이 그대로 재사용)
- `Assets/Item/Check.md` (Phase 17 정적/Play 모드 검증), `.planning/phases/18-inventory-system-depends-on-phase-17/18-UAT.md` (Phase 18 UAT 7/7 PASS)

## 검증

`partial` — 커밋 `ec03fd9510a7c5d59d3a8e4397896046c768245a`(2026-09-21)와 2026-09-24 작업 트리를 확인했다. Phase 17(아이템 코어)·Phase 18(인벤토리 로직+월드 픽업)은 Unity CLI로 구동한 Play 모드에서 각각 실측 완료(Phase 17 Play 모드 체크리스트, Phase 18 UAT 7/7 PASS). UI가 없어 실제 플레이어 입력 경로(클릭/키 입력)를 통한 사용은 검증 대상 자체가 아직 없다.
