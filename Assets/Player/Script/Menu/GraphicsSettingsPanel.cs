using UnityEngine;
using TMPro;

// 일시정지 > 그래픽 탭: 화면 모드 설정
// ◀ ▶ 버튼 OnClick → OnScreenModePrev / OnScreenModeNext 연결
public class GraphicsSettingsPanel : MonoBehaviour
{
    [Header("화면 모드")]
    public TMP_Text screenModeValueText;

    private static readonly string[] ScreenModeNames = { "Fullscreen", "Borderless", "Windowed" };
    private static readonly FullScreenMode[] ScreenModes =
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed,
    };

    private int _screenModeIndex;

    private void OnEnable()
    {
        _screenModeIndex = SaveLoadManager.CurrentSettings.ScreenMode;
        RefreshText();
    }

    // ◀ 버튼 OnClick
    public void OnScreenModePrev()
    {
        _screenModeIndex = (_screenModeIndex - 1 + ScreenModes.Length) % ScreenModes.Length;
        Apply();
    }

    // ▶ 버튼 OnClick
    public void OnScreenModeNext()
    {
        _screenModeIndex = (_screenModeIndex + 1) % ScreenModes.Length;
        Apply();
    }

    private void Apply()
    {
        Screen.fullScreenMode = ScreenModes[_screenModeIndex];
        SaveLoadManager.CurrentSettings.ScreenMode = _screenModeIndex;
        RefreshText();
    }

    private void RefreshText()
    {
        if (screenModeValueText != null)
            screenModeValueText.text = ScreenModeNames[_screenModeIndex];
    }

    // 게임 시작 시 저장된 화면 모드 복원 (씬 로드 후 자동 호출용)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RestoreOnStartup()
    {
        int saved = SaveLoadManager.CurrentSettings.ScreenMode;
        if (saved >= 0 && saved < ScreenModes.Length)
            Screen.fullScreenMode = ScreenModes[saved];
    }
}
