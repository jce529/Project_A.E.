using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 일시정지 > 사운드 탭: BGM / SFX 볼륨 슬라이더
// AudioManager 싱글톤을 통해 볼륨 적용 및 PlayerPrefs 저장
public class SoundSettingsPanel : MonoBehaviour
{
    [Header("BGM")]
    public Slider bgmSlider;
    public TMP_Text bgmValueText;   // "75%" 형식 표시 (없으면 생략 가능)

    [Header("SFX")]
    public Slider sfxSlider;
    public TMP_Text sfxValueText;

    private void OnEnable()
    {
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            bgmSlider.SetValueWithoutNotify(bgm);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            sfxSlider.SetValueWithoutNotify(sfx);
        }
        UpdateBGMText(bgm);
        UpdateSFXText(sfx);
    }

    private void OnDisable()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }

    public void OnBGMChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        UpdateBGMText(value);
    }

    public void OnSFXChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        UpdateSFXText(value);
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
