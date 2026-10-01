---
phase: 17-spirit-boss-animation-integration
plan: 02
subsystem: enemy-animation
tags: [unity, animator, spirit-boss]
requires: ["17-01"]
provides: ["Charge/Repel/Ranged/Stealth animator triggers at existing windups"]
affects: [17-04]
key-files:
  modified:
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritCharge.cs
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritWakeRepel.cs
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritFarProjectile.cs
    - Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritStealth.cs
decisions:
  - "AnimationName stays empty on all four strategies; gating unchanged."
metrics:
  duration: 5min
  completed: 2026-10-01
---

# Phase 17 Plan 02: Attack Animator Hooks Summary

Four one-line guarded `PlayAnim` calls (Charge, Repel, Ranged, Stealth) placed immediately before each existing windup yield, with no change to timing, damage, or strategy selection.

## Tasks
1. Charge + wake Repel triggers - 0a0549e
2. Ranged + Stealth triggers (Stealth in shared `StealthRoutine`, so heavy combos and clones animate too) - 1bcca1e

## Verification
- [Attack hooks] -> verified by diff: 4 insertions, each before its existing `WaitForSeconds`/collider disable; `AnimationName` untouched; `SpiritRepel.cs` and `SpiritProjectileAttack.cs` unmodified; CRLF/UTF-8 preserved.
- `dotnet build Assembly-CSharp.csproj` exceeded the 120s tool timeout and did not return a result; compile is not confirmed here (calls use the existing public `PlayAnim(string)`). Play mode timing is checked in 17-04.

## Deviations from Plan
None - plan executed as written (build result unconfirmed, see above).

## Known Stubs
None.

## Self-Check: PASSED
Commits 0a0549e and 1bcca1e exist; four files modified.
