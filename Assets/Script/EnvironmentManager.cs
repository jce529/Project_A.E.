using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("연결 정보")]
    public WaterController waterController;
    public AudioSource bgmSource;

    // 새로 추가된 필터 연결 변수!
    public AudioLowPassFilter lowPassFilter;

    [Header("환경 설정 (배경/타일 색조 등)")]
    public SpriteRenderer[] backgroundRenderers;
    public Color aliveColor = Color.white;
    public Color neutralColor = Color.gray;
    public Color witheredColor = new Color(0.4f, 0.2f, 0.2f);

    [Header("BGM 먹먹함(Low Pass) 설정")]
    // 22000이 원본 소리(필터 없음)입니다. 숫자가 낮아질수록 더 먹먹해집니다.
    public float aliveCutoff = 22000f;    // 처음 (생기 있음 - 아주 맑은 소리)
    public float neutralCutoff = 5000f;   // 중간 (스크린샷에 있는 수치 정도의 먹먹함)
    public float witheredCutoff = 1000f;  // 시듦 (물속에 잠긴 듯 웅웅거리는 소리)

    private enum EnvironmentState { None, Alive, Neutral, Withered }
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
        Color targetColor = Color.white;

        switch (state)
        {
            case EnvironmentState.Alive:
                targetColor = aliveColor;
                ChangeBGMCutoff(aliveCutoff); // 필터 수치 변경
                break;

            case EnvironmentState.Neutral:
                targetColor = neutralColor;
                ChangeBGMCutoff(neutralCutoff); // 필터 수치 변경
                break;

            case EnvironmentState.Withered:
                targetColor = witheredColor;
                ChangeBGMCutoff(witheredCutoff); // 필터 수치 변경
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

    // 노래를 멈추지 않고 필터의 Cutoff Frequency 수치만 슉슉 바꿔주는 함수
    void ChangeBGMCutoff(float targetCutoff)
    {
        // 오디오 소스나 필터가 연결 안 되어있으면 에러 방지
        if (bgmSource == null || lowPassFilter == null) return;

        // BGM이 꺼져있다면 재생 시작
        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

        // 먹먹함 정도 변경
        lowPassFilter.cutoffFrequency = targetCutoff;
    }
}