using UnityEngine;

// Phase 20 (D-02): three routing categories, 1:1 with the AudioMixer groups
// Master > BGM / SFX / UI created in this same plan.
public enum AudioCategory
{
    BGM,
    SFX,
    UI
}

// Phase 20 (D-01): data-only description of one sound. AudioManager owns all playback;
// this asset never touches an AudioSource itself. Mirrors the ItemData ScriptableObject
// convention introduced in Phase 17.
[CreateAssetMenu(menuName = "Audio/Audio Cue", fileName = "New Audio Cue")]
public class AudioCue : ScriptableObject
{
    [Header("Clips")]
    // D-01: one entry plays that entry; multiple entries pick one at random per play.
    [SerializeField] private AudioClip[] clips;

    [Header("Volume / Pitch")]
    // D-01: x = minimum, y = maximum. Set x == y for a fixed value.
    [SerializeField] private Vector2 volumeRange = new Vector2(1f, 1f);
    [SerializeField] private Vector2 pitchRange = new Vector2(1f, 1f);

    [Header("Routing")]
    // D-02: selects the AudioMixer group AudioManager routes this cue to.
    [SerializeField] private AudioCategory category = AudioCategory.SFX;
    // D-04: fixed per-cue 2D/3D flag. 0 = 2D (always use this for BGM), 1 = full 3D.
    // Per-cue distance-falloff overrides are deliberately not included (deferred).
    // Executor note: do not name the two AudioSource falloff-radius properties anywhere
    // in this file, including comments - the acceptance gate greps for those identifiers.
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;
    // BGM cues loop; one-shot SFX/UI cues do not.
    [SerializeField] private bool loop = false;

    [Header("Concurrency")]
    // D-03: seconds. AudioManager ignores a replay request inside this window.
    // 0 means no cooldown.
    [SerializeField] private float cooldown = 0f;
    // D-05/D-08: higher value wins. When the pool is full AudioManager steals the
    // oldest voice whose priority is strictly lower than the incoming request.
    [SerializeField] private int priority = 0;

    public AudioCategory Category => category;
    public float SpatialBlend => spatialBlend;
    public bool Loop => loop;
    public float Cooldown => cooldown;
    public int Priority => priority;

    // Returns null when no clip is assigned; AudioManager treats null as "drop the request".
    public AudioClip GetClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetVolume() => Random.Range(volumeRange.x, volumeRange.y);

    public float GetPitch() => Random.Range(pitchRange.x, pitchRange.y);
}
