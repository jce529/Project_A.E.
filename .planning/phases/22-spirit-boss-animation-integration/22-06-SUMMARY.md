---
phase: 17-spirit-boss-animation-integration
plan: 06
subsystem: enemy-animation
tags: [unity, animator, water-spirit, play-mode-verification, gap-closure]
requires:
  - phase: 17-05
    provides: Clone hold and Death-end fixes to verify
provides:
  - Play-mode evidence that P9 and P12 PASS
  - New observed defect P19 (death while Groggy)
affects: [phase-17-completion]
key-files:
  modified:
    - .planning/phases/17-spirit-boss-animation-integration/Check.md
  created:
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-C-session1-stage2.log
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-C-session1-charge.log
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-C-session2-stage2-start.log
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-C-session2-death.log
decisions:
  - "P19 (Death overridden by Groggy when killed during Groggy) recorded as FAIL, not fixed here (out of plan scope)"
metrics:
  tasks: 1
  files: 5
  completed: 2026-10-01
---

# Phase 17 Plan 06: Play-mode re-verification Summary

P9 and P12 now PASS by observation after the 17-05 fixes; a new defect (P19, Death overridden by Groggy when the boss is killed while Groggy) was observed and is recorded as FAIL.

## Observations

- [P9] Clone held 1.17-1.31 s before the next attack state on 11 Stage 2 re-entries over two sessions (e.g. 49.32 -> 50.52, 65.36 -> 66.56). On first entry the queued Hit from the threshold damage interrupts Clone after 0.04 s (allowed by plan); the next pattern still starts 1.17 s after Clone entry.
- [P12] Non-Groggy kill: Death entered 293.34, nt 1.00 at 294.93, inactive at 294.99 (+1.65 s >= +1.58 s).
- [Regression] P3 windup +0.52 s, P11 Groggy ~5.0 s, P13 position unchanged, P14 clones 2 -> 0, P15 `BossProgress.WaterSpirit=true`: all PASS.
- [P19, new] Killed during Groggy: Death entered 129.46, back to Groggy at 129.51, deactivated at 131.41 via the clip length + 0.5 s cap. Probable cause (unconfirmed): Groggy bool stays true and Any State -> Groggy overrides Death.
- Clone/Stealth pure exit-time return, U1-U4 remain UNVERIFIED.

## Cleanup

save.json restored from backup (SHA-256 `7df6fe74...8907` matches), no Assets/Temp files, `git status` identical to pre-run except new evidence logs. No Assets, scene or prefab changes.

## Deviations from Plan

None in scope. Death was triggered from an in-Editor update hook (conditional on non-Groggy Stage2 state) because polling via separate eval calls never hit the window. The first kill happened to occur during Groggy, which revealed P19; a second session produced the clean P12 result.

## Known Stubs

None.

## Issues

- Phase 17 should not be declared complete until P19 is fixed or explicitly accepted by the user.

## Self-Check: PASSED
