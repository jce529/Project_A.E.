using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused,
        Inventory,
        Loading,
        GameClear,
        Puzzle
    }

    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public event Action<GameState> OnGameStateChange;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        switch (newState)
        {
            case GameState.Paused:
            case GameState.Inventory:
            case GameState.GameClear:
            case GameState.Puzzle:
                Time.timeScale = 0f;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                break;
        }

        OnGameStateChange?.Invoke(newState);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool stateChanged = CurrentState != GameState.Playing;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        if (stateChanged)
            OnGameStateChange?.Invoke(CurrentState);
    }
}
