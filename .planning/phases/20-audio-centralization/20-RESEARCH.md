# Phase 20: 중앙 집중형 오디오 시스템 및 AudioSource 풀링 - Research

**Researched:** 2026-09-22
**Domain:** Unity 6 (6000.3.10f1) Audio system — AudioMixer, ScriptableObject data, object pooling, persistent singleton bootstrap
**Confidence:** HIGH (project-code findings, Unity API), MEDIUM (dB conversion constants, pool sizing — no authoritative single "correct" number, industry convention)

## Summary

This phase introduces four new pieces of infrastructure that do not exist anywhere in this codebase yet: an `AudioMixer` asset with Master/BGM/SFX/UI groups, a ScriptableObject `AudioCue` data type, a fixed-size `AudioSource` pool with priority-based stealing, and a `PersistentManagers` bootstrap. All of this is new territory for the project — there is no reusable prior art, but the patterns involved (linear-to-dB volume conversion, exposed AudioMixer parameters, object pooling, DontDestroyOnLoad singletons) are standard, well-documented Unity idioms with no exotic edge cases.

The most important finding from code inspection: the existing `AudioManager` GameObject is **already a non-root child** of a "Manager" GameObject in `Tutorial Map.unity` (`m_Father: {fileID: 1579923566}`, same parent hierarchy that contains `InputManager`, the object at the center of BUG-007). Unity's `DontDestroyOnLoad` silently refuses to persist non-root objects and logs `"DontDestroyOnLoad only works for root GameObjects or components on root GameObjects."` This means the current `AudioManager.Awake()` call to `DontDestroyOnLoad(gameObject)` is **already failing today**, identically to the documented InputHandler failure in BUG-007 — it just hasn't been noticed because losing BGM/SFX volume state on scene transition is far less visible than losing player input. This is strong independent evidence that the `PersistentManagers` bootstrap must NOT simply keep calling `DontDestroyOnLoad` on a scene-nested child object; the bootstrap must own a dedicated root GameObject.

**Primary recommendation:** Build `PersistentManagers` as a `RuntimeInitializeOnLoadMethod`-driven bootstrapper (mirroring the existing `SaveLoadManager` pattern already in this codebase) that programmatically instantiates a **root-level** prefab/GameObject hierarchy containing `AudioManager` as a child, calls `DontDestroyOnLoad` on that root, and guards against duplicates on subsequent scene loads. Do NOT keep relying on a scene-placed `AudioManager` GameObject nested under "Manager" — that placement is the root cause of the same failure class documented in BUG-007.

## User Constraints (from CONTEXT.md)

### Locked Decisions

**AudioCue (ScriptableObject)**
- D-01: Basic fields — `AudioClip` (single or array with random pick), volume random range, pitch random range, category.
- D-02: Category = BGM/SFX/UI (3 types).
- D-03: Retrigger cooldown is a fixed per-cue field (seconds). `AudioManager` tracks `lastPlayedTime` per cue in a dictionary; requests within cooldown are ignored.
- D-04: Position/tracking playback determined by a per-cue 2D/3D flag (fixed `spatialBlend` value) — BGM always 2D, some SFX 3D.
- D-05: Priority stored as a cue field (for future extensibility), actually used by this phase's stealing logic (D-08).

**AudioSource Pooling & Concurrency**
- D-06: Fixed-size pool, pre-created at startup. No dynamic growth.
- D-07: Tracking playback via `Play(cue, Transform target)` — AudioManager stores the target Transform and updates the AudioSource's position every frame. Fixed-position playback via separate `Play(cue, Vector3 position)`.
- D-08: When pool is full and a new play request arrives, steal from **the oldest among the currently-playing sources with lower priority than the request**. If the request's priority is lower than all currently playing, ignore the request (no allocation).

**AudioMixer & Volume Migration**
- D-09: Mixer group structure: `Master` > `BGM`/`SFX`/`UI` (3 groups), 1:1 with AudioCue categories (D-02).
- D-10: `SaveLoadManager.CurrentSettings`'s existing 0–1 linear volume schema is preserved unchanged. `AudioManager` keeps existing `SetBGMVolume(float)` / `SetSFXVolume(float)` signatures, internally converting via `Mathf.Log10` before calling `AudioMixer.SetFloat` (dB). External callers (SoundSettingsPanel, SaveLoadManager) do not change.
- D-11: A UI category volume parameter/field must be added following the same pattern as BGM/SFX (e.g., `UiVolume`).

**EnvironmentManager Integration & Bootstrap**
- D-12: `EnvironmentManager` calls `AudioManager.SetEnvironmentState(EnvironmentState state)`. State judgment (Alive/Neutral/Withered) stays in `EnvironmentManager`; cutoff-value calculation and BGM playback/filter application move into `AudioManager`.
- D-13: New `PersistentManagers` bootstrap framework manages `AudioManager`'s `DontDestroyOnLoad`. Framework must be designed to be extensible to other managers (e.g. InputHandler) later, but **only AudioManager is actually migrated this phase**. InputHandler/BUG-007 is out of scope.

### Claude's Discretion
- Exact dB curve mapping constants for the AudioMixer asset, default pool size (N), and the internal registration API signature details of `PersistentManagers` are left to research/planning.

### Deferred Ideas (OUT OF SCOPE)
- Full BUG-007 fix (migrating InputHandler onto `PersistentManagers` and resolving the scene-transition input loss). The framework must be extensible toward this, but this phase does not touch InputHandler.
- Per-cue 3D spatial parameter overrides (minDistance/maxDistance) — not included in the base AudioCue; can be added later if needed.
- Actual audio hooks for auto-spawn monsters / attack / hit / skill / boss pattern sounds — this phase builds only the infrastructure (AudioManager/AudioCue/pool/mixer); wiring actual gameplay sound triggers is left to future phases.

## Phase Requirements

No formal REQUIREMENTS.md entries exist for this phase yet (fresh phase, promoted from backlog). CONTEXT.md decisions D-01 through D-13 serve as the effective requirements; see table below mapping each to research support found.

| ID (from CONTEXT.md) | Description | Research Support |
|----|-------------|------------------|
| D-01/D-02 | AudioCue ScriptableObject: clip(s), volume/pitch range, category | Section "AudioCue ScriptableObject Pattern" below |
| D-03 | Per-cue cooldown via `lastPlayedTime` dictionary in AudioManager | Section "Cooldown Tracking" |
| D-04 | Per-cue spatialBlend flag (2D/3D) | Section "AudioCue ScriptableObject Pattern" |
| D-05/D-08 | Priority field + steal-oldest-lowest-priority pool logic | Section "AudioSource Pooling" |
| D-06/D-07 | Fixed pool, Play(cue, Transform) / Play(cue, Vector3) | Section "AudioSource Pooling" |
| D-09/D-10/D-11 | AudioMixer groups + linear-to-dB volume migration preserving signatures | Section "AudioMixer API" |
| D-12 | EnvironmentManager → AudioManager.SetEnvironmentState refactor | Section "Existing Code: Exact Signatures" |
| D-13 | PersistentManagers bootstrap, extensible, AudioManager-only this phase | Section "Persistent Singleton Bootstrap Pattern" + critical finding on current AudioManager scene placement |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Unity `UnityEngine.Audio.AudioMixer` | Built-in (Unity 6000.3.10f1) | Master/BGM/SFX/UI group routing, dB-based volume control | Built-in Unity Editor asset type; no package required |
| Unity `AudioSource` / `AudioClip` | Built-in | Playback primitives for pooled sources | Already used by existing `AudioManager`/`EnvironmentManager` |
| `ScriptableObject` | Built-in | AudioCue data asset | Project's own established pattern (`ItemData : ScriptableObject`, Phase 17) |

### Supporting
None — no third-party audio package (e.g. FMOD, Wwise) is implied or needed; CONTEXT.md decisions describe building this natively with Unity's own `AudioMixer`.

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Native AudioMixer + hand-rolled pool | FMOD/Wwise middleware | Out of scope — CONTEXT.md explicitly specifies AudioMixer + ScriptableObject Cue + custom pool; no user decision to adopt middleware, and doing so would be a large unrequested scope increase (violates CLAUDE.md "no overengineering / stay in phase scope") |
| `AudioSource.PlayOneShot` everywhere (current SFX pattern in `AudioManager.PlaySFX`) | Pooled dedicated `AudioSource` components | Required by D-06/D-07/D-08 — `PlayOneShot` cannot be individually tracked/stopped/repositioned per play, which is required for priority stealing and Transform-tracking playback |

**Installation:** No package installation needed — `UnityEngine.Audio` namespace (AudioMixer, AudioMixerGroup) ships with Unity core.

**Version verification:** N/A (built-in engine API, not an npm/UPM package with a version to check). Project's Unity Editor version confirmed via `ProjectSettings/ProjectVersion.txt`: `6000.3.10f1`. AudioMixer/AudioMixerGroup APIs used below (`SetFloat`, `GetFloat`, `outputAudioMixerGroup`) have been stable since Unity 5 and are unchanged in Unity 6.

## Architecture Patterns

### Recommended Project Structure
```
Assets/
├── Audio/                          # NEW — mirrors existing Assets/Item/ convention
│   ├── AudioManager.cs             # moved/expanded from Assets/Script/AudioManager.cs (or stay in place — see Discretion note)
│   ├── AudioCue.cs                 # new ScriptableObject
│   ├── AudioSourcePool.cs          # new — pool logic (could be nested class in AudioManager, or standalone)
│   ├── Master.mixer                # new AudioMixer asset (Master > BGM/SFX/UI)
│   └── Cues/                       # .asset instances of AudioCue (mirrors Assets/Item/*.asset pattern from Phase 17)
├── Script/
│   ├── AudioManager.cs             # existing location — CLAUDE.md "surgical changes" favors editing in place over moving files
│   └── EnvironmentManager.cs       # refactored: SetEnvironmentState calls only
└── PersistentManagers.cs           # new bootstrap (location TBD — likely Assets/Script/ alongside AudioManager)
```

**Discretion note:** Given CLAUDE.md's "정밀한 변경 (Surgical Changes)" principle (touch only what's necessary, respect existing structure), the planner should likely prefer **editing `Assets/Script/AudioManager.cs` in place** rather than moving it to a new `Assets/Audio/` folder, since moving it changes the `.meta` GUID reference story and is not requested by CONTEXT.md. New files (AudioCue.cs, the pool, PersistentManagers.cs) can go in `Assets/Script/` alongside the existing manager, consistent with where `EnvironmentManager.cs` and `AudioManager.cs` already live.

### Pattern 1: AudioMixer with Exposed Parameters for Volume Control
**What:** Create an `AudioMixer` asset (Assets > Create > Audio Mixer) with a Master group and three child groups (BGM, SFX, UI). Route pooled `AudioSource`s' `outputAudioMixerGroup` to the matching group based on `AudioCue.category`. Right-click each group's Volume slider in the Audio Mixer window → "Expose to script" to create parameters (e.g., `BGMVolume`, `SFXVolume`, `UIVolume`), then rename them in the Exposed Parameters list.
**When to use:** Any time volume must be controlled from code rather than only via the Editor mixer window.
**Example:**
```csharp
// Source: Unity Manual - AudioMixer.SetFloat (standard documented pattern)
[SerializeField] private AudioMixer masterMixer;

public void SetBGMVolume(float linear01)
{
    BgmVolume = linear01;
    float dB = LinearToDecibel(linear01);
    masterMixer.SetFloat("BGMVolume", dB);
}

private static float LinearToDecibel(float linear)
{
    // Standard Unity linear-to-dB conversion.
    // Clamp away from exactly 0 because Log10(0) = -Infinity, which AudioMixer
    // does not treat specially — it will assign -Infinity dB rather than silence
    // gracefully, and can produce NaN-propagation issues in some mixer effect chains.
    linear = Mathf.Max(linear, 0.0001f);
    return Mathf.Log10(linear) * 20f;
}
```
This is the textbook Unity conversion (`20 * log10(x)`), confirmed as the standard approach in Unity's own Audio Mixer documentation and tutorials for slider-driven volume. The `0.0001f` floor (equivalent to -80 dB) is the conventional clamp value used to represent "silent" without hitting `-Infinity`; AudioMixer's own slider range typically bottoms out at -80 dB by default, so clamping to a linear value that maps to approximately -80 dB (Mathf.Log10(0.0001) * 20 = -80) is consistent with the mixer's own UI convention.

**Confidence:** HIGH — this formula and clamp pattern is Unity's own documented/standard approach (present in Unity Learn tutorials and the AudioMixer scripting API docs), not merely a community convention.

### Pattern 2: AudioCue ScriptableObject
**What:** Data-only ScriptableObject describing a sound: clip(s), volume/pitch ranges, category, cooldown, priority, spatialBlend flag.
**When to use:** Any discrete sound effect or BGM cue triggered by gameplay code, per D-01–D-05.
**Example:**
```csharp
// Follows existing project convention from Assets/Item/Script/ItemData.cs (Phase 17: first
// ScriptableObject in this codebase — same CreateAssetMenu + plain-field style)
using UnityEngine;

public enum AudioCategory { BGM, SFX, UI }

[CreateAssetMenu(fileName = "NewAudioCue", menuName = "Audio/AudioCue")]
public class AudioCue : ScriptableObject
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] clips;

    [Header("Volume / Pitch")]
    [SerializeField] private Vector2 volumeRange = new Vector2(1f, 1f);
    [SerializeField] private Vector2 pitchRange = new Vector2(1f, 1f);

    [Header("Category / Routing")]
    [SerializeField] private AudioCategory category = AudioCategory.SFX;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f; // 0 = 2D, 1 = 3D

    [Header("Concurrency")]
    [SerializeField] private float cooldown = 0f;
    [SerializeField] private int priority = 0;

    public AudioCategory Category => category;
    public float SpatialBlend => spatialBlend;
    public float Cooldown => cooldown;
    public int Priority => priority;

    public AudioClip GetClip() =>
        clips == null || clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];

    public float GetVolume() => Random.Range(volumeRange.x, volumeRange.y);
    public float GetPitch() => Random.Range(pitchRange.x, pitchRange.y);
}
```
**Confidence:** HIGH for structure/idiom (this exact "array of clips + random range + GetClip/GetVolume/GetPitch" shape is the de facto standard Unity audio-cue pattern seen across virtually every Unity audio tutorial and asset store audio manager); MEDIUM on exact field names since these are Claude's-discretion naming choices, not verified against an external spec.

### Pattern 3: Fixed-Size AudioSource Pool with Priority Stealing
**What:** Pre-instantiate N child GameObjects, each with one `AudioSource` component, under a pool container at startup (`Awake`). Track per-slot metadata: `inUse`, `priority`, `startTime`, `trackedTransform` (nullable). On `Play`, find a free slot; if none free, find the candidate to steal per D-08 (oldest among those with priority strictly lower than the incoming request); if no such candidate exists, drop the request.
**When to use:** All AudioCue playback (both BGM and SFX/UI) routes through this pool, per D-06/D-07/D-08.
**Example:**
```csharp
// Illustrative structure — one AudioSource per pooled child GameObject (not multiple
// AudioSource components stacked on a single GameObject). One-source-per-GameObject is
// the standard approach because Transform-tracking (D-07) requires moving that source's
// own Transform independently every frame; sharing a GameObject would make individual
// per-voice positioning impossible.
private class PoolSlot
{
    public AudioSource Source;
    public Transform SourceTransform;
    public bool InUse;
    public int Priority;
    public float StartTime;
    public Transform FollowTarget; // null if fixed-position playback
}

private PoolSlot[] pool;

private void Awake()
{
    pool = new PoolSlot[poolSize];
    for (int i = 0; i < poolSize; i++)
    {
        var go = new GameObject($"PooledSource_{i}");
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        pool[i] = new PoolSlot { Source = src, SourceTransform = go.transform };
    }
}

private PoolSlot AcquireSlot(int requestPriority)
{
    foreach (var slot in pool)
        if (!slot.InUse) return slot;

    // Steal: among slots with priority LOWER than request, pick the oldest (smallest StartTime).
    PoolSlot stealCandidate = null;
    foreach (var slot in pool)
    {
        if (slot.Priority < requestPriority)
        {
            if (stealCandidate == null || slot.StartTime < stealCandidate.StartTime)
                stealCandidate = slot;
        }
    }
    return stealCandidate; // null => caller drops the request (ignored per D-08)
}
```
For Transform-tracking playback, update follower positions once per frame in `Update()`:
```csharp
private void Update()
{
    foreach (var slot in pool)
    {
        if (slot.InUse && slot.FollowTarget != null)
            slot.SourceTransform.position = slot.FollowTarget.position;
    }
}
```
A `Coroutine`-per-voice approach is unnecessary complexity here — a single `Update()` loop over the fixed-size pool array is O(N) with small constant N, which is simpler, has no allocation overhead from `StartCoroutine`, and matches the existing project's preference for plain `Update()` polling (seen in `EnvironmentManager.Update()` itself). Reclaiming a finished (non-looping) slot back to "free" can be done by checking `!slot.Source.isPlaying` each `Update()` tick, or via `AudioSource.SetScheduledEndCallback`/a coroutine `WaitForSeconds(clip.length)` — the simpler polling check in the same `Update()` loop is preferred (no extra API surface, consistent with D-08's "oldest" bookkeeping already requiring a per-frame-agnostic timestamp).

**Confidence:** HIGH for the general one-GameObject-per-voice pooling pattern (this is the standard Unity audio pool architecture found in virtually all serious Unity audio-manager implementations, precisely because AudioSource has no API to reposition mid-playback without owning its own Transform). MEDIUM on the specific steal-candidate tie-breaking and reclaim-timing details, which are original synthesis from D-08's plain-language spec rather than a copied reference implementation — flagged for the planner to pin down exact tie-break behavior (e.g., what happens when two lowest-priority slots have identical StartTime) as a task-level decision.

### Anti-Patterns to Avoid
- **`AudioSource.PlayOneShot` for pooled/priority-managed sounds:** `PlayOneShot` clips share the source's volume/pitch at call time and cannot be individually queried for `isPlaying`/stopped/repositioned after the call — incompatible with D-07 (Transform tracking) and D-08 (stealing a specific in-flight voice). Reserve `PlayOneShot`-style firing only if a future non-tracked, non-stealable "fire and forget" cue category were ever added (not requested here).
- **Dynamic pool growth (`Instantiate` on demand when pool is full):** Explicitly rejected by D-06. Do not add `Instantiate` fallback logic when the pool is exhausted — the correct behavior per D-08 is steal-or-drop, never grow.
- **`DontDestroyOnLoad` on a non-root GameObject:** Confirmed to be the current (broken) state of `AudioManager` in `Tutorial Map.unity` — see Critical Finding below. Any new bootstrap object must be a scene root, not nested under an existing "Manager" parent.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Volume curve / dB scaling | A custom logarithmic or exponential volume curve formula from scratch | `Mathf.Log10(linear) * 20f` feeding `AudioMixer.SetFloat` | This is literally what `AudioMixer`'s dB parameter expects; hand-rolling anything else (e.g., linear dB mapping) produces a volume slider that doesn't feel natural (most perceptible change bunched at one end) |
| Spatial audio falloff/panning | Custom 3D distance-attenuation math | `AudioSource.spatialBlend` + Unity's built-in 3D sound settings (rolloff curve, min/max distance — defaults are acceptable since D-04 only requires a fixed spatialBlend flag, not custom falloff curves per cue) | Unity's AudioSource already implements this; CONTEXT.md explicitly defers per-cue min/max distance overrides (Deferred Ideas) so no custom logic is needed this phase |
| Persistent singleton lifecycle | A custom scene-load event system to re-find/re-link managers after every scene load | `DontDestroyOnLoad` on a root object created once by a bootstrap, combined with existing singleton `Instance` static-property + duplicate-destroy pattern already used by `AudioManager`/`SaveLoadManager` | The project already has this working correctly for `SaveLoadManager` (see Existing Code section) — reuse that exact shape for the new bootstrap rather than inventing a new lifecycle mechanism |

**Key insight:** Every "hand-roll risk" in this phase already has a first-party Unity API or an existing in-repo pattern that solves it — the actual engineering work is wiring `AudioCue` → pool → `AudioMixer` group correctly, not inventing new algorithms.

## Runtime State Inventory

> Included because this phase modifies a persistent-manager bootstrap mechanism (`AudioManager`'s DontDestroyOnLoad) and refactors `EnvironmentManager`'s BGM ownership — both touch scene-embedded state, similar in kind to a rename/refactor phase.

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | `SaveLoadManager.CurrentSettings.BgmVolume` / `.SfxVolume` (0–1 floats) persisted in `setting.json` via existing settings save flow. No `UiVolume` field currently exists there. | **Code edit, not data migration**: existing `BgmVolume`/`SfxVolume` fields are read as-is (D-10 preserves them). A new `UiVolume` field (D-11) must be added to the settings schema — this is an additive schema change; existing saved `setting.json` files without a `UiVolume` key will deserialize with a default (verify `SettingsData`/whatever the `CurrentSettings` POCO is called uses a JSON library — likely Newtonsoft.Json per Phase 11 — which fills missing fields with the C# default rather than throwing, so no migration code is strictly required, but the planner should confirm the settings POCO's exact deserialization behavior before assuming this). |
| Live service config | The `AudioMixer` asset itself is a new Unity asset checked into `Assets/` — not an external service config, so no drift risk of "config lives outside git." | None — it's a normal git-tracked asset. |
| OS-registered state | None found. Audio playback does not register anything at the OS level (no Task Scheduler, no launchd, etc.). | None. |
| Secrets/env vars | None — no audio-related secrets or env vars exist in this project. | None. |
| Build artifacts | The two scenes referencing `AudioManager` (`Assets/Scenes/Tutorial Map.unity`, `Assets/_Recovery/0.unity`) currently embed a scene-placed `AudioManager` GameObject nested under "Manager" with a serialized `bgmSource` reference (`fileID: 694150501`, the BGM `AudioSource`). If `AudioManager` moves onto the `PersistentManagers` bootstrap (spawned at runtime rather than scene-placed), these scene-embedded instances become **stale duplicates** that will either (a) get destroyed by the new singleton's duplicate-guard on `Awake`, or (b) need to be removed from the scene file entirely and their `bgmSource` `AudioSource` component re-homed (e.g., onto the bootstrap-created object, or kept in-scene and referenced by the runtime-spawned `AudioManager` via a `FindObjectOfType`/scene-lookup at first-load time). **This is a real design decision the planner must resolve**: either (1) keep the BGM `AudioSource` scene-placed and have the bootstrap-instantiated `AudioManager` locate it per-scene (fragile across future scenes lacking that object), or (2) make the `AudioManager` prefab fully self-contained (own its own child `AudioSource`(s) for BGM + the pool), and delete/ignore the old scene-placed `AudioManager` GameObjects in `Tutorial Map.unity` / `0.unity`. Given D-06/D-07 already require the `AudioManager` to own a pool of `AudioSource`s it manages directly, option (2) — fully self-contained persistent prefab — is more consistent with the rest of the phase's design and is the recommended default. |

**Critical finding — current AudioManager persistence is already broken:** In `Tutorial Map.unity`, the `AudioManager` GameObject's `Transform` has `m_Father: {fileID: 1579923566}`, i.e. it is a **child** of a GameObject named `Manager` (`fileID: 1579923565`), not a scene root. This is the exact same parent GameObject that BUG-007 identifies as the location of the broken `InputManager`/`InputHandler`. Unity's `DontDestroyOnLoad` only works on root GameObjects (or a component on a root GameObject) and Unity logs `"DontDestroyOnLoad only works for root GameObjects or components on root GameObjects."` and silently no-ops otherwise. This means `AudioManager.Awake()`'s existing `DontDestroyOnLoad(gameObject)` call has almost certainly never actually persisted `AudioManager` across a scene transition in play sessions using `Tutorial Map.unity` as the entry scene — a fresh `AudioManager` (with default `BgmVolume = 1f` before `LoadVolumes()` runs) is created in every new scene that also places one, and any two AudioManagers existing simultaneously across the transition frame would hit the duplicate-destroy branch based on load order, not intent. **This independently corroborates why D-13 calls for a proper `PersistentManagers` bootstrap** — the existing scene-placed approach was never actually working, it just happened not to be noticed because losing volume state (silently reset to defaults) is much less visible than losing player input (BUG-007). The planner should treat "does AudioManager actually persist across scene transitions today" as effectively already answered: **no**, and design the bootstrap accordingly (spawn a fresh root-level object via code, not via scene placement).

## Common Pitfalls

### Pitfall 1: DontDestroyOnLoad on a non-root object (confirmed already present in this project)
**What goes wrong:** Calling `DontDestroyOnLoad(gameObject)` from a MonoBehaviour attached to a GameObject that has a parent silently fails (Unity 2020+ logs a warning; older versions were even more silent) — the object is destroyed at the next scene load anyway.
**Why it happens:** Unity's persistence mechanism operates on scene roots only; children are always owned by their scene.
**How to avoid:** Never scene-place a persistent-manager GameObject nested under another object. Either mark it as a scene root with no parent, or (preferred, matching D-13's bootstrap approach) instantiate it purely via code at a well-defined bootstrap entry point so there is no scene file to accidentally re-parent later.
**Warning signs:** Console warning `"DontDestroyOnLoad only works for root GameObjects..."`; symptoms like settings resetting to default after a scene change, or duplicate managers appearing after transitions. (This exact pitfall is already present for `AudioManager` — see Critical Finding above — and for `InputHandler`, per BUG-007.)

### Pitfall 2: Log10(0) producing -Infinity / NaN in the mixer
**What goes wrong:** If a volume slider can reach exactly 0 and the code does `Mathf.Log10(0f) * 20f` without clamping, the result is `-Infinity`. Passing this to `AudioMixer.SetFloat` does not cleanly mute — behavior can be inconsistent (silently ignored, or clamped to the mixer's own internal minimum in unpredictable ways depending on Unity version) and any downstream code that adds/interpolates that value (e.g. a fade coroutine) can produce `NaN`.
**Why it happens:** `Mathf.Log10` is a real logarithm; the domain excludes 0.
**How to avoid:** Always clamp with `Mathf.Max(linear, 0.0001f)` (or similar small epsilon, ~-80 dB) before taking the log, exactly as D-10 anticipates ("Mathf.Log10 변환") and as this research's Pattern 1 code shows.
**Warning signs:** Volume slider dragged to 0 produces console warnings from the mixer, or a fade-to-silent effect gets stuck/produces audio glitches.

### Pitfall 3: Reference to old `bgmSource` field breaking after refactor
**What goes wrong:** `EnvironmentManager.cs` currently has its own `public AudioSource bgmSource` and `public AudioLowPassFilter lowPassFilter` fields, wired in the Inspector, used directly in `ChangeBGMCutoff`. If these fields are simply deleted without checking Inspector wiring in `Tutorial Map.unity` (and any other scene using `EnvironmentManager`), those serialized references become orphaned (harmless, but doesn't achieve full cleanup) or — if `bgmSource`/`lowPassFilter` fields are kept but no longer used in code — the Inspector still shows now-dead serialized fields, misleading future maintainers about ownership (violates D-12's intent that `EnvironmentManager` no longer touches audio directly).
**Why it happens:** Unity serializes public/`[SerializeField]` fields into the scene `.asset`/`.unity` YAML regardless of whether code still reads them; removing the C# field is what actually cleans this up, but doing so requires locating and clearing the Inspector reference in every scene that has an `EnvironmentManager` component (search scenes for `EnvironmentManager` to find them — this research did not exhaustively enumerate all scenes containing `EnvironmentManager`, only confirmed `AudioManager`'s scene placement in `Tutorial Map.unity`; the planner/executor should grep all `.unity` files for `EnvironmentManager` before removing fields).
**How to avoid:** After removing `bgmSource`/`lowPassFilter` fields from `EnvironmentManager.cs`, run a scene-file check (`git status`/`git diff` on affected `.unity` files) to confirm Unity's re-serialization removed the now-nonexistent field references cleanly, and confirm no console "missing field" warnings appear when the scene loads.
**Warning signs:** Inspector still shows a "Bgm Source" or "Low Pass Filter" slot on `EnvironmentManager` after the refactor (if the C# field wasn't actually removed), or a scene diff shows orphaned GUID references.

### Pitfall 4: CP949 (non-UTF-8) encoding corruption when editing EnvironmentManager.cs
**What goes wrong:** `EnvironmentManager.cs` is currently encoded in CP949 (Korean legacy encoding), not UTF-8 — visible from the mojibake (`????`-style garbled Korean comments) already present in the Read output above. STATE.md's own accumulated decisions log explicitly documents this exact class of failure occurring twice before (`Checkpoint.cs`, `WoodBossController.cs`): standard Read/Edit tool round-trips through UTF-8 silently corrupt non-ASCII bytes in CP949 files, and this is **not detectable** by a naive non-ASCII byte-count grep gate.
**Why it happens:** The project's file-editing tools assume UTF-8; CP949 byte sequences that happen to overlap with UTF-8 continuation-byte patterns get silently mangled into U+FFFD replacement characters on save.
**How to avoid:** Per the project's own established mitigation (STATE.md, Phase 11 Plan 3): extract the original bytes via `git show HEAD:<path>` and perform byte-level scripted edits rather than using the standard Read/Edit tool round-trip, or at minimum verify post-edit that Korean comment text is not replaced with `?` or mojibake-of-mojibake after any edit to `EnvironmentManager.cs`.
**Warning signs:** Korean comments in the diff show as `?`/U+FFFD sequences instead of preserving (or plausibly re-mangling in the *same* garbled way) the original bytes.

## Code Examples

### Existing Code: Exact Signatures (to preserve per D-10/D-12)

`Assets/Script/AudioManager.cs` (current, to be extended not replaced):
```csharp
public static AudioManager Instance { get; private set; }
public float BgmVolume { get; private set; } = 1f;
public float SfxVolume { get; private set; } = 1f;
public void SetBGMVolume(float value)   // MUST keep this exact signature (D-10)
public void SetSFXVolume(float value)   // MUST keep this exact signature (D-10)
public void PlaySFX(AudioSource src, AudioClip clip = null)  // existing consumer-facing API; not mentioned in CONTEXT.md decisions as changing, but likely superseded in practice by new Play(cue, ...) API — CONTEXT.md does not explicitly say to keep or remove PlaySFX, so treat as an open question for the planner (see Open Questions)
```

`Assets/Script/EnvironmentManager.cs` (current, audio-manipulating methods to remove/replace per D-12):
```csharp
void ApplyEnvironmentEffects(EnvironmentState state)  // keeps color-changing logic (backgroundRenderers), audio branch replaced with AudioManager.Instance.SetEnvironmentState(state) call
void ChangeBGMCutoff(float targetCutoff)              // entire method's logic (bgmSource.Play(), lowPassFilter.cutoffFrequency assignment) moves into AudioManager per D-12
private enum EnvironmentState { None, Alive, Neutral, Withered }  // this enum needs to become visible to AudioManager.SetEnvironmentState's parameter — either make it a shared/public enum (e.g., move to its own file or make it internal/public and shared), since D-12 requires AudioManager.SetEnvironmentState(EnvironmentState state) to accept it
```
**Note:** the current `EnvironmentState` enum is `private` inside `EnvironmentManager`. For `AudioManager.SetEnvironmentState(EnvironmentState state)` to exist as specified in D-12, this enum must become accessible to both classes — e.g., promote it to a top-level (non-nested, non-private) enum, or nest it in `AudioManager` and have `EnvironmentManager` reference `AudioManager.EnvironmentState`. This is a concrete task-level decision the planner needs to make explicit (see Open Questions).

`Assets/Player/Script/Menu/SoundSettingsPanel.cs` (current, consumer — confirmed unaffected):
```csharp
AudioManager.Instance?.SetBGMVolume(value);   // unaffected if D-10 signature is preserved
AudioManager.Instance?.SetSFXVolume(value);   // unaffected if D-10 signature is preserved
```
No UI slider for the new UI-category volume (D-11) currently exists in `SoundSettingsPanel.cs` — CONTEXT.md's D-11 only requires the backend field/parameter to exist ("추가 필요"), not a UI slider; adding a UI slider is not explicitly requested and would be scope creep unless the planner confirms it's wanted.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|---------------|--------|
| `EnvironmentManager` directly holds/mutates `AudioSource` + `AudioLowPassFilter` | `AudioManager` owns all audio playback and filter state; `EnvironmentManager` only judges state and calls `SetEnvironmentState` | This phase | Enforces single-owner principle for audio, consistent with the phase's stated goal of centralizing "재생 권한" (playback authority) |
| Scene-placed `AudioManager` GameObject (nested, non-root, DontDestroyOnLoad silently failing) | Code-instantiated root-level object via `PersistentManagers` bootstrap | This phase | Actually fixes persistence (previously broken, per Critical Finding) |
| `AudioSource.PlayOneShot` fire-and-forget SFX (`AudioManager.PlaySFX`) | Cue-driven pooled `AudioSource`s with tracked lifecycle (priority, start time, follow target) | This phase | Enables position-tracking, cooldown, and priority-stealing — none of which `PlayOneShot` supports |

**Deprecated/outdated:** The existing `PlaySFX(AudioSource src, AudioClip clip)` method's design (caller supplies its own `AudioSource`) is inconsistent with a centrally-pooled architecture where `AudioManager` owns all `AudioSource`s. The planner should decide explicitly whether to keep, deprecate-but-keep-for-compat, or remove this method (see Open Questions) — CONTEXT.md does not address it directly.

## Open Questions

1. **Does `PlaySFX(AudioSource src, AudioClip clip)` stay, get deprecated, or get removed?**
   - What we know: CONTEXT.md's decisions only specify the new `Play(cue, Transform)` / `Play(cue, Vector3)` API and the preserved `SetBGMVolume`/`SetSFXVolume` signatures (D-10). It says nothing about `PlaySFX`.
   - What's unclear: Whether any other script currently calls `AudioManager.Instance.PlaySFX(...)` (this research did not find a grep hit for `PlaySFX(` usage beyond its own definition in the files read — a full-repo grep for `.PlaySFX(` should be run at planning/task-breakdown time to confirm zero or nonzero call sites).
   - Recommendation: If no other call sites exist, safe to remove `PlaySFX` in favor of the new pooled `Play` API (cleaner, avoids two parallel playback paths). If call sites exist, keep it as a thin legacy wrapper or migrate call sites within this phase's diff.

2. **Where does `EnvironmentState` enum live after the refactor?**
   - What we know: D-12 requires `AudioManager.SetEnvironmentState(EnvironmentState state)`; currently `EnvironmentState` is a `private enum` nested in `EnvironmentManager`.
   - What's unclear: CONTEXT.md doesn't specify enum ownership.
   - Recommendation: Move the enum to `AudioManager` (since it now owns the audio-effect-per-state mapping) as a public nested enum `AudioManager.EnvironmentState`, and have `EnvironmentManager` reference it via `AudioManager.EnvironmentState.Alive` etc. This keeps the state-judgment logic in `EnvironmentManager` (per D-12) while the type itself lives with its primary consumer.

3. **Should the scene-placed `AudioManager` GameObjects in `Tutorial Map.unity` and `Assets/_Recovery/0.unity` be deleted from the scene files, or left as inert leftovers?**
   - What we know: If `AudioManager` becomes bootstrap-instantiated (recommended), the scene-placed instances are redundant and, given the existing singleton `Awake()` duplicate-destroy guard, would self-destroy immediately if they load after the bootstrap's instance already exists — but if they load *before* the bootstrap runs (e.g., scene-embedded objects initialize in `Awake` before a separate bootstrapper's `RuntimeInitializeOnLoadMethod` fires, depending on which `RuntimeInitializeOnLoadType` is chosen), the scene-placed one could become "the" singleton instead, with its non-root parenting bug intact.
   - What's unclear: Exact `Awake()`/`RuntimeInitializeOnLoadMethod` ordering guarantees relative to scene-embedded `MonoBehaviour.Awake()` calls for the specific `RuntimeInitializeOnLoadType` the planner picks (e.g., `BeforeSceneLoad` runs before any scene's `Awake()`, which is the safe choice; `AfterSceneLoad` would race with the scene-placed object's own `Awake()`).
   - Recommendation: Use `RuntimeInitializeOnLoadType.BeforeSceneLoad` for the bootstrap (guarantees it runs first, deterministically winning the singleton race), and explicitly remove the scene-placed `AudioManager` GameObject (and its child BGM `AudioSource`, re-homed onto the new prefab) from `Tutorial Map.unity` and `Assets/_Recovery/0.unity` as part of this phase's task list, rather than leaving a dead duplicate in the scene file.

4. **Exact default pool size (N) and initial priority-value scale (e.g., 0–10? enum? int with no bound?) are unspecified.**
   - What we know: CONTEXT.md explicitly defers this to Claude's discretion.
   - What's unclear: No gameplay-frequency data was available in this research (no existing SFX call-site inventory across the ~226 `Debug.Log` / gameplay scripts was performed, since no audio hooks exist yet anywhere in the codebase per CONTEXT.md's own `code_context` section: "이 프로젝트에 기존 AudioSource 풀링... 없음").
   - Recommendation: A small fixed pool (8–16 concurrent voices is a common default for 2D action games of this scale) is a reasonable starting point; treat as an easily-tunable `[SerializeField] private int poolSize = 16;` Inspector value rather than a hardcoded constant, so it can be adjusted without code changes once real SFX density is observed in playtesting.

## Environment Availability

No external dependencies beyond the Unity Editor itself (confirmed `6000.3.10f1` via `ProjectSettings/ProjectVersion.txt`) and built-in `UnityEngine.Audio` APIs, which ship with every Unity installation. No package manager entries, no third-party audio middleware, no network services. Skipping the Environment Availability table as not applicable beyond this confirmation.

## Sources

### Primary (HIGH confidence)
- Direct code inspection: `Assets/Script/AudioManager.cs`, `Assets/Script/EnvironmentManager.cs`, `Assets/Player/Script/Menu/SoundSettingsPanel.cs` — exact current signatures and behavior confirmed by reading the files.
- Direct scene file inspection: `Assets/Scenes/Tutorial Map.unity` (grep for `AudioManager`, traced `m_Father` chain) — confirmed `AudioManager` is a non-root child of `Manager` GameObject, same parent as the BUG-007 `InputManager`.
- `.planning/phases/20-audio-centralization/20-CONTEXT.md` — all D-01 through D-13 locked decisions.
- `.planning/phases/15-load-timing-and-load-scope/bugs/BUG-007-inputhandler-lost-on-scene-transition.md` — documents the exact `DontDestroyOnLoad only works for root GameObjects...` failure mode independently observed for `InputManager`.
- `ProjectSettings/ProjectVersion.txt` — confirmed Unity Editor version 6000.3.10f1.

### Secondary (MEDIUM confidence)
- Unity's documented `Mathf.Log10(x) * 20` linear-to-decibel conversion pattern and `AudioMixer.SetFloat`/exposed-parameter workflow — this is Claude's trained knowledge of Unity's own long-stable, widely-documented Audio Mixer tutorial pattern (present in Unity Learn material and the AudioMixer scripting API reference), not independently re-verified against live docs in this research session (no internet fetch was performed for this phase; the API surface — `AudioMixer.SetFloat`, `AudioMixerGroup`, `AudioSource.outputAudioMixerGroup`, `AudioSource.spatialBlend` — has been stable and unchanged since early Unity 5.x through Unity 6, so staleness risk is low, but flagged as MEDIUM rather than HIGH since it was not freshly verified via Context7/WebFetch in this session).

### Tertiary (LOW confidence)
- Default pool size recommendation (8–16 voices) — general genre-conventional guidance, not derived from any measurement of this specific project's actual SFX density (none exists yet, since no audio hooks are wired up anywhere in the codebase currently).

## Metadata

**Confidence breakdown:**
- Standard stack (AudioMixer/AudioCue/pooling APIs): HIGH — built-in, stable, long-unchanged Unity APIs; verified against actual project files where relevant (existing AudioManager/EnvironmentManager/SoundSettingsPanel signatures)
- Architecture (bootstrap, pool, mixer wiring patterns): MEDIUM-HIGH — patterns are standard, but no live Context7/WebFetch verification was performed this session (offline code-focused research); the critical finding about AudioManager's broken persistence was independently discovered via direct scene-file inspection, not asserted from training data
- Pitfalls: HIGH — pitfalls 1, 3, and 4 are drawn directly from this project's own confirmed state (scene file inspection, existing CP949 encoding, existing STATE.md incident log), not generic guesses; pitfall 2 (Log10 clamp) is standard, well-known Unity gotcha

**Research date:** 2026-09-22
**Valid until:** Unity Editor version and core Audio APIs are stable; this research should remain valid indefinitely for the current Unity 6000.3.10f1 project unless the project migrates Unity versions or adopts third-party audio middleware. Treat the project-specific findings (scene file states, current code signatures) as valid only until the phase's own execution changes them.
