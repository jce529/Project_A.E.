# Phase 17: spirit-boss-animation-integration - Research

**Researched:** 2026-10-01
**Domain:** Unity 6 Animator parameter wiring in an existing C# state-machine boss (WaterSpirit)
**Confidence:** HIGH (all findings come from reading the repo code and the controller YAML; no external library involved)

## Summary

This phase is a thin script wiring job. The assets already exist: `WaterSpirit.controller` has Bool `Move`/`Groggy` and Trigger `Charge/Repel/Clone/Ranged/Stealth/Hit/Death`, plus `WaterSpirit_Stage2.overrideController`. The prefab Animator already points at the controller. `BossController` already exposes `public Animator Anim`, auto-resolved in Awake. The WaterMonster pattern is `if (boss.Anim != null) boss.Anim.SetTrigger(AnimationName);` at the start of `ExecuteAttack`.

There are three traps that a naive "copy the WaterMonster pattern" would hit:
1. **`AnimationName` is also used for attack-completion gating.** `CombatState.Execute` calls `boss.CheckAnimationState(_currentAttack.AnimationName)`. With `""` this returns true immediately, so the attack ends at once. This is the current timing behavior. Putting a trigger name or a state name in `AnimationName` changes gating and can regress the state machine or hit the timeout path.
2. **`SpiritStats.Die()` calls `gameObject.SetActive(false)` immediately**, so a Death clip never plays unless the deactivation is deferred.
3. **The clone prefab (`Assets/Resources/Spirit Clone.prefab`) has an Animator with `m_Controller: {fileID: 0}`.** `SetTrigger`/`SetBool` on it logs warnings every call. Guard on `runtimeAnimatorController != null`.

**Primary recommendation:** Add one small helper on `SpiritController` (`PlayTrigger(string)` / `SetMoveBool(bool)`) that null-guards `Anim` and `Anim.runtimeAnimatorController`. Call it from each attack strategy and state. Leave every `AnimationName` as `""`. Swap `Anim.runtimeAnimatorController` to the Stage2 override in `OnStage2Trigger()`. Defer `SetActive(false)` in `Die()` by the Death clip length.

## User Constraints

No CONTEXT.md exists for this phase. Constraints come from ROADMAP and CLAUDE.md:
- Logic and state machine must not regress.
- Projectile and effect sprites are out of scope.
- v2.0 "no animation" constraint is waived for this phase only.
- Phase isolation: write nothing for later phases.

### Project Constraints (from CLAUDE.md)
- Plan before coding; atomic tasks. Minimal code only, with no speculative abstraction or flexibility.
- Surgical changes: do not refactor or reformat adjacent code. Match existing style. Remove only the orphans you create.
- Every changed line must trace to the plan.
- Verifiable goals: state each step as `[step] -> verify: [how]`.
- Do not continue to the next phase on your own after finishing. Summarize the results and wait.
- Per STATE.md, 46 `.cs` files in the project are CP949-encoded. The WaterSpirit files are NOT among them (audit: WaterSpirit 0), so UTF-8 edits are safe there. `GroggyState.cs` and `IdleState.cs`/`ChaseStates.cs` in `Assets/Enemy/NewBoss/Script/States` have garbled Korean comments, which means they ARE CP949. Do not edit them with a tool that re-encodes the file (see Pitfall 5).

<phase_requirements>
## Phase Requirements
No requirement IDs (TBD). Planner should derive the success criteria from the ROADMAP goal:
| Behavior | Research Support |
|----------|------------------|
| State-matched animation plays (Idle/Move/Charge/Repel/Clone/Ranged/Stealth/Hit/Groggy/Death) | Hook-point map below |
| Stage 2 entry swaps to the override controller | Pattern 2 |
| No logic or state-machine regression | Pitfalls 1, 2 |
</phase_requirements>

## Standard Stack

No new packages. Everything is built in: `UnityEngine.Animator`, `RuntimeAnimatorController`, `AnimatorOverrideController`. Unity 6 (the code uses `Rigidbody2D.linearVelocity`).

Use `Animator.StringToHash` constants only if you want them; WaterMonster uses plain strings, so plain strings that match the existing style are fine.

## Architecture Patterns

### Controller graph (parsed from WaterSpirit.controller)
- Entry state is Idle.
- Idle to Move is on Bool `Move` (true). Move to Idle is on `Move` false.
- Any State to Death on trigger `Death`, no exit time. This looks like a terminal state.
- Any State to Groggy on Bool `Groggy` true.
- Any State to Hit on trigger `Hit`, and to Clone on trigger `Clone`.
- Charge/Repel/Ranged/Stealth/Clone/Hit return to Idle by exit time. This is inferred from one transition with no condition and `HasExitTime: 1`. Verify in the Animator window during Play.
- Note: Groggy being a Bool means you must set it back to false on Groggy exit, or the controller stays in Groggy.

### Hook-point map (the actual work)

| Anim | Where to call | Notes |
|------|---------------|-------|
| Move (Bool) | `ChaseState` calls `boss.MoveTo`; `IdleState` and the `StopMove()` paths stop. Simplest: in `SpiritController.Update`, `SetBool("Move", _rb.linearVelocity.sqrMagnitude > 0.01f)`. | Avoids editing the shared CP949 `ChaseStates.cs`/`IdleState.cs`. Charge sets velocity directly, so the Move bool will also go true during a charge. Charge is a trigger state reached from Any State, so this is acceptable. Confirm visually. |
| Charge (Trigger) | Top of `SpiritCharge.ChargeRoutine` or `ExecuteAttack` | Teleport happens first, then windup. Trigger at `ExecuteAttack` start, so the windup animation covers the 0.5s windup. |
| Repel (Trigger) | `SpiritRepel.ExecuteAttack` (damage is applied here) and/or `SpiritWakeRepel` at the start of the 0.4s wake delay | Pick one. Trigger in `SpiritWakeRepel.WakeRepelRoutine` start gives a wind-up. `SpiritRepel` is also called directly, so check for a double trigger. |
| Ranged (Trigger) | `SpiritFarProjectile.FarProjectileRoutine` after teleport (aim wait 0.4s), or `SpiritProjectileAttack.ExecuteAttack` at the spawn instant | Prefer the aim wait, matching the windup. |
| Stealth (Trigger) | Start of `SpiritStealth.StealthRoutine` | `HeavyComboRoutine` calls `StealthRoutine` directly, so a hook inside the routine covers both paths. |
| Clone (Trigger) | `Stage2CombatState.Enter` or `SpawnClones`, on the real boss only (`!IsDummy`) | |
| Hit (Trigger) | Subscribe to `Stats.OnDamageTaken` in `SpiritController`, or call it in `SpiritStats.TakeDamage` after `InvokeOnDamageTaken()` | `BossController.Start` already subscribes `HandleDamageTaken` (private). Add a second subscription in `SpiritController`.Start override, and unsubscribe in OnDestroy. Dummies return before the event fires, so there is no Hit on clones. |
| Groggy (Bool) | `GroggyState.Enter/Exit` is shared and CP949. Alternative: `SpiritController.Update` sets `SetBool("Groggy", CurrentState is GroggyState)`. | Same non-intrusive polling approach as Move. |
| Death (Trigger) | `SpiritStats.Die()` | Needs a deferral (Pitfall 2). |
| Idle | Default state; no call needed. | |

Note: `SpiritController.Update` is already overridden and calls `base.Update()`, so it is the natural place for state polling (Move and Groggy). It runs per frame, so cache the hashes or accept strings.

### Pattern 1: guarded helper on SpiritController
```csharp
// Source: repo pattern, Assets/Enemy/WaterMonster/Script/States/Attacks/WaterRangedSpit.cs
public void PlayAnim(string trigger)
{
    if (Anim != null && Anim.runtimeAnimatorController != null) Anim.SetTrigger(trigger);
}
```
Strategies call `spirit.PlayAnim("Charge")` after the existing `boss is SpiritController spirit` cast. Several strategies already do this cast.

### Pattern 2: Stage 2 controller swap
```csharp
// Add a field: [Header("Animation")] public RuntimeAnimatorController Stage2AnimController;
public void OnStage2Trigger()
{
    if (IsStage2) return;
    IsStage2 = true;
    if (Anim != null && Stage2AnimController != null) Anim.runtimeAnimatorController = Stage2AnimController;
    ChangeState(new Stage2CombatState());
}
```
Assign `WaterSpirit_Stage2.overrideController` to that field in the prefab, via the Inspector or a Unity CLI/YAML edit (the asset step was done via the Unity CLI). An `AnimatorOverrideController` is a `RuntimeAnimatorController`, so this assignment is valid.
Behavior to know: swapping `runtimeAnimatorController` at runtime resets the Animator state back to the default state (Idle). Parameters are reset to defaults as well, so Bool `Move`/`Groggy` go false and pending Triggers are lost. Stage-2 entry is followed immediately by `ChangeState(new Stage2CombatState())`, which is fine. Set the controller BEFORE firing the Clone trigger in the same frame.
Clones: `Spirit Clone.prefab` has no controller. Clones are spawned after IsStage2, so if clone visuals should animate, assign a controller to the Clone prefab. This is optional and may be out of scope. The guard in Pattern 1 makes it safe either way. Flag it to the user as an open question.

### Anti-Patterns to Avoid
- **Setting `AnimationName` to a state name or trigger name.** It feeds `CheckAnimationState`, which gates attack completion. Leave it `""` to preserve timing (Pitfall 1).
- **Calling `Anim.SetTrigger` unguarded** (clone prefab warning spam).
- **Editing the shared `GroggyState`/`IdleState`/`ChaseState`** (CP949, shared by other bosses: WaterMonster, TutorialBoss). Poll from `SpiritController.Update` instead.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead |
|---------|-------------|-------------|
| Stage 2 sprite variants | Per-clip `if (IsStage2)` branching | `AnimatorOverrideController` swap (already built) |
| Waiting for a clip to finish | A custom timer | `Anim.GetCurrentAnimatorStateInfo(0).length`, or a fixed delay |
| Playing an animation by state name | `Anim.Play("...")` | Parameters, as the controller is designed |

## Common Pitfalls

### Pitfall 1: AnimationName feeds attack gating
**What goes wrong:** Setting `AnimationName = "Charge"` makes `CheckAnimationState("Charge")` wait for the state, and the attack extends until the clip completes or `MaxAttackDuration` triggers a timeout warning. The pattern cadence changes.
**How to avoid:** Keep `AnimationName => ""`, call the trigger directly. Verify with the Check.md-style log: no new "애니메이션 타임아웃" warnings.

### Pitfall 2: Death never visible
**What goes wrong:** `Die()` runs `SaveOnBossDefeated` and `SetActive(false)` in the same frame as the Death trigger.
**How to avoid:** In `Die()`, play Death, keep `CleanupClones()` and the save call as is, and defer only `SetActive(false)`. A coroutine on a MonoBehaviour that is itself being deactivated stops, so run it from a component that stays active until the end (the `SpiritController` on the same object works, because it is deactivated by the coroutine's final line). Also stop the boss before the Death clip: `StopMove`, and ChangeState to a no-op or disable attacks, otherwise the `CombatState` keeps attacking during the Death clip. Make sure Death does not re-enter `Die()` (a second hit while dying; the `_currentHealth <= 0` path calls `Die()` again). Add a `_dying` guard. This is the only logic-touching change, so keep it minimal and verify the boss-defeated save and the Phase 15 `IsBossDefeated` Awake check still work.

### Pitfall 3: Clone prefab without a controller
See above. Guard on `runtimeAnimatorController`.

### Pitfall 4: Controller swap resets the state
Pending triggers and bools are cleared. Do the swap first.

### Pitfall 5: Encoding and shared files
Do not touch the CP949 shared states. WaterSpirit scripts are UTF-8, so edit them normally. Check `git diff --stat` for unexpected whole-file diffs.

### Pitfall 6: Trigger left pending
A trigger set while the Animator is in a state with no matching transition stays set and fires later. Because the Any State transitions exist for Hit/Clone/Death, those are fine. Charge/Repel/Ranged/Stealth transitions are presumably Any State too. Verify, and if a stale trigger shows up, call `ResetTrigger`.

### Pitfall 7: Flip and scale
`LookAtTarget` flips via `localScale.x`. The animation does not touch the scale. Verify the clips do not animate scale or position (a pivot-at-collider-center setup was done in the asset step).

## Code Examples
Move and Groggy polling in `SpiritController.Update`:
```csharp
protected override void Update()
{
    base.Update();
    if (Anim != null && Anim.runtimeAnimatorController != null)
    {
        Anim.SetBool("Move", _rb.linearVelocity.sqrMagnitude > 0.01f);
        Anim.SetBool("Groggy", CurrentState is GroggyState);
    }
    // ...existing interceptor unchanged
}
```
Death deferral (sketch):
```csharp
protected override void Die()
{
    if (_dying) return;
    _dying = true;
    // existing CleanupClones + SaveOnBossDefeated unchanged
    var c = GetComponent<SpiritController>();
    if (c != null && c.Anim != null && c.Anim.runtimeAnimatorController != null && gameObject.activeInHierarchy)
        c.PlayDeath(); // StopMove, Anim.SetTrigger("Death"), coroutine WaitForSeconds(len) then SetActive(false)
    else gameObject.SetActive(false);
}
```

## Runtime State Inventory
Not a rename or migration phase. Skipped.

## Environment Availability
| Dependency | Required By | Available | Fallback |
|------------|------------|-----------|----------|
| Unity Editor (Play mode) | Verification | Not probed from the shell; the user has used the Unity CLI in this project | Verification is a manual checkpoint (the project already uses this pattern: Phases 12/16 "Play mode verification") |

## Validation Architecture
Skipped. `workflow.nyquist_validation` is `false` in `.planning/config.json`. Note: the project has no automated test coverage for this; the verification is Play mode (a manual checklist) plus static grep checks (for example, that no `AnimationName` value changed from `""`, and that `ChaseStates.cs`/`GroggyState.cs` are untouched in `git diff`).

Suggested Play-mode checklist: Idle then Move on chase; each of the 4 stage-1 patterns plays its clip; Hit on damage; Groggy plays (note SpiritCombatState never goes to Groggy by barrier; it appears after the Stage 2 heavy combo); Stage 2 entry changes the sprites and spawns clones; Stage 2 clips differ; Death clip is visible before disappearing; no new Console warnings, such as the timeout warning or "Animator is not playing an AnimatorController"; Check.md items (random pattern selection, Exhaustion to WakeRepel chain, Stage 2 cycle) still pass.

## State of the Art
Nothing time-sensitive. `Animator.runtimeAnimatorController` assignment of an `AnimatorOverrideController` is the long-standing supported approach.

## Open Questions
1. **Should clones animate?** The clone prefab has no controller, so clones are static sprites. Recommendation: out of scope unless the user wants it. If yes, assign the Stage2 override controller to the clone prefab (clones only exist in Stage 2).
2. **Exhaustion and "vulnerable" visuals.** No clip maps to `SpiritExhaustion` (it only does StopMove). Recommendation: it stays Idle.
3. **Which clip for Repel timing.** WakeRepel (0.4s lead-in) versus Repel (the damage instant). Recommend the trigger at the WakeRepel start (Repel anim covers the lead-in). Check that the Repel clip length is about 0.4s or more.
4. **Is Death trigger deferral acceptable** given it touches `Die()`? It is required to make Death visible; flag to the user.
5. **Groggy trigger conditions.** `GroggyState` is only reached in Stage 2 (after the heavy combo). `ShouldTransitionToGroggy` is false for the spirit.
6. **Clip lengths and the transitions** (exit time values) are inferred from partial YAML parsing (MEDIUM). Confirm in the Animator window.

## Sources
### Primary (HIGH, repo files read)
- `Assets/Enemy/WaterSpirit/Script/SpiritController.cs`, `SpiritStats.cs`, `States/SpiritCombatState.cs`, `States/Stage2CombatState.cs`, `States/Attacks/*.cs`
- `Assets/Enemy/NewBoss/Script/BossController.cs`, `States/CombatState.cs`, `GroggyState.cs`, `IdleState.cs`, `ChaseStates.cs`
- `Assets/Enemy/WaterMonster/Script/States/Attacks/WaterRangedSpit.cs` (pattern)
- `Assets/Enemy/WaterSpirit/Animations/WaterSpirit.controller`, `WaterSpirit_Stage2.overrideController`
- `Assets/Resources/Spirit Clone.prefab` (Animator with no controller), `Assets/Script/Combat/CombatSpawner.cs`
- `.planning/ROADMAP.md` Phase 17 section; `.planning/STATE.md`

### Secondary / Tertiary
None. Unity API behavior (controller swap resets state; SetTrigger warning on a controller-less Animator) is from my training knowledge (MEDIUM). Verify in Play mode.

## Metadata
**Confidence breakdown:**
- Standard stack: HIGH, no new dependencies.
- Architecture and hook points: HIGH for the code facts, MEDIUM for the controller transition details (parsed with a script, not viewed in the editor).
- Pitfalls: MEDIUM-HIGH. Pitfalls 1 to 3 are verified from the code; 4 and 6 depend on Unity behavior.

**Research date:** 2026-10-01
**Valid until:** 30 days (stable code; invalidated if SpiritController or CombatState change)
