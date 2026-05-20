using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerWaterUI : MonoBehaviour
{
    [Header("References")]
    public WaterController waterController;
    public Transform bottlesParent;
    public GameObject bottleSlotPrefab;

    [Header("Text Settings (Optional)")]
    public TextMeshProUGUI waterCountText; // "3 / 5" 처럼 숫자로도 표시

    [Header("Sprites")]
    public Sprite waterBottleSprite;        // 가득 찬 물병 (순수)
    public Sprite corruptedWaterBottleSprite; // 가득 찬 물병 (오염)
    public Sprite emptyBottleSprite;         // 빈 물병 (순수 계열)
    public Sprite corruptedEmptyBottleSprite; // 빈 물병 (오염 계열)

    private List<Image> bottleImages = new List<Image>();

    private void Start()
    {
        if (waterController == null)
            waterController = FindAnyObjectByType<WaterController>();

        if (waterController != null)
        {
            InitBottles();
        }
    }

    private void Update()
    {
        if (waterController != null)
        {
            UpdateWaterUI();
        }
    }

    private void InitBottles()
    {
        // 기존 UI 제거
        foreach (Transform child in bottlesParent)
        {
            Destroy(child.gameObject);
        }
        bottleImages.Clear();

        // WaterController의 초기 병 개수만큼 슬롯 생성
        for (int i = 0; i < waterController.bottles.Count; i++)
        {
            GameObject slot = Instantiate(bottleSlotPrefab, bottlesParent);
            Image img = slot.GetComponent<Image>();
            if (img == null) img = slot.GetComponentInChildren<Image>();
            if (img == null)
                Debug.LogError($"[PlayerWaterUI] bottleSlotPrefab({slot.name})에 Image 컴포넌트가 없습니다!", slot);
            bottleImages.Add(img);
        }
    }

    public void UpdateWaterUI()
    {
        // 런타임에 병 개수가 변할 수 있으므로 체크
        if (bottleImages.Count != waterController.bottles.Count)
        {
            InitBottles();
            return;
        }

        int filledCount = 0;
        for (int i = 0; i < waterController.bottles.Count; i++)
        {
            int[] bottleState = waterController.bottles[i];
            bool isFilled = bottleState[0] == 1;
            bool isCorrupted = bottleState[1] == 1;

            if (bottleImages[i] == null) continue;

            if (isFilled)
            {
                filledCount++;
                bottleImages[i].sprite = isCorrupted ? corruptedWaterBottleSprite : waterBottleSprite;
            }
            else
            {
                bottleImages[i].sprite = isCorrupted ? corruptedEmptyBottleSprite : emptyBottleSprite;
            }
        }

        // 텍스트 업데이트 (현재 물 양 / 전체 병 개수)
        if (waterCountText != null)
        {
            waterCountText.text = $"{filledCount} / {waterController.bottles.Count}";
        }
    }
}
