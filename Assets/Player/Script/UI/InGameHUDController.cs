using UnityEngine;

// UI Canvas에 붙여서 사용
// hudElements에 WaterUIController, HealthBarController 등 HUD 오브젝트를 직접 할당
public class InGameHUDController : MonoBehaviour
{
    public GameObject[] hudElements;

    private void Start()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnGameStateChange += OnGameStateChanged;

        var state = GameStateManager.Instance != null
            ? GameStateManager.Instance.CurrentState
            : GameStateManager.GameState.Playing;

        SetHUDVisible(state == GameStateManager.GameState.Playing);
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnGameStateChange -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameStateManager.GameState newState)
    {
        SetHUDVisible(newState == GameStateManager.GameState.Playing);
    }

    private void SetHUDVisible(bool visible)
    {
        foreach (var obj in hudElements)
            if (obj != null) obj.SetActive(visible);
    }
}
