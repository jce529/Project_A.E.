---
phase: 19-savedata-items-list-itemsaveentry-itemid-count-saveversion-saveloadmanager-depends-on-phase-18
plan: 02
subsystem: save-load
tags: [unity, inventory, newtonsoft-json, resources, save-load]

requires:
  - phase: 19-01-runtime-item-assets-and-save-schema
    provides: SaveData version 3, ItemSaveEntry, and Resources/Items assets
provides:
  - Inventory capture through the shared Save() path
  - Post-scene-load inventory restoration through Inventory.TryAddItem
  - Cached ItemData resolution by ItemData.Id
  - Phase 19 Play-mode verification checklist
affects: [phase-19-play-mode-verification, inventory-persistence]

tech-stack:
  added: []
  patterns:
    - Preserve saved inventory data when the active scene has no Inventory
    - Resolve persisted item IDs through cached Resources.LoadAll and ItemData.Id matching

key-files:
  created: []
  modified:
    - Assets/SaveSystem/Script/SaveLoadManager.cs
    - Assets/SaveSystem/Check.md

key-decisions:
  - Capture inventory exactly once in Save(), immediately after player stats capture.
  - Restore inventory only after the scene-load yield and after player stats restoration.
  - Treat missing inventory, unknown item IDs, and insufficient capacity as warnings rather than fatal load failures.

patterns-established:
  - Inventory persistence stores itemId/count entries without slot indices.
  - Main-menu saves skip inventory capture so existing item entries are retained.

requirements-completed: [D-01, D-02, D-03, D-04]

duration: 5min
completed: 2026-09-21
---

# Phase 19 Plan 02: Inventory Save/Load Wiring Summary

**Inventory entries now flow through the existing save/load pipeline using stable item IDs, with non-fatal handling for missing or unknown items.**

## Performance

- **Duration:** 5 min
- **Started:** 2026-09-21T11:04:00Z
- **Completed:** 2026-09-21T11:09:00Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments

- Added inventory capture to the shared `Save()` path while preserving existing saved items when no scene inventory exists.
- Added cached `ItemData.Id` resolution and post-scene-load restoration through `Inventory.TryAddItem`.
- Added warning-only handling for unknown IDs, missing inventory instances, and full inventory capacity.
- Added two `Phase19/` diagnostic ContextMenu hooks and the six-item Phase 19 Play-mode checklist.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add the item resolver, inventory capture, and fix EnsureCollections** - `112bae4` (feat)
2. **Task 2: Add the load-time restore hook, Phase19 debug hooks, and checklist** - `03ef9fd` (feat)

**Plan metadata:** Not committed because the execution request explicitly required exactly two task commits and restricted each commit to its listed pathspec.

## Files Created/Modified

- `Assets/SaveSystem/Script/SaveLoadManager.cs` - Captures inventory items, resolves item assets by ID, restores items after scene activation, and exposes Phase 19 diagnostics.
- `Assets/SaveSystem/Check.md` - Adds the six unchecked Play-mode verification steps for Phase 19.

## Decisions Made

- Followed the plan exactly: no new migration dispatcher, registry asset, filename-based item load, or inventory singleton was introduced.
- Kept item restoration separate from fatal player-stat restoration; inventory-related failures only warn and continue.
- Left all checklist items unchecked because no Play-mode observations were performed in this execution.

## Deviations from Plan

None - Tasks 1 and 2 were executed exactly as written.

## Issues Encountered

- The first Git index write was blocked by workspace sandbox permissions; the same scoped stage/commit operation succeeded after approval.
- Both required builds completed with 0 errors and the same 6 pre-existing warnings outside the modified files.

## Verification

- Task 1 acceptance checks passed: single adjacent capture call, typed collection guard, cached `Resources.LoadAll<ItemData>("Items")` resolver, warning-only missing-inventory guard, and ASCII-only C# source.
- Task 2 acceptance checks passed: single restore call after `yield return op`, `TryAddItem` replay, no `AbortLoadToMainMenu` in item restoration, exactly two Phase 19 hooks, and exactly six new unchecked checklist boxes.
- Repository regression searches found no `List<string>` item schema/guard, filename-based `Resources.Load<ItemData>`, or `MigrateFromV2` under `Assets/`.
- `Inventory.cs` and `ItemData.cs` have zero changed lines.
- `dotnet build "Projeect_A.E.sln" --nologo -v quiet` passed before each task commit with 0 errors and 6 pre-existing warnings.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Task 3 (Play 모드 실측 검증)은 사용자가 직접 수행해야 하므로 이번 실행 범위에서 제외했다.
- Unity Editor was not opened, and no Phase 19 checklist item was marked PASS or FAIL.
- The implementation is statically verified and ready for the prescribed save, restart, load, unknown-ID, and no-Inventory-scene Play-mode checks.

## Self-Check: PASSED

- Both requested task commits exist and contain only their allowed pathspecs.
- Production changes are committed; the requested summary remains uncommitted to preserve the exactly-two-commits constraint.

---
*Phase: 19-savedata-items-list-itemsaveentry-itemid-count-saveversion-saveloadmanager-depends-on-phase-18*
*Completed: 2026-09-21*
