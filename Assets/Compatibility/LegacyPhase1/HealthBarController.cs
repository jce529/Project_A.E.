using UnityEngine;

// Original scene GUID retained; rendering and subscription lifetime use the shared HUD.
public class HealthBarController : MonoBehaviour
{
    public Transform heartsParent;
    public GameObject heartContainerPrefab;
    public float healthPerHeart = 10f;
    private PlayerHealthUI hud;
    private void Awake()
    {
        hud = GetComponent<PlayerHealthUI>();
        if (hud == null) hud = gameObject.AddComponent<PlayerHealthUI>();
        hud.heartsParent = heartsParent;
        hud.heartContainerPrefab = heartContainerPrefab;
        hud.healthPerHeart = healthPerHeart;
    }
    public void UpdateHeartsHUD() { if (hud != null) hud.UpdateHealthUI(); }
}
