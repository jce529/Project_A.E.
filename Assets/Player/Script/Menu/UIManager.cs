using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject BackgroundPanel;
    public GameObject ClearPanel;

    private Stack<GameObject> uiStack = new Stack<GameObject>();

    // 현재 화면에 표시 중인 최상단 패널 추적 필드
    private GameObject _currentPanel;

    public GameObject TopPanel => _currentPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PushPanel(GameObject panel)
    {
        if (panel == null) return;

        // 현재 패널 숨기기 (겹침 방지)
        if (_currentPanel != null)
            _currentPanel.SetActive(false);

        uiStack.Push(panel);
        _currentPanel = panel;
        _currentPanel.SetActive(true);

        if (BackgroundPanel != null) BackgroundPanel.SetActive(true);

        UpdateGameState();
    }

    public void PopPanel()
    {
        if (uiStack.Count == 0) return;

        // 현재 패널 닫기
        uiStack.Pop().SetActive(false);

        // 이전 패널로 복귀
        if (uiStack.Count > 0)
        {
            _currentPanel = uiStack.Peek();
            _currentPanel.SetActive(true);
        }
        else
        {
            _currentPanel = null;
            if (BackgroundPanel != null) BackgroundPanel.SetActive(false);
        }

        UpdateGameState();
    }

    public void CloseAll()
    {
        while (uiStack.Count > 0)
            uiStack.Pop().SetActive(false);

        _currentPanel = null;

        if (BackgroundPanel != null) BackgroundPanel.SetActive(false);

        UpdateGameState();
    }

    public bool IsUIOpen()
    {
        return uiStack.Count > 0;
    }

    private void UpdateGameState()
    {
        if (GameStateManager.Instance == null) return;

        if (_currentPanel == null)
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
            return;
        }

        UIPanelProperties properties = _currentPanel.GetComponent<UIPanelProperties>();
        GameStateManager.Instance.SetState(
            properties != null ? properties.targetState : GameStateManager.GameState.Paused);
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
