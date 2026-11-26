using UnityEngine;
using UnityEngine.UI;

public class PlayerWaterUI : MonoBehaviour
{
    [Header("References")]
    public WaterController waterController;

    [Header("Water Bar Prefab Settings")]
    public GameObject waterBarPrefab;  // 소환할 마나(물) 슬라이더 프리팹
    public Transform canvasParent;     // 프리팹이 생성될 캔버스(부모) 위치

    [Header("Slider Settings")]
    [Tooltip("마나가 0일 때 슬라이더가 멈출 최소 비율 (바닥 단면 유지용)")]
    public float minSliderValue = 0.07f;
    [Tooltip("마나가 꽉 찼을 때 슬라이더의 최대 비율")]
    public float maxSliderValue = 1f;

    // 소환된 프리팹에서 자동으로 찾아 연결할 컴포넌트들
    private Slider waterSlider;
    private Image fillImage;

    private void Start()
    {
        if (waterController == null)
            waterController = FindAnyObjectByType<WaterController>();

        // 1. 프리팹 생성 및 슬라이더 내부 구조 찾기
        InitWaterBar();
    }

    private void Update()
    {
        if (waterController != null)
        {
            UpdateWaterUI();
        }
    }

    private void InitWaterBar()
    {
        if (waterBarPrefab != null && canvasParent != null)
        {
            // 캔버스 하위에 프리팹 소환
            GameObject spawnedBar = Instantiate(waterBarPrefab, canvasParent);

            // 최상단의 Slider 컴포넌트 찾기
            waterSlider = spawnedBar.GetComponent<Slider>();
            if (waterSlider != null)
            {
                waterSlider.minValue = 0f;
                waterSlider.maxValue = 1f;
            }

            // 슬라이더 하위에서 Fill 오브젝트를 찾아 Image 캐싱 
            // (만약 유니티 에디터에서 구조 이름을 바꿨다면 아래 경로 문자열을 수정하세요)
            Transform fillTransform = spawnedBar.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
            {
                fillImage = fillTransform.GetComponent<Image>();
            }
        }
        else
        {
            Debug.LogWarning("Water Bar Prefab이나 Canvas Parent가 인스펙터에 할당되지 않았습니다!");
        }
    }

    public void UpdateWaterUI()
    {
        if (waterSlider == null) return;

        int maxWater = waterController.bottles.Count;
        int filledCount = 0;

        // 1. 상태 계산
        for (int i = 0; i < maxWater; i++)
        {
            int[] bottleState = waterController.bottles[i];
            bool isFilled = bottleState[0] == 1;

            if (isFilled)
            {
                filledCount++;
            }
        }

        // 2. 슬라이더 Value 업데이트
        float currentRatio = (float)filledCount / maxWater;
        waterSlider.value = Mathf.Lerp(minSliderValue, maxSliderValue, currentRatio);

        // 3. 내용물(Fill) 활성화/비활성화 (물이 0이면 숨김)
        if (fillImage != null)
        {
            bool hasWater = filledCount > 0;
            fillImage.gameObject.SetActive(hasWater);
        }
    }
}