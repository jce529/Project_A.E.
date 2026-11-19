---
phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json
plan: 01
subsystem: save-system
tags: [newtonsoft-json, unity-packages, save-data, poco, player-stats]

# Dependency graph
requires: []
provides:
  - Direct Newtonsoft.Json package dependency (manifest.json), immune to indirect-dependency loss
  - SaveData / PlayerStatsSaveData POCO schema (scene+spawn-point-name location, player stats, boss progress dict, map gimmick dict, item list)
  - PlayerStats.RestoreStats(float, float, float) additive external restore method
affects: [11-02 (SaveLoadManager), 11-03, 11-04]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "POCO save schema separate from MonoBehaviour, no [System.Serializable] (Newtonsoft-only, not Unity-inspector-facing)"
    - "Name-based location restore (SceneName + SpawnPointName) instead of raw coordinates"
    - "Additive-only external mutation method (RestoreStats) instead of adding setter properties"

key-files:
  created:
    - Assets/SaveSystem/Script/SaveData.cs
  modified:
    - Packages/manifest.json
    - Assets/Player/Script/PlayerStats.cs

key-decisions:
  - "Newtonsoft.Json pinned at 3.2.2 (not 3.2.1 as originally noted in 11-CONTEXT.md) per Library/PackageCache resolved version - confirmed correct per plan's explicit correction"
  - "RestoreStats() assigns maxTotalHealth -> maxHealth -> health -> ClampHealth() in that order to avoid saved health being clamped by a stale maxHealth"
  - "No setter properties added to PlayerStats; RestoreStats is the sole external write path to keep the existing damage pipeline as the only other mutator"

patterns-established:
  - "Save schema field ordering documents each field's owning decision (D-02/D-03/D-03b/D-03c/D-05) directly in code comments"

requirements-completed: [D-02, D-03, D-03b, D-03c]

# Metrics
duration: 15min
completed: 2026-08-10
---

# Phase 11 Plan 01: Newtonsoft.Json pin + SaveData schema + PlayerStats.RestoreStats Summary

**Newtonsoft.Json promoted to a direct manifest.json dependency, new SaveData/PlayerStatsSaveData POCO schema created, and PlayerStats gained an additive RestoreStats(health, maxHealth, maxTotalHealth) method for future save/load restoration.**

## Performance

- **Duration:** 15 min
- **Started:** 2026-08-10T01:28:00Z (approx.)
- **Completed:** 2026-08-10T01:43:20Z
- **Tasks:** 3
- **Files modified:** 3 (1 created, 2 modified)

## Accomplishments
- `com.unity.nuget.newtonsoft-json: 3.2.2` added as a direct dependency in `Packages/manifest.json`, removing the risk of losing Newtonsoft.Json if an AI package (its current indirect source) is later removed
- `Assets/SaveSystem/Script/SaveData.cs` created with `SaveData` (SaveVersion, SceneName, SpawnPointName, PlayerStats, BossProgress dict, MapGimmickState dict, Items list) and `PlayerStatsSaveData` (Health, MaxHealth, MaxTotalHealth) POCOs
- `PlayerStats.RestoreStats(float, float, float)` added to `Assets/Player/Script/PlayerStats.cs` as the only external write path into protected/private stat fields, correctly ordered to avoid stale-maxHealth clamping

## Task Commits

Each task was committed atomically:

1. **Task 1: manifest.json Newtonsoft.Json direct dependency** - `a1b14ed` (chore)
2. **Task 2: SaveData.cs schema POCO classes** - `82510fd` (feat)
3. **Task 3: PlayerStats.RestoreStats additive method** - `1b26ecc` (feat)

**Plan metadata:** (pending - this commit)

## Files Created/Modified
- `Packages/manifest.json` - Added `"com.unity.nuget.newtonsoft-json": "3.2.2"` to dependencies (pure 1-line insertion)
- `Assets/SaveSystem/Script/SaveData.cs` - New POCO save schema (SaveData + PlayerStatsSaveData)
- `Assets/Player/Script/PlayerStats.cs` - Added `RestoreStats(float, float, float)` method (pure insertion, 0 lines deleted)

## Decisions Made
- Used version `3.2.2` per the plan's explicit correction over the `3.2.1` figure recorded in 11-CONTEXT.md, verified against `Library/PackageCache/com.unity.nuget.newtonsoft-json@4dfd81071c64/package.json`
- Kept `SaveData`/`PlayerStatsSaveData` as plain POCOs (no `MonoBehaviour`, no `[System.Serializable]`) since Newtonsoft.Json does not require Unity serialization attributes and these types have no inspector-facing role
- Preserved the pre-existing trailing-whitespace-only line in `PlayerStats.cs` byte-for-byte (used a raw insertion rather than a text-replace edit) to guarantee 0 deleted lines per the plan's acceptance criteria

## Deviations from Plan

### Auto-fixed Issues

**1. [Process correction] First manifest.json commit accidentally included pre-existing unrelated staged renames**
- **Found during:** Task 1 commit
- **Issue:** The working tree had pre-existing staged changes from the user's in-progress work (`Assets/Camera/Script/BossZoomTrigger.cs` -> `CameraZoomTrigger.cs` rename, staged before this plan started). `git add Packages/manifest.json` did not touch that pre-existing staged rename, but `git commit` (without a pathspec) committed the entire index, including it, into the Task 1 commit.
- **Fix:** Used `git reset --soft HEAD~1` to undo the commit while preserving the index, restored the camera files to their original staged/unstaged split, and re-committed Task 1 using `git commit -m "..." -- Packages/manifest.json` (pathspec-scoped commit) so only the intended file was included.
- **Files affected:** None beyond the plan's own `Packages/manifest.json` — the camera files were restored to their pre-existing state (content untouched throughout, only index/staging bookkeeping was corrected).
- **Verification:** `git show --stat HEAD` for the corrected Task 1 commit shows only `Packages/manifest.json`; `git status --short` afterward shows the same camera-file entries as before this plan began.
- **Commit:** `a1b14ed` (corrected Task 1 commit)

**2. [Rule 1 - Bug] Task 3 edit initially failed its own "0 deletions" acceptance criterion**
- **Found during:** Task 3 (PlayerStats.RestoreStats insertion)
- **Issue:** A straightforward text-replace `Edit` on the method-insertion point necessarily rewrote the pre-existing trailing-whitespace-only line before the closing brace, producing a 1-line deletion + 1-line insertion in `git diff --numstat`, violating the plan's explicit `git diff --numstat ... 삭제 라인 수 == 0` acceptance criterion.
- **Fix:** Reverted the file to clean HEAD state and performed a byte-precise insertion via a small Node script that located the exact original bytes (`    }\r\n    \r\n}`) and spliced the new method text in between without altering any existing byte, guaranteeing a pure-insertion diff.
- **Files modified:** `Assets/Player/Script/PlayerStats.cs`
- **Verification:** `git diff --numstat` shows `13  0` (13 insertions, 0 deletions); all other Task 3 acceptance criteria (signature, assignment order, 12 non-ASCII comment lines preserved, existing methods intact, no setter added) verified via grep.
- **Committed in:** `1b26ecc` (Task 3 commit)

---

**Total deviations:** 2 (1 process correction, 1 auto-fixed bug per Rule 1)
**Impact on plan:** No scope creep, no unintended file changes. Both deviations were self-corrections of the executor's own git/edit mechanics, not changes to plan scope. Final state matches all plan acceptance criteria exactly, and pre-existing unrelated user work (camera scripts, scene file, `_Recovery` folder) remains untouched.

## Issues Encountered
None beyond the two self-corrected deviations documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- `SaveData`/`PlayerStatsSaveData` schema and `PlayerStats.RestoreStats` are ready for 11-02 (SaveLoadManager) to consume
- Newtonsoft.Json is now a direct dependency; Unity will resolve/pin it on next editor open, no further action needed for this plan
- No blockers for 11-02

---
*Phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json*
*Completed: 2026-08-10*

## Self-Check: PASSED

All created/modified files verified present on disk (`Assets/SaveSystem/Script/SaveData.cs`, `Packages/manifest.json`, `Assets/Player/Script/PlayerStats.cs`). All 3 task commits verified present in git history (`a1b14ed`, `82510fd`, `1b26ecc`).
