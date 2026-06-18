using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 일시정지 > 사운드 탭: BGM / SFX 볼륨 슬라이더
// AudioMixer 미사용 — PlayerPrefs 저장 + 씬 내 AudioSource 태그로 적용
public class SoundSettingsPanel : MonoBehaviour
{
    [Header("BGM")]
    public Slider bgmSlider;
    public TMP_Text bgmValueText;   // "75%" 같이 표시 (없으면 생략 가능)

    [Header("SFX")]
    public Slider sfxSlider;
    public TMP_Text sfxValueText;

    private void OnEnable()
    {
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (bgmSlider != null) { bgmSlider.value = bgm; UpdateBGMText(bgm); }
        if (sfxSlider != null) { sfxSlider.value = sfx; UpdateSFXText(sfx); }
    }

    // BGM Slider OnValueChanged
    public void OnBGMChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
        UpdateBGMText(value);
        ApplyBGMToSources(value);
    }

    // SFX Slider OnValueChanged
    public void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
        UpdateSFXText(value);
        ApplySFXToSources(value);
    }

    // "BGM" 태그가 붙은 AudioSource에 볼륨 적용
    private void ApplyBGMToSources(float volume)
    {
        foreach (var src in FindObjectsOfType<AudioSource>())
        {
            if (src.CompareTag("BGM"))
                src.volume = volume;
        }
    }

    // "SFX" 태그가 붙은 AudioSource에 볼륨 적용
    private void ApplySFXToSources(float volume)
    {
        foreach (var src in FindObjectsOfType<AudioSource>())
        {
            if (src.CompareTag("SFX"))
                src.volume = volume;
        }
    }

    private void UpdateBGMText(float value)
    {
        if (bgmValueText != null)
            bgmValueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    private void UpdateSFXText(float value)
    {
        if (sfxValueText != null)
            sfxValueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
