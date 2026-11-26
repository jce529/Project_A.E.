---
phase: 19-savedata-items-list-itemsaveentry-itemid-count-saveversion-saveloadmanager-depends-on-phase-18
plan: 01
subsystem: save-data
tags: [unity, scriptableobject, resources, newtonsoft-json, save-schema]

requires:
  - phase: 18-inventory
    provides: Inventory runtime model consumed by the next Phase 19 plan
provides:
  - Runtime-discoverable ItemData assets under Resources/Items
  - ItemSaveEntry itemId/count save-schema POCO
  - SaveData.Items typed as List<ItemSaveEntry>
  - Save schema version 3
affects: [19-02-saveloadmanager-item-capture-restore]

tech-stack:
  added: []
  patterns:
    - Resources.LoadAll<ItemData>("Items") assets keyed by ItemData.Id
    - Flat Newtonsoft.Json POCO entries for inventory persistence

key-files:
  created:
    - Assets/Resources/Items/HealthPotion.asset
    - Assets/Resources/Items/AncientKey.asset
  modified:
    - Assets/SaveSystem/Script/SaveData.cs

key-decisions:
  - Preserve both ItemData GUIDs through pure git renames with zero content changes.
  - Persist item identity through ItemData.Id rather than asset filenames.
  - Defer the intentionally stale EnsureCollections List<string> assignment to Plan 19-02.

patterns-established:
  - ItemSaveEntry contains only itemId and count; slot positions are not persisted.
  - Item save schema uses default Newtonsoft.Json public-field serialization.

requirements-completed: [D-01, D-02, D-04]

duration: 6min
completed: 2026-09-21
---

# Phase 19 Plan 01: Runtime Item Assets and Save Schema Summary

**Two ItemData assets are runtime-discoverable with preserved GUIDs, while SaveData version 3 stores itemId/count entries.**

## Performance

- **Duration:** 6 min
- **Started:** 2026-09-21T10:58:00Z
- **Completed:** 2026-09-21T11:04:04Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments

- Relocated HealthPotion and AncientKey assets and their meta files into `Assets/Resources/Items/` as 100% git renames.
- Preserved GUIDs `1ca8a1cec6cae3595e071afccaf8623e` and `65cc29b1f44b856171d66dbe8ddec240` with zero asset or meta content changes.
- Added `ItemSaveEntry` with exactly `itemId` and `count`, retyped `SaveData.Items`, and bumped `SaveVersion` to 3.

## Task Commits

Each task was committed atomically:

1. **Task 1: Relocate ItemData assets for runtime loading** - `da2d046` (feat)
2. **Task 2: Add ItemSaveEntry and evolve SaveData.Items** - `8db5e1f` (feat)

**Plan metadata:** Not committed because the execution request explicitly required exactly two task commits and restricted each commit to its listed pathspec.

## Files Created/Modified

- `Assets/Resources/Items/HealthPotion.asset` - Runtime-resolvable heal consumable with `id: health_potion_01`.
- `Assets/Resources/Items/HealthPotion.asset.meta` - Preserved original HealthPotion GUID.
- `Assets/Resources/Items/AncientKey.asset` - Runtime-resolvable progression item with `id: ancient_key_01`.
- `Assets/Resources/Items/AncientKey.asset.meta` - Preserved original AncientKey GUID.
- `Assets/SaveSystem/Script/SaveData.cs` - SaveVersion 3, typed Items list, and ItemSaveEntry POCO.

## Decisions Made

- Followed the plan's `Resources.LoadAll<ItemData>("Items")` discovery path; asset filenames remain unchanged and saved keys use `ItemData.Id`.
- Added no migration method because all existing saves serialize the previous Items field as an empty array.
- Did not modify `SaveLoadManager.EnsureCollections()`; its planned type mismatch remains for Plan 19-02.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Reconciled contradictory comment and grep requirements**
- **Found during:** Task 2 acceptance verification
- **Issue:** The prescribed comment contained the literal token `JsonConverter`, while an acceptance command required zero occurrences of `JsonConverter` in the file.
- **Fix:** Used the semantically equivalent wording `JSON converter` in that comment so the required grep returns zero without changing code behavior.
- **Files modified:** `Assets/SaveSystem/Script/SaveData.cs`
- **Verification:** Forbidden-token count is zero; ItemSaveEntry remains a plain two-field POCO.
- **Committed in:** `8db5e1f`

---

**Total deviations:** 1 auto-fixed blocking verification contradiction.
**Impact on plan:** Comment wording only; implementation and scope are unchanged.

## Issues Encountered

- `dotnet build "Projeect_A.E.sln" --nologo -v quiet` failed with the single expected CS0029 error at `SaveLoadManager.cs:415`, where `EnsureCollections()` still assigns `new List<string>()`. No unrelated compile errors were reported; six pre-existing warnings remain.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 19-02 can update `EnsureCollections()`, add inventory capture/restore, and resolve saved IDs through `Resources.LoadAll<ItemData>("Items")`.
- The project intentionally remains uncompilable until the Plan 19-02 type update lands.

## Self-Check: PASSED

- Task 1 paths, GUIDs, IDs, rename similarity, and zero-content-change checks passed.
- Task 2 schema, forbidden-member, using-count, ASCII-only, and repository-wide stale Items declaration checks passed.
- Build produced only the explicitly allowed `EnsureCollections()` type error.

---
*Phase: 19-savedata-items-list-itemsaveentry-itemid-count-saveversion-saveloadmanager-depends-on-phase-18*
*Completed: 2026-09-21*
