using UnityEngine;

public class InteractableWall : MonoBehaviour, IPlayerInteractable
{
    public GameObject wallTilemap;
    [Tooltip("Enable only for walls opened by the player. Boss-locked walls keep this disabled.")]
    public bool allowPlayerInteraction = false;

    public bool CanInteract(PlayerInteraction player) => player != null && allowPlayerInteraction;

    public void Interact(PlayerInteraction player)
    {
        if (CanInteract(player)) UnlockWall();
    }

    // Preserve existing boss-death UnityEvent bindings.
    public void UnlockWall()
    {
        if (wallTilemap != null) wallTilemap.SetActive(false);
        gameObject.SetActive(false);
    }
}
