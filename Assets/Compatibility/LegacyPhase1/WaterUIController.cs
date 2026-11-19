using UnityEngine;

// Keep the legacy serialized spelling while routing updates through the common HUD.
public class WaterUIController : MonoBehaviour
{
    public GameObject bottleSlot;
    public Transform bottlsSlotParent;
    public Sprite emptyBottleSprite, corruptedemptyBottleSprite, waterBottleSprite, corruptedWaterBottleSprite;
    public WaterController waterController;
    private void Awake()
    {
        var hud = GetComponent<PlayerWaterUI>();
        if (hud == null) hud = gameObject.AddComponent<PlayerWaterUI>();
        hud.waterController = waterController;
        hud.bottleSlotPrefab = bottleSlot;
        hud.bottlesParent = bottlsSlotParent;
        hud.emptyBottleSprite = emptyBottleSprite;
        hud.corruptedEmptyBottleSprite = corruptedemptyBottleSprite;
        hud.waterBottleSprite = waterBottleSprite;
        hud.corruptedWaterBottleSprite = corruptedWaterBottleSprite;
    }
}
