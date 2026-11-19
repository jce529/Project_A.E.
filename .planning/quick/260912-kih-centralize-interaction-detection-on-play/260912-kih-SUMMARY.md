---
phase: quick-260912-kih
plan: 01
subsystem: player-interaction
tags: [unity, physics2d, input-system, tmp]
requires: []
provides: [player-owned nearest interaction, current binding prompt, unified puddle dispatch]
affects: [player, checkpoints, portals, walls, puzzles, water-puddles]
tech-stack:
  added: []
  patterns: [IPlayerInteractable, reusable overlap query buffers]
key-files:
  created: [Assets/Player/Script/IPlayerInteractable.cs, Assets/Player/Script/PlayerInteraction.cs, Assets/Player/Script/PlayerInteractionPrompt.cs, Assets/Editor/PlayerInteractionVerification.cs]
  modified: [Assets/Player/Script/PlayerController.cs, Assets/map/script/SignpostPortal.cs, Assets/map/script/Checkpoint.cs, Assets/map/script/InteractableWall.cs, Assets/map/script/3 stage/SlidingPuzzleTrigger.cs, Assets/Enemy/WaterMonster/Script/Phase2/PlayerAbsorb.cs, Assets/Enemy/WaterMonster/Script/Phase2/WaterPuddle.cs]
key-decisions:
  - Player uses a default 2-unit radius and selects one nearest eligible object by squared transform distance; instance ID breaks ties.
  - User added a key prompt, so reusable central queries also run in LateUpdate; key input always revalidates.
  - Prompt uses actual Input System binding strings and refreshes on binding changes; formatted text is cached.
requirements-completed: [QUICK-INTERACTION-01, QUICK-INTERACTION-02, QUICK-INTERACTION-03]
duration: 20min
completed: 2026-09-12
verification-status: compile-passed-runtime-pending
---

# Quick 260912-kih: Player interaction and key prompt

Player-owned nearest eligible dispatch replaces world proximity/input subscriptions, with a current-binding TMP prompt above the selected object and mutually exclusive puddle absorption.

## Task commits

| Task | Commit | Result |
|---|---|---|
| 1 | 840b0f2 | Contract, player dispatcher, automatic installation, key prompt, editor test entry |
| 2 | b496f49 | Portal/checkpoint/wall/puzzle action-only migration |
| 3 | 737216f | Puddle integration, alternate absorption modes, regression checks, cached prompt formatting |

Implementation commits were pushed to origin/주창은. The user subsequently requested that this PLAN, SUMMARY, and updated STATE.md also be committed and pushed to the same repository, overriding commit_docs=false for this task. ROADMAP.md is unchanged.

## Verification

- PASS: all runtime C# sources and the editor verification source compiled using installed Unity 6000.3.11f1 assemblies and Roslyn, exit 0. Existing warnings were retained; this is C# compilation, not a successful Unity import/Play run.
- PASS: git diff --check; production OnInteractEvent subscription exists only in PlayerInteraction. Migrated world scripts contain no interaction input subscriptions or player proximity callbacks.
- Added Tools > Verification > Player Interaction / PlayerInteractionVerification.Run checks for empty/nearest/duplicate/outside/disabled/inactive/tie, ineligible/locked skip, target clearing, binding override strings, water recovery, puddle/world exclusivity, stack registration, pool reset/unregister, alternate input absorption, player disable/reenable dispatch.
- NOT EXECUTED: the Unity behavior suite. A separate temporary project avoided changing the original 6000.3.10f1 project. Initial batch attempt failed licensing initialization in the sandbox; escalated attempt connected to licensing but exited with Unity return code 1 before executing the suite. No further attempts. Owned temporary Unity processes were stopped.
- NOT VERIFIED: visual prompt rendering/placement, real key-event resubscription, save/respawn, scene transitions, puzzle completion, gameplay absorption in Play mode. No actual save files or game scene assets were changed.
- Compile logs and isolated verification project remain under ignored Temp/ for diagnosis; no generated files were added to Git.

## Deviations from Plan

- User explicitly added the current interaction-key UI during execution. The player now scans once per LateUpdate for prompt maintenance as well as on input, preserving reusable buffers and no object scans. New PlayerInteractionPrompt.cs is included.
- [Rule 1 - Bug] Guarded repeated SetIndestructible registration and missing stack manager; the action can now be selected beyond old puddle triggers.
- Unity execution unavailable as described above; implemented checks are supplied but runtime success is not claimed.

## Setup and remaining checks

Existing PlayerController objects install PlayerInteraction automatically. For Inspector tuning before Play, explicitly attach PlayerInteraction (radius/layers) and optionally PlayerInteractionPrompt (world offset, default 1.5 units above transform); duplicate components are prevented. Interactable objects still need a Collider2D on themselves or descendants. The selected target alone displays a TMP key label through the main camera; its binding label updates after rebinding and hides when no target remains.

In Unity, run the verification menu in Edit mode, then verify overlapping world objects/puddles, key rebinding and visible label, checkpoint/portal/puzzle gameplay, and scene reloads in Play mode.

## Known Stubs

None. Runtime and visual verification are outstanding, not replaced with mock production behavior.

## Self-Check: PASSED

All four created C# files and their meta files exist; all three task commits exist. No tracked file deletions were introduced.
