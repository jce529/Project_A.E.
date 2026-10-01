---
phase: 17-spirit-boss-animation-integration
verified: 2026-10-01T00:00:00Z
status: gaps_found
score: 4/5 must-haves verified
gaps:
  - truth: "Boss plays the Death animation on death (all states)"
    status: partial
    reason: "P19 (Check.md): when killed while in GroggyState, Death is overridden by Groggy after 0.05s. Code confirms likely cause: SpiritController.Update sets Anim.SetBool('Groggy', CurrentState is GroggyState) but returns early once _dying is true (line ~152), so the Groggy bool is never cleared and Any State -> Groggy re-fires over Death."
    artifacts:
      - path: "Assets/Enemy/WaterSpirit/Script/SpiritController.cs"
        issue: "On death (around lines 111-132), Groggy bool is not reset to false before PlayAnim('Death'); Update stops refreshing it once _dying is set"
    missing:
      - "Call Anim.SetBool('Groggy', false) (and Move false) when entering death, before SetTrigger('Death')"
      - "Re-run Play-mode check: kill boss during GroggyState; Death must play to nt 1.0 before deactivation"
---

# Phase 17: Spirit Boss Animation Integration Verification Report

**Phase Goal:** Water spirit boss plays animations matching state (Idle/Move/Charge/Repel/Clone/Ranged/Stealth/Hit/Groggy/Death); Stage 2 swaps to the Stage2 override controller; no logic/state-machine regressions.
**Status:** gaps_found
**Re-verification:** No (initial verification). Play-mode results are taken from Check.md; none were re-run by the verifier.

## Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | State-matched animations play for Idle, Move, Charge, Repel, Clone, Ranged, Stealth, Hit, Groggy | VERIFIED | Code: PlayAnim calls in SpiritCharge, SpiritFarProjectile, SpiritStealth, SpiritWakeRepel, Stage2CombatState; Hit via PlayHitAnim; Move/Groggy bools in Update. Check.md P1-P7, P11 PASS. Stealth and Clone pure exit-time return is UNVERIFIED. |
| 2 | Stage 2 entry swaps runtimeAnimatorController to the Stage2 override, before the Clone trigger | VERIFIED | SpiritController.cs:87 assigns Stage2AnimController; P8 and P10 PASS (clips _S1 to _S2, clones also use the override) |
| 3 | Death animation plays before deactivation | PARTIAL | Normal path passes (P12: Death reaches nt 1.00 before deactivation). Death while Groggy fails (P19) |
| 4 | Existing logic and state machine have no regressions | VERIFIED | S3-S6 static checks (AnimationName stays "", only expected files changed); P3-P5, P13-P18 (damage 12/15/10, projectile spawn, pattern cycle, save/load). U1-U4 are UNVERIFIED but not contradicted |
| 5 | Animator wiring is null-safe, with no console errors | VERIFIED | CanAnimate guard (SpiritController.cs:51); Check.md reports 0 Animator warnings and S2 compile OK |

**Score:** 4/5 (truth 3 partial)

## Required Artifacts

| Artifact | Status | Details |
|----------|--------|---------|
| SpiritController.cs | VERIFIED (with the P19 defect) | PlayAnim, Stage2AnimController, Death flow, Move/Groggy bools |
| States/Attacks/SpiritCharge, FarProjectile, Stealth, WakeRepel .cs | VERIFIED | Each calls PlayAnim with the right trigger |
| States/Stage2CombatState.cs | VERIFIED | PlayAnim("Clone"), plus the Clone-length hold from 17-05 |
| Animations/WaterSpirit.controller, WaterSpirit_Stage2.overrideController, 19 clips | EXISTS and WIRED | Referenced by the prefabs per S7 |

## Key Links

| From | To | Status |
|------|----|--------|
| Attack strategies | SpiritController.PlayAnim to Animator trigger | WIRED |
| OnStage2Trigger | Anim.runtimeAnimatorController = Stage2AnimController | WIRED |
| SpiritStats death | SpiritController death coroutine / Death trigger | WIRED (Groggy bool not cleared, see gap) |

## Requirements Coverage

No requirement IDs for this phase (TBD in ROADMAP). No orphaned requirements.

## Anti-Patterns / Notes

| Item | Severity | Impact |
|------|----------|--------|
| P19: Groggy overrides Death | Blocker for "all states" Death | Defect exists only when killed during Groggy |
| `Assets/Enemy/WaterSpirit/Animations/` and `Resource/` are git-untracked; the committed prefabs reference them by GUID | Warning | A fresh checkout would have an empty controller; the files need to be committed |
| Exhaustion to WakeRepel chain, knockback, clone hit/death, visual quality: UNVERIFIED (U1-U4) | Info | Human check |

## Human Verification Required

1. Visual quality of sprites and positions in the Game view (U4). The boss was off-screen in the captures.
2. Kill the boss during Groggy after the fix and confirm Death plays fully.
3. Exhaustion to WakeRepel chain, knockback, clone hit/death (U1-U3).

## Gaps Summary

The phase is nearly achieved. Animations are wired and Stage 2 override is verified in Play mode. One defect remains, P19: the Groggy bool stays true after death starts because SpiritController.Update returns early when `_dying` is set. A small fix is likely: clear the Groggy and Move bools when death begins. Also commit the untracked Animations/Resource assets.

---
_Verified: 2026-10-01_
_Verifier: Claude (gsd-verifier)_
