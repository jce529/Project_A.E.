using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// PersistentManagers creates and persists this manager, which owns mixer volume,
// BGM playback, the voice pool, and environment filtering.
// Settings are held in memory until the settings panel saves them.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public enum EnvironmentState { None, Alive, Neutral, Withered }

    private const float AliveCutoff = 22000f;
    private const float NeutralCutoff = 5000f;
    private const float WitheredCutoff = 1000f;

    private AudioMixer mixer;
    private AudioMixerGroup bgmGroup;
    private AudioMixerGroup sfxGroup;
    private AudioMixerGroup uiGroup;
    private AudioSource bgmSource;
    private AudioLowPassFilter bgmLowPass;
    private EnvironmentState currentEnvironmentState = EnvironmentState.None;
    [SerializeField] private int poolSize = 16;

    private class PoolSlot
    {
        public AudioSource Source;
        public Transform SourceTransform;
        public bool InUse;
        public int Priority;
        public float StartTime;
        public Transform FollowTarget;
    }

    private PoolSlot[] pool;
    private readonly Dictionary<AudioCue, float> lastPlayedTime = new Dictionary<AudioCue, float>();

    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;
    public float UiVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadMixer();
        CreatePool();
        CreateBgmSource();
        LoadVolumes();
    }

    private void LoadMixer()
    {
        mixer = Resources.Load<AudioMixer>("Master");
        if (mixer == null)
        {
            Debug.LogError("AudioManager: Assets/Audio/Resources/Master.mixer not found. Volume control and group routing are disabled.");
            return;
        }
        bgmGroup = FindGroup("BGM");
        sfxGroup = FindGroup("SFX");
        uiGroup = FindGroup("UI");
    }

    private AudioMixerGroup FindGroup(string groupName)
    {
        var groups = mixer.FindMatchingGroups(groupName);
        if (groups == null || groups.Length == 0)
        {
            Debug.LogError("AudioManager: AudioMixer group '" + groupName + "' not found in Master.mixer.");
            return null;
        }
        return groups[0];
    }

    public AudioMixerGroup GetMixerGroup(AudioCategory category)
    {
        switch (category)
        {
            case AudioCategory.BGM: return bgmGroup;
            case AudioCategory.UI: return uiGroup;
            default: return sfxGroup;
        }
    }

    private void CreatePool()
    {
        pool = new PoolSlot[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("PooledSource_" + i);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            pool[i] = new PoolSlot { Source = src, SourceTransform = go.transform };
        }
    }

    private PoolSlot AcquireSlot(int requestPriority)
    {
        for (int i = 0; i < pool.Length; i++)
            if (!pool[i].InUse) return pool[i];

        PoolSlot steal = null;
        for (int i = 0; i < pool.Length; i++)
        {
            var slot = pool[i];
            if (slot.Priority >= requestPriority) continue;
            if (steal == null || slot.StartTime < steal.StartTime) steal = slot;
        }
        if (steal != null) steal.Source.Stop();
        return steal;
    }

    private void Update()
    {
        if (pool == null) return;
        for (int i = 0; i < pool.Length; i++)
        {
            var slot = pool[i];
            if (!slot.InUse) continue;
            if (!slot.Source.isPlaying)
            {
                slot.InUse = false;
                slot.FollowTarget = null;
                slot.Priority = 0;
                continue;
            }
            if (slot.FollowTarget != null)
                slot.SourceTransform.position = slot.FollowTarget.position;
        }
    }

    private void CreateBgmSource()
    {
        var go = new GameObject("BGMSource");
        go.transform.SetParent(transform, false);
        bgmSource = go.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = 1f;
        bgmSource.outputAudioMixerGroup = bgmGroup;
        bgmLowPass = go.AddComponent<AudioLowPassFilter>();
        bgmLowPass.cutoffFrequency = AliveCutoff;
    }

    private void LoadVolumes()
    {
        var s = SaveLoadManager.CurrentSettings;
        SetBGMVolume(s.BgmVolume);
        SetSFXVolume(s.SfxVolume);
        SetUIVolume(s.UiVolume);
    }

    public void SetBGMVolume(float value)
    {
        BgmVolume = value;
        ApplyVolume("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        SfxVolume = value;
        ApplyVolume("SFXVolume", value);
    }

    public void SetUIVolume(float value)
    {
        UiVolume = value;
        ApplyVolume("UIVolume", value);
    }

    private void ApplyVolume(string exposedParam, float linear01)
    {
        if (mixer == null) return;
        mixer.SetFloat(exposedParam, LinearToDecibel(linear01));
    }

    private static float LinearToDecibel(float linear)
    {
        return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
    }

    public void Play(AudioCue cue)
    {
        PlayInternal(cue, transform.position, null);
    }

    public void Play(AudioCue cue, Vector3 position)
    {
        PlayInternal(cue, position, null);
    }

    public void Play(AudioCue cue, Transform target)
    {
        if (target == null) return;
        PlayInternal(cue, target.position, target);
    }

    private void PlayInternal(AudioCue cue, Vector3 position, Transform followTarget)
    {
        if (cue == null || pool == null) return;
        AudioClip clip = cue.GetClip();
        if (clip == null) return;

        float now = Time.unscaledTime;
        if (cue.Cooldown > 0f
            && lastPlayedTime.TryGetValue(cue, out float last)
            && now - last < cue.Cooldown)
            return;

        PoolSlot slot = AcquireSlot(cue.Priority);
        if (slot == null) return;
        lastPlayedTime[cue] = now;

        slot.InUse = true;
        slot.Priority = cue.Priority;
        slot.StartTime = now;
        slot.FollowTarget = followTarget;
        slot.SourceTransform.position = position;

        AudioSource src = slot.Source;
        src.clip = clip;
        src.volume = cue.GetVolume();
        src.pitch = cue.GetPitch();
        src.loop = cue.Loop;
        src.spatialBlend = cue.SpatialBlend;
        src.outputAudioMixerGroup = GetMixerGroup(cue.Category);
        src.Play();
    }

    public void SetEnvironmentState(EnvironmentState state)
    {
        if (state == currentEnvironmentState) return;
        currentEnvironmentState = state;

        float cutoff;
        switch (state)
        {
            case EnvironmentState.Alive: cutoff = AliveCutoff; break;
            case EnvironmentState.Neutral: cutoff = NeutralCutoff; break;
            case EnvironmentState.Withered: cutoff = WitheredCutoff; break;
            default: return;
        }

        // Existing scenes hold their BGM clip on the EnvironmentManager object.
        // Adopt that clip once so the new persistent source can actually play it.
        if (bgmSource != null && bgmSource.clip == null)
        {
            var environment = FindAnyObjectByType<EnvironmentManager>();
            var sceneSource = environment != null ? environment.GetComponent<AudioSource>() : null;
            if (sceneSource != null) bgmSource.clip = sceneSource.clip;
        }

        if (bgmSource != null && bgmSource.clip != null && !bgmSource.isPlaying)
            bgmSource.Play();
        if (bgmLowPass != null)
            bgmLowPass.cutoffFrequency = cutoff;
    }
}
