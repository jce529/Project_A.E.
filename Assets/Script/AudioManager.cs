using UnityEngine;

// BGM/SFX 볼륨을 관리하는 싱글톤. DontDestroyOnLoad로 씬 전환 후에도 유지됨.
// Inspector에서 bgmSource(BGM AudioSource)를 연결하세요.
// SFX는 AudioManager.Instance.SfxVolume을 참조하거나 PlaySFX()를 사용하세요.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM AudioSource")]
    [SerializeField] private AudioSource bgmSource;

    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LoadVolumes();
    }

    private void LoadVolumes()
    {
        BgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        SfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (bgmSource != null) bgmSource.volume = BgmVolume;
    }

    public void SetBGMVolume(float value)
    {
        BgmVolume = value;
        if (bgmSource != null) bgmSource.volume = value;
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        SfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    // SFX AudioSource에서 직접 호출: 현재 SFX 볼륨을 적용한 뒤 재생
    public void PlaySFX(AudioSource src, AudioClip clip = null)
    {
        if (src == null) return;
        if (clip != null)
            src.PlayOneShot(clip, SfxVolume);
        else
        {
            src.volume = SfxVolume;
            src.Play();
        }
    }
}
