---
phase: 17-spirit-boss-animation-integration
plan: 03
subsystem: enemy-animation
tags: [unity, animator, waterspirit, clone]
requires: ["17-01"]
provides: ["Clone prefab Stage 2 animation controller", "Clone trigger at each Stage 2 cycle"]
affects: ["17-04"]
tech-stack:
  added: []
  patterns: ["override controller assigned directly to clone prefab"]
key-files:
  modified:
    - Assets/Resources/Spirit Clone.prefab
    - Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs
decisions:
  - "Clone prefab Animator uses WaterSpirit_Stage2.overrideController directly (D-03)."
metrics:
  duration: 5min
  tasks: 2
  files: 2
  completed: 2026-10-01
---

# Phase 17 Plan 03: Clone Animation Summary

Spirit Clone prefab Animator now references WaterSpirit_Stage2.overrideController, and the real boss calls `PlayAnim("Clone")` right before `SpawnClones` in each Stage 2 cycle.

## Tasks

1. Clone prefab controller assignment -> verify: `m_Controller` is `{fileID: 22100000, guid: 93af4d6152ac7264fa00fec5b56f801c, type: 2}` and `fileID: 0` is gone. Commit e2297e0.
2. Clone trigger -> verify: single `PlayAnim("Clone")` after the `IsDummy` early return, before `SpawnClones(spirit)`. Commit c71fdbe.

## Deviations from Plan

None - plan executed exactly as written. Play-mode visual verification is deferred to 17-04.

## Known Stubs

None.

## Self-Check: PASSED
