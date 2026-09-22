# Phase 17: 아이템 코어 (IItem + ItemData ScriptableObject) - Research

**Researched:** 2026-09-20  
**Domain:** Unity ScriptableObject data layer + IItem interface implementation  
**Confidence:** HIGH

## Summary

Phase 17 implements `ItemData`, the project's first ScriptableObject (SO), which directly implements the existing `IItem` interface and encodes static item metadata (id, type, consumable/progression effect parameters). The phase focuses purely on data layer — no UI, no inventory — and includes concrete example `.asset` files to verify end-to-end: at minimum a Heal consumable that calls `PlayerStats.Heal(amount)` via `PlayerInteraction.GetComponent<PlayerStats>()`.

The project has zero existing ScriptableObject usage and no custom SO conventions, so this phase establishes the pattern. Core decisions from CONTEXT.md D-01 through D-07 are locked; discretion areas involve enum naming, field layout, and GetComponent caching strategy (constrained by the project's null-guard-free singleton convention).

**Primary recommendation:** Implement `ItemData : ScriptableObject, IItem` as a single file with nested enums, minimal fields (id, type, effectType Consumable-only, amount Consumable-only), and full UseEffect body that branches on type. Call `player.GetComponent<PlayerStats>()` without null guards to match existing pattern. Create 1–2 example `.asset` files via CreateAssetMenu to verify schema end-to-end.

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** `ItemData : ScriptableObject, IItem` — `ItemData` directly implements `IItem`, no separate runtime class. Data and use logic coexist in one file.
- **D-02:** `UseEffect()` is fully implemented, not a stub. Branches on type: consumables apply real effects; progression items are no-op (future content phases handle their semantics).
- **D-03:** Consumable `effectType` enum includes only **Heal** in this phase, with float `amount` parameter. Calls existing `PlayerStats.Heal(float)` via `PlayerInteraction` → `GetComponent<PlayerStats>()`. Do not add new PlayerStats methods.
- **D-04:** `ItemType` enum: `Consumable` / `Progression`. No subclass hierarchy — single class + enum branching.
- **D-05:** `id` field is `[SerializeField] string id`, manual input, no auto-generation or validation tool in this phase.
- **D-06:** Minimal fields only: `id` (string), `type` (ItemType), `effectType` (Consumable-only), `amount` (Consumable-only). Display metadata (displayName, icon, description) deferred to Phase 18+.
- **D-07:** Create 1–2 example `.asset` files in `Assets/Item/` — minimum: one Heal consumable + one Progression item, verified to work in scene.

### Claude's Discretion
- Exact naming for `ItemType` / `ConsumableEffectType` enums; file layout (nested vs. separate files)
- `CreateAssetMenu` attribute menuName/fileName strings
- Whether `GetComponent<PlayerStats>` result is cached as a field or called directly each use — constrained by project's null-guard-free pattern (see Pitfall: Null Guards)
- Example asset names and numeric values (id string, amount float)

### Deferred Ideas (OUT OF SCOPE)
- Display metadata fields (displayName, icon, description) — Phase 18+ when UI layer exists
- Item ID validation / duplicate checking — currently manual, tool deferred
- Extended consumable effectTypes (buffs, debuffs, currency, etc.) — add to enum as needed

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| (None mapped yet) | Phase 17 scope is defined by D-01 through D-07 in CONTEXT.md; no formal requirements document exists for this phase | CONTEXT.md locked decisions provide full specification |

## Project Constraints (from CLAUDE.md)

1. **Simplicity First** — Implement minimal code; avoid over-engineering scopes outside this phase.
2. **Surgical Changes** — Only modify `Assets/Item/Script/` and create `.asset` files; do not refactor neighboring code.
3. **Null-Guard Convention** — Project explicitly does NOT null-guard singletons or GetComponent results (e.g., `CameraController.Instance.Shake()` in PlayerStats.cs line 75, no check). UseEffect must follow this pattern.
4. **Scope Discipline** — Stay within data layer: schema definition + example assets. No inventory, no UI, no integration with PlayerInteraction's interaction system yet (Phase 18).

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Unity ScriptableObject | Built-in | Base class for ItemData | First SO in project; standard for static game data (items, configurations, balancing parameters) |
| Unity.Serialization | Built-in | `[SerializeField]`/`[SerializeReference]` | Canonical Unity data layer pattern for SO and editor exposure |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `IItem` interface | Project-local | ItemData contract | `Assets/Item/Script/IItem.cs` — already exists, enforces `void UseEffect(PlayerInteraction player)` |
| `PlayerStats.Heal(float)` | Project (Phase 11) | Consumable healing effect | Existing public method at `Assets/Player/Script/PlayerStats.cs:37` — reuse as-is |

## Architecture Patterns

### Recommended Project Structure

```
Assets/Item/
├── Script/
│   ├── IItem.cs                  (existing interface)
│   └── ItemData.cs               (NEW — Phase 17 deliverable)
└── (item .asset files created via CreateAssetMenu)
```

### Pattern 1: ScriptableObject with IItem Implementation

**What:** `ItemData : ScriptableObject, IItem` — a single class encodes item static data and implements the use effect logic. The SO serves both as data container (inspector-editable, serializable to `.asset` files) and as the runtime behavior object (Phase 18 inventory will hold references and call `UseEffect`).

**When to use:** This pattern is appropriate because:
- Phase 17 explicitly prohibits a separate runtime class (D-01)
- Items are inherently static (no per-instance state mutation outside of player consequences)
- Phase 18+ will fetch ItemData from inventory and call UseEffect directly
- Nesting enums inside ItemData.cs keeps the schema self-contained

**Example:**
```csharp
// Source: Phase 17 CONTEXT.md D-01, D-02, D-03, D-04, D-06

[CreateAssetMenu(menuName = "Items/Item Data", fileName = "New Item")]
public class ItemData : ScriptableObject, IItem
{
    [SerializeField] private string id;
    [SerializeField] private ItemType type;
    [SerializeField] private ConsumableEffectType effectType;  // Consumable-only parameter
    [SerializeField] private float amount;                      // Consumable-only parameter

    public enum ItemType
    {
        Consumable,
        Progression
    }

    public enum ConsumableEffectType
    {
        Heal  // Only Heal in this phase (D-03)
    }

    // D-02: Full implementation, not a stub. Branches on type.
    // Progression items are no-op; consumables apply real effects.
    public void UseEffect(PlayerInteraction player)
    {
        if (type == ItemType.Consumable)
        {
            // D-03: Only Heal in this phase. Reuse existing PlayerStats.Heal(float).
            // No null guard on GetComponent (matches project convention in Phase 12/15).
            PlayerStats stats = player.GetComponent<PlayerStats>();
            stats.Heal(amount);
        }
        // else: Progression item — no-op in this phase
    }
}
```

**Key points:**
- `id` is `[SerializeField] string` for manual inspector entry (D-05)
- `effectType` and `amount` are Consumable-only; they are still serialized fields but only populated when `type == Consumable` (Inspector UI discipline)
- No null guards on `GetComponent` (matches Phase 12 comment in PlayerStats.cs:74)
- No display metadata fields — deferred to Phase 18+ (D-06)

### Pattern 2: CreateAssetMenu for Editor Integration

**What:** The `[CreateAssetMenu]` attribute auto-registers ItemData in the right-click asset menu, letting designers create `.asset` files without code.

**When to use:** Always on ScriptableObjects that need multiple instances edited by humans.

**Example:**
```csharp
[CreateAssetMenu(menuName = "Items/Item Data", fileName = "New Item")]
public class ItemData : ScriptableObject, IItem
{
    // ...
}
```

After adding this, right-click in `Assets/Item/` → Create → Items → Item Data, and assign id/type/parameters in the Inspector. Each created asset is a serialized instance.

### Pattern 3: Example Assets for Phase Verification

**What:** Create 1–2 concrete `.asset` files (D-07) to verify the schema works end-to-end in an actual scene:
- **Consumable (Heal):** e.g., `HealthPotion.asset` with id="health_potion_01", type=Consumable, effectType=Heal, amount=20
- **Progression:** e.g., `QuestItem.asset` with id="quest_item_01", type=Progression (amount/effectType ignored)

**When to use:** Always for data-layer phases; these assets become test data for Phase 18 inventory development.

**How to create:**
1. Right-click in `Assets/Item/` → Create → Items → Item Data
2. Name it (e.g., `HealthPotion.asset`)
3. In Inspector, fill id, type, and (if Consumable) effectType + amount
4. Save the scene and verify no serialization errors in the console

### Anti-Patterns to Avoid

- **No separate SO subclasses:** Avoid `ConsumableItemData : ItemData` and `ProgressionItemData : ItemData`. D-04 explicitly chooses enum branching in a single class to minimize schema surface area early.
- **No complex nested data structures yet:** Do not add `List<Effect>` or `Dictionary<string, float>` parameters. Start with scalar fields (string, enum, float); arrays/dicts are Phase 18+ concern.
- **Do not null-guard GetComponent:** The project convention (seen in PlayerStats.cs:75 and CameraController) is to assume components exist. Writing `if (stats != null) stats.Heal(amount)` breaks consistency.
- **Do not auto-generate ids:** D-05 specifies manual string input. Do not implement asset-name-based or hash-based id generation in this phase.
- **No display name in ItemData yet:** Phase 17 has no UI; D-06 defers icon/name/description fields. Do not add them — Phase 18 will.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Generic data containers (key-value storage, JSON serialization) | Custom property bag or reflection-based serializer | Unity's built-in `[SerializeField]` + SO | Unity's Inspector and save/load handles all serialization automatically; custom solutions are fragile and lose editor integration |
| Enum-based branching on item types | A large if-else chain with duplicate fields | Single `ItemType` enum + conditional field visibility in Editor (Phase 18 UI can add `[HideInInspector]` if needed) | Enums are type-safe and handle extensibility cleanly; if-else chains are error-prone as item types grow |
| Singleton access to item data | Custom global registry or static references | Phase 18 inventory will hold ItemData references directly | ItemData is data, not behavior; no need for global lookup. Inventory owns the collection |
| Caching GetComponent result | A side-effect field in ItemData (e.g., `private PlayerStats _cachedStats`) | Call GetComponent directly in UseEffect | ScriptableObjects are scene-agnostic; caching a component reference violates that separation and causes stale references across scene loads |

**Key insight:** ScriptableObjects are data; they should not hold cached MonoBehaviour references or stateful game objects. That's Phase 18 inventory's job.

## Code Examples

### ItemData.cs Full Template

```csharp
// Source: Phase 17 CONTEXT.md, standard Unity SO pattern

using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Data", fileName = "New Item")]
public class ItemData : ScriptableObject, IItem
{
    [SerializeField] private string id;
    [SerializeField] private ItemType type;
    [SerializeField] private ConsumableEffectType effectType;
    [SerializeField] private float amount;

    public enum ItemType
    {
        Consumable,
        Progression
    }

    public enum ConsumableEffectType
    {
        Heal
    }

    public void UseEffect(PlayerInteraction player)
    {
        if (type == ItemType.Consumable)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            stats.Heal(amount);
        }
        // Progression items: no-op in this phase
    }
}
```

### Creating an Example Asset (HealthPotion)

After implementing ItemData.cs:
1. Right-click in `Assets/Item/` folder
2. Create → Items → Item Data
3. Name it `HealthPotion.asset`
4. In Inspector, set:
   - **id:** `health_potion_01`
   - **type:** `Consumable`
   - **effectType:** `Heal`
   - **amount:** `20`
5. Save the scene

The `.asset` file is now ready for Phase 18 inventory tests.

## Common Pitfalls

### Pitfall 1: ScriptableObject Caching Component References

**What goes wrong:** A developer caches the PlayerStats reference in ItemData:
```csharp
private PlayerStats _cached;
void UseEffect(PlayerInteraction player)
{
    if (_cached == null) _cached = player.GetComponent<PlayerStats>();
    _cached.Heal(amount);
}
```
After a scene reload, `_cached` points to the old scene's (destroyed) PlayerStats. The heal has no effect, or crashes.

**Why it happens:** Confusion between data (SO, scene-agnostic) and behavior (MonoBehaviour, scene-local). SO should never hold stateful references.

**How to avoid:** Always fetch fresh via `GetComponent` in UseEffect. The call is cheap (single physics hashtable lookup) and keeps SO stateless.

**Warning signs:** Any field in ItemData that holds a MonoBehaviour reference; any cache that survives scene loads.

### Pitfall 2: Null-Guarding Against Project Convention

**What goes wrong:** A task author adds:
```csharp
PlayerStats stats = player.GetComponent<PlayerStats>();
if (stats != null)
    stats.Heal(amount);
```
This breaks consistency with the pattern used in PlayerStats.cs:75 (`CameraController.Instance.Shake()` no guard) and Phase 12 CONTEXT comments. Tests fail because a game object is missing expected components; the fix is to ensure the setup is correct, not to silently no-op.

**Why it happens:** Defensive programming instinct; assumption that null-guards are always safe.

**How to avoid:** Read the project's existing null-guard convention (Phase 12 PlayerStats, Phase 13 codebase audit, CameraController). Match the pattern: assume expected components exist, or fail loudly.

**Warning signs:** Any check like `if (stats != null)` or `?.` operator in new code; comments about "possibly missing" components.

### Pitfall 3: Misunderstanding ScriptableObject Lifecycle

**What goes wrong:** A developer expects ItemData instances to be instantiated at runtime and destroyed:
```csharp
ItemData item = Instantiate(itemDataAsset);
```
This works but is wasteful — each Instantiate creates a new object copy. If the original `.asset` is never modified, cloning is unnecessary.

**Why it happens:** Confusion with Prefabs or component objects.

**How to avoid:** Keep ItemData assets as singletons. Phase 18 inventory will hold direct references (`List<ItemData>` or `Dictionary<string, ItemData>`), not instantiated copies.

**Warning signs:** Calls to `Instantiate(itemData)` outside of copy/clone scenarios; discussion of "runtime item instances" in Phase 17 scope.

### Pitfall 4: Enum Bloat Without Clear Effect Hierarchy

**What goes wrong:** The ConsumableEffectType enum grows unchecked:
```csharp
public enum ConsumableEffectType
{
    Heal,
    Poison,
    Slow,
    Invisible,
    WeaponUpgrade,
    // ...
}
```
Suddenly UseEffect is 50 lines of if-else, mixing numeric buffs, status effects, and item upgrades. Hard to refactor; new effects require touching the enum and UseEffect.

**Why it happens:** No clear design boundary; every new item type triggers enum extension.

**How to avoid:** In Phase 17, only define **Heal**. When Phase 18+ adds new consumable types, coordinate with item designer to group by effect category (numeric damage/heal vs. status vs. upgrade). Consider a Scriptable Pattern object if effects become complex (future phase).

**Warning signs:** Enum with more than 3–4 values with very different behavior; long if-else chains in UseEffect; difficulty naming new enum values.

### Pitfall 5: Forgetting Consumable-Only Field Rules in Editor

**What goes wrong:** A designer creates a Progression item but forgets it has no consumable fields. At runtime, `type == Progression` but `effectType` has garbage data (leftover from copy-paste). Phase 18 code that checks `if (type == Consumable)` is safe, but it's confusing that the asset has "junk" data.

**Why it happens:** The Editor shows all fields; no visual distinction of which are valid per type.

**How to avoid:** (Phase 18 concern, but plan ahead) Consider `[HideInInspector]` attributes on Consumable-only fields when type != Consumable. For Phase 17, just document the rule: "Set effectType/amount only if type=Consumable; leave them 0/default otherwise."

**Warning signs:** Example assets have mismatched type/field combinations; test code that's surprised by non-zero amount on a Progression item.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Game items as Prefabs with MonoBehaviour data | Items as ScriptableObjects with IItem interface | Phase 17 (this phase) | SO are lightweight, scene-agnostic, editable in Inspector without instantiation. Cleaner separation of data from scene objects |
| Multiple item type subclasses (ConsumableItem : ItemData, ProgressionItem : ItemData) | Single ItemData class + ItemType enum branching | Phase 17 design decision (D-04) | Fewer files, easier to extend with new effect types, simpler for Phase 18 to hold heterogeneous items in a single list |
| Effect parameters as separate data files (EffectConfig.json) | Effect parameters embedded in ItemData fields (amount, effectType) | Phase 17 design decision (D-03, D-06) | Minimal schema; each item is self-contained. Advanced parameter data (curves, multi-step effects) can be deferred to Phase 18+ |

## Environment Availability

(Skipped — this is a data-layer phase with no external tool dependencies beyond Unity editor, which is assumed present for development)

## Open Questions

1. **Example asset file names and initial values:** Should the Heal item be "HealthPotion" id=`health_potion_01` amount=`20`, or follow a different scheme? **Recommendation:** Use clear, lowercase_underscore_cased id strings and round numeric values (10, 20, 50) for placeholder amounts. Phase 18 can retune.

2. **Enum file layout:** Should `ItemType` and `ConsumableEffectType` be nested inside ItemData.cs, or split into `ItemType.cs` / `ConsumableEffectType.cs`? **Recommendation:** Nested enums inside ItemData.cs (single-file design per D-01). Simpler, easier to refactor as effects grow. If enums are needed elsewhere (Phase 18+), Phase 18 can extract.

3. **Caching strategy:** Should ItemData cache the player's GetComponent result, or fetch it fresh on each UseEffect call? **Recommendation:** No caching — fetch fresh. Keeps SO stateless and safe across scene loads. The GetComponent cost is negligible.

4. **Visibility of id/type fields:** Should they be public getters, or private `[SerializeField]`? **Recommendation:** Private `[SerializeField]` only (shown in CLAUDE.md Pattern 3 example). Phase 18 inventory can expose a read-only public property if needed.

## Sources

### Primary (HIGH confidence)

- **IItem.cs** (Unity project file) — Confirmed exact interface signature: `void UseEffect(PlayerInteraction player)`
- **PlayerInteraction.cs** (Unity project file) — Confirmed GetComponent<T>() available; MonoBehaviour context ensures same GameObject can hold PlayerStats
- **PlayerStats.cs** (Unity project file) — Confirmed `public override void Heal(float amount)` at line 37; confirmed null-guard-free pattern at line 75 (`CameraController.Instance.Shake()` no check)
- **CameraController.cs** (Unity project file) — Confirmed project-wide null-guard-free singleton pattern at line 75 of PlayerStats call to Instance.Shake()
- **CONTEXT.md** (Phase 17 discussion output) — Locked decisions D-01 through D-07 provide complete specification
- **STATE.md** (Project GSD state) — Confirmed Phase 15 complete, Phase 17 discuss-phase done, no prior SO examples in project

### Secondary (MEDIUM confidence)

- **Unity ScriptableObject documentation** (implicit; standard Unity practice) — CreateAssetMenu attribute, [SerializeField] pattern, SO serialization are well-established conventions. No version mismatch expected (project is Unity 2022 LTS environment)

### Tertiary (References for future phases, not directly research)

- **Phase 15 PlayerStats.cs** — Established singleton access pattern without null guards
- **Phase 18 planned inventory** — Will consume ItemData references; inform field names/visibility for Phase 17 design

## Metadata

**Confidence breakdown:**
- Standard Stack: **HIGH** — Confirmed IItem/PlayerStats signatures directly from code; Unity SO is standard library
- Architecture (single ItemData class, IItem implementation, CreateAssetMenu): **HIGH** — Locked decisions D-01, D-02, D-04 from user; example matches standard Unity practice
- Null-guard convention (no guards on GetComponent): **HIGH** — Confirmed in PlayerStats.cs:75 and project comments
- Pitfalls: **HIGH** — Cache/reference issues are well-known SO antipatterns; identified in project context (SO is first in codebase)

**Research date:** 2026-09-20  
**Valid until:** 2026-10-20 (stable domain, minor format tweaks possible in Phase 18+)

---

*Phase: 16-iitem-itemdata-scriptableobject-id-useeffect-ui*  
*Context: IItem interface exists at Assets/Item/Script/IItem.cs; PlayerInteraction.GetComponent<PlayerStats>() confirmed safe; project convention: no null guards on singletons or component fetches.*
