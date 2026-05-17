using UnityEngine;        
using UnityEngine.UI;

public class BossHealthBarController : MonoBehaviour
{
    public GameObject healthBarPrefab;
    public Transform uiParentCanvas;
    public GameObject bossObject;

    private HP bossHP;
    private boss bossScript;
    private GameObject barInstance;
    private Image fillImage;

    void Start()
    {
        bossScript = bossObject.GetComponent<boss>();
        bossHP = bossObject.GetComponent<HP>();

        barInstance = Instantiate(healthBarPrefab, uiParentCanvas);
        fillImage = barInstance.transform.Find("Fill")?.GetComponent<Image>();
        barInstance.SetActive(false);

        if (bossScript != null)
            bossScript.OnPlayerDetectionChanged += ToggleBar;

        if (bossHP != null)
            bossHP.onHealthChangedCallback += UpdateBar;
    }

    void ToggleBar(bool show)
    {
        if (barInstance != null)
            barInstance.SetActive(show);

        if (show)
            UpdateBar();
    }

    void UpdateBar()
    {
        if (fillImage != null && bossHP != null)
            fillImage.fillAmount = Mathf.Clamp01(bossHP.Health / bossHP.MaxHealth);
    }

    void OnDestroy()
    {
        if (bossScript != null)
            bossScript.OnPlayerDetectionChanged -= ToggleBar;
        if (bossHP != null)
            bossHP.onHealthChangedCallback -= UpdateBar;
    }
}
