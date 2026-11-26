using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar Prefab Settings")]
    public GameObject healthBarPrefab; // 생성할 체력바 프리팹 할당
    public Transform canvasParent;     // 체력바가 소환될 부모 캔버스 (또는 패널) 위치

    private Slider healthSlider;       // 소환된 프리팹에서 자동으로 뽑아올 슬라이더 컴포넌트

    private void Start()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("PlayerStats Instance를 찾을 수 없습니다.");
            return;
        }

        // 1. 프리팹 소환 및 슬라이더 연결
        InitHealthBar();

        // 2. 체력 변경 이벤트 구독 (체력이 변할 때마다 UpdateHealthUI 실행)
        PlayerStats.Instance.onHealthChangedCallback += UpdateHealthUI;

        // 3. 게임 시작 시 체력 UI 초기 갱신
        UpdateHealthUI();
    }

    private void OnDestroy()
    {
        // 스크립트 파괴 시 이벤트 구독 해제
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.onHealthChangedCallback -= UpdateHealthUI;
        }
    }

    private void InitHealthBar()
    {
        if (healthBarPrefab != null && canvasParent != null)
        {
            // 지정한 캔버스(부모) 하위에 체력바 프리팹 생성
            GameObject spawnedBar = Instantiate(healthBarPrefab, canvasParent);

            // 생성된 오브젝트에서 Slider 컴포넌트를 찾아서 캐싱
            healthSlider = spawnedBar.GetComponent<Slider>();

            // 만약 프리팹 최상위가 아니라 자식 오브젝트에 슬라이더가 있다면 아래 코드를 대신 사용하세요:
            // healthSlider = spawnedBar.GetComponentInChildren<Slider>();
        }
        else
        {
            Debug.LogWarning("체력바 프리팹이나 Canvas 부모가 인스펙터에 할당되지 않았습니다!");
        }
    }

    public void UpdateHealthUI()
    {
        // 슬라이더가 정상적으로 생성되고 연결되었을 때만 작동
        if (healthSlider != null)
        {
            healthSlider.maxValue = PlayerStats.Instance.MaxHealth;
            healthSlider.value = PlayerStats.Instance.Health;
        }
    }
}