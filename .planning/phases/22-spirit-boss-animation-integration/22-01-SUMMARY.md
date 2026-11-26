---
phase: 17-spirit-boss-animation-integration
plan: 01
subsystem: enemy-animation
tags: [unity, animator, boss, waterspirit]
requires: []
provides:
  - "SpiritController.PlayAnim(trigger) guarded Animator API"
  - "Move/Groggy polling, Hit on damage, Stage 2 controller swap, Death playback"
affects: [17-02, 17-03, 17-04]
key-files:
  modified:
    - Assets/Enemy/WaterSpirit/Script/SpiritController.cs
    - Assets/Enemy/WaterSpirit/Script/SpiritStats.cs
  created:
    - Assets/Enemy/WaterSpirit/WaterSpirit.prefab (first commit of previously untracked prefab)
key-decisions:
  - "Death waits for the WaterSpirit_Death clip length (looked up in runtimeAnimatorController.animationClips) then SetActive(false); falls back to immediate deactivation."
  - "Stage2AnimController swap happens before ChangeState(Stage2CombatState) and before the Clone trigger of 17-03."
metrics:
  completed: 2026-10-01
  tasks: 3
  files: 3
---

# Phase 17 Plan 01: Spirit Animator hooks and death lifecycle Summary

WaterSpirit now drives Move/Groggy bools, Hit on damage, a Stage 2 override controller swap, and a clip-length Death playback, all behind a null-guarded `PlayAnim` API.

## Steps -> Verification
- Task 1 hooks (PlayAnim, Hit subscribe/unsubscribe, Move/Groggy polling, Stage 2 swap) -> Assembly-CSharp compiled with zero errors; shared `NewBoss/Script/States` untouched.
- Task 2 prefab Stage2AnimController -> line `Stage2AnimController: {fileID: 22100000, guid: 93af4d6152ac7264fa00fec5b56f801c, type: 2}` present once; base Animator controller untouched.
- Task 3 death -> `_dying` guards in SpiritStats TakeDamage/Die; CleanupClones and SaveOnBossDefeated order unchanged; Awake defeated check intact; `BeginDeath` does StopAllCoroutines, SetCharging(false), StopMove, PlayAnim("Death"), waits clip length. Update returns while dying.

## Commits
- 6e0cb41: SpiritController/SpiritStats (Tasks 1 and 3, same files, committed together)
- 44acaab: WaterSpirit.prefab + meta (Task 2)

## Deviations from Plan
- Tasks 1 and 3 share two files, so they were committed as one commit rather than two.
- `dotnet build Assembly-CSharp.csproj` also builds referenced projects; it reported 2 errors (CS0118 'Image') only in the unrelated Unity.AI.Assistant package project. No errors in Assembly-CSharp. Out of scope.
- WaterSpirit.prefab was untracked, so its whole content (not just the one line) entered the commit; its meta was committed with it. Animations/Resource folders remain untracked (other plans).

## Known Stubs
None. Play-mode validation (single Death, no attack during death, defeated-load) is deferred to 17-04.

## Self-Check: PASSED
