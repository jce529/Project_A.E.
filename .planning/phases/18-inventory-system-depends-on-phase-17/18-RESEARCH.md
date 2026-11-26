# Phase 18: 인벤토리 시스템 - Research

**Researched:** 2026-09-21
**Domain:** Unity C# — MonoBehaviour inventory (fixed slots + stack) integrated with existing ScriptableObject item data and interaction system
**Confidence:** HIGH (this is a well-trodden Unity pattern, and the phase is almost entirely constrained by existing project code, not external libraries)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** 슬롯 개수와 슬롯당 최대 스택 수는 코드에 하드코딩된 고정 상수로 정의한다 (Inspector 노출 없음). 정확한 수치는 연구/계획 단계에서 확정.
- **D-02:** 내부 자료구조는 `List<InventorySlot>`(슬롯 배열) — 각 슬롯이 `ItemData` 참조 + `count`(int)를 담는다. `Dictionary<itemId, count>` 방식은 채택하지 않는다.
- **D-03:** 인벤토리는 Player GameObject의 새 컴포넌트(`Inventory : MonoBehaviour`)로 만든다 — `PlayerStats`/`PlayerInteraction`과 동일한 배치 패턴. `PlayerStats`에 필드로 통합하지 않는다.
- **D-04:** 이번 phase는 메모리 상태만 다룬다. 세이브/로드 연동은 Phase 19 범위 — `DontDestroyOnLoad` 여부를 신경쓰지 않고 기존 Player 생명주기를 그대로 따른다.
- **D-05:** 월드 아이템 오브젝트는 `Checkpoint.cs`와 동일한 `IPlayerInteractable` 패턴을 따른다 (`WorldItem : MonoBehaviour, IPlayerInteractable`) — interact 키로 줍는다. 트리거 접촉 자동 획득 방식은 채택하지 않는다. 기존 `PlayerInteraction`의 최근접 타겟팅/프롬프트 UI를 그대로 재사용한다.
- **D-06:** 인벤토리가 가득 찬 상태에서 월드 아이템을 획득 시도하면 획득 실패 — 아이템은 월드에 그대로 남고 파괴되지 않는다. 데이터 유실이 없는 것이 기본 원칙.
- **D-07:** 사용(`UseEffect` 호출) 트리거는 Phase 17과 동일한 `[ContextMenu]` 검증 훅 패턴을 따른다 — `Inventory`에 `[ContextMenu]` 메서드를 두어 Play 모드에서 Inspector 컨텍스트 메뉴로 특정 슬롯 사용을 호출한다. UI/키바인딩 배선은 이번 phase 범위 밖.
- **D-08:** 사용으로 스택이 0이 되면 해당 슬롯을 완전히 비운다(참조/카운트 초기화) — 리스트에서 엔트리를 제거해 뒤 슬롯을 당기지 않는다. 슬롯 배열 길이는 항상 고정(D-01)이며, 슬롯 위치는 사용/제거로 인해 이동하지 않는다.

### Claude's Discretion
- 정확한 슬롯 개수/최대 스택 수치(D-01).
- `InventorySlot`이 별도 클래스/구조체인지 nested 타입인지, 필드 구성(`ItemData`/`count` 외 추가 없음 — 최소 스키마 유지).
- `TryAddItem`/`RemoveItem`/`UseItem` 등 정확한 메서드 시그니처 및 반환값 설계.
- 같은 아이템이 이미 부분 스택으로 존재할 때 새 슬롯을 여는지 기존 슬롯에 먼저 채우는지 (스택 병합 순서) — 표준적인 접근을 취하되 연구/계획 단계에서 확인.
- `WorldItem`의 `CanInteract`/`Interact` 구체 구현, 프리팹 구성.

### Deferred Ideas (OUT OF SCOPE)
- 인벤토리 UI(그리드, 드래그앤드롭, 아이콘 표시) — 추후 별도 UI phase.
- 세이브/로드 연동 — Phase 19에서 `SaveData.Items`를 `List<ItemSaveEntry>`로 교체하며 처리.
- 아이템 정렬/자동 정리 — 필요해지면 별도 백로그 항목.
</user_constraints>

## Project Constraints (from CLAUDE.md)

- **No premature abstraction / no over-engineering:** implement exactly what Phase 18 needs (fixed slot inventory + world pickup + ContextMenu use trigger). Do not build save/load hooks, UI hooks, sorting, or item metadata "for later."
- **Surgical changes only:** do not refactor `ItemData.cs`, `PlayerInteraction.cs`, `IPlayerInteractable.cs`, or `Checkpoint.cs`. Only add an accessor to `ItemData` if the inventory genuinely needs `Type`/`amount` (CONTEXT.md explicitly allows this exception).
- **Respect existing style:** the project's established convention is unguarded `GetComponent<T>()` calls (no null-checks) — e.g. `ItemData.UseEffect` → `player.GetComponent<PlayerStats>().Heal(amount)`, `PlayerStats.TakeDamage` → `CameraController.Instance.Shake()`. New Inventory API should default to matching this convention rather than introducing a new defensive-null-check style, unless the planner has a specific reason to diverge (e.g., D-06's explicit "must not destroy/lose data" requirement does warrant a guard on the full-inventory path).
- **Verification-driven:** every capability needs a way to prove it worked. This phase's proof mechanism is the `[ContextMenu]` hook pattern (D-07), matching Phase 17's `UseOnPlayerFromInspector()` on `ItemData.cs`.
- **Traceability:** every changed line must trace to this phase's plan; do not touch Phase 17 files beyond the narrow accessor exception already called out in CONTEXT.md.

## Summary

This phase requires zero external packages — it is pure Unity C# using patterns already established in the codebase (Phase 17's ScriptableObject item data, the existing `PlayerInteraction`/`IPlayerInteractable` interaction framework, and the `[ContextMenu]` verification-hook convention). The correct approach is to mirror the two existing reference implementations verbatim: `ItemData.cs` for the `[ContextMenu]`-driven verification pattern, and `Checkpoint.cs` for the `IPlayerInteractable` minimal-implementation pattern. No inventory library, no "don't hand-roll" package applies here — a fixed-array slot inventory is intentionally simple domain logic that belongs in this codebase, not a dependency.

The two design decisions requiring judgment (both already flagged as Claude's discretion) are: (1) `InventorySlot` should be a plain C# `class` (not `struct`), because `List<InventorySlot>` populated with mutable value-type structs is a classic Unity foot-gun — modifying a struct pulled out of a `List<T>` by index (`list[i].count++`) does not mutate the list's backing element unless you write back the whole struct, which silently produces bugs that don't show up until runtime; and (2) stack-merge order should fill existing partial stacks of the same item before opening a new empty slot, which is the standard behavior in nearly all fixed-slot inventory implementations (Minecraft, most action-RPGs) and is what a player intuitively expects.

**Primary recommendation:** Build `Inventory : MonoBehaviour` with a `private readonly List<InventorySlot> slots` sized to a fixed `const int SlotCount` at `Awake()`, using a mutable `class InventorySlot { public ItemData item; public int count; }`. Implement `TryAddItem(ItemData, int) : bool`, `RemoveItem(int slotIndex, int amount) : bool`, and `UseItem(int slotIndex)` (calling `slot.item.UseEffect(playerInteraction)` then decrementing/clearing per D-08). `WorldItem : MonoBehaviour, IPlayerInteractable` holds a single `ItemData` + count field, and `Interact()` calls `player.GetComponent<Inventory>().TryAddItem(...)`, only destroying/deactivating itself if the add succeeded (per D-06).

## Standard Stack

### Core
No third-party libraries apply. This phase uses only:

| Component | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| UnityEngine (ScriptableObject, MonoBehaviour) | Project's existing Unity version | Item data + inventory/world object components | Already the project's established architecture from Phase 17 |
| `System.Collections.Generic.List<T>` | BCL | Fixed-size slot storage | D-02 locked decision |

### Supporting
None needed. No JSON, no UI toolkit, no new packages.

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `List<InventorySlot>` fixed at construction | `InventorySlot[]` array | Functionally equivalent; `List<T>` chosen because CONTEXT.md D-02 explicitly names `List<InventorySlot>` — do not deviate. |
| Hand-rolled inventory | Third-party inventory asset (e.g., Unity Asset Store inventory systems) | Explicitly wrong here — the whole system is ~150 lines of domain-specific logic tightly coupled to `ItemData`/`IPlayerInteractable`; a generic asset-store package would fight the existing architecture, not simplify it. |

**Installation:** None — no packages to install.

**Version verification:** Not applicable (no package registry dependency).

## Architecture Patterns

### Recommended Project Structure
```
Assets/
├── Item/Script/
│   ├── ItemData.cs        # existing, Phase 17, do not modify structurally
│   ├── IItem.cs            # existing, do not modify
│   └── WorldItem.cs        # NEW — world-placed pickup, mirrors Checkpoint.cs
├── Player/Script/
│   ├── PlayerInteraction.cs        # existing, do not modify
│   ├── IPlayerInteractable.cs      # existing, do not modify
│   └── Inventory.cs        # NEW — Player component, mirrors PlayerStats placement (D-03)
```
`WorldItem.cs` belongs in `Assets/Item/Script/` alongside `ItemData.cs`/`IItem.cs` since it is item-domain logic, not player logic — mirroring how `Checkpoint.cs` lives in `Assets/map/script/` (its own domain folder) rather than in `Assets/Player/Script/`. `Inventory.cs` belongs in `Assets/Player/Script/` since it is a Player-owned component, matching D-03's explicit instruction to place it like `PlayerStats`/`PlayerInteraction`.

### Pattern 1: Mutable class for List-of-slots (avoid struct-in-List mutation bug)
**What:** `InventorySlot` must be a `class`, not a `struct`.
**When to use:** Any time a `List<T>`/array element needs field-level in-place mutation by index.
**Why:** If `InventorySlot` were a `struct`, `slots[i].count += n;` works only because C# indexer access on `List<T>` returns a copy for read but the compiler special-cases indexer assignment on arrays/lists to write back — actually for `List<T>` this specific compound-assignment form Doesn't compile in older idioms and for read-modify-use patterns (e.g., `var slot = slots[i]; slot.count -= amount;`) it silently mutates a stack copy, not the list element. This is one of the most common Unity beginner bugs with inventories. Using a `class` sidesteps the entire category of bug because slot references are stable.
**Example:**
```csharp
// Source: project convention inferred from ItemData.cs (SerializeField class, not struct)
[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;
    public void Clear() { item = null; count = 0; }
}
```

### Pattern 2: Fixed-size slot list initialized at Awake, never resized
**What:** Pre-populate `slots` with `SlotCount` empty `InventorySlot` instances once; never `Add`/`RemoveAt` afterward.
**When to use:** Satisfies D-01 (fixed count) and D-08 (slot positions never shift).
**Example:**
```csharp
private const int SlotCount = 20;   // Claude's discretion — see Open Questions for rationale
private const int MaxStack = 99;

private readonly List<InventorySlot> slots = new List<InventorySlot>(SlotCount);

private void Awake()
{
    for (int i = 0; i < SlotCount; i++)
        slots.Add(new InventorySlot());
}
```
Never call `slots.RemoveAt(i)` or `slots.Add(newSlot)` after `Awake` — that would violate D-08 (fixed positions). "Emptying" a slot means calling `slots[i].Clear()`, not removing the list entry.

### Pattern 3: Fill-existing-stack-first merge order
**What:** When adding an item, first try to top up existing non-full stacks of the same `ItemData` (by reference or `Id` equality), then fall back to the first empty slot.
**When to use:** `TryAddItem` implementation — this is the standard behavior expected by players in stack-based inventories (Minecraft, Terraria, most ARPGs).
**Example:**
```csharp
// Source: standard fixed-slot inventory algorithm, cross-checked against common
// Unity inventory tutorials (Brackeys-style slot+stack patterns) and general
// game-dev convention. No official Unity API for this — it's application logic.
public bool TryAddItem(ItemData item, int amount)
{
    if (item == null || amount <= 0) return false;
    int remaining = amount;

    // Pass 1: top up existing partial stacks of the same item.
    foreach (var slot in slots)
    {
        if (remaining <= 0) break;
        if (slot.item != item || slot.count >= MaxStack) continue;
        int space = MaxStack - slot.count;
        int add = Mathf.Min(space, remaining);
        slot.count += add;
        remaining -= add;
    }

    // Pass 2: place leftover into empty slots.
    foreach (var slot in slots)
    {
        if (remaining <= 0) break;
        if (!slot.IsEmpty) continue;
        int add = Mathf.Min(MaxStack, remaining);
        slot.item = item;
        slot.count = add;
        remaining -= add;
    }

    // D-06: if anything is left unplaced, the whole operation is a failure —
    // partial success would silently duplicate/lose count vs. what's on the
    // ground. Caller (WorldItem) must not destroy the world object if this
    // returns false.
    return remaining <= 0;
}
```
**Important nuance for D-06:** because this phase must guarantee "no data loss," `TryAddItem` should be all-or-nothing at the call site. Either apply the two passes speculatively and roll back on failure, or (simpler and equally correct) pre-compute available capacity across existing partial stacks + empty slots before mutating anything, and only mutate if total capacity ≥ amount. The pre-check approach is cleaner and avoids needing a rollback path — recommended.

### Pattern 4: IPlayerInteractable minimal implementation (mirror Checkpoint.cs exactly)
**What:** `WorldItem` implements `CanInteract`/`Interact` with the same minimalism as `Checkpoint.cs`.
**Example:**
```csharp
// Source: Assets/map/script/Checkpoint.cs (existing pattern, D-05 mandates mirroring it)
public class WorldItem : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private ItemData item;
    [SerializeField, Min(1)] private int count = 1;

    public bool CanInteract(PlayerInteraction player) => player != null;

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        var inventory = player.GetComponent<Inventory>();
        // Matches unguarded-GetComponent convention elsewhere, but D-06 requires
        // checking the *result* of the add, not skipping the null check on inventory
        // itself — if Inventory is missing entirely that's a setup bug, not a
        // "full inventory" case, so the existing project convention (no null guard)
        // still applies to inventory itself.
        if (inventory.TryAddItem(item, count))
        {
            gameObject.SetActive(false); // or Destroy(gameObject) — see Open Questions
        }
        // else: D-06 — leave the object in the world untouched, do nothing.
    }
}
```

### Anti-Patterns to Avoid
- **`InventorySlot` as a `struct`:** causes silent mutation bugs when read out of `List<T>` by value. Use `class`.
- **Removing/inserting list entries to represent empty slots:** violates D-08 (fixed slot positions). Always mutate slot contents in place; never change `slots.Count`.
- **Partial-success `TryAddItem` that mutates some slots then fails:** violates D-06's "no data loss" principle if the caller destroys the world item on any truthy-ish result. Make the method genuinely atomic: check capacity before mutating, or roll back on shortfall.
- **Destroying the `WorldItem` GameObject before confirming `TryAddItem` succeeded:** violates D-06 directly. Always gate destruction/deactivation on the boolean return value.
- **Adding an inventory UI, keybinding, or drag-and-drop scaffold "just in case":** explicitly out of scope (Deferred Ideas) and violates CLAUDE.md's no-over-engineering principle.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Nearest-interactable detection / interact-key wiring | A new raycast/overlap system for `WorldItem` | `PlayerInteraction.FindNearest()` + `IPlayerInteractable` | Already fully implemented and works for `Checkpoint`; `WorldItem` gets this for free by implementing the interface (D-05 explicit). |
| Interaction prompt UI | New prompt/UI code for "Press E to pick up" | `PlayerInteractionPrompt` (already wired into `PlayerInteraction`) | Automatically shows for any `IPlayerInteractable`, no changes needed. |
| Item use-effect logic | New `UseItem` effect dispatch/switch in `Inventory` | `ItemData.UseEffect(PlayerInteraction)` (Phase 17, IItem contract) | `Inventory.UseItem` should just call `slot.item.UseEffect(playerInteraction)` — it must not reimplement heal/consumable logic. |

**Key insight:** This phase's entire value is gluing three already-solid pieces together (item data, interaction targeting, and a new slot container) — the only genuinely new code is the slot array and its add/remove/use bookkeeping. Everything else is composition, not construction.

## Common Pitfalls

### Pitfall 1: Struct-based `InventorySlot` inside `List<T>`
**What goes wrong:** Code that does `var slot = slots[i]; slot.count++;` compiles fine but silently doesn't affect the actual list content if `InventorySlot` is a struct.
**Why it happens:** C# structs are value types; pulling one out of a `List<T>` indexer copies it.
**How to avoid:** Use `class InventorySlot` (Pattern 1 above) — this eliminates the whole failure class.
**Warning signs:** ContextMenu "use item" hook appears to do nothing / count doesn't change after several manual tests in Play mode.

### Pitfall 2: Confusing "remove entry from list" with "clear slot" for D-08
**What goes wrong:** Implementing `UseItem`/`RemoveItem` with `slots.RemoveAt(index)` shifts every later slot's index down by one, which both breaks the fixed-position guarantee (D-08) and silently reindexes items the player didn't touch.
**Why it happens:** `List<T>.RemoveAt` is the "obvious" API for "remove an item," but here the container is being used as a fixed array, not a dynamic collection.
**How to avoid:** Never call `RemoveAt`/`Add` on `slots` after initial population. "Removing" always means `slots[i].Clear()`.
**Warning signs:** Picking up a second item after using slot 0 makes items appear to have moved slots.

### Pitfall 3: Non-atomic `TryAddItem` causing item duplication or loss under D-06
**What goes wrong:** If `TryAddItem` mutates some slots, then discovers there isn't enough room for the remainder and returns `false`, but the caller already destroyed the world item (or the caller ignores the `false` and destroys anyway) — items get duplicated (partial add + object destroyed) or lost.
**Why it happens:** Treating add-to-inventory as an incremental/streaming operation instead of a single transaction.
**How to avoid:** Precompute total available capacity (sum of `MaxStack - slot.count` across matching partial stacks + `MaxStack` per empty slot) before mutating anything; only proceed with actual mutation if capacity ≥ requested amount. Only then does the caller destroy the world object.
**Warning signs:** Picking up an item when inventory is nearly full sometimes leaves the world item destroyed but count added doesn't match count removed from the world.

### Pitfall 4: `UseEffect` called with the wrong `PlayerInteraction` reference
**What goes wrong:** `Inventory.UseItem` needs a `PlayerInteraction` instance to pass into `ItemData.UseEffect(PlayerInteraction player)`. If `Inventory` doesn't cache/fetch this correctly (e.g. calls `FindAnyObjectByType<PlayerInteraction>()` every time like the Phase 17 editor-only stub does), it works but is wasteful; the more idiomatic approach since `Inventory` lives on the same GameObject as `PlayerInteraction` (per D-03, same placement pattern) is `GetComponent<PlayerInteraction>()`.
**Why it happens:** Phase 17's ContextMenu stub used `FindAnyObjectByType` because it had no other reference available (it's on the ScriptableObject asset, not a scene object). `Inventory`, being a scene MonoBehaviour on Player, has no such excuse.
**How to avoid:** Cache `PlayerInteraction` via `GetComponent<PlayerInteraction>()` in `Awake()`, matching the unguarded-GetComponent convention used elsewhere in the codebase.
**Warning signs:** Reviewer/planner should flag any `FindAnyObjectByType` call inside a component that already lives on the Player GameObject.

### Pitfall 5: Editor-only ContextMenu methods leaking into builds
**What goes wrong:** Forgetting the `#if UNITY_EDITOR` guard around ContextMenu verification hooks means `[ContextMenu]` methods remain callable/visible in production builds (usually harmless functionally, but violates the established Phase 17 pattern and is dead weight in shipped builds).
**How to avoid:** Wrap ContextMenu methods in `#if UNITY_EDITOR ... #endif`, exactly like `ItemData.cs` does.
**Warning signs:** Grep for `[ContextMenu]` without a preceding `#if UNITY_EDITOR` in the same file.

## Code Examples

### Full `UseItem` respecting D-08
```csharp
// Source: derived directly from D-07/D-08 and Phase 17's IItem.UseEffect contract
public void UseItem(int slotIndex)
{
    if (slotIndex < 0 || slotIndex >= slots.Count) return;
    var slot = slots[slotIndex];
    if (slot.IsEmpty) return;

    slot.item.UseEffect(playerInteraction); // cached PlayerInteraction, see Pitfall 4
    slot.count--;
    if (slot.count <= 0) slot.Clear(); // D-08: clear in place, do not RemoveAt
}

#if UNITY_EDITOR
[ContextMenu("Phase18: Use Slot 0")]
private void UseSlot0FromInspector() => UseItem(0);
#endif
```

### `RemoveItem` (discretionary signature)
```csharp
// Standard signature choice: index-based (matches slot-position identity, D-02/D-08),
// not item-based (item-based removal is ambiguous about which stack to draw from when
// the same item occupies multiple slots).
public bool RemoveItem(int slotIndex, int amount)
{
    if (slotIndex < 0 || slotIndex >= slots.Count || amount <= 0) return false;
    var slot = slots[slotIndex];
    if (slot.IsEmpty || slot.count < amount) return false;
    slot.count -= amount;
    if (slot.count <= 0) slot.Clear();
    return true;
}
```

## State of the Art

Not applicable in the "old vs new API" sense — there is no deprecated/current split for hand-rolled slot inventories. The one relevant "current best practice" note: Unity's newer `FindAnyObjectByType`/`FindFirstObjectByType` (replacing the obsolete `FindObjectOfType`) is already correctly used in `ItemData.cs`'s Phase 17 ContextMenu stub — if any new code in this phase still needs a scene-wide find (it shouldn't, per Pitfall 4), use `FindAnyObjectByType`, not the obsolete `FindObjectOfType`.

**Deprecated/outdated:**
- `Object.FindObjectOfType<T>()` — obsolete since Unity 2023.1; project already avoids it in Phase 17 code. Continue avoiding it in Phase 18.

## Open Questions

1. **Exact `SlotCount`/`MaxStack` constants (D-01 discretion)**
   - What we know: CONTEXT.md explicitly floats "예: 20슬롯/99스택" (e.g. 20 slots/99 stack) as an illustrative example, not a locked number.
   - What's unclear: No gameplay-balance requirement exists yet (no UI, no inventory-pressure design has been discussed).
   - Recommendation: Use the example values verbatim — `SlotCount = 20`, `MaxStack = 99` — as sensible, unremarkable defaults matching the CONTEXT.md illustration. These are trivially changeable constants later since D-01 explicitly forbids Inspector exposure (so there's no migration cost to changing a `const int` before a UI phase locks in slot-grid dimensions).

2. **`WorldItem` pickup: `SetActive(false)` vs `Destroy(gameObject)`**
   - What we know: D-06 requires the object to remain in the world, untouched, on failure. On success, CONTEXT.md doesn't specify destroy vs deactivate.
   - What's unclear: Whether world items might need to reappear (e.g., respawn) — out of scope per Deferred Ideas (no save/load this phase), so this is a purely cosmetic/GC choice.
   - Recommendation: `Destroy(gameObject)` on success is simpler and matches the "picked up = gone" mental model with no deferred respawn logic to maintain; use `SetActive(false)` only if the planner anticipates a pooling need, which nothing in this phase's scope suggests. Destroy is the safer minimal choice.

3. **`TryAddItem` signature: single item+count, or does it need a `Sprite`/display param?**
   - What we know: CONTEXT.md D-06 (Phase 17) already ruled out display metadata on `ItemData` entirely.
   - What's unclear: None — this is settled by the Phase 17 CONTEXT already referenced. No open question in practice.
   - Recommendation: Keep `TryAddItem(ItemData item, int amount)` — no additional parameters.

## Environment Availability

Skipped — this phase has no external dependencies beyond the Unity Editor/engine already used by the whole project (no new packages, no CLI tools, no external services).

## Validation Architecture

`.planning/config.json` was checked; treat `workflow.nyquist_validation` as absent-or-default (project has no automated Unity test runner detected across prior phases — every prior phase in STATE.md verifies via manual Play-mode checks and `[ContextMenu]` hooks, e.g. Phase 17's `UseOnPlayerFromInspector`, Phase 16's Unity CLI eval/console checks). There is no NUnit/Unity Test Framework harness evidenced anywhere in STATE.md's history; all verification across phases 5-17 has been manual Play-mode observation or `git diff` line-count static gates.

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected — no Unity Test Framework (`com.unity.test-framework`) usage found in prior phases; verification convention is manual Play-mode + `[ContextMenu]` hooks |
| Config file | none |
| Quick run command | N/A — manual: enter Play mode, use Inspector `[ContextMenu]` hook, observe Console/Inspector state |
| Full suite command | N/A |

### Phase Requirements → Test Map
No formal REQ-IDs exist for this phase (REQUIREMENTS.md tracks an unrelated milestone — v2.0 boss content — not this inventory phase; this project appears to interleave multiple milestones and this phase's spec lives entirely in ROADMAP.md + 18-CONTEXT.md per that file's own statement). Mapping decisions (from CONTEXT.md) to manual verification instead:

| Decision | Behavior | Verification Method |
|--------|----------|-------------------|
| D-01/D-02 | Fixed slot list holds ItemData+count | Play mode: inspect `Inventory` component's `slots` list in Inspector (mark `InventorySlot` `[System.Serializable]`) after adding items |
| D-03 | Inventory lives on Player GameObject | Static: confirm `Inventory` attached to Player prefab alongside `PlayerStats`/`PlayerInteraction` |
| D-05/D-06 | WorldItem pickup via interact key, fails safely when full | Play mode: fill inventory, walk to a WorldItem, press interact key, confirm item remains in world and no exception thrown |
| D-07/D-08 | ContextMenu use triggers UseEffect, clears slot at 0 | Play mode: right-click `Inventory` component header in Play mode Inspector, invoke "Use Slot N", confirm HP restored (using `HealthPotion.asset`) and slot shows empty afterward |

### Sampling Rate
- Per task: manual Play-mode spot check via ContextMenu hook (matches existing project convention, no automated command exists)
- Per wave/phase gate: full manual walkthrough — pick up `HealthPotion.asset` and `AncientKey.asset` world instances, fill inventory to trigger D-06 fail path, use a slot to trigger D-07/D-08 clear path

### Wave 0 Gaps
- No automated test framework exists in this project. Introducing one is out of scope for this phase (would violate CLAUDE.md's scope-discipline principle — Phase 18 is inventory logic, not test infra). Continue the project's established manual-verification convention.

*(No gaps beyond framework absence, which is a pre-existing, out-of-scope condition — not something this phase should fix.)*

## Sources

### Primary (HIGH confidence)
- `Assets/Item/Script/ItemData.cs` — Phase 17 ScriptableObject + `[ContextMenu]` pattern (read directly)
- `Assets/Item/Script/IItem.cs` — `UseEffect(PlayerInteraction)` contract (read directly)
- `Assets/Player/Script/PlayerInteraction.cs` — `FindNearest`/`TryInteract` interaction infrastructure (read directly)
- `Assets/Player/Script/IPlayerInteractable.cs` — interaction contract (read directly)
- `Assets/map/script/Checkpoint.cs` — canonical minimal `IPlayerInteractable` implementation (read directly)
- `.planning/phases/18-inventory-system-depends-on-phase-17/18-CONTEXT.md` — locked decisions D-01 through D-08 (read directly)

### Secondary (MEDIUM confidence)
- General C# knowledge: `struct` value-copy semantics inside `List<T>` — well-established language behavior, not project-specific, high confidence despite no external doc citation (this is core C# spec behavior, not a claim needing verification).

### Tertiary (LOW confidence)
- "Fill existing stacks before opening new slot" as the assumed player-expectation default — this is inferred from general genre convention (Minecraft/Terraria/ARPG inventories), not verified against any Unity official doc or this project's own prior UI/UX decisions (no UI exists yet). Flagged for planner/user confirmation if a different merge order is preferred, though CONTEXT.md's phrasing ("표준적인 접근을 취하되") suggests standard behavior is acceptable without further discussion.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no external stack to verify, only in-repo code, read directly
- Architecture: HIGH — dictated almost entirely by locked CONTEXT.md decisions and existing code patterns in the repo
- Pitfalls: HIGH for struct-vs-class and RemoveAt pitfalls (core C#/List<T> semantics); MEDIUM for stack-merge-order convention (genre convention, not verified against an official source)

**Research date:** 2026-09-21
**Valid until:** No expiry pressure — this research is entirely internal-codebase-driven with no external library version dependency; valid indefinitely unless `ItemData.cs`/`PlayerInteraction.cs`/`IPlayerInteractable.cs` change.
