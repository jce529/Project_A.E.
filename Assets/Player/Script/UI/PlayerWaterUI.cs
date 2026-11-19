using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWaterUI : MonoBehaviour
{
    public WaterController waterController;
    public Transform bottlesParent;
    public GameObject bottleSlotPrefab;
    public TextMeshProUGUI waterCountText;
    public Sprite waterBottleSprite, corruptedWaterBottleSprite, emptyBottleSprite, corruptedEmptyBottleSprite;
    private readonly List<GameObject> slots = new List<GameObject>();
    private readonly List<Image> bottleImages = new List<Image>();
    public GameObject waterBarPrefab;
    public Transform canvasParent;
    public float minSliderValue = 0.07f;
    public float maxSliderValue = 1f;
    private Slider waterSlider;
    private Image fillImage;
    private WaterController _subscribedWater;
    private bool _started;

    private void Start()
    {
        if (waterController == null) waterController = FindAnyObjectByType<WaterController>();
        if (waterBarPrefab != null && canvasParent != null)
        {
            var bar = Instantiate(waterBarPrefab, canvasParent);
            waterSlider = bar.GetComponent<Slider>();
            if (waterSlider != null) { waterSlider.minValue = 0f; waterSlider.maxValue = 1f; }
            var fill = bar.transform.Find("Fill Area/Fill");
            if (fill != null) fillImage = fill.GetComponent<Image>();
        }
        _started = true;
        Subscribe();
    }

    private void OnEnable() { if (_started) Subscribe(); }
    private void OnDisable()
    {
        if (_subscribedWater != null) _subscribedWater.OnWaterChanged -= UpdateWaterUI;
        _subscribedWater = null;
    }

    private void Subscribe()
    {
        if (waterController != null && _subscribedWater == null)
        {
            _subscribedWater = waterController;
            _subscribedWater.OnWaterChanged += UpdateWaterUI;
        }
        UpdateWaterUI();
    }

    public void UpdateWaterUI()
    {
        if (waterController == null) return;
        int count = waterController.TotalWater;
        if (waterSlider != null)
        {
            float ratio = waterController.Capacity > 0 ? (float)count / waterController.Capacity : 0f;
            waterSlider.value = Mathf.Lerp(minSliderValue, maxSliderValue, ratio);
            if (fillImage != null) fillImage.gameObject.SetActive(count > 0);
        }
        if (bottlesParent != null && bottleSlotPrefab != null)
        {
            while (slots.Count < waterController.Capacity)
            {
                var slot = Instantiate(bottleSlotPrefab, bottlesParent);
                slots.Add(slot);
                bottleImages.Add(slot.GetComponentInChildren<Image>());
            }
            for (int i = 0; i < slots.Count; i++)
            {
                bool active = i < waterController.Capacity;
                slots[i].SetActive(active);
                if (!active || bottleImages[i] == null) continue;
                int[] state = waterController.bottles[i];
                if (state == null || state.Length < 2) continue;
                bottleImages[i].sprite = state[0] == 1
                    ? (state[1] == 1 ? corruptedWaterBottleSprite : waterBottleSprite)
                    : (state[1] == 1 ? corruptedEmptyBottleSprite : emptyBottleSprite);
            }
        }
        if (waterCountText != null) waterCountText.text = $"{count} / {waterController.Capacity}";
    }
}
