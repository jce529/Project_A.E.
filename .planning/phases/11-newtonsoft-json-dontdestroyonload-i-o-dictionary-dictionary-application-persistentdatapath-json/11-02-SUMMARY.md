---
phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json
plan: 02
subsystem: save-system
tags: [newtonsoft-json, singleton, dontdestroyonload, coroutine, scene-load, persistentdatapath]

# Dependency graph
requires:
  - phase: 11-01
    provides: SaveData/PlayerStatsSaveData POCO schema, PlayerStats.RestoreStats(float, float, float) additive method
provides:
  - SaveLoadManager DontDestroyOnLoad singleton, bootstrapped via RuntimeInitializeOnLoadMethod (no manual scene placement)
  - Public save API - Save(), SaveAtCheckpoint(string), SaveOnBossDefeated(string), HasSaveFile(), NewGame()
  - LoadGame() coroutine-based flow - reads save.json, sets PlayerSpawner.targetSpawnPointName before LoadSceneAsync, restores PlayerStats after scene activation
affects: [11-03 (Checkpoint/boss integration), 11-04]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Project's first LoadSceneAsync usage, driven via IEnumerator/StartCoroutine (zero async/await/Task anywhere, matching project convention)"
    - "RuntimeInitializeOnLoadMethod(BeforeSceneLoad) bootstrap - singleton exists in every scene without manual placement, unlike GameManager/GameStateManager which rely on being placed in a scene"
    - "Static-field handoff to PlayerSpawner.targetSpawnPointName before LoadSceneAsync, mirroring SignpostPortal's existing pattern instead of inventing a second respawn path"

key-files:
  created:
    - Assets/SaveSystem/Script/SaveLoadManager.cs
  modified: []

key-decisions:
  - "Comment text describing the coroutine-vs-async rationale was reworded twice to avoid literal 'async'/'await' substrings anywhere in the file - both the task-level word-boundary grep gate and the plan-level plain-substring grep gate (Assets/SaveSystem/ wide) needed 0 matches simultaneously, so even words like 'asynchronous'/'awaiting' (which are substring-matched by the plain grep) had to be avoided"
  - "SaveOnBossDefeated does not introduce a new spawn point on boss defeat - it only records BossProgress[bossId]=true and reuses whatever scene/spawn point the last checkpoint activation stored, keeping respawn semantics purely checkpoint-based (RESEARCH Open Question 1 resolution, carried from plan)"

patterns-established:
  - "Public API surface for 11-03 integration: Checkpoint.cs calls SaveAtCheckpoint(name), boss death handlers call SaveOnBossDefeated(bossId), both call through to the shared private Save()/CapturePlayerStats() path"

requirements-completed: [D-01, D-02, D-04, D-05, D-06]

# Metrics
duration: 8min
completed: 2026-08-10
---

# Phase 11 Plan 02: SaveLoadManager Singleton Summary

**DontDestroyOnLoad SaveLoadManager singleton (bootstrapped without scene placement) with an in-memory SaveData cache, single-slot save.json file I/O confined to Save()/LoadGame(), and a coroutine-driven LoadSceneAsync flow that restores player stats only after the target scene is active.**

## Performance

- **Duration:** 8 min
- **Started:** 2026-08-10T01:43:00Z (approx.)
- **Completed:** 2026-08-10T01:51:00Z
- **Tasks:** 2
- **Files modified:** 1 (created)

## Accomplishments
- `SaveLoadManager` singleton created with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` bootstrap so `SaveLoadManager.Instance` is always valid from any scene, with zero manual editor placement
- Public save API implemented: `Save()`, `SaveAtCheckpoint(string)`, `SaveOnBossDefeated(string)`, `HasSaveFile()`, `NewGame()` - all mutate the in-memory `SaveData` cache only, with file I/O confined strictly to `Save()`
- `LoadGame()` implemented as the single entry point for both "continue game" and "checkpoint revive" - reads `save.json`, deserializes with Newtonsoft.Json, then runs a coroutine that sets `PlayerSpawner.targetSpawnPointName` before `SceneManager.LoadSceneAsync`, and restores `PlayerStats` via `RestoreStats()` only after the scene load `yield return` completes (avoiding the `HP.Awake()` clamp-to-max overwrite)
- `NewGame()` resets memory only per D-06 - the existing save file on disk is left untouched until the next `Save()` trigger

## Task Commits

Each task was committed atomically:

1. **Task 1: SaveLoadManager.cs singleton + bootstrap + memory cache + save API** - `dbde39c` (feat)
2. **Task 2: LoadGame + async scene load coroutine + stat restore** - `e83203b` (feat)

**Plan metadata:** (pending - this commit)

## Files Created/Modified
- `Assets/SaveSystem/Script/SaveLoadManager.cs` - New DontDestroyOnLoad singleton owning all save/load logic (227 lines): bootstrap, in-memory `SaveData` cache, `Save`/`SaveAtCheckpoint`/`SaveOnBossDefeated`/`HasSaveFile`/`NewGame`/`IsBossDefeated` public API, and `LoadGame` + `LoadSceneAndRestoreRoutine` coroutine + `EnsureCollections`/`ApplyPlayerStatsFromSave` private helpers

## Decisions Made
- Reworded the in-code comment explaining "coroutine instead of async/await" twice: first pass used "asynchronous/awaiting" to dodge the task-level word-boundary regex gate, but that still tripped the plan-level overall verification's plain-substring `grep -rn "async\|await" Assets/SaveSystem/` (which matches "asynchronous" and "awaiting" as substrings, no word boundaries). Final wording ("Coroutine-based (IEnumerator/StartCoroutine) - the codebase does not use C# Task-based keywords anywhere.") avoids the literal substrings "async" and "await" entirely while preserving the same rationale, satisfying both gates simultaneously. Continues the pattern already logged in STATE.md for Phase 9 Plan 1 (`DontDestroyOnLoad` literal) and Phase 10 Plan 1 (`deadzoneHeight` literal) - plan-authored explanatory comments occasionally collide with their own verification grep gates.
- Followed the plan's exact code verbatim for both tasks (singleton bootstrap, save API, load coroutine) - no architectural deviation from the plan text.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Plan-specified comment text collided with the plan's own verification gates**
- **Found during:** Task 2 (LoadGame + coroutine insertion), acceptance-criteria self-check
- **Issue:** The plan's action block specified inserting the exact comment `// Coroutine, not async/await - the codebase has zero async/Task usage.` while also requiring, in the same task's acceptance criteria, `grep -cE "\basync\b|\bawait\b|System\.Threading\.Tasks" ...SaveLoadManager.cs == 0`, and in the plan's overall `<verification>` section, `grep -rn "async\|await" Assets/SaveSystem/` to return 0 lines. The literal comment text as written in the plan would fail both of these self-imposed gates.
- **Fix:** Reworded the comment to preserve the exact same explanatory intent (coroutine chosen over C# async/await/Task, matching the project's zero-async convention) without using the substrings "async" or "await" anywhere, including within longer words like "asynchronous"/"awaiting" that the plain (non-word-boundary) plan-level grep would still match.
- **Files modified:** `Assets/SaveSystem/Script/SaveLoadManager.cs` (single comment line, no code/logic change)
- **Verification:** `grep -cE "\basync\b|\bawait\b|System\.Threading\.Tasks" Assets/SaveSystem/Script/SaveLoadManager.cs` == 0; `grep -rn "async\|await" Assets/SaveSystem/` returns no lines; all other Task 2 acceptance criteria (order gate, method signatures, RestoreStats call, EnsureCollections, DeserializeObject) verified via grep/awk and passed
- **Committed in:** `e83203b` (Task 2 commit, comment already reworded before commit)

---

**Total deviations:** 1 auto-fixed (1 bug per Rule 1)
**Impact on plan:** No scope creep, no behavior change. The only change from the plan's literal text is the wording of one explanatory comment; all functional code matches the plan verbatim.

## Issues Encountered
None beyond the one self-corrected deviation documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- `SaveLoadManager.Instance` public API (`Save`, `SaveAtCheckpoint`, `SaveOnBossDefeated`, `LoadGame`, `HasSaveFile`, `NewGame`, `IsBossDefeated`) is ready for 11-03 to wire into `Checkpoint.cs` and the four boss death handlers
- No blockers for 11-03
- Unity compile verification (this execution environment cannot open the Unity Editor) is deferred to the user or a later checkpoint plan, consistent with prior phases in this project

---
*Phase: 11-newtonsoft-json-dontdestroyonload-i-o-dictionary-dictionary-application-persistentdatapath-json*
*Completed: 2026-08-10*

## Self-Check: PASSED

All created files verified present on disk (`Assets/SaveSystem/Script/SaveLoadManager.cs`). Both task commits verified present in git history (`dbde39c`, `e83203b`).
