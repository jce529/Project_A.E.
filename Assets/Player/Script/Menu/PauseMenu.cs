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
