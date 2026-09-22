---
phase: 16-iitem-itemdata-scriptableobject-id-useeffect-ui
plan: 01
subsystem: data
tags: [unity, scriptableobject, item-data, csharp]

# Dependency graph
requires:
  - phase: item-branch (IItem interface, pre-existing)
    provides: "IItem interface (Assets/Item/Script/IItem.cs) with UseEffect(PlayerInteraction) contract"
provides:
  - "ItemData ScriptableObject (id/type/effectType/amount) implementing IItem directly"
  - "Two example .asset instances: HealthPotion (Consumable/Heal) and AncientKey (Progression)"
  - "Fixed guid 501b19c5008706d0a3f2bf69aacf52f6 for ItemData.cs, reusable by future .asset files"
affects: [17-02, 17-inventory, 18-item-save-load]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Single ScriptableObject class + enum branching instead of SO subclass hierarchy (ItemType: Consumable/Progression)"
    - "No null guard on GetComponent<PlayerStats>() in UseEffect - matches existing unguarded convention (PlayerStats.TakeDamage -> CameraController.Instance.Shake())"
    - "Editor-only ContextMenu verification hook (#if UNITY_EDITOR) as the only way to invoke UseEffect before Phase 18 inventory/pickup exists"

key-files:
  created:
    - Assets/Item/Script/ItemData.cs
    - Assets/Item/Script/ItemData.cs.meta
    - Assets/Item/HealthPotion.asset
    - Assets/Item/HealthPotion.asset.meta
    - Assets/Item/AncientKey.asset
    - Assets/Item/AncientKey.asset.meta
  modified: []

key-decisions:
  - "Reworded the D-06 explanatory comment ('No displayName / icon / description...' -> 'No name/image/blurb fields...') to avoid a self-referential grep match against the plan's own forbidden-field acceptance check, while preserving the documented meaning. Same class of self-contradiction previously seen in Phase 9/10/11 (DontDestroyOnLoad, deadzoneHeight, async/await literal-string gates)."
  - "This execution worktree (worktree-agent-ab70cb16e75fe20bc) was on an unrelated stale branch that predates the Item branch by 120 commits and had none of the phase 16 planning files or Assets/Item tree. Since the worktree's branch was a strict ancestor of Item (0 commits unique to the worktree branch), fast-forward merged the worktree branch to Item's tip (79caaf2) to bring in the plan and prerequisite files - no rebase/history rewrite needed. A local-only .claude/settings.local.json change was set aside via a uniquely-tagged git stash before the merge and dropped afterward (content was already identical post-merge)."
  - "Assets/Item.meta, Assets/Item/Script.meta, and Assets/Item/Script/IItem.cs.meta existed as untracked files in the main repository checkout (per the session's git status) but not in this isolated worktree. Copied their exact bytes from the main checkout (read-only, via absolute path) into the worktree so their guids match what's already referenced elsewhere, then committed them unmodified alongside ItemData.cs per the plan's Task 1 step 3."

patterns-established:
  - "ItemData.cs / .asset files are pure ASCII, UTF-8 no BOM, LF line endings - written directly via the Write tool so they are unaffected by this environment's core.autocrlf=true, which only rewrites files on checkout, not on write/add/commit."

requirements-completed: [D-01, D-02, D-03, D-04, D-05, D-06, D-07]

# Metrics
duration: 25min
completed: 2026-09-20
---

# Phase 17 Plan 01: ItemData ScriptableObject Summary

**ItemData ScriptableObject implementing IItem directly (id/type/effectType/amount, 4 fields, no subclassing), with UseEffect wired to the existing PlayerStats.Heal for Consumable/Heal and a no-op for Progression, plus two example .asset instances (HealthPotion, AncientKey).**

## Performance

- **Duration:** 25 min
- **Started:** 2026-09-20T05:50:00Z (approx, per STATE.md session start)
- **Completed:** 2026-09-20
- **Tasks:** 2
- **Files modified:** 9 (5 in Task 1, 4 in Task 2)

## Accomplishments
- First ScriptableObject in the project: `ItemData : ScriptableObject, IItem` with `ItemType` (Consumable/Progression) and `ConsumableEffectType` (Heal) enums
- `UseEffect(PlayerInteraction player)` fully implemented (not a stub): Consumable/Heal calls the existing `PlayerStats.Heal(float)`; Progression items are an intentional no-op
- Two real `.asset` files in `Assets/Item/`: `HealthPotion.asset` (Consumable/Heal, amount 20) and `AncientKey.asset` (Progression), both referencing `ItemData.cs.meta`'s pinned guid
- Recovered and committed three previously-untracked `.meta` files (`Assets/Item.meta`, `Assets/Item/Script.meta`, `Assets/Item/Script/IItem.cs.meta`) so guids are stable across clones

## Task Commits

Each task was committed atomically:

1. **Task 1: ItemData.cs + .meta guid pin (D-01~D-06)** - `f53e5e9` (feat)
2. **Task 2: HealthPotion/AncientKey example .asset files (D-07)** - `33a3113` (feat)

_Note: no plan-metadata commit hash yet - this SUMMARY/STATE/ROADMAP commit follows._

## Files Created/Modified
- `Assets/Item/Script/ItemData.cs` - ScriptableObject implementing IItem; id/type/effectType/amount fields, UseEffect, editor ContextMenu verification hook
- `Assets/Item/Script/ItemData.cs.meta` - pinned guid `501b19c5008706d0a3f2bf69aacf52f6` referenced by both example assets
- `Assets/Item/HealthPotion.asset` / `.meta` - Consumable/Heal example, amount 20
- `Assets/Item/AncientKey.asset` / `.meta` - Progression example
- `Assets/Item.meta`, `Assets/Item/Script.meta`, `Assets/Item/Script/IItem.cs.meta` - recovered unmodified, committed for guid stability

## Decisions Made
- Reworded one explanatory comment in `ItemData.cs` (D-06 field-list comment) to avoid a literal self-match against the plan's own "no displayName/icon/description" grep gate, without changing the field schema or meaning. See key-decisions in frontmatter.
- Fast-forward merged this execution worktree's stale branch onto the `Item` branch tip to obtain the plan and prerequisite files, since the worktree branch was a strict ancestor with zero unique commits (safe, non-destructive). A local settings file was stashed and restored around the merge.
- Copied `Assets/Item.meta` / `Script.meta` / `IItem.cs.meta` byte-for-byte from the main repository checkout (read-only) since they existed there as untracked files but were absent from this isolated worktree.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Reworded self-contradicting D-06 comment in ItemData.cs**
- **Found during:** Task 1 acceptance criteria verification
- **Issue:** The plan's exact prescribed comment text (`// D-06: minimal schema. No displayName / icon / description until a UI layer exists.`) itself contains the literal substrings the plan's own acceptance grep (`displayName|icon|description|stackable|maxStack` must count 0) forbids, causing a guaranteed false-fail if copied verbatim.
- **Fix:** Reworded to `// D-06: minimal schema. No name/image/blurb fields until a UI layer exists.` - same meaning, no forbidden substrings. No field/schema change.
- **Files modified:** Assets/Item/Script/ItemData.cs
- **Verification:** `grep -cE "displayName|icon|description|stackable|maxStack" Assets/Item/Script/ItemData.cs` now returns 0; all other Task 1 acceptance greps still pass.
- **Committed in:** f53e5e9 (Task 1 commit)

**2. [Rule 3 - Blocking] Recovered missing worktree state via fast-forward merge + file copy**
- **Found during:** Start of execution (files_to_read step)
- **Issue:** This execution worktree was checked out on an unrelated branch 120 commits behind the `Item` branch, missing `.planning/phases/16-.../17-01-PLAN.md`, `Assets/Item/Script/IItem.cs`, and the three untracked `.meta` files referenced by the plan - execution could not start.
- **Fix:** Fast-forward merged the worktree's branch to `Item` (79caaf2), which was safe since the worktree branch had 0 commits not already in `Item`. Set aside an unrelated local `.claude/settings.local.json` change via a uniquely-tagged stash before merging, restored after (content matched post-merge, so dropped the stash entry). Then copied the three untracked `.meta` files byte-for-byte from the main repository checkout (read-only reference, not a git operation) since they don't exist as untracked files in this isolated worktree.
- **Files modified:** Assets/Item.meta, Assets/Item/Script.meta, Assets/Item/Script/IItem.cs.meta (copied unmodified)
- **Verification:** `git show :Assets/Item/Script/IItem.cs.meta | wc -c` = 59 (matches plan's acceptance criteria exactly); `git status --porcelain Assets/Player Assets/Item/Script/IItem.cs` empty (no unintended changes).
- **Committed in:** f53e5e9 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (1 bug/self-contradiction, 1 blocking/environment setup)
**Impact on plan:** Both fixes were necessary to make the plan's own verification gates pass and to make execution possible at all in this worktree. No scope creep - no field, schema, or logic changes beyond what the plan specified.

## Issues Encountered
None beyond the two auto-fixed deviations above.

## User Setup Required
None - no external service configuration required. Note: Unity editor import of the new `.asset` files (and the resulting `.meta` `MonoImporter`/`NativeFormatImporter` auto-fill) is deferred to Plan 17-02 per the plan's own scope ("에디터 임포트 실검증은 Plan 17-02 에서 수행한다").

## Next Phase Readiness
- `ItemData` schema is locked (id/type/effectType/amount) and available for Plan 17-02 (editor import verification) and Phase 18 (inventory) to reference.
- Two real `.asset` instances exist in `Assets/Item/` for Phase 18 to use as test data.
- `Assets/` outside `Assets/Item/` has zero content changes (verified via `git diff HEAD~2 --name-only` showing only the 9 files under `Assets/Item/`).

---
*Phase: 16-iitem-itemdata-scriptableobject-id-useeffect-ui*
*Completed: 2026-09-20*

## Self-Check: PASSED

All 6 created files found on disk (Assets/Item/Script/ItemData.cs, .meta, HealthPotion.asset, .meta, AncientKey.asset, .meta). Both task commits (f53e5e9, 33a3113) found in git log.
