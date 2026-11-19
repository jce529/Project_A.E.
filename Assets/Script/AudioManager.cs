using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps global audio settings while scene-local objects own their AudioSources.
/// Its lifetime is owned by the PersistentManagers root.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource bgmSource;

    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadVolumes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void LoadVolumes()
    {
        SettingsData settings = SaveLoadManager.CurrentSettings;
        BgmVolume = settings.BgmVolume;
        SfxVolume = settings.SfxVolume;
        ApplyBgmVolume();
    }

    public void RegisterBgmSource(AudioSource source)
    {
        bgmSource = source;
        ApplyBgmVolume();
    }

    public void UnregisterBgmSource(AudioSource source)
    {
        if (bgmSource == source)
            bgmSource = null;
    }

    public void SetBGMVolume(float value)
    {
        BgmVolume = value;
        ApplyBgmVolume();
    }

    public void SetSFXVolume(float value)
    {
        SfxVolume = value;
    }

    public void PlaySFX(AudioSource source, AudioClip clip = null)
    {
        if (source == null)
            return;

        if (clip != null)
        {
            source.PlayOneShot(clip, SfxVolume);
            return;
        }

        source.volume = SfxVolume;
        source.Play();
    }

    private void ApplyBgmVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = BgmVolume;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // SaveLoadManager is also bootstrapped BeforeSceneLoad. Refreshing here makes
        // the result independent of callback ordering between the two bootstraps.
        LoadVolumes();
    }
}
