using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [Header("첫 번째 메뉴의 패널")]
    public GameObject pausePanel;

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
        if (UIManager.Instance.IsUIOpen())
        {
            UIManager.Instance.PopPanel();
        }
        // 2. 게임 중이면 -> 일시 정지 메뉴(PausePanel) 열기
        else if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
        {
            if (pausePanel != null)
            {
                UIManager.Instance.PushPanel(pausePanel);
            }
        }
    }
}
