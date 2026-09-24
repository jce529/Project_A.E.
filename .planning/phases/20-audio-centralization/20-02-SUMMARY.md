---
phase: 20-audio-centralization
plan: 02
subsystem: audio
tags: [unity, audio-mixer, settings]
requires:
  - phase: 20-01
    provides: AudioCue categories and Master.mixer groups
provides:
  - Mixer-backed BGM/SFX/UI volume settings
  - AudioManager environment-state filter API
affects: [20-03, 20-04, 20-05, 20-06]
key-files:
  created: []
  modified:
    - Assets/SaveSystem/Script/SettingsData.cs
    - Assets/Script/AudioManager.cs
requirements-completed: [D-09, D-10, D-11, D-12]
completed: 2026-09-24
---

# Phase 20 Plan 02 Summary

`AudioManager` now loads `Master.mixer`, routes BGM/SFX/UI volumes through exposed dB parameters, and owns the BGM source and low-pass filter. `SettingsData.UiVolume` defaults to 1 while the settings version remains unchanged.

## Verification

- `SettingsData.cs` diff was exactly one added line, committed as `33b0797`.
- Existing `SetBGMVolume(float)` and `SetSFXVolume(float)` signatures remain; `PlaySFX` is absent. The dB conversion uses a 0.0001 floor and `20 * log10`.
- `SoundSettingsPanel.cs` has no diff. `AudioManager.cs` was committed as `94823cb`.

## Deviations from Plan

- The plan created a BGM source without assigning a clip, while the existing clip is serialized on the scene's `EnvironmentManager` GameObject. `SetEnvironmentState` adopts that clip once from the scene's AudioSource before playback. This keeps the new persistent BGM source audible without retaining audio fields in `EnvironmentManager.cs`.

## Next Phase Readiness

Mixer group lookup and the environment API are ready for the pool and bootstrap plans. Runtime audio still needs Plan 20-06 Play mode verification.
