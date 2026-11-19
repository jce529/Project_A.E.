---
phase: 10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller
plan: 03
subsystem: camera
tags: [unity, csharp, camera, input-handler, smoothdamp]

# Dependency graph
requires:
  - phase: 10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller
    provides: "Plan 10-01 hard-cut base deadzone + boss zone branch, Plan 10-02 dynamic asymmetrical deadzone offset"
provides:
  - "InputHandler.OnMoveEvent subscription with symmetric OnEnable/OnDisable lifecycle (no leak across scene loads)"
  - "Cached PlayerController reference for movementLocked / IsGrounded gating"
  - "UpdatePeekOffset: input-based vertical peeking gated on idle + grounded + unlocked + held vertical input, cancelled by a speed-spike proxy"
affects: [camera, player-movement]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Event subscription lifecycle: OnEnable binds (with Start-time retry via TryBindMoveEvent for init-order races), OnDisable always unbinds"
    - "Movement-magnitude-per-second proxy (no isDashing/isKnockedBack flags) used to detect both idle and cancel-worthy speed spikes"

key-files:
  created: []
  modified:
    - "Assets/Camera/Script/CameraController.cs"

key-decisions:
  - "peekCancelSpeed defaults to 12 (between PlayerController.runSpeed=7 and dashSpeed=20) so normal running never cancels peeking but dashing/knockback always does, without adding public accessors to isDashing/isKnockedBack (D-11)"
  - "HandleMoveInput caches the raw input unconditionally, mirroring PlayerController.OnMove's own convention; the movementLocked/IsGrounded/idle gate is applied only at the UpdatePeekOffset consumption point, avoiding stale-input bugs when lock state changes between events"
  - "_lastTargetPos is updated only once, at the very end of ApplyNormalStageCamera, after UpdateDeadzoneCenter/UpdateDynamicOffset/UpdatePeekOffset have all read the previous frame's value - updating earlier would make the speed delta always 0"

patterns-established:
  - "Peeking as a pure additive Y offset (_currentPeekY) layered on top of _followBaseY - never a second deadzone axis"

requirements-completed: [D-08, D-09, D-10, D-11, D-12, D-13]

# Metrics
duration: 10min
completed: 2026-08-04
---

# Phase 10 Plan 03: Input-based Peeking Summary

**Vertical camera peeking (SmoothDamp offset up to peekDistance=3, threshold 0.5s hold) driven by InputHandler.OnMoveEvent, gated on grounded+idle+unlocked and cancelled by a dash/knockback speed-spike proxy (peekCancelSpeed=12), layered on top of Plan 10-01/10-02's deadzone pipeline in CameraController.cs.**

## Performance

- **Duration:** ~10 min
- **Tasks:** 2 completed
- **Files modified:** 1

## Accomplishments
- `CameraController` subscribes to `InputHandler.Instance.OnMoveEvent` with a leak-safe `OnEnable`/`OnDisable` pair, retrying in `Start` to cover the case where `OnEnable` runs before `InputHandler.Awake` in the first scene.
- `PlayerController` reference is cached once in `Start` (no per-frame `GetComponent`).
- `UpdatePeekOffset` composes the four required gates (`movementLocked`, `IsGrounded()`, idle-speed proxy, held vertical input past `peekThreshold`) and drives `_currentPeekY` via `Mathf.SmoothDamp`, using a faster `peekReturnSmoothTime` whenever the target offset collapses back to 0.
- Camera Y composition now reads `_followBaseY + _currentPeekY`; boss-zone entry resets all peek state every frame via `ResetNormalStageState`.
- Zero changes to `PlayerController.cs` / `InputHandler.cs` (verified via `git status --porcelain`).

## Task Commits

Each task was committed atomically:

1. **Task 1: InputHandler.OnMoveEvent subscription lifecycle + PlayerController ref cache** - `5d5b55e` (feat)
2. **Task 2: Peeking Inspector fields + UpdatePeekOffset helper + camera Y composition/state reset** - `b4ee51a` (feat)

**Plan metadata:** (this commit)

## Files Created/Modified
- `Assets/Camera/Script/CameraController.cs` - Added OnMoveEvent subscription lifecycle, PlayerController ref cache, 6 new Inspector peeking fields, `UpdatePeekOffset` helper, camera Y composition change, and boss-zone peek-state reset.

## Decisions Made
- See `key-decisions` in frontmatter (peekCancelSpeed=12 rationale, unconditional input caching convention, `_lastTargetPos` update ordering).

## Deviations from Plan

None in terms of code content - both tasks were implemented exactly as specified in the plan's `<action>` blocks (verbatim anchors, verbatim new code blocks).

**One verification-gate discrepancy (not a code deviation):** both tasks' automated verify commands compare `git diff ef6f164 -- CameraController.cs` deletion-line counts against baseline `ef6f164` (a Phase 9 commit predating all of Phase 10). This gate returns `0` deletions in both tasks instead of the plan's expected `2` (Task 1) and `3` (Task 2), because git's diff algorithm folds Plan 10-02's line modifications into the larger insertion hunk when diffed against a baseline that predates Plan 10-01 entirely - the same root cause already documented in STATE.md for Phase 10 Plan 2 ("baseline commit selection error"). Verified correctness instead against the immediate parent commit at each step:
- Task 1 vs `434a3e0` (end of Plan 10-02): 0 deletions (task is pure insertion, as expected).
- Task 2 vs `5d5b55e` (Task 1 commit): exactly 1 deletion (`p.y = _followBaseY;` → `p.y = _followBaseY + _currentPeekY;`), 0 non-ASCII, matching the plan's own stated expectation of "이 플랜에서 1줄을 수정".

All other gate conditions (field presence, subscribe/unsubscribe counts, `GetComponent` call count, encoding byte-count==5, read-only file `git status --porcelain` emptiness) passed exactly as specified for both tasks.

## Issues Encountered
None beyond the verification-gate baseline discrepancy documented above, which does not affect code correctness.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 10's three camera techniques (base deadzone, dynamic asymmetrical deadzone, input-based peeking) are now all layered into `CameraController.cs`.
- Runtime/Play-mode verification of the full peeking behavior (hold up/down while idle and grounded, movement/dash/hit cancellation, boss-zone bypass) has not been performed in this execution environment and should be checked in Unity Play mode before closing out Phase 10.

---
*Phase: 10-3-base-deadzone-dynamic-asymmetrical-deadzone-input-based-peeking-phase-9-cameracontroller*
*Completed: 2026-08-04*

## Self-Check: PASSED

- FOUND: Assets/Camera/Script/CameraController.cs
- FOUND: commit 5d5b55e (Task 1)
- FOUND: commit b4ee51a (Task 2)
