# Phase 20 Audio Verification

## Static regression results

| Check | Command or probe | Observed | Verdict |
| --- | --- | --- | --- |
| Legacy playback removed | `rg -n 'PlaySFX' Assets --glob '*.cs'` | 0 lines | PASS |
| Scene manager copies removed | `rg -l 'AudioManager' Assets --glob '*.unity'` | 0 files | PASS |
| Persistence owner | `rg -n 'DontDestroyOnLoad' Assets/Script/AudioManager.cs Assets/Script/PersistentManagers.cs` | AudioManager 0, PersistentManagers 1 | PASS |
| Fixed pool | `rg -n 'Instantiate' Assets/Script/AudioManager.cs` | 0 lines | PASS |
| Silent slider clamp | `rg -n 'Mathf.Max(linear, 0.0001f)' Assets/Script/AudioManager.cs` | 1 line | PASS |
| Existing slider API | `rg -n 'public void Set(BGM|SFX)Volume\(float value\)' Assets/Script/AudioManager.cs` | 2 signatures | PASS |
| Sound panel unchanged | `git diff origin/20-audio-centralization -- Assets/Player/Script/Menu/SoundSettingsPanel.cs` | 0 lines | PASS |
| Environment audio delegation | `rg -n 'bgmSource|lowPassFilter|ChangeBGMCutoff' Assets/Script/EnvironmentManager.cs` | 0 lines | PASS |
| Korean byte preservation | Byte comparison with pre-edit HEAD | All surviving non-ASCII lines identical; 0 replacement sequences | PASS |
| InputHandler unchanged | `git diff origin/20-audio-centralization -- Assets/Player/Script/InputHandler.cs` | 0 lines | PASS |
| Unity compilation | `unity command recompile_status` | completed, failed=false, errors=[] | PASS |
| Unity imports | `git ls-files` for six new `.meta` paths | 6 tracked paths | PASS |
| Play mode structure probe | Unity CLI `eval` in Tutorial Map | persistent root, 17 children, 17 AudioSources, BGM group and clip present | PASS |

## Play mode checklist

Before testing, turn off Console > Error Pause. The existing BUG-008 exception can pause Play mode.

1. [ ] Enter Play mode in `Tutorial Map`. Console has no root-object persistence warning and no missing `Master.mixer` error.
2. [ ] Under the persistent scene, `PersistentManagers` contains one AudioManager, `PooledSource_0` through `PooledSource_15`, and `BGMSource`.
3. [ ] No second AudioManager exists anywhere in the Hierarchy.
4. [ ] Pause menu > Sound: moving BGM slider changes the `BGM` mixer group level and audible BGM volume.
5. [ ] BGM slider at 0 is silent and mixer reads -80 dB, with no infinity or NaN.
6. [ ] Moving SFX slider changes the `SFX` mixer group level.
7. [ ] Cross water-ratio thresholds above 0.66, between 0.33 and 0.66, and below 0.33. BGM filtering and background colours change at each state.
8. [ ] BGM continues from its current position through an environment-state transition.
9. [ ] Transition to another scene. The same AudioManager survives and the BGM/SFX values remain set.
10. [ ] Return to the pause menu after the transition. Sliders show the saved values and continue to work.

## Import side effects

Unity reserialized default and obsolete fields while saving the two scene files in Plan 20-04; these changes are documented in `20-04-SUMMARY.md`. During Plan 20-06 recompilation, `ProjectSettings/EditorBuildSettings.asset` acquired a line-ending-only working-tree change; it was restored and not committed. The Unity-generated `.meta` files are committed.

## Known limitations

- Eleven scenes still contain inert serialized `bgmSource` and `lowPassFilter` entries for EnvironmentManager. The C# fields are removed; Unity strips those entries when each scene is saved. They were not hand-edited.
- No `AudioCue` `.asset` instances or gameplay `Play(...)` call sites exist yet. Pool cooldown and steal behavior compile but have no gameplay Play mode coverage in this phase.
- `InputHandler` remains outside PersistentManagers; BUG-007 is still open.
