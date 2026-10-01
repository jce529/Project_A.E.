---
phase: 17-spirit-boss-animation-integration
plan: 04
subsystem: enemy-animation
tags: [unity, animator, waterspirit, verification, play-mode]
requires: ["17-01", "17-02", "17-03"]
provides: ["Phase 17 Check.md with observed Play mode results", "Raw Animator/combat logs under evidence/"]
affects: []
tech-stack:
  added: []
  patterns: ["EditorApplication.update eval hook that logs Animator state/clip/trigger changes during Play mode"]
key-files:
  created:
    - .planning/phases/17-spirit-boss-animation-integration/Check.md
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-A-death.log
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-A-sprite-name-counts.txt
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-B-anim.log
    - .planning/phases/17-spirit-boss-animation-integration/evidence/play-B-combat.log
  modified: []
decisions:
  - "Phase 17 is NOT declared complete: two observed FAILs (P9 Clone cast overwritten, P12 Death clip truncated) need a focused gap plan."
metrics:
  duration: ~75min
  tasks: 2
  files: 5
  completed: 2026-10-01
---

# Phase 17 Plan 04: Play mode verification Summary

Static checks and a live Unity Play mode run of the WaterSpirit encounter were recorded in Check.md: 14 Play items PASS, 2 FAIL, 4 UNVERIFIED. No game asset or script was modified.

## Tasks

| Task | Name | Commit |
|------|------|--------|
| 1 | Static regression checks, Check.md skeleton | cd18c38 |
| 2 | Play mode observations in Check.md (+ evidence logs) | 02c0157 |

## What was observed (Play mode, Unity CLI)

- Charge, Repel, Ranged: animation starts at the beginning of the 0.5 s / 0.4 s / 0.4 s windups. Damage and projectile spawn land at the expected offsets (Repel +0.40 s, Ranged projectile +0.40 s) and match Inspector values (Repel 10, Charge 15, Projectile 12). No stale or duplicated triggers.
- Stage 2: override controller and `_S2` clips/sprites are active before the Clone state; clones animate on the override; heavy combo plays Stealth then Charge on boss and clones together; Groggy bool true then false after about 5 s; cycle repeats.
- Death: clones cleaned immediately, no movement or attack while dying, boss defeat saved (`BossProgress.WaterSpirit=true`), and after `LoadSlot` the defeated boss stays inactive.
- Console: no Animator warnings; remaining warnings are scene-level and not attributable to Phase 17.

## Deviations from Plan

None to game code. Test-environment workarounds, all Play-mode-only and reverted: temporary ground collider and player repositioning (scene has no floor near the boss), `Application.runInBackground=true` at runtime, `Stats.TakeDamage` calls to trigger Hit/Stage 2/Death. `save.json` was backed up and restored byte-identical (SHA-256 checked). A stray `Assets/Temp` PNG folder created by `capture_game_view` was deleted.

## Issues Found (FAIL, need a gap plan)

1. **P9 Clone cast overwritten** - the boss's Clone animation (1.17 s clip) is replaced after 0.03-0.05 s by the immediate first Stage 2 attack (Charge via Any State), so the cast is effectively invisible. Suggested: hold the first pattern until the Clone clip ends, or block Charge during Clone.
2. **P12 Death clip truncated** - `DeactivateAfter(length)` starts at `BeginDeath`, but Death state begins ~0.16 s later (Hit queued first), so the last ~0.13-0.16 s of the clip is cut. Suggested: wait from Death entry, or add a small margin.

## UNVERIFIED

Exhaustion to WakeRepel chain (not directly logged), knockback magnitude, clone hit/death behavior, visual quality (boss off-screen in captures), exit-time return for Stealth and Clone (always interrupted by the next trigger).

## Out-of-scope findings

`Assets/Enemy/WaterSpirit/Animations/` (controller, clips, override controller) and `Resource/` remain untracked in git although committed prefabs reference them by GUID. Not changed here.

## Known Stubs

None.

## Self-Check: PASSED

Check.md, the four evidence files, commits cd18c38 and 02c0157 verified present.
