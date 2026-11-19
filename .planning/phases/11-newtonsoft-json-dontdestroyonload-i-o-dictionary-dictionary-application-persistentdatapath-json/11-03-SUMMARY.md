---
phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json
plan: 03
subsystem: save-system
tags: [checkpoint, boss-defeat, save-trigger, hp-ondeath, bossstatssystem-die]

# Dependency graph
requires:
  - phase: 11-02
    provides: "SaveLoadManager singleton with public SaveAtCheckpoint(string) and SaveOnBossDefeated(string) API"
provides:
  - "Checkpoint.cs S-key activation now calls SaveLoadManager.Instance.SaveAtCheckpoint(gameObject.name)"
  - "All four boss controllers now call SaveLoadManager.Instance.SaveOnBossDefeated(bossId) at their respective death entry points"
affects: [11-04]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Two boss-defeat integration shapes coexist: Group A (HP.OnDeath event already subscribed - insert into existing HandleDeath()) vs Group B (BossStatsSystem.Die() has no event - insert directly inside the Die() override)"

key-files:
  created: []
  modified:
    - Assets/map/script/Checkpoint.cs
    - Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs
    - Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs
    - Assets/Enemy/WaterSpirit/Script/SpiritStats.cs
    - Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs

key-decisions:
  - "Two of the five target files (Checkpoint.cs, WoodBossController.cs) are CP949-encoded. The standard Edit tool round-trips file content through UTF-8 in memory, which silently replaces every CP949 non-ASCII byte sequence in the ENTIRE file (not just the touched lines) with U+FFFD replacement-character bytes on write-back - a corruption that is invisible in `grep -cP \"[^\\x00-\\x7F]\"` line-count checks (count stays the same) but destroys the actual Korean text. Fixed by extracting the original bytes via `git show HEAD:<path>`, performing the insertion with a byte-level Python script (binary read/write, ASCII-only anchor/insert text, no decode/encode step), then overwriting the corrupted working-tree file with the byte-correct result. Verified via `git diff --numstat` showing pure insertion (0 deletions) and via raw `xxd` comparison against the original CP949 bytes."
  - "A pre-existing, already-staged, unrelated rename (BossZoomTrigger.cs -> CameraZoomTrigger.cs + its .meta) was sitting in the git index before this plan started (prior in-progress camera work, not part of this plan). The first task commit used a bare `git commit -m` after `git add <file>`, which committed the ENTIRE index including that unrelated staged rename. Fixed immediately with `git reset --soft HEAD~1` (restores the index to its pre-commit state without touching the working tree) followed by `git commit -m \"...\" -- <specific path(s)>` so only the plan's own file(s) land in the commit. All three task commits in this plan now use explicit trailing `-- <paths>` pathspecs to prevent recurrence."

patterns-established:
  - "For any future edit to a CP949-encoded file in this project: never use the Read+Edit UTF-8 round-trip tool pair. Extract bytes via `git show HEAD:<path>`, insert ASCII-only text via a raw byte-level script, then overwrite the working-tree file with the result."
  - "When the git index may already contain unrelated staged changes (multi-plan / multi-agent working tree), always commit task files with an explicit `-- <pathspec>` rather than a bare `git commit`, even after `git add <file>` for only the intended file."

requirements-completed: [D-01, D-05]

# Metrics
duration: 15min
completed: 2026-08-10
---

# Phase 11 Plan 03: Checkpoint/Boss Save Trigger Integration Summary

**Five save-trigger call sites wired into existing code (1 checkpoint + 4 bosses) calling the already-built SaveLoadManager API, with a CP949 encoding-corruption bug caught and fixed via byte-level insertion, and a git index cross-contamination bug caught and fixed via scoped reset+recommit.**

## Performance

- **Duration:** 15 min
- **Started:** 2026-08-10T10:45:00+09:00 (approx.)
- **Completed:** 2026-08-10T10:59:19+09:00
- **Tasks:** 3
- **Files modified:** 5

## Accomplishments
- `Checkpoint.cs`: S-key checkpoint activation now calls `SaveLoadManager.Instance.SaveAtCheckpoint(gameObject.name)` immediately after `isActiveCheckpoint = true`, null-guarded
- Group A (`HP.OnDeath` already subscribed) - `TutorialBossController.HandleDeath()` and `WoodBossController.HandleDeath()` each call `SaveLoadManager.Instance.SaveOnBossDefeated("TutorialBoss"/"WoodBoss")` before their existing dead-state transition / death coroutine start, with zero new event subscriptions added
- Group B (no `OnDeath` event exists on `BossStatsSystem`) - `SpiritStats.Die()` and `WaterMonsterStats.Die()` each call `SaveLoadManager.Instance.SaveOnBossDefeated("WaterSpirit"/"WaterMonster")` directly inside their `Die()` override, immediately before `gameObject.SetActive(false)`
- All five insertions are pure (0 deletions) - verified via `git diff --numstat` per file
- All five save triggers connected: `grep -rn "SaveLoadManager.Instance.SaveAtCheckpoint" Assets/` -> 1 hit, `grep -rn "SaveLoadManager.Instance.SaveOnBossDefeated" Assets/` -> 4 hits with 4 distinct bossIds

## Task Commits

Each task was committed atomically:

1. **Task 1: Checkpoint.cs checkpoint-activation save trigger** - `7e2960e` (feat)
2. **Task 2: Group A (HP.OnDeath) - TutorialBoss / WoodBoss defeat save trigger** - `e36a76c` (feat)
3. **Task 3: Group B (BossStatsSystem.Die override) - WaterSpirit / WaterMonster defeat save trigger** - `1fcf28a` (feat)

**Plan metadata:** (pending - this commit)

## Files Created/Modified
- `Assets/map/script/Checkpoint.cs` - S-key activation branch now calls `SaveLoadManager.Instance.SaveAtCheckpoint(gameObject.name)` (CP949, byte-safe insertion)
- `Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs` - `HandleDeath()` now calls `SaveLoadManager.Instance.SaveOnBossDefeated("TutorialBoss")` before `ChangeState(new TutorialDeadState())`
- `Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs` - `HandleDeath()` now calls `SaveLoadManager.Instance.SaveOnBossDefeated("WoodBoss")` before `StartCoroutine(DeathSequence())` (CP949, byte-safe insertion)
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs` - `Die()` override now calls `SaveLoadManager.Instance.SaveOnBossDefeated("WaterSpirit")` before `gameObject.SetActive(false)`
- `Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs` - `Die()` override now calls `SaveLoadManager.Instance.SaveOnBossDefeated("WaterMonster")` before `gameObject.SetActive(false)`

## Decisions Made
- CP949 file editing approach: abandoned the standard Read/Edit tool pair for `Checkpoint.cs` and `WoodBossController.cs` after discovering it corrupts every non-ASCII line in the file (UTF-8 round-trip converts CP949 byte sequences to literal U+FFFD replacement-character byte sequences on write-back). Detected via `xxd` byte comparison against `git show HEAD:<path>` after the first attempt on `Checkpoint.cs` produced a suspicious `git diff` showing every Korean comment line as "changed" despite looking visually identical. Fixed by re-doing the insertion at the byte level (Python, binary mode, ASCII-only inserted text, anchor matched as raw bytes) against the original `git show HEAD:<path>` bytes, then overwriting the corrupted working-tree file.
- Git index scoping: the working tree had a pre-existing, unrelated staged rename (`BossZoomTrigger.cs` -> `CameraZoomTrigger.cs` + meta) left over from in-progress camera work before this plan began (per project_notes warning). The first task commit (`git add <file>` then bare `git commit -m`) accidentally swept this staged rename into the commit. Caught immediately by inspecting `git show --stat HEAD`, fixed with `git reset --soft HEAD~1` (index preserved) followed by `git commit -m "..." -- <specific paths>`. All subsequent task commits in this plan used explicit `-- <pathspec>` to prevent recurrence.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] CP949 encoding corruption from standard Edit tool on Checkpoint.cs**
- **Found during:** Task 1 (Checkpoint.cs insertion), post-edit `git diff` inspection
- **Issue:** The Edit tool's UTF-8 read/write round-trip silently replaced every CP949 non-ASCII byte sequence in the file with U+FFFD replacement-character byte sequences, even on lines never touched by the edit. The `grep -cP "[^\x00-\x7F]"` count-based acceptance check (==9) still passed because the number of non-ASCII lines was unchanged - only the actual bytes were destroyed, which the plan's own encoding_warning section anticipated as a risk but the count-based gate alone could not catch.
- **Fix:** Re-extracted the original bytes with `git show HEAD:Assets/map/script/Checkpoint.cs`, performed the 5-line insertion with a byte-level Python script (binary mode, ASCII-only anchor and inserted text, no encode/decode step), and overwrote the corrupted working-tree file with the byte-correct result. Verified via `git diff` showing a clean 5-line pure insertion and via `xxd` spot-check that untouched CP949 lines match the original bytes exactly.
- **Files modified:** `Assets/map/script/Checkpoint.cs` (fix applied before Task 1 commit, so the committed version is the corrected one)
- **Verification:** `git diff --numstat` == 5 insertions / 0 deletions; `grep -cP "[^\x00-\x7F]"` == 9; all Task 1 acceptance criteria re-run and passed
- **Committed in:** `7e2960e` (Task 1 commit - only the corrected version was ever committed)

**2. [Rule 1 - Bug] Same CP949 corruption risk pre-empted for WoodBossController.cs**
- **Found during:** Task 2 (Group A insertion), before editing
- **Issue:** Same class of corruption as above would have occurred if the standard Edit tool had been used on this second CP949 file.
- **Fix:** Applied the same byte-level Python insertion approach proactively (extract via `git show HEAD:<path>`, insert as raw ASCII bytes, overwrite working tree) instead of using the Edit tool.
- **Files modified:** `Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs`
- **Verification:** `git diff --numstat` == 4 insertions / 0 deletions; `grep -cP "[^\x00-\x7F]"` == 7
- **Committed in:** `e36a76c` (Task 2 commit)

**3. [Rule 3 - Blocking] First task commit swept in an unrelated pre-staged rename**
- **Found during:** Task 1 commit, immediate post-commit `git show --stat HEAD` inspection
- **Issue:** `git add "Assets/map/script/Checkpoint.cs"` followed by a bare `git commit -m "..."` (no pathspec) committed the entire git index, which already contained an unrelated staged rename (`BossZoomTrigger.cs` -> `CameraZoomTrigger.cs` + `.meta`) from pre-existing in-progress camera work outside this plan's scope - exactly the failure mode called out in the task's project_notes.
- **Fix:** `git reset --soft HEAD~1` to undo the commit while preserving the index exactly as it was, then `git commit -m "..." -- "Assets/map/script/Checkpoint.cs"` with an explicit pathspec so only the intended file was committed. The unrelated camera rename remains staged in the working tree, untouched, exactly as it was before this plan started.
- **Files modified:** None beyond the git history correction (working tree unaffected)
- **Verification:** `git show --stat HEAD` after the fix shows only `Assets/map/script/Checkpoint.cs`; `git status --short` shows the camera rename still staged and unmodified
- **Committed in:** `7e2960e` (replaces the reverted commit)

---

**Total deviations:** 3 auto-fixed (2 bugs - CP949 corruption risk, 1 blocking - git index scoping)
**Impact on plan:** No scope creep, no behavior change beyond what the plan specified. All three fixes were necessary to actually satisfy the plan's own stated constraints (pure insertion, encoding preservation, touching only the five listed files) rather than merely passing a count-based grep gate that could not detect byte-level corruption or index cross-contamination.

## Issues Encountered
None beyond the three self-corrected deviations documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All five save triggers (1 checkpoint + 4 boss defeats) are live and call into the `SaveLoadManager` public API built in 11-02
- Ready for Phase 11 Plan 04
- Unity compile verification (this execution environment cannot open the Unity Editor) is deferred to the user or a later checkpoint plan, consistent with prior plans in this phase
- The unrelated pre-existing staged camera-script changes (`BossZoomTrigger.cs` -> `CameraZoomTrigger.cs` rename, `CameraBoundsTrigger.cs`/`CameraController.cs` modifications, `Tutorial Map.unity` scene changes) remain exactly as they were before this plan - not committed, not modified, not investigated (out of scope per project_notes)

---
*Phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json*
*Completed: 2026-08-10*

## Self-Check: PASSED

All five modified files verified present on disk. All three task commits (`7e2960e`, `e36a76c`, `1fcf28a`) verified present in git history.
