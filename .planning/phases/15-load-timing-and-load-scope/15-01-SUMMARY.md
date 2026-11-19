---
phase: 15-load-timing-and-load-scope
plan: 01
subsystem: player-lifecycle
tags: [unity, save-load, player-health, scene-reload]

requires:
  - phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json
    provides: SaveLoadManager load flow and PlayerStats.RestoreStats entry point
  - phase: 14-save-slot-expansion
    provides: active-slot save-file selection
provides:
  - Player death reloads the active save or restarts the current scene when no save exists
  - Restored player health values satisfy 0 < health <= maxHealth <= maxTotalHealth
  - Player prefab starts with maxHealth 100 and maxTotalHealth 200
affects: [15-04-verification, player-death, save-load, health-progression]

tech-stack:
  added: []
  patterns: [PlayerStats death-policy override, anchor-based save-value normalization]

key-files:
  created: []
  modified:
    - Assets/Player/Script/PlayerStats.cs
    - Assets/Player.prefab

key-decisions:
  - "PlayerStats overrides HP.Die without calling the base implementation so scene loading replaces object destruction."
  - "maxTotalHealth is the normalization anchor; maxHealth and health clamp downward in that order with a floor of 1."
  - "The existing unrelated Player.prefab attackBox assignment was preserved and included in the prefab commit as required."

patterns-established:
  - "Death load guard: set a per-player re-entrancy flag before starting any scene transition."
  - "Restore ordering: maxTotalHealth, maxHealth, health, then ClampHealth()."

requirements-completed: [D-01, D-02, D-08, D-09]

duration: 6min
completed: 2026-09-10
---

# Phase 15 Plan 01: Player Death Reload and Health Invariants Summary

**Player death now reloads the active save or restarts the current scene, while restored and prefab health values obey the configured 100-to-200 progression bounds.**

## Performance

- **Duration:** 6 min
- **Started:** 2026-09-10T14:10:48Z
- **Completed:** 2026-09-10T14:16:48Z
- **Tasks:** 3
- **Files modified:** 2

## Accomplishments

- Added one-shot player death handling that preserves the player object until the save-load or scene-restart transition takes over.
- Normalized all restored health values to `0 < health <= maxHealth <= maxTotalHealth` and logs only actual corrections.
- Set the Player prefab's starting maximum health to 100 while retaining the 200 growth ceiling and the user's existing attackBox assignment.

## Task Commits

Each task was committed atomically:

1. **Task 1: Enforce health invariants in RestoreStats** - `6575989` (fix)
2. **Task 2: Reload after player death** - `6bf0a58` (feat)
3. **Task 3: Set Player prefab starting maximum health** - `5b037f8` (fix)

## Files Created/Modified

- `Assets/Player/Script/PlayerStats.cs` - Adds death load/restart routing and ordered save-value normalization while preserving UTF-8 BOM, CRLF, and existing Korean comments.
- `Assets/Player.prefab` - Changes `maxHealth` from 400 to 100; the pre-existing unrelated attackBox assignment remains intact and was necessarily included in the same path-scoped commit.

## Decisions Made

- Followed the plan's fixed `PlayerStats.Die()` override hook and intentionally avoided `HP.cs`, `FallZone.cs`, `ManualDeath`, and `GameStateManager` changes.
- Used `maxTotalHealth` as the authoritative saved-value anchor and retained the locked assignment order before `ClampHealth()`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Resolved contradictory literal-string requirements for `base.Die()`**

- **Found during:** Task 2 (player death reload)
- **Issue:** The prescribed comment contained the literal `base.Die()`, while the mandatory acceptance command required that literal to occur zero times in `PlayerStats.cs`.
- **Fix:** Kept the exact runtime behavior and meaning but changed the comment wording to "the base implementation," allowing the zero-count gate to prove no base call exists.
- **Files modified:** `Assets/Player/Script/PlayerStats.cs`
- **Verification:** Literal count is 0, `public override void Die()` count is 1, and the solution builds with zero errors.
- **Committed in:** `6bf0a58`

---

**Total deviations:** 1 auto-fixed (1 blocking plan inconsistency).
**Impact on plan:** No runtime or architectural scope change; only an equivalent comment phrase changed.

## Issues Encountered

- `apply_patch` introduced mixed line endings in `PlayerStats.cs`; line endings were immediately normalized back to CRLF with the original UTF-8 BOM retained before each commit. Byte checks confirmed no U+FFFD replacement characters.
- The solution reports six pre-existing compiler warnings in unrelated files; compilation completes with zero errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 15-01 is ready for Phase 15's remaining load-failure, boss-progress restoration, and consolidated verification plans.
- No blockers were introduced. Runtime Play-mode coverage remains assigned to Plan 15-04.

## Self-Check: PASSED

- Verified both modified production files exist.
- Verified task commits `6575989`, `6bf0a58`, and `5b037f8` exist.
- Re-ran all task acceptance checks and the plan-level build/scope verification successfully.

---
*Phase: 15-load-timing-and-load-scope*
*Completed: 2026-09-10*
