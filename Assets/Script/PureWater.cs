using UnityEngine;

public class PureWater : MonoBehaviour, IPlayerInteractable
{
    public WaterController waterController;

    public bool CanInteract(PlayerInteraction player)
    {
        if (player == null) return false;
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
