using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트 표시를 위해 추가
using System.Collections.Generic;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Heart Settings")]
    public Transform heartsParent;
    public GameObject heartContainerPrefab;
    public float healthPerHeart = 5f; // 하트 1개당 체력 수치

    [Header("Slider Settings (Optional)")]
    public Slider healthSlider; // 슬라이더를 쓰고 싶다면 할당

    [Header("Text Settings (Optional)")]
    public TextMeshProUGUI healthText; // "25 / 50" 처럼 표시할 텍스트

    private List<GameObject> heartContainers = new List<GameObject>();
    private List<Image> heartFills = new List<Image>();

    private void Start()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("PlayerStats Instance를 찾을 수 없습니다. 씬에 PlayerStats가 있는지 확인하세요.");
            return;
        }

        // 초기화 및 콜백 등록
        InitHearts();
        PlayerStats.Instance.onHealthChangedCallback += UpdateHealthUI;
        UpdateHealthUI();
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.onHealthChangedCallback -= UpdateHealthUI;
        }
    }

    private void InitHearts()
    {
        // 기존 하트 제거
        foreach (Transform child in heartsParent)
        {
            Destroy(child.gameObject);
        }
        heartContainers.Clear();
        heartFills.Clear();

        // PlayerStats의 MaxTotalHealth만큼 하트 생성
        // 체력 5당 하트 1개 계산
        int maxHeartCount = Mathf.CeilToInt(PlayerStats.Instance.MaxTotalHealth / healthPerHeart);
        
        for (int i = 0; i < maxHeartCount; i++)
        {
            GameObject heart = Instantiate(heartContainerPrefab, heartsParent);
            heartContainers.Add(heart);
            
            // 프리팹 구조에 따라 HeartFill 이미지를 찾음
            Transform fillTransform = heart.transform.Find("HeartFill");
            if (fillTransform != null)
            {
                heartFills.Add(fillTransform.GetComponent<Image>());
            }
        }
    }

    public void UpdateHealthUI()
    {
        float currentHealth = PlayerStats.Instance.Health;
        float maxHealth = PlayerStats.Instance.MaxHealth;

        // 1. 하트 업데이트
        for (int i = 0; i < heartContainers.Count; i++)
        {
            // 현재 최대 체력 범위 내의 하트만 활성화
            if (i * healthPerHeart < maxHealth)
            {
                heartContainers[i].SetActive(true);
                
                // 해당 하트가 담당하는 체력 범위 내에서 FillAmount 계산
                // 예: 하트 0번(체력 0~5), 하트 1번(체력 5~10)
                float healthInThisHeart = currentHealth - (i * healthPerHeart);
                float fillValue = healthInThisHeart / healthPerHeart;
                
                heartFills[i].fillAmount = Mathf.Clamp01(fillValue);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }

        // 2. 슬라이더 업데이트 (할당된 경우)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // 3. 텍스트 업데이트 (할당된 경우)
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}
