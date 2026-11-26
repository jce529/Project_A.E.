using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

// Keep the script GUID and all serialized fields; fix the old file/class name mismatch.
[MovedFrom(true, sourceNamespace: null, sourceAssembly: null, sourceClassName: "InteractionPrompt")]
public class InteractivePrompt : MonoBehaviour, IPlayerInteractable
{
    public GameObject promptUI;
    public float interactionDistance = 3f;
    public WaterController waterController;

    private void OnEnable()
    {
        // The shared PlayerInteractionPrompt now owns the only visible prompt.
        if (promptUI != null) promptUI.SetActive(false);
    }

    public bool CanInteract(PlayerInteraction player)
    {
        if (player == null || (player.transform.position - transform.position).sqrMagnitude > interactionDistance * interactionDistance)
            return false;
        var water = player.WaterController != null ? player.WaterController : waterController;
        return water != null && water.waterCounter() + water.corruptedwaterCounter() < water.bottles.Count;
    }

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player)) return;
        var water = player.WaterController != null ? player.WaterController : waterController;
        water.RecoveryWater();
    }
}
