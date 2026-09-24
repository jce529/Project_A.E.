# Phase 19: 아이템 저장/로드 연동 - Research

**Researched:** 2026-09-21
**Domain:** Unity 6 C# / Newtonsoft.Json save-schema evolution + Resources-based asset resolution
**Confidence:** HIGH

## Summary

This is a small, purely internal integration phase: no new packages, no third-party research needed. All the risk is in three narrow spots, all of which were directly verifiable by reading the actual code and asset files rather than guessing:

1. **`ItemData.Id` and the asset filename already diverge today** (`HealthPotion.asset` → `id: health_potion_01`, `AncientKey.asset` → `id: ancient_key_01`). This resolves the CONTEXT.md D-01 discretion point empirically, not theoretically: `Resources.Load<ItemData>("Items/" + id)` would fail out of the box unless the assets are renamed to match `id` first. `Resources.LoadAll<ItemData>("Items")` + linear match on the `Id` property works with **zero renaming** and stays correct even if a future asset's filename and `id` diverge again (which they already have, twice).
2. **Newtonsoft.Json deserializing an empty JSON array (`[]`) into a `List<ItemSaveEntry>`** where the field used to be typed `List<string>` is unconditionally safe — an empty array has no elements to convert, so no per-element type resolution ever happens. This is default `JsonConvert` behavior (no custom converters, no `TypeNameHandling` in `SaveLoadManager.JsonSettings`), verified by inspecting `JsonSettings` in `SaveLoadManager.cs` (only `Formatting` and `NullValueHandling` are set).
3. **Inventory restore must run after `Inventory.Awake()`** because `Awake()` unconditionally clears `slots` and refills with 20 empty `InventorySlot`s (confirmed by reading `Inventory.cs` lines 29-34) — same ordering constraint that already forced `ApplyPlayerStatsFromSave()` to run after `yield return op` in `LoadSceneAndRestoreRoutine`, since `HP.Awake()` also resets state on scene load.

**Primary recommendation:** Use `Resources.LoadAll<ItemData>("Items")` + `Id`-field matching (not filename-based `Resources.Load`), add `CaptureInventoryItems()`/`ApplyInventoryFromSave()` mirroring the existing `CapturePlayerStats()`/`ApplyPlayerStatsFromSave()` pair exactly, and hook the restore call immediately after `ApplyPlayerStatsFromSave()` inside `LoadSceneAndRestoreRoutine`.

## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** itemId → ItemData resolved via a `Resources` folder. Move/copy `Assets/Item/*.asset` to `Assets/Resources/Items/`. No registry ScriptableObject (avoids forgetting to register new items). Whether resolution is by filename (`Resources.Load`) or by `Id` field (`Resources.LoadAll` + match) is Claude's Discretion — resolved below in Architecture Patterns using directly-observed asset data.
- **D-02:** No slot-index preservation. `ItemSaveEntry` has exactly two fields: `itemId` (string), `count` (int) — this shape is locked by the ROADMAP phase title itself. Capture non-empty slots in slot order into `List<ItemSaveEntry>`; restore by calling `Inventory.TryAddItem()` sequentially in that same order. If restoring merges what were previously multiple slots of the same item into fewer slots (because `TryAddItem`'s stack-fill-first logic), that is accepted behavior, not a bug — there is no inventory UI yet to expose the difference.
- **D-03:** Inventory capture happens **unconditionally inside `SaveLoadManager.Save()`**, mirroring the existing `CapturePlayerStats()` call. Must null-guard for scenes without an `Inventory` (e.g. main menu) using the same style as `CapturePlayerStats()`: log a warning and skip, do not throw, and do not overwrite `_data.Items` with an empty result when the Inventory can't be found.
- **D-04:** `SaveVersion` bumps from 2 to 3. No explicit `MigrateFromV2()`-style function needed — `Items` has always been an empty `List<string>` stub in every existing save file on disk, so deserializing `[]` into `List<ItemSaveEntry>` is safe (see Common Pitfalls / Code Examples for the exact mechanics). Only a null-guard is needed in `EnsureCollections()`.

### Claude's Discretion

- `Resources.Load` by filename vs. `Resources.LoadAll<ItemData>("Items")` + match by `Id` field — **resolved: use `LoadAll` + `Id` match** (see Architecture Patterns; empirically the two existing assets already have filename ≠ Id).
- Whether `ItemSaveEntry` is a standalone class file or lives alongside `SaveData.cs` — **recommendation: same file as `SaveData.cs`**, following the existing convention where `PlayerStatsSaveData` is a second small POCO class living in `SaveData.cs` rather than its own file.
- Exact method names/signatures for the capture/restore functions — **recommendation: `CaptureInventoryItems()` (void, no args, mirrors `CapturePlayerStats()`) and `ApplyInventoryFromSave()` (void, no args, mirrors `ApplyPlayerStatsFromSave()`)**.
- Exact hook point in the load flow for restoring inventory — **resolved: call `ApplyInventoryFromSave()` directly after `ApplyPlayerStatsFromSave()` inside `LoadSceneAndRestoreRoutine`**, same coroutine frame, no additional yield needed since `Inventory.Awake()` already completed synchronously by the time `yield return op` resumes (Unity scene-load Awake/Start ordering guarantee — same reasoning already documented in the existing code comment on line 437-439 of `SaveLoadManager.cs`).

### Deferred Ideas (OUT OF SCOPE)

- `ItemSaveEntry.slotIndex` for exact slot-position preservation — deferred to a future inventory-UI phase (drag-and-drop) when slot position becomes user-visible.
- `ItemDatabase` registry ScriptableObject — deferred until `Resources.Load`/`LoadAll` overhead (load time, memory) actually becomes a measured problem as item count grows.
- Item auto-sort/cleanup — already deferred from Phase 18, still out of scope here.

## Phase Requirements

No formal REQ-IDs exist for this phase (ROADMAP.md: "Requirements: TBD"; the project's REQUIREMENTS.md tracks an unrelated v2.0 boss milestone). Per Phase 17/18 precedent, CONTEXT.md's D-01 through D-04 decisions and the ROADMAP.md Phase 19 title serve as the requirement spec:

| ID | Description | Research Support |
|----|-------------|------------------|
| D-01 | itemId → ItemData resolution via Resources, no registry SO | Architecture Patterns: `Resources.LoadAll` + `Id` match recommendation, backed by actual asset inspection |
| D-02 | `ItemSaveEntry(itemId, count)`, no slot index, sequential `TryAddItem` restore | Code Examples: capture/restore method bodies |
| D-03 | Unconditional capture in `Save()`, null-guarded like `CapturePlayerStats()` | Architecture Patterns Pattern 1; Code Examples |
| D-04 | `SaveVersion` 2→3, no migration function, null-guard only | Common Pitfalls: Newtonsoft empty-array-to-new-type verification |

## Standard Stack

No new libraries. This phase reuses exactly what's already in the project.

### Core (already present, no version change)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Newtonsoft.Json (Json.NET for Unity) | already installed, version unchanged by this phase | Serializes `SaveData` (including the new `List<ItemSaveEntry>`) | Locked since Phase 11 specifically because `Dictionary<string,bool>` needs native round-tripping; `UnityEngine.JsonUtility` cannot do this |
| `UnityEngine.Resources` API | Unity 6000.3.10f1 (project's confirmed editor version per Phase 17 ROADMAP success criteria) | Runtime asset resolution for itemId → `ItemData` | Only Unity built-in mechanism that works in both Editor and player builds without an addressables/registry system; explicitly chosen in D-01 over building a registry SO |

**No installation step required** — nothing new is added to `Packages/manifest.json` or `Assets/Plugins`.

**Version verification:** N/A — no package versions change in this phase. (Confirmed: no `Newtonsoft*` package folder found directly under `Assets/`; it is resolved via Unity Package Manager, unaffected by this phase's work.)

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `Resources.LoadAll` + `Id` match | `Resources.Load` by filename | Filename-based load is O(1) instead of O(n) linear scan, but **breaks today** because `HealthPotion.asset`'s `id` field is `health_potion_01`, not `HealthPotion` — would require renaming both existing assets and enforcing filename==id discipline forever. Given only 2 items exist and Progression/Consumable growth is expected to stay small, `LoadAll`'s O(n) scan is irrelevant in practice. |
| `Resources`-based resolution (D-01, locked) | `ItemDatabase` ScriptableObject registry | Explicitly rejected in CONTEXT.md — risk of forgetting to register new items outweighs Resources' minor load-time/memory cost at this item count. Not re-researched per locked decision. |
| Explicit `MigrateFromV2()` version-branch function | The chosen null-guard-only approach (D-04) | A version-branch function would be needed only if old saves ever had *non-empty* `Items` arrays under the old schema — verified false for this project (see Common Pitfalls). Building it anyway would be pure unused code (over-engineering, against CLAUDE.md scope discipline). |

## Architecture Patterns

### Recommended Project Structure (no new folders except one)
```
Assets/
├── Item/
│   └── Script/
│       └── ItemData.cs           # unchanged (0 lines per prior phase convention of not touching stable dependencies)
├── Resources/
│   └── Items/                    # NEW — move (not duplicate) HealthPotion.asset, AncientKey.asset here
│       ├── HealthPotion.asset
│       └── AncientKey.asset
├── SaveSystem/
│   └── Script/
│       ├── SaveData.cs           # Items: List<string> -> List<ItemSaveEntry>; SaveVersion 2 -> 3; add ItemSaveEntry class
│       └── SaveLoadManager.cs    # add CaptureInventoryItems(), ApplyInventoryFromSave(), ResolveItemData(string), EnsureCollections() guard, ContextMenu debug hooks
```

**Important:** moving assets out of `Assets/Item/` to `Assets/Resources/Items/` changes their asset path but **not their GUID** (Unity tracks assets by `.meta` GUID, not path) — this move is a folder relocation, not a re-creation, and existing references/GUIDs used elsewhere remain valid. Use a plain file move (or Unity Editor drag) rather than delete+recreate, so the `.meta` file (and its GUID) travels with the asset.

### Pattern 1: Capture/Restore Method Pair Mirrors Existing PlayerStats Pattern

**What:** Add a private `CaptureInventoryItems()` called unconditionally from `Save()`, right alongside `CapturePlayerStats()`. Add a private `ApplyInventoryFromSave()` called from `LoadSceneAndRestoreRoutine()`, right after `ApplyPlayerStatsFromSave()`.

**When to use:** Any time a new player-owned subsystem needs save/load — this is now the project's established shape for such additions (2nd instance after PlayerStats in Phase 11).

**Example (based on directly-read `SaveLoadManager.cs` lines 137-152, 344-354, 419-456):**
```csharp
// In Save(), alongside the existing call:
public void Save()
{
    CapturePlayerStats();
    CaptureInventoryItems();
    string json = JsonConvert.SerializeObject(_data, JsonSettings);
    File.WriteAllText(SavePath, json);
    // ... unchanged AutoSaveTimer.NotifySaveWritten() etc.
}

private void CaptureInventoryItems()
{
    Inventory inv = FindAnyObjectByType<Inventory>(); // same "hunt for the scene instance" style as PlayerStats.Instance, adapted since Inventory has no singleton
    if (inv == null)
    {
        Debug.LogWarning("[SaveLoadManager] Inventory not found in scene - items not captured.");
        return; // D-03: do NOT overwrite _data.Items with an empty list
    }

    var entries = new List<ItemSaveEntry>();
    for (int i = 0; i < inv.SlotCountTotal; i++)
    {
        InventorySlot slot = inv.GetSlot(i);
        if (slot == null || slot.IsEmpty) continue;
        entries.Add(new ItemSaveEntry { itemId = slot.item.Id, count = slot.count });
    }
    _data.Items = entries;
}
```

```csharp
// In LoadSceneAndRestoreRoutine, right after ApplyPlayerStatsFromSave():
ApplyPlayerStatsFromSave();
ApplyInventoryFromSave();

private void ApplyInventoryFromSave()
{
    Inventory inv = FindAnyObjectByType<Inventory>();
    if (inv == null)
    {
        Debug.LogWarning("[SaveLoadManager] Inventory not found after scene load - items not restored.");
        return;
    }
    foreach (var entry in _data.Items)
    {
        ItemData item = ResolveItemData(entry.itemId);
        if (item == null)
        {
            Debug.LogWarning("[SaveLoadManager] Unknown itemId '" + entry.itemId + "' - skipped.");
            continue;
        }
        inv.TryAddItem(item, entry.count);
    }
}
```

**Note on `FindAnyObjectByType<Inventory>()`:** `Inventory` has no `Instance` singleton (unlike `PlayerStats`), confirmed by reading `Inventory.cs` in full — there is no static field. `FindAnyObjectByType<T>()` is the Unity 6 replacement for the obsolete `FindObjectOfType<T>()` and is the correct call to locate the scene's single Inventory component without a singleton. This matches the project's own precedent: `ItemData.cs`'s existing `UseOnPlayerFromInspector()` debug hook already uses `FindAnyObjectByType<PlayerInteraction>()`.

### Pattern 2: Resources.LoadAll + Id-Field Matching (resolves D-01 discretion)

**What:** Cache `Resources.LoadAll<ItemData>("Items")` results once, then look up by `Id` on each resolve call, rather than trusting filename to equal `id`.

**When to use:** Every `itemId` string → `ItemData` resolution in `ApplyInventoryFromSave()`.

**Why this over filename-based `Resources.Load`:** Verified directly against the two existing assets — `HealthPotion.asset` has `m_Name: HealthPotion` but `id: health_potion_01`; `AncientKey.asset` has `m_Name: AncientKey` but `id: ancient_key_01`. Filename-based `Resources.Load<ItemData>("Items/" + id)` would look for an asset literally named `health_potion_01.asset`, which does not exist and never will unless someone remembers to rename it — reintroducing exactly the "forgot to register" risk D-01 was trying to eliminate by rejecting a manual registry SO. `LoadAll` + `Id` match has no such naming coupling.

**Example:**
```csharp
// In SaveLoadManager.cs, add a small resolver — cached because Resources.LoadAll
// re-scans the Resources folder on every call otherwise.
private Dictionary<string, ItemData> _itemLookupCache;

private ItemData ResolveItemData(string itemId)
{
    if (_itemLookupCache == null)
    {
        _itemLookupCache = new Dictionary<string, ItemData>();
        foreach (var item in Resources.LoadAll<ItemData>("Items"))
        {
            _itemLookupCache[item.Id] = item;
        }
    }
    ItemData found;
    return _itemLookupCache.TryGetValue(itemId, out found) ? found : null;
}
```

### Anti-Patterns to Avoid
- **Filename-based `Resources.Load<ItemData>("Items/" + id)`:** Breaks today against the project's actual two assets (see Pattern 2). Do not use.
- **Writing a `MigrateFromV2()` version-branch function:** Unnecessary per D-04 and per the verified fact that `Items` has never held real data — this would be speculative code with no reachable old-format input, against CLAUDE.md's "don't add unrequested flexibility" rule.
- **Restoring inventory before `yield return op` completes:** `Inventory.Awake()` runs during scene activation and unconditionally clears all slots — any restore attempted before the scene is fully loaded and `Awake()` has run will be wiped out. Always restore after the `yield return op` line, same reasoning as the existing `PlayerStats` restore.
- **Re-scanning `Resources.LoadAll` on every single item resolve:** wasteful; cache once per `SaveLoadManager` instance (it's a `DontDestroyOnLoad` singleton, so the cache is safe to keep for the whole play session — but see Common Pitfalls for the one edge case where invalidation matters).

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| itemId → ItemData resolution | Custom `Dictionary<string, ItemData>` populated via `[CreateAssetMenu]`-time manual registration, or a hand-rolled registry ScriptableObject | `Resources.LoadAll<ItemData>("Items")` (built-in Unity API) | D-01 already rejected the registry-SO approach; `Resources` is Unity's own built-in mechanism for exactly this — no reason to build a parallel one |
| Save schema versioning | A generic version-dispatch/migration framework | A single `if (_data.Items == null)` null-guard in `EnsureCollections()` | The actual migration need here is zero — building a framework for a problem that doesn't exist yet is speculative complexity (CLAUDE.md: 오버엔지니어링 금지) |
| JSON serialization of the new type | Custom `JsonConverter<ItemSaveEntry>` | Default Newtonsoft.Json POCO serialization (plain public fields, same as `PlayerStatsSaveData`) | `ItemSaveEntry` is a flat two-field POCO; Newtonsoft handles this with zero configuration, exactly like the existing `PlayerStatsSaveData` |

**Key insight:** Every part of this phase already has a locked, superior alternative in either the CONTEXT.md decisions or Unity's own built-in APIs. There is no legitimate reason to write custom infrastructure anywhere in this phase.

## Common Pitfalls

### Pitfall 1: Assuming `List<string>` → `List<ItemSaveEntry>` deserialization needs a migration path
**What goes wrong:** Fear that changing the C# field type while old JSON files exist on disk will throw a `JsonSerializationException` on load.
**Why it happens:** It's true in general that changing a field's type is a breaking JSON schema change — Newtonsoft.Json can throw `JsonSerializationException: Error converting value "..." to type '...'` when a JSON array contains elements it cannot convert to the new element type.
**How to avoid / actual verified behavior:** The failure mode only occurs when the array is **non-empty** and its elements don't structurally match the new type. Verified directly: `SaveData.cs`'s `Items` field has been `new List<string>()` since Phase 11 and — per the D-03b comment on line 32 ("스텁. Empty list - no item/inventory system exists in the project yet") and confirmed by the fact Phase 17/18 only just introduced `ItemData`/`Inventory` — no code path in the shipped project has ever added an element to it. Every real save file on disk therefore serializes `"Items": []`. Deserializing an empty JSON array into `List<ItemSaveEntry>` performs zero per-element conversions (there are no elements), so it succeeds trivially and produces an empty `List<ItemSaveEntry>`, regardless of what the old element type was. **This is standard Json.NET array-deserialization behavior** — the converter only inspects tokens that exist inside the array; an empty array short-circuits before any element-type resolution happens.
**For completeness — the hypothetical failure case:** IF an old save somehow had `"Items": ["abc", "def"]` (non-empty string array), deserializing that into `List<ItemSaveEntry>` (a complex object type, not a primitive) WOULD throw `JsonSerializationException` at the first element, because Newtonsoft cannot implicitly convert a JSON string token into an object with `itemId`/`count` fields. This is the theoretical risk D-04 correctly identifies as *not applicable here* — it would only matter if such a file existed, and it doesn't.
**Warning signs:** If manual/hand-edited save files ever contain non-empty `Items` string arrays (e.g. a QA tester manually edited JSON), loading would throw inside `LoadGame()`'s existing `try/catch (System.Exception e)` block and correctly route to `AbortLoadToMainMenu` — the existing error handling already covers this edge case with no code changes needed.

### Pitfall 2: Filename/Id mismatch silently breaking item resolution
**What goes wrong:** Using `Resources.Load<ItemData>("Items/" + id)` assumes the asset filename equals `ItemData.Id`. If they diverge (as they already do for both existing assets), the load silently returns `null` with no exception — Unity's `Resources.Load` returns `null` for a missing path rather than throwing.
**Why it happens:** `id` is a hand-typed `[SerializeField]` string (per `ItemData.cs` D-05 comment) independent of the asset's filename; nothing in the project enforces they match.
**How to avoid:** Use `Resources.LoadAll<ItemData>("Items")` + `Id`-field matching (Pattern 2) instead of path-based `Resources.Load`. This makes filename irrelevant.
**Warning signs:** `ApplyInventoryFromSave()` logging "Unknown itemId" warnings for items that were definitely saved — check whether the resolver is filename-based before assuming a data corruption issue.

### Pitfall 3: Inventory cache staleness across scene loads
**What goes wrong:** If `Resources.LoadAll` result were cached as a `static` field or cached once and item assets were ever hot-reloaded/changed at runtime (not applicable in a build, but can matter in Editor Play mode iteration), a stale cache could miss newly added items.
**Why it happens:** `Resources.LoadAll` scans the actual folder contents at call time; caching trades a full rescan for staleness risk.
**How to avoid:** Cache at the instance level (not `static`) on `SaveLoadManager`, which is itself re-created fresh each Editor Play session via the `RuntimeInitializeOnLoadMethod` bootstrap — this naturally invalidates the cache every Play mode entry, which is the only time asset changes would matter during development. No further invalidation logic needed.
**Warning signs:** N/A expected in practice given the bootstrap lifecycle already handles this.

### Pitfall 4: Calling `TryAddItem` in the wrong scene-load frame
**What goes wrong:** Calling `ApplyInventoryFromSave()` before `yield return op` (i.e., before the scene is loaded) means `FindAnyObjectByType<Inventory>()` finds nothing (previous scene's Inventory, if any, already destroyed) or finds the previous scene's stale Inventory.
**Why it happens:** `LoadSceneAndRestoreRoutine` is a coroutine; code executed before `yield return op` runs synchronously in the *old* scene's context, before `SceneManager.LoadSceneAsync` has swapped scenes.
**How to avoid:** Only call `ApplyInventoryFromSave()` after `yield return op` — same placement as the existing `ApplyPlayerStatsFromSave()` call at line 440, right where its own comment already explains this exact ordering requirement for `HP.Awake()`.
**Warning signs:** Items always fail to restore, or console shows "Inventory not found after scene load" warnings.

## Code Examples

### ItemSaveEntry class (co-located in SaveData.cs, following PlayerStatsSaveData precedent)
```csharp
// Source: pattern taken directly from PlayerStatsSaveData in the same file (SaveData.cs lines 36-40)
public class ItemSaveEntry
{
    public string itemId = "";
    public int count;
}
```

### SaveData.cs field change
```csharp
// Was:
// public int SaveVersion = 2;
// ...
// public List<string> Items = new List<string>();

// Becomes:
public int SaveVersion = 3;
// ...
public List<ItemSaveEntry> Items = new List<ItemSaveEntry>();
```

### EnsureCollections() guard addition
```csharp
private void EnsureCollections()
{
    if (_data.PlayerStats == null) _data.PlayerStats = new PlayerStatsSaveData();
    if (_data.BossProgress == null) _data.BossProgress = new Dictionary<string, bool>();
    if (_data.MapGimmickState == null) _data.MapGimmickState = new Dictionary<string, bool>();
    if (_data.Items == null) _data.Items = new List<ItemSaveEntry>(); // was List<string>
}
```

## State of the Art

Not applicable — this is a project-internal integration phase with no external ecosystem to track. Newtonsoft.Json's default `List<T>` deserialization behavior described above is stable, documented core behavior, not a recently-changed feature (unchanged across all Json.NET versions relevant to Unity's package distribution).

## Open Questions

1. **Should the two existing `.asset` files be moved or copied into `Assets/Resources/Items/`?**
   - What we know: D-01 says "move (or copy)". A move is cleaner (no duplicate assets, no risk of the two copies drifting) and preserves GUID via the accompanying `.meta` file.
   - What's unclear: Whether any other scene/prefab currently references `Assets/Item/HealthPotion.asset` or `AncientKey.asset` by direct Inspector reference (which would still resolve correctly after a move, since Unity references by GUID not path) — not verified by grepping scene/prefab YAML in this research pass.
   - Recommendation: Move (not copy) the two assets. Before moving, planner should grep `.unity`/`.prefab` files for the two GUIDs to confirm no path-based (not GUID-based) reference exists; Unity's standard reference serialization is GUID-based so this is expected to be a non-issue, but a quick grep costs nothing and removes any doubt.

## Environment Availability

Not applicable — this phase has no external tool/service/runtime dependencies beyond the already-installed Unity Editor (6000.3.10f1, confirmed via Phase 17 ROADMAP success criteria) and the already-installed Newtonsoft.Json package. No new dependency is introduced.

## Sources

### Primary (HIGH confidence — direct code/asset inspection, this session)
- `Assets/SaveSystem/Script/SaveData.cs` — current schema, `SaveVersion`, `Items` stub comment
- `Assets/SaveSystem/Script/SaveLoadManager.cs` — full file read: `Save()`, `LoadGame()`, `EnsureCollections()`, `CapturePlayerStats()`, `ApplyPlayerStatsFromSave()`, `LoadSceneAndRestoreRoutine()`, `JsonSettings` config
- `Assets/Player/Script/Inventory.cs` — full file read: `Awake()` slot-reset behavior, `SlotCountTotal`, `GetSlot`, `TryAddItem`, no singleton `Instance`
- `Assets/Item/Script/ItemData.cs` — full file read: `Id` property, `FindAnyObjectByType` precedent in debug hook
- `Assets/Item/HealthPotion.asset`, `Assets/Item/AncientKey.asset` — raw YAML inspection confirming `m_Name` vs `id` field divergence (the key empirical finding resolving D-01's discretion point)
- `.planning/phases/19-.../19-CONTEXT.md` — locked decisions D-01 through D-04, discretion items, canonical refs
- `.planning/ROADMAP.md` — Phase 17 success criteria (confirms Unity 6000.3.10f1, confirms `.asset` files and Id field origin), Phase 19 entry (confirms no formal REQ-IDs yet)
- `.planning/config.json` — confirms `workflow.nyquist_validation: false`, so Validation Architecture section is correctly omitted

### Secondary (MEDIUM confidence)
- Newtonsoft.Json empty-array-to-differently-typed-List<T> deserialization behavior — not independently re-verified via Context7/live docs this session (no network doc fetch performed), but this is well-established, version-stable Json.NET behavior consistent across all versions used by the Unity Newtonsoft.Json package, and is directly consistent with the observable fact that `JsonSettings` in this project has no custom converters or `TypeNameHandling` that would change this behavior.

### Tertiary (LOW confidence)
- None used.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no new libraries, all reuse of already-verified project code
- Architecture: HIGH — patterns extracted directly from existing, working code (`CapturePlayerStats`/`ApplyPlayerStatsFromSave`) plus a hard empirical finding (filename≠Id) that resolves the one open discretion point unambiguously
- Pitfalls: HIGH for the Newtonsoft empty-array claim (verified against actual JsonSettings config and actual historical stub state) and the Awake-ordering claim (verified by reading Inventory.Awake() and the existing PlayerStats-restore-ordering comment); MEDIUM for the general Json.NET empty-array behavior claim since it relies on documented framework behavior rather than a live doc fetch this session

**Research date:** 2026-09-21
**Valid until:** No expiry concern — findings are pinned to this project's actual code state at commit time, not to an external, moving ecosystem. Re-verify only if `SaveData.cs`, `SaveLoadManager.cs`, `Inventory.cs`, `ItemData.cs`, or the two `.asset` files change before planning begins.
