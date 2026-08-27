---
quick_task: 260827-h5y
title: PlayerPrefs to setting.json settings migration
subsystem: SaveSystem / Settings UI / Input rebinding
tags: [newtonsoft-json, settings, playerprefs-removal, input-rebinding, audio]
status: tasks-1-3-complete-checkpoint-4-pending
dependency_graph:
  requires:
    - Assets/SaveSystem/Script/SaveLoadManager.cs (Phase 11 DontDestroyOnLoad singleton + save.json flow)
  provides:
    - Assets/SaveSystem/Script/SettingsData.cs (SettingsData POCO)
    - "SaveLoadManager.SaveSettings()/LoadSettings()/Settings/CurrentSettings/SettingsPath/HasSettingsFile()"
  affects:
    - Assets/Player/Script/Menu/GameSettingsPanel.cs
    - Assets/Player/Script/Menu/GraphicsSettingsPanel.cs
    - Assets/Player/Script/Menu/SoundSettingsPanel.cs
    - Assets/Script/AudioManager.cs
    - Assets/Player/Script/InputHandler.cs
    - Assets/Player/Script/Menu/PauseMenu.cs
tech_stack:
  added: []
  patterns:
    - "Single-source-of-truth in-memory settings object (SaveLoadManager.CurrentSettings), disk write only on explicit save action"
key_files:
  created:
    - Assets/SaveSystem/Script/SettingsData.cs
  modified:
    - Assets/SaveSystem/Script/SaveLoadManager.cs
    - Assets/Player/Script/Menu/GameSettingsPanel.cs
    - Assets/Player/Script/Menu/GraphicsSettingsPanel.cs
    - Assets/Player/Script/Menu/SoundSettingsPanel.cs
    - Assets/Script/AudioManager.cs
    - Assets/Player/Script/InputHandler.cs
    - Assets/Player/Script/Menu/PauseMenu.cs
decisions:
  - "Renamed SaveLoadManager private field SaveSettings -> JsonSettings (CS0102 avoidance, required by the new public SaveSettings() method)"
  - "AudioManager.LoadVolumes() (Awake, once) and SoundSettingsPanel.OnEnable() previously read volume from two divergent PlayerPrefs-adjacent paths; both now read SaveLoadManager.CurrentSettings, closing the pre-existing volume-not-persisted bug"
  - "GameSettingsPanel.RestoreOnStartup() deleted outright (SettingsData.Language=1 default now plays that role); GraphicsSettingsPanel.RestoreOnStartup() kept because it must apply Screen.fullScreenMode at boot, just reads from CurrentSettings now"
metrics:
  duration: "~35min (Tasks 1-3 only; Task 4 is a human checkpoint)"
  completed: "2026-08-27 (Tasks 1-3); Task 4 checkpoint pending"
---

# Quick Task 260827-h5y: PlayerPrefs to setting.json settings migration Summary

Replaced PlayerPrefs-immediate-write settings storage with an in-memory `SettingsData` object that is only flushed to `Application.persistentDataPath/setting.json` when the user explicitly clicks a new [설정 저장] button, using the same `SaveLoadManager` singleton and Newtonsoft.Json serialization already used for `save.json`.

## Status

Tasks 1, 2, 3 (all `type="auto"`) are complete and committed. **Task 4 (`type="checkpoint:human-verify"`) is NOT done** — it requires a Unity Editor session (scene/prefab UI wiring + Play mode verification) that this execution environment cannot perform. See "Task 4 — Pending Checkpoint" below.

## What Changed

### Task 1 — `SettingsData` POCO + `SaveLoadManager` setting.json API
- New `Assets/SaveSystem/Script/SettingsData.cs`: ASCII-only POCO with 8 fields (`SettingsVersion`, `Language`, `ScreenShake`, `TutorialHint`, `ScreenMode`, `BgmVolume`, `SfxVolume`, `InputBindingsJson`). Defaults (`Language=1`, `ScreenShake=true`, `TutorialHint=true`, `ScreenMode=0`, `BgmVolume=1f`, `SfxVolume=1f`, `InputBindingsJson=""`) exactly mirror the previous hard-coded PlayerPrefs fallbacks, so a missing `setting.json` behaves like a fresh install did before.
- `Assets/SaveSystem/Script/SaveLoadManager.cs`:
  - Renamed the pre-existing private `JsonSerializerSettings SaveSettings` field to `JsonSettings` (3 lines touched: declaration + 2 call sites) — **required** to avoid a CS0102 duplicate-member compile error against the new public `SaveSettings()` method. No logic changed in `Save()`/`LoadGame()`/`SaveAtCheckpoint()`/`SaveOnBossDefeated()`/`NewGame()`.
  - Added a new "Settings (setting.json)" region: `SettingsFileName = "setting.json"`, `SettingsPath`, `_fallbackSettings`/`_settings`, `Settings`, static `CurrentSettings` (null-safe even before `Instance` exists), `HasSettingsFile()`, `SaveSettings()` (the only settings disk write), `LoadSettings()` (missing/corrupt file leaves defaults intact).
  - Added one line, `LoadSettings();`, right after `DontDestroyOnLoad(gameObject);` in `Awake()`'s `Instance == null` branch — runs before any other scene's `Awake()`/`Start()` because `Bootstrap()` runs at `BeforeSceneLoad`.
  - Added `[ContextMenu("Settings/1-3 ...")]` debug hooks (Save/Load/Log), mirroring the existing Phase 11 `[ContextMenu("Phase11/...")]` pattern, so save/load can be verified from the Hierarchy gear menu before any UI button exists.

### Task 2 — Settings panels + `AudioManager` to memory-based
- `GameSettingsPanel.cs`: `OnEnable()`, `OnScreenShakeChanged`, `OnTutorialHintChanged`, `ApplyLanguage()` now read/write `SaveLoadManager.CurrentSettings` instead of `PlayerPrefs`. `RestoreOnStartup()` (and its `[RuntimeInitializeOnLoadMethod]`) deleted — it only used to seed a PlayerPrefs default, which `SettingsData.Language = 1` now does structurally.
- `GraphicsSettingsPanel.cs`: `OnEnable()`/`Apply()`/`RestoreOnStartup()` read/write `SaveLoadManager.CurrentSettings.ScreenMode`. `RestoreOnStartup()` kept (still needed to apply `Screen.fullScreenMode` at boot before any panel opens), body swapped to read `CurrentSettings`.
- `AudioManager.cs`: `LoadVolumes()` now reads `SaveLoadManager.CurrentSettings.BgmVolume/SfxVolume`; `SetBGMVolume`/`SetSFXVolume` dropped their `PlayerPrefs.SetFloat`/`PlayerPrefs.Save()` calls and are now live-apply only.
- `SoundSettingsPanel.cs`: `OnEnable()` reads from `CurrentSettings`; `OnBGMChanged`/`OnSFXChanged` still call `AudioManager.Instance?.SetBGMVolume/SetSFXVolume` for live apply, and additionally write `SaveLoadManager.CurrentSettings.BgmVolume/SfxVolume`.
- **Notable discovery** (documented per plan's `<output>` requirement): the original task framing assumed "SoundSettingsPanel has no save code at all," but the save call was actually hidden inside `AudioManager.SetBGMVolume/SetSFXVolume`. `AudioManager.LoadVolumes()` only ran once (`Awake`), while `SoundSettingsPanel.OnEnable()` read from PlayerPrefs on a separate path — the two could diverge. Both now read the same `SaveLoadManager.CurrentSettings` object, closing that gap (this is the mechanism behind the "volume not permanently saved" bug called out in the plan's success criteria).

### Task 3 — `InputHandler` rebinding storage + `PauseMenu` save button
- `Assets/Player/Script/InputHandler.cs` (targeted edits only — file has 640 pre-existing U+FFFD corrupted characters from an earlier encoding round-trip; **not** rewritten wholesale):
  - Removed the `SAVE_KEY` constant and its (already-corrupted) preceding comment line.
  - `SaveBindingOverrides()` body replaced: writes `inputActions.SaveBindingOverridesAsJson()` into `SaveLoadManager.CurrentSettings.InputBindingsJson` (memory only; reaches disk via the settings save button).
  - `LoadBindingOverrides()` body replaced: adds an `inputActions == null` guard (not present before, harmless), reads `SaveLoadManager.CurrentSettings.InputBindingsJson`, no-ops on empty, else calls `inputActions.LoadBindingOverridesFromJson(json)`.
  - All added lines are pure ASCII (English comments only), per the file's encoding constraint.
- `Assets/Player/Script/Menu/PauseMenu.cs`: added `public void OnSaveSettingsBtnClick()` below `OnQuitBtnClick()` — null-guards `SaveLoadManager.Instance`, calls `SaveLoadManager.Instance.SaveSettings()`, logs success. `Start`/`OnDestroy`/`OnPauseInput`/`Open`/`Close` untouched.
- `Assets/Player/Script/Menu/ControlsSettingsPanel.cs` — **not modified** (verified `git diff HEAD` = 0 lines). Its `FinishRebind()`/`ResetAllBindings()` calls to `InputHandler.Instance?.SaveBindingOverrides()` automatically pick up the new in-memory flow because the method signature didn't change.

## Diff Summary (git diff, plan-commit baseline `5589270` to `HEAD`)

| File | +/- |
|---|---|
| `Assets/SaveSystem/Script/SettingsData.cs` (new) | +26 |
| `Assets/SaveSystem/Script/SaveLoadManager.cs` | +99/-3 |
| `Assets/Player/Script/Menu/GameSettingsPanel.cs` | +9/-19 |
| `Assets/Player/Script/Menu/GraphicsSettingsPanel.cs` | +3/-4 |
| `Assets/Player/Script/Menu/SoundSettingsPanel.cs` | +5/-4 |
| `Assets/Script/AudioManager.cs` | +5/-5 |
| `Assets/Player/Script/InputHandler.cs` | +9/-16 |
| `Assets/Player/Script/Menu/PauseMenu.cs` | +12/-0 |
| **Total** | **166 insertions(+), 53 deletions(-)** |

`Assets/Player/Script/InputHandler.cs` U+FFFD count: **640** (baseline was 716 before this task — the count dropped because the deleted `SAVE_KEY` comment/const lines and the old `LoadBindingOverrides()` comment happened to contain some of the corrupted characters; no new corruption introduced, well under the "≤716" gate).

## Deviations from Plan

### Auto-fixed / Documented Issues

**1. [Verification-gate authoring mismatch] Task 1's `JsonSettings` occurrence-count gate (`grep -c` == 3) does not match the plan's own Task 1-C action code**
- **Found during:** Task 1 verification
- **Issue:** The plan's automated Task 1 gate expects `grep -c 'JsonSettings' SaveLoadManager.cs` to equal `3` (the 1 declaration + 2 original `Save()`/`LoadGame()` call sites). However, the same task's `<action>` block (section 1-C) explicitly specifies that the new `SaveSettings()`/`LoadSettings()` methods must also serialize/deserialize using the same `JsonSettings` field (`JsonConvert.SerializeObject(_settings, JsonSettings)` / `JsonConvert.DeserializeObject<SettingsData>(json, JsonSettings)`). Implementing the action exactly as specified makes the real count **5**, not 3. This is the same class of "plan action vs. plan verification gate self-conflict" already documented multiple times in `.planning/STATE.md` (Phase 9 Plan 1 `DontDestroyOnLoad`, Phase 10 Plan 1 `deadzoneHeight`, Phase 10 Plan 2/3 `git diff` baseline).
- **Fix:** Implemented the code exactly as specified in the action block (5 occurrences), verified via `git diff HEAD` that the *only* deleted/renamed lines are the 3 original ones (confirmed: exactly 3, all `SaveSettings` → `JsonSettings`), and that everything else is additive. Did not artificially reduce `JsonSettings` usage to hit the stale gate number.
- **Files:** `Assets/SaveSystem/Script/SaveLoadManager.cs`
- **Commit:** `ea05191`

No functional bugs found. No architectural changes required (Rule 4 not triggered).

## Task 4 — Pending Checkpoint (NOT executed)

Task 4 is `type="checkpoint:human-verify"` and requires a Unity Editor + Play mode session:
1. Confirm the Editor recompiles with 0 console errors (especially no CS0102 from the `JsonSettings` rename).
2. Manually create a [설정 저장] button UI under the pause panel and wire its `OnClick` to `PauseMenu.OnSaveSettingsBtnClick`.
3. Play-mode verification: live-apply behavior per tab, no-disk-write-until-save, save button writes `setting.json`, restart restores all 4 tabs + key rebindings, `setting.json` missing falls back to hard-coded defaults, and a regression check that `save.json` / checkpoint / continue flow is unaffected.

This execution environment has no Unity compiler/runtime, so none of the above could be performed or faked. Until a user runs this checklist and reports back (or explicitly approves), this quick task is **not considered fully verified**, even though all static/automated verification gates for Tasks 1-3 passed.

**STATE.md note:** `.planning/STATE.md` already has an unrelated uncommitted change in the working tree (a pre-existing "Phase 50 added" ROADMAP-evolution line) from outside this task's scope. To avoid bundling an unrelated change into this task's commits, STATE.md was intentionally left untouched by this execution. The orchestrator/user should record an Active TODO for Task 4 (Unity compile check + [설정 저장] button wiring + Play mode checklist above) when STATE.md is next updated.

## Self-Check: PASSED

- FOUND: Assets/SaveSystem/Script/SettingsData.cs
- FOUND: Assets/SaveSystem/Script/SaveLoadManager.cs (modified)
- FOUND: Assets/Player/Script/Menu/GameSettingsPanel.cs (modified)
- FOUND: Assets/Player/Script/Menu/GraphicsSettingsPanel.cs (modified)
- FOUND: Assets/Player/Script/Menu/SoundSettingsPanel.cs (modified)
- FOUND: Assets/Script/AudioManager.cs (modified)
- FOUND: Assets/Player/Script/InputHandler.cs (modified)
- FOUND: Assets/Player/Script/Menu/PauseMenu.cs (modified)
- FOUND commit: ea05191 (Task 1)
- FOUND commit: d42ea6f (Task 2)
- FOUND commit: ef745bb (Task 3)
- Global `PlayerPrefs` grep across `Assets/**/*.cs`: 0 matches (confirmed)
