using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 일시정지 > 게임 탭: 언어, 화면흔들림, 튜토리얼 힌트 설정 + 진행상황 저장
public class GameSettingsPanel : MonoBehaviour
{
    public const string SaveDoneCopy   = "진행상황을 저장했습니다.";
    public const string SaveFailedCopy = "저장에 실패했습니다.";

    [Header("언어 설정")]
    public TMP_Text languageValueText;  // 현재 언어 표시 텍스트

    [Header("토글")]
    public Toggle screenShakeToggle;
    public Toggle tutorialHintToggle;

    [Header("진행상황 저장")]
    public TMP_Text saveProgressFeedbackText;  // 저장 결과 안내 (선택 - 비워둬도 동작함)

    private static readonly string[] Languages = { "한국어", "English" };
    private int _langIndex;

    private void OnEnable()
    {
        var s = SaveLoadManager.CurrentSettings;
        _langIndex = s.Language;
        RefreshLanguageText();

        if (screenShakeToggle  != null) screenShakeToggle.isOn  = s.ScreenShake;
        if (tutorialHintToggle != null) tutorialHintToggle.isOn = s.TutorialHint;

        // 이전에 띄운 저장 안내는 탭을 다시 열 때 지운다.
        SetSaveFeedback("");
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

    // [진행상황 저장] 버튼 OnClick - 현재 슬롯 파일에 지금까지의 진행상황을 기록한다.
    // 위치는 마지막으로 활성화한 체크포인트가 기준이다 (D-05: 원시 좌표를 저장하지 않음).
    public void OnSaveProgressBtnClick()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[GameSettingsPanel] SaveLoadManager.Instance is null - 진행상황 저장 실패");
            SetSaveFeedback(SaveFailedCopy);
            return;
        }

        SaveLoadManager.Instance.SaveAnywhere();
        SetSaveFeedback(SaveDoneCopy);
    }

    private void SetSaveFeedback(string message)
    {
        if (saveProgressFeedbackText != null)
            saveProgressFeedbackText.text = message;
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
