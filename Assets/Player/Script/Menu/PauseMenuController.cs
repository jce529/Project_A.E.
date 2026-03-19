using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    // 외부에서 접근할 일이 거의 없으므로 싱글톤 제거 가능 (원하면 유지)
    public static PauseMenuController Instance;

    [Header("첫 번째 메뉴만 연결")]
    public GameObject pausePanel; // 메인 일시정지 패널만 연결

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnPauseEvent += HandlePauseInput;
        }

        // 시작 시 메인 패널만 확실히 꺼줌 (나머지는 각자 부모 밑에 있으니 알아서 꺼짐)
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnPauseEvent -= HandlePauseInput;
    }

    private void HandlePauseInput()
    {
        // 1. UI가 열려있으면 -> 뒤로가기
        if (!UIManager.Instance.TopPanel)
        {
            UIManager.Instance.PopPanel();
        }
        // 2. 게임 중이면 -> 메인 메뉴(PausePanel) 열기
        else if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
        {
            if (pausePanel != null)
            {
                UIManager.Instance.PushPanel(pausePanel);
            }
        }
    }
}