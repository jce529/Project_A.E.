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
        var s = SaveLoadManager.CurrentSettings;
        _langIndex = s.Language;
        RefreshLanguageText();

        if (screenShakeToggle  != null) screenShakeToggle.isOn  = s.ScreenShake;
        if (tutorialHintToggle != null) tutorialHintToggle.isOn = s.TutorialHint;
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
        SaveLoadManager.CurrentSettings.ScreenShake = value;
    }

    // 튜토리얼 힌트 Toggle OnValueChanged
    public void OnTutorialHintChanged(bool value)
    {
        SaveLoadManager.CurrentSettings.TutorialHint = value;
    }

    private void ApplyLanguage()
    {
        SaveLoadManager.CurrentSettings.Language = _langIndex;
        RefreshLanguageText();
    }

    private void RefreshLanguageText()
    {
        if (languageValueText != null)
            languageValueText.text = Languages[_langIndex];
    }
}
