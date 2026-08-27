using System;
using UnityEngine;

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
    //�̱��� ����
    //������ ���� �� �ϳ��ν� �ϳ��� ��ü�� �����Ͽ� ����Ͽ� �� ��ü���� ����ϴ� ������ �����Դϴ�.

    //��ɸ��� ���� Ŭ������ ������������ ����� ���� �޸����� �����̱� ������ �ϳ��� ���

    // �̱��� �ν��Ͻ�
    public static GameStateManager Instance { get; private set; }

    // ���� ���� ���¸� �ܺο��� ���� �� �ְ� ��
    public GameState CurrentState { get; private set; } = GameState.Playing;

    // ���� ���� �� �̺�Ʈ�� �߻����� �ٸ� �ý����� �����ϵ��� ��
    public event Action<GameState> OnGameStateChange;

    private void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (Instance == null)
        {
            Instance = this;
            // ���� �ٲ� �ı����� �ʰ� ���� (�ɼ�)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���¸� �����ϴ� �ٽ� �Լ�
    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

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

        // 2. �����ڵ鿡�� ���°� ����Ǿ����� �˸�
        OnGameStateChange?.Invoke(newState);
    }
}