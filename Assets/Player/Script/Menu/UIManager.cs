using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject BackgroundPanel;
    public GameObject ClearPanel; // 보스 컨트롤러 참조용

    private Stack<GameObject> uiStack = new Stack<GameObject>();

    // [중요] 외부에서 현재 맨 위의 패널을 확인할 수 있는 프로퍼티 (PauseMenu 오류 해결용)
    public GameObject TopPanel
    {
        get
        {
            if (uiStack.Count > 0) return uiStack.Peek();
            return null;
        }
    }

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PushPanel(GameObject panel)
    {
        if (panel == null) return;

        panel.SetActive(true);
        uiStack.Push(panel);

        if (BackgroundPanel != null) BackgroundPanel.SetActive(true);

        UpdateGameState();
    }

    public void PopPanel()
    {
        if (uiStack.Count == 0) return;

        GameObject topPanel = uiStack.Pop();
        topPanel.SetActive(false);

        if (uiStack.Count > 0)
        {
            uiStack.Peek().SetActive(true);
        }
        else
        {
            if (BackgroundPanel != null) BackgroundPanel.SetActive(false);
        }

        UpdateGameState();
    }

    // [중요] 모든 창을 닫는 기능 (PauseMenu 오류 해결용)
    public void CloseAll()
    {
        while (uiStack.Count > 0)
        {
            GameObject panel = uiStack.Pop();
            panel.SetActive(false);
        }

        if (BackgroundPanel != null) BackgroundPanel.SetActive(false);

        UpdateGameState();
    }

    // 현재 UI가 열려있는지 확인
    public bool IsUIOpen()
    {
        return uiStack.Count > 0;
    }

    private void UpdateGameState()
    {
        if (GameStateManager.Instance == null) return;

        if (uiStack.Count == 0)
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
            return;
        }

        GameObject topPanel = uiStack.Peek();
        UIPanelProperties properties = topPanel.GetComponent<UIPanelProperties>();

        if (properties != null)
        {
            GameStateManager.Instance.SetState(properties.targetState);
        }
        else
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.Paused);
        }
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}