using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 일시정지 > 게임 탭: 언어, 화면흔들림, 튜토리얼 힌트 설정
public class GameSettingsPanel : MonoBehaviour
{
    [Header("언어 설정")]
    public TMP_Text languageValueText;  // 현재 언어 표시 텍스트

    [Header("토글")]
    public Toggle screenShakeToggle;
    public Toggle tutorialHintToggle;

    private static readonly string[] Languages = { "한국어", "English" };
    private int _langIndex;

    private void OnEnable()
    {
        _langIndex = PlayerPrefs.GetInt("Language", 1);
        RefreshLanguageText();

        if (screenShakeToggle  != null) screenShakeToggle.isOn  = PlayerPrefs.GetInt("ScreenShake",   1) == 1;
        if (tutorialHintToggle != null) tutorialHintToggle.isOn = PlayerPrefs.GetInt("TutorialHint",  1) == 1;
    }

    // ◀ 버튼 OnClick
    public void OnLanguagePrev()
    {
        _langIndex = (_langIndex - 1 + Languages.Length) % Languages.Length;
        ApplyLanguage();
    }

    // ▶ 버튼 OnClick
    public void OnLanguageNext()
    {
        _langIndex = (_langIndex + 1) % Languages.Length;
        ApplyLanguage();
    }

    // 화면 흔들림 Toggle OnValueChanged
    public void OnScreenShakeChanged(bool value)
    {
        PlayerPrefs.SetInt("ScreenShake", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 튜토리얼 힌트 Toggle OnValueChanged
    public void OnTutorialHintChanged(bool value)
    {
        PlayerPrefs.SetInt("TutorialHint", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyLanguage()
    {
        PlayerPrefs.SetInt("Language", _langIndex);
        PlayerPrefs.Save();
        RefreshLanguageText();
    }

    private void RefreshLanguageText()
    {
        if (languageValueText != null)
            languageValueText.text = Languages[_langIndex];
    }
}
