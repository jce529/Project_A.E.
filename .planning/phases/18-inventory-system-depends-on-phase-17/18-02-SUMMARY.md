---
phase: 18-inventory-system-depends-on-phase-17
plan: 02
subsystem: inventory
tags: [unity, csharp, inventory, interaction, prefab]

requires:
  - phase: 18-inventory-system-depends-on-phase-17
    plan: 01
    provides: Fixed-slot Inventory and atomic TryAddItem API
provides:
  - IPlayerInteractable world pickup that destroys itself only after a successful inventory add
  - Player prefab wiring for PlayerInteraction and Inventory
affects: [phase-18-play-mode-verification, phase-19-save-load]

tech-stack:
  added: []
  patterns: [existing nearest-interactable dispatch, success-gated world object destruction, additive Unity prefab YAML wiring]

key-files:
  created:
    - Assets/Item/Script/WorldItem.cs
    - Assets/Item/Script/WorldItem.cs.meta
  modified:
    - Assets/Player.prefab

key-decisions:
  - "WorldItem relies entirely on PlayerInteraction's existing nearest-target dispatch and adds no trigger-based pickup path."
  - "WorldItem destruction is gated on Inventory.TryAddItem returning true so failed full-inventory pickups preserve the world object."
  - "PlayerInteraction was wired alongside Inventory because it existed in code but was not attached to any prefab or scene."

patterns-established:
  - "World pickups implement IPlayerInteractable and delegate capacity handling to Inventory.TryAddItem."
  - "Prefab component wiring is performed as a pure insertion with fixed script GUIDs and stable fileIDs."

requirements-completed: [D-03, D-05, D-06, D-07, D-08]

duration: 7min
completed: 2026-09-21
---

# Phase 18 Plan 02: World Item Pickup and Player Wiring Summary

**Interact-key world pickup backed by atomic inventory insertion, with PlayerInteraction and Inventory wired onto the Player prefab**

## Performance

- **Duration:** 7 min
- **Started:** 2026-09-21T18:21:00+09:00
- **Completed:** 2026-09-21T18:27:22+09:00
- **Tasks:** 2 of 3 completed
- **Files modified:** 3 implementation files; this summary added separately

## Accomplishments

- Added `WorldItem`, an `IPlayerInteractable` pickup that is targetable through the existing `PlayerInteraction.FindNearest` path and has no automatic trigger/collision pickup code.
- Gated `Destroy(gameObject)` on `Inventory.TryAddItem(item, count)` success, preserving the world object when inventory capacity is insufficient.
- Added both `PlayerInteraction` and `Inventory` to the Player GameObject in `Assets/Player.prefab` using a 31-line, insertion-only YAML diff.
- Resolved the pre-existing integration gap where `PlayerInteraction.cs` existed but its script GUID was not wired into any prefab or scene, leaving no usable D-05 interaction path.

## Task Commits

Each requested implementation task was committed atomically:

1. **Task 1: WorldItem.cs — IPlayerInteractable world pickup** - `1f91ae3` (feat)
2. **Task 2: Wire Inventory + PlayerInteraction on Player.prefab** - `93709cb` (feat)

This summary is intentionally not staged or committed because the execution request limited staging and commits to the two task-specific pathspecs and required exactly two commits.

## Files Created/Modified

- `Assets/Item/Script/WorldItem.cs` - Implements interact-key pickup and success-gated destruction.
- `Assets/Item/Script/WorldItem.cs.meta` - Unity MonoImporter metadata with fixed GUID `18b2d5f8c4e15e3bca07f6e8a1b2c3d4`.
- `Assets/Player.prefab` - Adds PlayerInteraction and Inventory component references and serialized MonoBehaviour blocks.

## Verification Results

- Task 1 acceptance checks: PASS (class/interface declaration, one `TryAddItem` gate, gated `Destroy`, zero trigger/collision pickup methods, two serialized fields, fixed meta GUID).
- Task 1 compile gate: PASS (`dotnet build "Projeect_A.E.sln" --nologo -v quiet`, 0 errors, 6 pre-existing warnings).
- Task 2 acceptance checks: PASS (`31` additions, `0` deletions; both GUIDs once; both fileIDs twice; `17` total component references; both editor class identifiers once; 0 non-ASCII bytes).
- Task 2 compile gate: PASS (`dotnet build "Projeect_A.E.sln" --nologo -v quiet`, 0 errors, 6 pre-existing warnings).
- Scope check: PASS (`ItemData.cs`, `IItem.cs`, `PlayerInteraction.cs`, `IPlayerInteractable.cs`, and `Checkpoint.cs` unchanged).
- `Assets/Item/Check.md`: unchanged.

## Decisions Made

None beyond the plan. The plan-specified fixed GUIDs, fileIDs, serialized values, and component order were used exactly.

## Deviations from Plan

None - Tasks 1 and 2 were executed exactly as written.

## Issues Encountered

- The sandbox initially denied `.git/index.lock` creation. The same explicitly scoped `git add` and `git commit` commands were rerun with approved repository write access.
- `git diff --check` reports the two plan-required `m_Name: ` lines as trailing whitespace. The single space after the colon was preserved because the plan explicitly requires it to match the existing Unity YAML block format.
- Both builds retained six pre-existing warnings in unrelated enemy and camera scripts; neither task introduced a build error.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Tasks 1 and 2 are implemented and committed.
- Task 3 (Play mode measurement for D-05/D-06/D-07/D-08) must be performed directly by the user and was explicitly excluded from this execution scope.
- Unity Editor was not opened, no Play mode result is claimed, and `Assets/Item/Check.md` was not modified. The phase must remain pending until those runtime checks are recorded.

## Self-Check: PASSED FOR REQUESTED SCOPE

- Both implementation commits exist and contain only their allowed pathspecs.
- All static acceptance criteria for Tasks 1 and 2 pass.
- Task 3 remains intentionally incomplete and unreported as PASS.

---
*Phase: 18-inventory-system-depends-on-phase-17*
*Completed: 2026-09-21 (Tasks 1-2 only)*
