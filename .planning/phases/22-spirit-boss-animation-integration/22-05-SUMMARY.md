---
phase: 17-spirit-boss-animation-integration
plan: 05
subsystem: enemy-animation
tags: [unity, animator, water-spirit, gap-closure]
requires:
  - phase: 17-04
    provides: Play-mode FAILs P9 and P12 (Check.md)
provides:
  - SpiritController.GetClipLength(prefix) helper
  - Death deactivation timed from Death state completion
  - Stage 2 first-pattern hold for Clone clip length
affects: [17-06]
key-files:
  modified:
    - Assets/Enemy/WaterSpirit/Script/SpiritController.cs
    - Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs
decisions:
  - "No ResetTrigger(Hit) before Death; the extra ~0.1-0.16 s Hit is accepted, only truncation fixed"
metrics:
  tasks: 2
  files: 2
  completed: 2026-10-01
---

# Phase 17 Plan 05: Clone hold and Death end fix Summary

Two surgical fixes: Stage 2 holds its first decision for the Clone clip length (P9), and boss deactivation now waits for the Death state to reach normalizedTime >= 1 with a clip length + 0.5 s cap (P12).

## Tasks

| Task | Commit | Notes |
|------|--------|-------|
| 1 P12 + GetClipLength | 4a931af | BeginDeath uses helper; DeactivateAfter polls Death state |
| 2 P9 | dbb747b | `_decisionTimer = spirit.GetClipLength("WaterSpirit_Clone")` after PlayAnim("Clone") |

## Verification

- [P12] -> grep/diff shows helper, `IsName("Death")` polling, `seconds + 0.5f` cap, old loop removed.
- [P9] -> `_decisionTimer` set right after `PlayAnim("Clone")`; CombatState.cs untouched.
- [Build] -> `dotnet build Assembly-CSharp.csproj`: no errors outside the known com.unity.ai.assistant package errors.
- CRLF line endings preserved in both files.
- Runtime confirmation is plan 17-06 (Play mode).

## Side effect (intended)

During the ~1.17 s hold the boss does not switch to ChaseState when the player is far, so first-entry clones are no longer removed after 0.08 s by the Chase transition.

## Deviations from Plan

None - plan executed as written.

## Known Stubs

None.

## Self-Check: PASSED
