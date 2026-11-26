# Phase 18: 인벤토리 시스템 - Context

**Gathered:** 2026-09-21
**Status:** Ready for planning

<domain>
## Phase Boundary

고정 슬롯+스택 자료구조를 가진 인벤토리, 추가/제거/사용 API, `PlayerInteraction` 연동
월드 아이템 획득까지가 이번 phase다. `ItemData`(Phase 17 완료)를 참조하는 슬롯 배열을 Player에
붙이고, 월드에 놓인 아이템 오브젝트를 상호작용으로 주울 수 있게 하며, 인벤토리에 담긴 아이템을
사용(`UseEffect` 호출)할 수 있어야 한다.

**범위 밖**: 세이브/로드 연동(Phase 19), 인벤토리 UI(추후 UI phase), 아이템 아이콘/이름 등 표시용
메타데이터(Phase 17에서 이미 범위 밖으로 확정).

</domain>

<decisions>
## Implementation Decisions

### 슬롯/스택 구조
- **D-01:** 슬롯 개수와 슬롯당 최대 스택 수는 코드에 하드코딩된 고정 상수로 정의한다
  (Inspector 노출 없음). 정확한 수치(예: 20슬롯/99스택)는 연구/계획 단계에서 확정.
- **D-02:** 내부 자료구조는 `List<InventorySlot>`(슬롯 배열) — 각 슬롯이 `ItemData` 참조 +
  `count`(int)를 담는다. `Dictionary<itemId, count>` 방식은 채택하지 않는다 — 슬롯 위치/순서가
  실제로 존재해야 하며, 향후 UI(드래그앤드롭 등)에서 슬롯 위치가 의미를 가질 수 있기 때문.

### 인벤토리 소유/배치
- **D-03:** 인벤토리는 Player GameObject의 새 컴포넌트(`Inventory : MonoBehaviour`)로 만든다 —
  `PlayerStats`/`PlayerInteraction`과 동일한 배치 패턴. `PlayerStats`에 필드로 통합하지 않는다.
- **D-04:** 이번 phase는 메모리 상태만 다룬다. 세이브/로드 연동(영속성)은 Phase 19 범위 —
  이번 phase는 `DontDestroyOnLoad` 여부를 신경쓰지 않고 기존 Player 생명주기를 그대로 따른다.

### 월드 아이템 획득 연동
- **D-05:** 월드 아이템 오브젝트는 `Checkpoint.cs`와 동일한 `IPlayerInteractable` 패턴을 따른다
  (`WorldItem : MonoBehaviour, IPlayerInteractable`) — interact 키로 줍는다. 트리거 접촉 시
  자동 획득 방식은 채택하지 않는다. 기존 `PlayerInteraction`의 최근접 타겟팅/프롬프트 UI를
  그대로 재사용한다.
- **D-06:** 인벤토리가 가득 찬 상태에서 월드 아이템을 획득 시도하면 획득 실패 — 아이템은
  월드에 그대로 남고 파괴되지 않는다. 데이터 유실이 없는 것이 기본 원칙.

### 추가/제거/사용 API 동작
- **D-07:** 사용(`UseEffect` 호출) 트리거는 Phase 17과 동일한 `[ContextMenu]` 검증 훅 패턴을
  따른다 — `Inventory`에 `[ContextMenu]` 메서드를 두어 Play 모드에서 Inspector 컨텍스트 메뉴로
  특정 슬롯 사용을 호출한다. UI/키바인딩 배선은 이번 phase 범위 밖(추후 UI phase).
- **D-08:** 사용으로 스택이 0이 되면 해당 슬롯을 완전히 비운다(참조/카운트 초기화) — 리스트에서
  엔트리를 제거해 뒤 슬롯을 당기지 않는다. 슬롯 배열 길이는 항상 고정(D-01)이며, 슬롯 위치는
  사용/제거로 인해 이동하지 않는다.

### Claude's Discretion
- 정확한 슬롯 개수/최대 스택 수치(D-01).
- `InventorySlot`이 별도 클래스/구조체인지 nested 타입인지, 필드 구성(`ItemData`/`count` 외
  추가 여부는 없음 — 최소 스키마 유지).
- `TryAddItem`/`RemoveItem`/`UseItem` 등 정확한 메서드 시그니처 및 반환값 설계.
- 같은 아이템이 이미 부분 스택으로 존재할 때 새 슬롯을 여는지 기존 슬롯에 먼저 채우는지
  (스택 병합 순서) — 표준적인 접근을 취하되 연구/계획 단계에서 확인.
- `WorldItem`의 `CanInteract`/`Interact` 구체 구현, 프리팹 구성.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 선행 구현 (변경 금지 — 그대로 사용해야 하는 계약)
- `Assets/Item/Script/ItemData.cs` — Phase 17에서 완성된 아이템 정적 데이터 + `IItem.UseEffect`
  구현. 인벤토리는 이 클래스의 인스턴스(.asset) 참조를 슬롯에 담는다. `Id` 프로퍼티만 공개돼
  있음 — `Type`/`amount` 접근자가 필요하면 이번 phase에서 추가 가능(Phase 17 CONTEXT 참고).
- `Assets/Item/Script/IItem.cs` — `void UseEffect(PlayerInteraction player)`.
- `Assets/Player/Script/PlayerInteraction.cs` — 최근접 타겟 탐색(`FindNearest`)과 interact 키
  배선(`TryInteract`/`HandleInteract`)이 이미 구현돼 있다. `WorldItem`은 `IPlayerInteractable`만
  구현하면 이 인프라에 자동으로 편입된다.
- `Assets/Player/Script/IPlayerInteractable.cs` — `bool CanInteract(PlayerInteraction)` /
  `void Interact(PlayerInteraction)`.
- `Assets/map/script/Checkpoint.cs` — `WorldItem`이 그대로 따를 참조 구현 패턴(IPlayerInteractable
  최소 구현 + `GetComponent`로 필요한 컴포넌트 획득).

### 재사용 대상
- `Assets/Item/HealthPotion.asset` / `Assets/Item/AncientKey.asset` — Phase 17에서 만든 예시
  아이템 2개. 이번 phase의 테스트 데이터로 재사용.

### 선행 페이즈 결정 (참고)
- `.planning/phases/17-iitem-itemdata-scriptableobject-id-useeffect-ui/17-CONTEXT.md` — D-06
  (표시용 메타데이터 범위 밖 원칙 유지), D-05(id는 string — 슬롯이 참조할 키).

**로드맵 원문 외 별도 ADR/스펙 문서는 없음 — ROADMAP.md Phase 18 섹션과 이 CONTEXT.md가 스펙
역할을 겸한다.**

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `PlayerInteraction.FindNearest()` / `TryInteract()` — 최근접 상호작용 대상 탐색과 interact
  키 배선이 이미 완성돼 있다. `WorldItem`은 `IPlayerInteractable`만 구현하면 됨.
- `PlayerInteractionPrompt` — 상호작용 가능 시 프롬프트 UI. `WorldItem`도 자동으로 프롬프트 대상이 됨.
- `Checkpoint.cs` — `IPlayerInteractable` 최소 구현 참조 패턴(2개 메서드, `CanInteract`는
  null 체크 정도로 단순하게 유지하는 관행).

### 프로젝트 관행
- 무-null가드 관행: `PlayerStats.TakeDamage` → `CameraController.Instance.Shake()`,
  `ItemData.UseEffect` → `player.GetComponent<PlayerStats>().Heal()` 모두 null 체크 없이 호출.
  Inventory API도 이 관행을 따를지는 계획 단계에서 확인하되, 기존 패턴이 강한 선례.
- Phase 17의 `#if UNITY_EDITOR` + `[ContextMenu]` 검증 훅 패턴 — UI 없는 phase에서 Play 모드
  동작을 눈으로 검증하는 표준 방법으로 자리잡음. Phase 18도 동일 패턴을 사용(D-07).

### Integration Points
- `Assets/Item/Script/` — `Inventory.cs`, `WorldItem.cs`(또는 유사 이름) 신규 위치 후보.
  기존 `IItem.cs`/`ItemData.cs`와 같은 폴더에 둘지, `Assets/Player/Script/`에 `Inventory.cs`를
  둘지는 계획 단계에서 결정(Player 컴포넌트이므로 후자도 합리적).
- Player prefab — `Inventory` 컴포넌트가 추가될 위치. 기존 `PlayerStats`/`PlayerInteraction`이
  같은 GameObject에 있는 구조를 그대로 따름.

</code_context>

<specifics>
## Specific Ideas

- "슬롯 위치가 나중에 UI에서 의미를 가질 수 있다" — Dictionary 대신 슬롯 배열을 선택한 이유.
- "인벤토리가 가득 차면 아이템은 그대로 월드에 남아야 한다" — 데이터 유실 없음이 절대 기준.
- "Phase 17의 ContextMenu 검증 훅을 그대로 재사용" — 새로운 임시 UI/키바인딩을 만들지 않고
  기존에 확립된 patttern을 이어간다는 일관성 원칙.

</specifics>

<deferred>
## Deferred Ideas

- **인벤토리 UI(그리드, 드래그앤드롭, 아이콘 표시)** — 추후 별도 UI phase.
- **세이브/로드 연동** — Phase 19에서 `SaveData.Items`를 `List<ItemSaveEntry>`로 교체하며 처리.
- **아이템 정렬/자동 정리** — 필요해지면 별도 백로그 항목.

### Reviewed Todos (not folded)
None — 이번 세션에서 todo match-phase 18 결과 확인되지 않음(사전 스캔 생략, 매칭 대상 없음으로 판단).

</deferred>

---

*Phase: 18-inventory-system-depends-on-phase-17*
*Context gathered: 2026-09-21*
