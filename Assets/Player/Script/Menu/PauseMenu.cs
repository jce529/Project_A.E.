using UnityEngine;

// 일시정지 패널 - ESC 입력 처리 및 버튼 이벤트 담당
// 탭 전환은 PauseMenuTabController가 담당
public class PauseMenu : MonoBehaviour
{
    private void Start()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnPauseEvent += OnPauseInput;
            Debug.Log("[PauseMenu] Start() 실행됨 - OnPauseEvent 구독 완료");
        }
        else
        {
            Debug.LogError("[PauseMenu] Start() 실행됨 - InputHandler.Instance가 null! 구독 실패");
        }
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnPauseEvent -= OnPauseInput;
    }

    private void OnPauseInput()
    {
        Debug.Log($"[PauseMenu] OnPauseInput 수신 - 현재 패널 활성: {gameObject.activeSelf}, GameState: {GameStateManager.Instance?.CurrentState}");
        if (gameObject.activeSelf)
            Close();
        else if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
            Open();
    }

    // [계속하기] 버튼 OnClick
    public void OnResumeBtnClick() => Close();

    // [나가기] 버튼 OnClick
    public void OnQuitBtnClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // [설정 저장] 버튼 OnClick - 4개 설정 탭의 현재 메모리 값을 setting.json 으로 기록
    public void OnSaveSettingsBtnClick()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[PauseMenu] SaveLoadManager.Instance is null - settings not saved.");
            return;
        }
        SaveLoadManager.Instance.SaveSettings();
        Debug.Log("[PauseMenu] 설정 저장 완료");
    }

    private void Open()
    {
        gameObject.SetActive(true);
        GameStateManager.Instance?.SetState(GameStateManager.GameState.Paused);
    }

    private void Close()
    {
        gameObject.SetActive(false);
        GameStateManager.Instance?.SetState(GameStateManager.GameState.Playing);
    }
}
