---
phase: 15-load-timing-and-load-scope
plan: 02
subsystem: save-load
tags: [unity, save-load, failure-recovery, scene-management]

requires:
  - phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json
    provides: SaveLoadManager coroutine load flow and post-scene player-stat restoration
  - phase: 14-save-slot-expansion
    provides: active-slot save path selection and main-menu slot loading
provides:
  - All five actionable load failures return the player to MainMenu
  - Corrupt or malformed save files remain untouched for manual recovery
  - MainMenu scene naming is centralized in SaveLoadManager
affects: [15-04-verification, save-load, main-menu, player-restoration]

tech-stack:
  added: []
  patterns: [centralized load-failure funnel, non-destructive save recovery]

key-files:
  created: []
  modified:
    - Assets/SaveSystem/Script/SaveLoadManager.cs

key-decisions:
  - "Every actionable load failure logs its reason and synchronously returns to the centralized MainMenu scene."
  - "A failed load never writes or deletes save data; the existing file remains available for manual recovery."
  - "The no-save guard remains a warning and return because absence of a save is a valid new-game/death-restart condition."

patterns-established:
  - "Load failure funnel: call AbortLoadToMainMenu(reason), then terminate the current method or coroutine."
  - "Recovery isolation: load-failure handling owns no file-write or file-delete operations."

requirements-completed: [D-04, D-05]

duration: 17min
completed: 2026-09-10
---

# Phase 15 Plan 02: Non-destructive Load Failure Recovery Summary

**Save loading now funnels malformed data, invalid scene targets, failed scene activation, and missing restored player state back to MainMenu without modifying the save file.**

## Performance

- **Duration:** 17 min
- **Started:** 2026-09-10T14:13:00Z
- **Completed:** 2026-09-10T14:30:11Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments

- Added the single `MainMenuSceneName` constant and `AbortLoadToMainMenu(reason)` recovery funnel.
- Routed all five actionable synchronous and coroutine load failures through the recovery funnel while retaining the valid no-save guard.
- Preserved the save file exactly as-is on failure: no delete, write, or `Save()` call was added to recovery handling.

## Task Commits

Each task was committed atomically:

1. **Task 1: Route synchronous load failures to MainMenu** - `1639e4b` (fix)
2. **Task 2: Route coroutine and post-load failures to MainMenu** - `40d0b37` (fix)

## Files Created/Modified

- `Assets/SaveSystem/Script/SaveLoadManager.cs` - Centralizes the MainMenu scene name and routes five load failure branches through non-destructive recovery.

## Decisions Made

- Kept `if (!HasSaveFile())` unchanged because a missing save is an expected control-flow condition handled by the new-game/death restart path, not a broken-load recovery case.
- Kept the existing `PlayerSpawner.targetSpawnPointName` assignment before asynchronous scene loading and retained the successful restore log.
- Preserved the independent `SaveSettings()` disk write; load recovery adds no file mutation.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Corrected an outdated `File.WriteAllText` acceptance baseline**

- **Found during:** Task 1 (synchronous load failure recovery)
- **Issue:** The plan expected one `File.WriteAllText` occurrence in the full file, but the pre-existing code already has two legitimate writes: `Save()` for gameplay data and `SaveSettings()` for settings data.
- **Fix:** Preserved both established write paths and verified that `AbortLoadToMainMenu` and all five failure branches contain no write, delete, or `Save()` operation.
- **Files modified:** None beyond the planned `Assets/SaveSystem/Script/SaveLoadManager.cs` implementation.
- **Verification:** Full-file count remains 2, `File.Delete` count is 0, both task diffs add no file mutation, and the solution builds with zero errors.
- **Committed in:** `1639e4b` (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking plan inconsistency).
**Impact on plan:** No runtime or architectural scope change; established settings persistence remains intact while D-05 is satisfied.

## Issues Encountered

- `apply_patch` introduced mixed line endings in the ASCII source; the file was mechanically normalized to UTF-8 without BOM and CRLF after implementation. Byte checks report zero non-ASCII bytes and zero bare LF characters.
- The solution reports six pre-existing compiler warnings in unrelated files; compilation completes with zero errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 15-02 is ready for Plan 15-03 boss-progress restoration and Plan 15-04 consolidated verification.
- No blockers were introduced.

## Self-Check: PASSED

- Verified `Assets/SaveSystem/Script/SaveLoadManager.cs` exists and remains ASCII UTF-8 without BOM using CRLF line endings.
- Verified task commits `1639e4b` and `40d0b37` exist and each modifies only the declared implementation file.
- Re-ran all feasible task acceptance checks and plan-level checks; the documented outdated write-count assertion was replaced with a baseline-aware non-mutation check.
- Rebuilt `Projeect_A.E.sln` with zero errors.

---
*Phase: 15-load-timing-and-load-scope*
*Completed: 2026-09-10*
