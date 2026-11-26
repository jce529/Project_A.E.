using UnityEngine;

// D-12: AudioManager owns environment audio.
using EnvironmentState = AudioManager.EnvironmentState;

public class EnvironmentManager : MonoBehaviour
{
    [Header("연결 정보")]
    public WaterController waterController;

    [Header("환경 설정 (배경/타일 색조 등)")]
    public SpriteRenderer[] backgroundRenderers;
    public Color aliveColor = Color.white;
    public Color neutralColor = Color.gray;
    public Color witheredColor = new Color(0.4f, 0.2f, 0.2f);

    private EnvironmentState currentState = EnvironmentState.None;

    void Start()
    {
        UpdateEnvironmentState();
    }

    void Update()
    {
        UpdateEnvironmentState();
    }

    void UpdateEnvironmentState()
    {
        if (waterController == null) return;

        int currentWater = waterController.waterCounter() + waterController.corruptedwaterCounter();
        int maxWater = waterController.bottles.Count;
        float waterRatio = (maxWater > 0) ? (float)currentWater / maxWater : 0;

        EnvironmentState newState;

        if (waterRatio > 0.66f)
            newState = EnvironmentState.Alive;
        else if (waterRatio > 0.33f)
            newState = EnvironmentState.Neutral;
        else
            newState = EnvironmentState.Withered;

        if (newState != currentState)
        {
            currentState = newState;
            ApplyEnvironmentEffects(currentState);
        }
    }

    void ApplyEnvironmentEffects(EnvironmentState state)
    {
        // D-12: report the judged state; AudioManager handles playback and filtering.
        AudioManager.Instance?.SetEnvironmentState(state);

        Color targetColor = Color.white;

        switch (state)
        {
            case EnvironmentState.Alive:
                targetColor = aliveColor;
                break;

            case EnvironmentState.Neutral:
                targetColor = neutralColor;
                break;

            case EnvironmentState.Withered:
                targetColor = witheredColor;
                break;
        }

        if (backgroundRenderers != null)
        {
            foreach (SpriteRenderer sr in backgroundRenderers)
            {
                if (sr != null) sr.color = targetColor;
            }
        }
    }
}