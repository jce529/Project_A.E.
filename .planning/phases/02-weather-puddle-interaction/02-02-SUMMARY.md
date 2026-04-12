---
phase: 02
plan: 02
subsystem: player-interaction
tags: [interaction, skill, absorb]
dependency-graph:
  requires: [02-01]
  provides: [puddle-destruction, puddle-absorb]
  affects: [player-input, wave-slice, water-controller]
tech-stack:
  added: [PlayerAbsorb]
  patterns: [Interaction Event Pattern, Overlap Detection]
key-files:
  created:
    - Assets/Enemy/WaterMonster/Script/Phase2/PlayerAbsorb.cs
  modified:
    - Assets/Player/Script/SkillScript/WaveSlice.cs
    - Assets/Player/Script/InputHandler.cs
decisions:
  - "Integrated F-key interaction into InputHandler using the existing 'Action' input action"
  - "PlayerAbsorb.TryAbsorb uses 2.0f radius for proximity checks, consistent with combat range"
metrics:
  duration: 12m
  completed_date: 2026-04-12
---

# Phase 02 Plan 02: Player-Puddle Interaction Summary

Implemented core player interactions with water puddles: destruction via WaveSlice skill and absorption via the F key.

## Key Features

### Puddle Destruction (WaveSlice)
- **WaveSlice.cs**: Modified the `waveSlice()` logic to detect and destroy `Destructible` WaterPuddle objects on hit.
- The check is performed before Boss/Enemy processing to ensure correct tag handling.

### Puddle Absorption (F key)
- **InputHandler.cs**: Exposed `OnInteractEvent`, bound to the F key ("Action" action).
- **PlayerAbsorb.cs**: New component for the Player that listens for `OnInteractEvent`.
- **Absorb Mechanic**: When F is pressed, the nearest in-range destructible puddle is absorbed, recovering one water bottle for the player and making the puddle `Indestructible` (with visual change and stack registration).

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED
- WaveSlice destruction verified via code review of tag handling order.
- InputHandler event propagation verified.
- PlayerAbsorb logic correctly calls `RecoveryWater()` and `SetIndestructible()`.
