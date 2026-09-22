---
phase: 16-iitem-itemdata-scriptableobject-id-useeffect-ui
verified: 2026-09-21T00:00:00Z
status: passed
score: 10/10 must-haves verified
---

# Phase 17: 아이템 코어 (IItem + ItemData ScriptableObject) Verification Report

**Phase Goal:** Project's first ScriptableObject `ItemData` exists, implementing `IItem` directly (no separate
runtime item class), schematizing item static data into exactly 4 fields (`id`/`type`/`effectType`/`amount`,
D-01~D-06), with `UseEffect` fully wired for Consumable/Heal (reusing existing `PlayerStats.Heal`) and a
no-op for Progression, plus two working example `.asset` files, verified to actually work in the Unity
editor and at runtime (D-07), not just statically well-formed.

**Verified:** 2026-09-21
**Status:** passed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | `ItemData` exists as `ScriptableObject, IItem` (no separate runtime class) | ✓ VERIFIED | `Assets/Item/Script/ItemData.cs:7` `public class ItemData : ScriptableObject, IItem` |
| 2 | Schema is exactly 4 serialized fields: id/type/effectType/amount | ✓ VERIFIED | `grep -cE "^\s*\[SerializeField\]"` = 4; forbidden-field grep (`displayName\|icon\|description\|stackable\|maxStack`) = 0 |
| 3 | Consumable+Heal `UseEffect` calls `PlayerStats.Heal(amount)` and actually restores health | ✓ VERIFIED | Code: `player.GetComponent<PlayerStats>().Heal(amount);` (no null guard, matches project convention). Runtime: Check.md item 7, `before=50 -> after=70` (exactly +20), 0 exceptions |
| 4 | Progression `UseEffect` is a true no-op (no exception) | ✓ VERIFIED | Code: `if (type != ItemType.Consumable) return;`. Runtime: Check.md item 8, `before=50 -> after=50`, 0 exceptions |
| 5 | Two real `.asset` files exist under `Assets/Item/`, referencing `ItemData` | ✓ VERIFIED | `Assets/Item/HealthPotion.asset` (Consumable/Heal/20), `Assets/Item/AncientKey.asset` (Progression); both `m_Script` guid `501b19c5008706d0a3f2bf69aacf52f6` matches `ItemData.cs.meta` |
| 6 | Unity 6000.3.10f1 actually imports `Assets/Item/` with 0 compile/import/missing-script errors | ✓ VERIFIED | Check.md static regression table item 8: PASS, batch-mode import, zero diff after import |
| 7 | Clamp behavior at max health works (`PlayerStats.ClampHealth`) | ✓ VERIFIED | Check.md item 9: `before=100 -> after=100` after HealthPotion use at full health |
| 8 | No pre-existing files were modified (PlayerStats.cs, PlayerInteraction.cs, IItem.cs, SaveData.cs) | ✓ VERIFIED | `git diff 58ac900 HEAD --stat` on all four files: empty diff |
| 9 | `ItemData` is the project's only ScriptableObject | ✓ VERIFIED | `grep -rn "ScriptableObject" Assets --include=*.cs` matches only `ItemData.cs` |
| 10 | Verification evidence is documented for Phase 18 developers | ✓ VERIFIED | `Assets/Item/Check.md` (75 lines): static regression table + 10-item Play-mode checklist, all checked with concrete before/after values |

**Score:** 10/10 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Assets/Item/Script/ItemData.cs` | SO implementing IItem, 4 fields, UseEffect, editor hook | ✓ VERIFIED | 67 lines; matches plan verbatim except one reworded comment (documented, non-functional) |
| `Assets/Item/Script/ItemData.cs.meta` | Fixed guid `501b19c5008706d0a3f2bf69aacf52f6` | ✓ VERIFIED | guid matches exactly |
| `Assets/Item/HealthPotion.asset` | Consumable/Heal example, id=health_potion_01, amount=20 | ✓ VERIFIED | All fields match; `m_Script` guid correct |
| `Assets/Item/AncientKey.asset` | Progression example, id=ancient_key_01 | ✓ VERIFIED | All fields match; `m_Script` guid correct |
| `Assets/Item/Check.md` | Static regression table + Play-mode checklist + results | ✓ VERIFIED | 75 lines (>60 min), 10/10 checklist items checked with real values, honest deviation notes |
| `Assets/Item/Check.md.meta` | TextScriptImporter meta | ✓ VERIFIED | guid `05511fb05942d6634e84eccc8a623d1f`, `TextScriptImporter:` present |
| `Assets/Item.meta`, `Assets/Item/Script.meta`, `Assets/Item/Script/IItem.cs.meta` | Recovered untracked metas, guid-stable | ✓ VERIFIED | `IItem.cs.meta` guid `1a97ea82c14ac9e449d2baeb51e3ba6b` present and unchanged after Unity re-import |

### Key Link Verification

| From | To | Via | Status | Details |
|------|-----|-----|--------|---------|
| `ItemData.cs` | `PlayerStats.Heal(float)` | `player.GetComponent<PlayerStats>().Heal(amount)` | ✓ WIRED | Exact string match, no null guard (project convention) |
| `HealthPotion.asset` | `ItemData.cs` | `m_Script` guid reference | ✓ WIRED | guid `501b19c5008706d0a3f2bf69aacf52f6` matches |
| `AncientKey.asset` | `ItemData.cs` | `m_Script` guid reference | ✓ WIRED | guid `501b19c5008706d0a3f2bf69aacf52f6` matches |
| `Check.md` | `ItemData.UseOnPlayerFromInspector()` ContextMenu hook | Play-mode checklist items 7/8 | ✓ WIRED | `Phase17: Use On Player` referenced 3x in Check.md |
| Unity batch-mode import log | `Assets/Item/*.asset` | Real editor import | ✓ WIRED | 0 compile errors, 0 missing-script, 0 diff after import (documented in Check.md item 8) |

### Data-Flow Trace (Level 4)

Not applicable in the strict sense (no UI/dynamic rendering component this phase — data layer only, per
phase scope). The equivalent trace performed here is runtime effect verification: `ItemData.UseEffect()`
was invoked live against a real `PlayerStats` component (via unity-mcp driving the open Unity Editor, not
a static grep), and the resulting health values (50->70, 50->50, 100->100) are concrete runtime evidence
that the `.asset` YAML values (`amount: 20`, `type: 0/1`) actually flow through `SerializedField` deserialization into a live `MonoBehaviour`'s Inspector-invoked method call — not just present in the YAML text.

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|---------------|--------|---------------------|--------|
| `ItemData.UseEffect()` | `amount`, `type`, `effectType` (SerializeField) | `.asset` YAML deserialized by Unity at runtime | Yes — health changed by exactly the configured `amount` (20), and Progression items produced exactly 0 change | ✓ FLOWING |

### Requirements Coverage

Per 17-CONTEXT.md and ROADMAP.md, this phase's requirements are locked via CONTEXT.md decisions D-01
through D-07 rather than formal `REQ-ID` entries in REQUIREMENTS.md. Confirmed: `grep -n "Phase 17\|D-0[1-7]" .planning/REQUIREMENTS.md` returns no matches — REQUIREMENTS.md does not track this phase, consistent
with the init output's stated approach. No orphaned requirements.

| Requirement | Source Plan | Description | Status | Evidence |
|--------------|------------|--------------|--------|----------|
| D-01 | 17-01 | ItemData implements IItem directly, no separate runtime class | ✓ SATISFIED | `ItemData : ScriptableObject, IItem` |
| D-02 | 17-01, 17-02 | UseEffect fully implemented (not stub); Consumable applies effect, Progression no-op | ✓ SATISFIED | Code + runtime evidence (Check.md items 7/8) |
| D-03 | 17-01, 17-02 | Heal is only consumable effectType this phase; reuses existing PlayerStats.Heal | ✓ SATISFIED | `PlayerStats.cs` 0-diff confirmed, `Heal(amount)` called directly |
| D-04 | 17-01 | ItemType = 2 values (Consumable/Progression), no SO subclassing | ✓ SATISFIED | enum has exactly 2 values, no subclass grep matches |
| D-05 | 17-01 | id is manually-typed `[SerializeField] string id`, no auto-gen/validation | ✓ SATISFIED | Field present as specified |
| D-06 | 17-01 | Exactly 4 fields, no display metadata | ✓ SATISFIED | 4 `[SerializeField]` fields, 0 forbidden-field matches |
| D-07 | 17-01, 17-02 | Two real .asset files exist and demonstrably work | ✓ SATISFIED | 2 assets exist, guid-wired, and runtime-verified via Play mode |

### Anti-Patterns Found

None blocking. Scan of `Assets/Item/Script/ItemData.cs` for TODO/FIXME/placeholder/stub patterns, empty
handlers, and hardcoded-empty returns found none. `UseEffect` is a real implementation, not `return null`/
`{}`/console-log-only.

ℹ️ Info: One explanatory comment in `ItemData.cs` was reworded during execution from the plan's literal
text to avoid self-matching the plan's own forbidden-field grep gate (`displayName|icon|description`).
Meaning preserved, no schema/logic change. Documented in 17-01-SUMMARY.md.

ℹ️ Info: Play-mode checklist item 6 (making the player take damage) was reproduced via `SerializedObject`
setting `health=50` directly rather than organic combat damage, because `PlayerStats.TakeDamage()` calls
an unguarded `CameraController.Instance.Shake()` that would NPE outside a real scene camera. This is
documented as an equivalent-condition substitution in Check.md, not silently glossed over — the resulting
before/after health values (50->70, 50->50, 100->100) are the load-bearing evidence regardless of how the
initial "damaged" state was produced.

ℹ️ Info: Two Console errors unrelated to Phase 17 (`InputHandler` missing Input Action Asset,
`TutorialBoss` missing Animator) appeared after exiting Play mode. Traced to pre-existing scene state,
unrelated to any `Assets/Item/` file or the test GameObject created during verification. Documented
honestly in Check.md rather than claimed as "0 errors" — does not affect this phase's goal, which was
scoped to `Assets/Item/`.

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| ItemData is the sole ScriptableObject in the project | `grep -rn "ScriptableObject" Assets --include=*.cs \| grep -v ItemData.cs` | empty | ✓ PASS |
| Protected files unmodified since IItem baseline | `git diff 58ac900 HEAD --stat -- PlayerStats.cs PlayerInteraction.cs IItem.cs SaveData.cs` | empty diff | ✓ PASS |
| Both example assets reference correct script guid | `grep -c "guid: 501b19c5008706d0a3f2bf69aacf52f6" HealthPotion.asset AncientKey.asset` | 1, 1 | ✓ PASS |
| No U+FFFD encoding corruption in new files | `grep -rc $'\xef\xbf\xbd' Assets/Item/*.asset Assets/Item/Script/*.cs` | 0 for all | ✓ PASS |
| UseEffect actually mutates runtime health (Play mode) | unity-mcp live Editor invocation, recorded in Check.md | before=50->after=70 (Heal), before=50->after=50 (no-op), before=100->after=100 (clamp) | ✓ PASS |

Unity batch-mode compile/import gate (Step 7b equivalent) was already run as part of Plan 17-02 Task 1 and
is not re-run here since re-running the editor is outside this verifier's scope; its documented log results
(0 compile errors, 0 Assets/Item import errors, 0 missing-script) are treated as artifact evidence per
Check.md's static regression table, cross-checked here against the actual `.asset`/`.meta` file contents
which match exactly what the log claims.

### Human Verification Required

None. All 10 Play-mode checklist items were completed with concrete recorded evidence (unity-mcp driving
a live, already-open Unity Editor at the user's explicit request — a legitimate, documented substitution
for manual GUI clicking, not a shortcut that was waved away). The deviation is fully traceable in
17-02-SUMMARY.md and Check.md, including the one substituted step (item 6) and the two unrelated,
out-of-scope Console errors (item 10).

### Gaps Summary

No gaps. Both plans' must_haves (truths, artifacts, key_links) are verified against the current codebase
state, not just SUMMARY claims. All 7 CONTEXT.md decisions (D-01~D-07) and all 10 ROADMAP.md success
criteria are satisfied with concrete evidence — static (grep against actual file contents) and dynamic
(recorded Play-mode health values from a live Unity Editor session). The Play-mode verification method
deviated from "manual GUI click-through" to "unity-mcp-driven live Editor scripting" per explicit user
request mid-execution; this is documented as a legitimate methodology change producing equivalent evidence,
not a shortcut — every checklist item carries a concrete observed value rather than an assumed PASS.

---

*Verified: 2026-09-21*
*Verifier: Claude (gsd-verifier)*
