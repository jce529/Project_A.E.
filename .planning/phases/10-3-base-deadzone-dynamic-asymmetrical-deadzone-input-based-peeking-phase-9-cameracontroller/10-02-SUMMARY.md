---
phase: 10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller
plan: 02

subsystem: camera
tags: [unity, csharp, camera-controller, deadzone, smoothdamp]

# Dependency graph
requires:
  - phase: 10-01
    provides: "Hard-cut base deadzone (_deadzoneCenterX, UpdateDeadzoneCenter, _isBossZone branch, ApplyNormalStageCamera/ResetNormalStageState helpers, deadzone Gizmo)"
provides:
  - "Dynamic asymmetrical deadzone offset layered on top of the base deadzone: _currentBoxOffsetX computed via hold-timer + SmoothDamp, camera X composed as _deadzoneCenterX - _currentBoxOffsetX"
  - "Push-direction tracking (_deadzonePushSign) recorded inside UpdateDeadzoneCenter"
  - "Boss-zone reset of all offset state so Phase 9 legacy follow is unaffected"
affects: ["10-03 (input-based peeking, which layers a further Y offset on this same composition point)"]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Offset state fields reset together in ResetNormalStageState (single source of truth for 'boss zone = fully disabled')"
    - "SmoothDamp velocity kept in a persistent private field (_offsetVelocityX), never a local, per Unity's SmoothDamp contract"

key-files:
  created: []
  modified:
    - "Assets/Camera/Script/CameraController.cs"

key-decisions:
  - "Sign convention A2 (locked by plan): _currentBoxOffsetX = -(pushDir * maxOffsetDistance), and camera X = _deadzoneCenterX - _currentBoxOffsetX, so running right (pushDir=+1) yields camera X = center + 1.5 (camera leads the player, opening up view ahead) - verified by grep gate and by re-deriving the sign algebra by hand"
  - "Clamp re-sync in D-17 now reads `_deadzoneCenterX = transform.position.x + _currentBoxOffsetX` (inverse of the X composition formula) so the box doesn't drift after a frame where ApplyXClamp() moved the camera"

requirements-completed: [D-04, D-05, D-06, D-07]

# Metrics
duration: 6min
completed: 2026-08-04
---

# Phase 10 Plan 2: Dynamic Asymmetrical Deadzone Summary

**Deadzone box now leans opposite the player's push direction via a held SmoothDamp offset (`_currentBoxOffsetX`), so running right/left opens up more view ahead in the direction of travel; boss zones remain fully unaffected.**

## Performance

- **Duration:** 6 min
- **Started:** 2026-08-04T07:39:00Z (approx, per STATE.md session start)
- **Completed:** 2026-08-04T07:46:30Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Added 3 tunable Inspector fields (`maxOffsetDistance`, `offsetSmoothTime`, `offsetHoldDuration`) and 4 internal state fields (`_currentBoxOffsetX`, `_offsetVelocityX`, `_offsetHoldTimer`, `_deadzonePushSign`)
- `UpdateDeadzoneCenter` now records which edge is being pushed (`_deadzonePushSign`) with no separate speed threshold (D-05)
- New `UpdateDynamicOffset` helper implements the user's formula `TargetOffset = -(pushDir * maxOffsetDistance)` with a SmoothDamp transition and a hold timer before easing back to 0 (D-06/D-07)
- Camera X composition changed from a pure hard-cut (`p.x = _deadzoneCenterX`) to `p.x = _deadzoneCenterX - _currentBoxOffsetX`, matching the locked sign convention (A2)
- Boss-zone entry resets all 4 new state fields every frame via `ResetNormalStageState()`, so Phase 9 legacy follow behavior is byte-for-byte unaffected (D-15)
- Post-clamp re-anchor updated to `_deadzoneCenterX = transform.position.x + _currentBoxOffsetX` so the box stays consistent with the offset after `ApplyXClamp()` moves the camera (D-17)

## Task Commits

Each task was committed atomically:

1. **Task 1: Offset Inspector fields + push-direction tracking + UpdateDynamicOffset helper** - `717e37f` (feat)
2. **Task 2: Apply offset to camera X composition + boss-zone state reset + clamp re-sync** - `f24d53a` (feat)

**Plan metadata:** (this commit, made after this summary is written)

## Files Created/Modified
- `Assets/Camera/Script/CameraController.cs` - Added dynamic asymmetrical deadzone offset layer (fields, `UpdateDynamicOffset`, camera X composition, boss-zone reset, clamp re-sync)

## Decisions Made
- Followed the plan's locked sign convention (A2) exactly: offset is computed on the box, and subtracted (not added) from the box center to get camera X. Verified by hand: pushDir=+1 (running right) -> offset=-1.5 -> camera X = center - (-1.5) = center + 1.5 (camera ahead to the right) - matches "진행 방향 시야가 열린다".
- Kept `targetOffsetX = _currentBoxOffsetX` (not 0) during the hold window so SmoothDamp has no incentive to move, which is what makes the offset "hold" rather than instantly decay (D-06).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking, verification-script only] Task 2's `git diff ef6f164` delcount gate could never equal 2 as literally written**
- **Found during:** Task 2 verification
- **Issue:** The plan's automated verify script and acceptance criteria for Task 2 check `git diff ef6f164 -- CameraController.cs | grep '^-' | grep -vc '^---'` and expect `2` (the two ASCII lines this task modifies: `p.x = _deadzoneCenterX;` and `if (!_isBossZone) _deadzoneCenterX = transform.position.x;`). However, both of those lines were themselves newly *inserted* by Plan 10-01 (commits `5a36816`/`95592bb`), which itself is a pure-insertion diff relative to `ef6f164`. Since `ef6f164` (the Phase 9 baseline) never contained those lines in any form, git's diff algorithm renders the whole surrounding block as a contiguous `+` insertion hunk when comparing straight to `ef6f164` - modifying a line that only exists in that insertion hunk does not produce a matched `-`/`+` pair against a baseline that never had it, so `git diff ef6f164` unavoidably reports `0` deletions for this task, not `2`.
- **Fix:** Re-ran the same substantive check against the immediately preceding commit (`git diff HEAD` before staging, i.e. against `717e37f`, this plan's own Task 1 commit) instead of `ef6f164`. That comparison correctly isolates Task 2's own edit and shows exactly 2 deleted lines, both ASCII (`p.x = _deadzoneCenterX;` and `if (!_isBossZone) _deadzoneCenterX = transform.position.x;`), with 0 non-ASCII characters among them - satisfying the actual intent of the gate (no Korean comment lines touched, exactly the 2 named lines modified). All other Task 2 acceptance criteria (grep checks for the new call sites, call-order line numbers, encoding gate == 5, Rigidbody/PlayerController/InputHandler untouched) passed unmodified against their original commands.
- **Files modified:** None beyond the plan's intended edits - this was a verification-methodology correction only, not a code change.
- **Verification:** `git diff HEAD -- Assets/Camera/Script/CameraController.cs | grep '^-' | grep -vc '^---'` == `2`, and piping those 2 lines through `LC_ALL=C grep -c '[^[:print:][:space:]]'` == `0`. Encoding gate `LC_ALL=C grep -c '[^[:print:][:space:]]' CameraController.cs` == `5` (unchanged from before this plan).
- **Committed in:** `f24d53a` (Task 2 commit) - the actual code is unaffected; only the verification comparison baseline was corrected.

---

**Total deviations:** 1 auto-fixed (1 blocking - verification script baseline mismatch, no code impact)
**Impact on plan:** None on the shipped code. The plan's own gate script referenced a stale baseline commit (`ef6f164`, pre-10-01) instead of the plan's own prior commit, which is a plan-authoring artifact identical in nature to two prior instances already logged in STATE.md (Phase 9 Plan 1's `DontDestroyOnLoad` literal conflict, Phase 10 Plan 1's `deadzoneHeight` literal conflict). No scope creep; no functional change beyond what the plan specified.

## Issues Encountered
- See deviation above. No other issues - all six insertions in Task 1 and all four edits in Task 2 applied cleanly on the first attempt against the anchors read directly from the file.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- `Assets/Camera/Script/CameraController.cs` now exposes `maxOffsetDistance` (1.5), `offsetSmoothTime` (0.35), `offsetHoldDuration` (0.4) in the Inspector for playtest tuning, alongside Plan 10-01's `deadzoneWidth`/`deadzoneHeight`.
- Composition point (`p.x = _deadzoneCenterX - _currentBoxOffsetX`) is ready for Plan 10-03 to add a further Y-axis peeking offset without touching this line.
- Recommended (not required): a Play-mode check that running right/left visibly shifts the camera ahead of the player and holds briefly after stopping, deferred to Plan 10-04 per the existing project pattern of batching Play-mode verification (see STATE.md Active TODOs).

---
*Phase: 10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller*
*Completed: 2026-08-04*

## Self-Check: PASSED

- FOUND: Assets/Camera/Script/CameraController.cs
- FOUND: .planning/phases/10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller/10-02-SUMMARY.md
- FOUND: commit 717e37f (Task 1)
- FOUND: commit f24d53a (Task 2)
